# Hero Build Form — Design

**Date:** 2026-06-14
**Status:** Approved (brainstorming) — ready for implementation plan

## Problem

Heroes can currently only be created or edited by calling the API directly (`POST /heroes`, `PUT /heroes/{heroId}` with a `HeroBuildRequest` body). The NS.Client SPA has no UI to build or edit a hero, so a user cannot go from "logged in, no heroes" to "viewing a live sheet" without out-of-band API calls. This slice adds a client form for creating and editing a hero's **build** attributes.

Reference-data seeding (shipped 2026-06-14) means the ancestry/background dropdowns now resolve real options end-to-end.

## Goals

- A **shared** create/edit form covering the `HeroBuildRequest` fields.
- **Create** at `/heroes/new` (POST → navigate to the new sheet) and **Edit** at `/heroes/[id]/edit` (PUT → back to the sheet), pre-filled from the existing hero.
- Single sectioned page (Layout A): all field groups stacked as cards, one Save action.
- Light client validation of the genuinely-required fields; the server stays authoritative.
- Entry points from the heroes list ("New hero") and the hero sheet ("Edit").

## Non-Goals

- Editing the hero's **collections** — weapons, armor, spells, magic items, gear, conditions, features. These are managed by separate add/remove endpoints and are deliberately preserved by the server's `Hero.UpdateBuild(...)`. They are out of scope for this form.
- Level-up, subclass, or play-state editing (separate endpoints/flows).
- Conditional-by-class resource fields (explicitly out: all class-resource fields are shown and optional).
- Server-side changes — the `HeroBuildRequest`, validator, and both endpoints already exist and are unchanged by this slice.

## Decisions (from brainstorming)

| Question | Decision |
|---|---|
| Scope | Create **and** Edit, sharing one form component |
| Layout | Single sectioned page (all groups as stacked cards, one Save) |
| Class resources & mana | Always shown, every field optional/blankable ("leave blank if not used by your class") |
| Validation | Light, required-fields-only on the client; server authoritative |
| Submission | Client-side (`apiFetch` → navigate); SvelteKit form actions are unavailable in this pure SPA |

## Architecture

NS.Client is a pure SPA (`ssr = false`, `@sveltejs/adapter-static`), so there is no `+page.server`/form-action path — submission happens client-side via the existing `apiFetch` wrappers, then `goto(...)`. A single shared `HeroBuildForm.svelte` serves both routes (DRY); the route pages are thin adapters that supply the initial model + reference data and handle the submit/navigate.

### Build model (`$lib/sheet/build/model.ts`)

`HeroBuildModel` — a TypeScript type mirroring `HeroBuildRequest` in camelCase, reusing the existing value-object/enum types from `$lib/api/types.ts`:

```ts
export interface HeroBuildModel {
  name: string;
  ancestryId: string;        // '' until selected
  backgroundId: string | null;
  heroClass: HeroClass;
  maxHp: number;
  maxMana: number | null;
  combatStats: HeroCombatStats;   // armor, hitDieType, initiativeBonus, speed
  resources: ClassResources;      // judgmentDiceCount/Type, layOnHandsPool, thrillCharges (all nullable)
  saves: HeroSaves;               // advantageOn, disadvantageOn
  skills: HeroSkills;             // 10 numbers
  stats: HeroStats;               // dexterity, intelligence, strength, will
}
```

- `blankBuildModel(): HeroBuildModel` — sensible defaults for create: empty name, `ancestryId: ''`, `backgroundId: null`, `heroClass: 'Berserker'` (first enum value), `maxHp: 1`, `maxMana: null`, stats/skills all `0`, `combatStats { armor: 0, hitDieType: 'D8', initiativeBonus: 0, speed: 6 }`, `saves { advantageOn: 'Strength', disadvantageOn: 'Dexterity' }`, `resources` all `null`.
- `heroToBuildModel(hero: Hero): HeroBuildModel` — maps a loaded hero's build fields onto the model for edit pre-fill (`heroClass` from `hero.class`; copies `combatStats`, `resources`, `saves`, `skills`, `stats`, `maxHp`, `maxMana`, `ancestryId`, `backgroundId`, `name`).

The model serializes directly to the `HeroBuildRequest` body (the API maps `heroClass` ⇄ the request's `HeroClass` property case-insensitively; enums travel as their string names).

### Validation (`$lib/sheet/build/validate.ts`)

A pure `validateBuild(model): BuildErrors` returning a record of field → message for the required fields only:

- `name` non-empty,
- `ancestryId` non-empty (a selection was made),
- `maxHp` > 0,
- `maxMana` ≥ 0 when not null.

(`heroClass` always carries a value from the select default, so it needs no rule.)

`BuildErrors` is `Partial<Record<'name' | 'ancestryId' | 'maxHp' | 'maxMana', string>>`. The form blocks submit when non-empty and shows inline messages; everything else (free-integer stats/skills/combat fields) defers to the server.

### Components (`$lib/sheet/build/`)

- **`HeroBuildForm.svelte`** — props `initial: HeroBuildModel`, `ancestries: Ancestry[]`, `backgrounds: Background[]`, `submitLabel: string`, `onsubmit: (model: HeroBuildModel) => Promise<void>`. Owns a reactive `$state` clone of `initial`, runs `validateBuild` on submit, surfaces a form-level error banner for a thrown `ApiError`, and disables Save while submitting. Renders the seven sections, each bound to its slice of the model.
- **Section components** (each small, one group): `IdentitySection`, `VitalsSection`, `CombatSection`, `StatsSection`, `SavesSection`, `SkillsSection`, `ClassResourcesSection`. Dark-tone Tailwind utilities consistent with the existing sheet components; enum selects (`HeroClass`, `DieType`, `StatType`) are populated from string-literal option arrays.

### Routes

- **`(app)/heroes/new/+page.ts`** — loads `ancestries` + `backgrounds` via the reference cache (`getCollection`); returns them. **`+page.svelte`** — `initial = blankBuildModel()`, `onsubmit` calls `createHero(model)` then `goto('/heroes/' + id)`.
- **`(app)/heroes/[id]/edit/+page.ts`** — loads the hero (`getHero(params.id)`) + the reference collections; maps a 404 to the SvelteKit `error(404, …)` (mirroring the existing `[id]` route's pattern). **`+page.svelte`** — `initial = heroToBuildModel(data.hero)`, `onsubmit` calls `updateHero(id, model)` then `goto('/heroes/' + id)`. A sibling **`+error.svelte`** renders the 404 boundary.
- Navigation entry points: a **"New hero"** button/link on `(app)/heroes/+page.svelte`; an **"Edit"** link on the hero sheet header at `/heroes/[id]` (added to `HeroSheet`/`HeroBanner` or the `[id]` page chrome).

### API wrappers (`$lib/api/client.ts`)

```ts
export function createHero(build: HeroBuildModel): Promise<{ id: string }> {
  return apiFetch<{ id: string }>('/heroes', { method: 'POST', body: JSON.stringify(build) });
}
export function updateHero(id: string, build: HeroBuildModel): Promise<void> {
  return apiFetch<void>(`/heroes/${id}`, { method: 'PUT', body: JSON.stringify(build) });
}
```

## Data flow

1. Route load fetches reference collections (cached) and, for edit, the hero.
2. Page builds `initial` (`blankBuildModel()` or `heroToBuildModel(hero)`) and renders `HeroBuildForm`.
3. User edits the bound `$state` model across the sections.
4. On Save: `validateBuild` runs; if clean, `onsubmit(model)` calls `createHero`/`updateHero`.
5. Success → `goto` the sheet. Failure → the thrown `ApiError`'s message shows in the form banner; the user stays on the form.

## Testing (Vitest, tests-after)

- `blankBuildModel()` returns the documented defaults; `heroToBuildModel(hero)` maps every build field (a fixture hero round-trips: each model field equals the hero's corresponding build field).
- `validateBuild`: passes a complete model; flags empty name, empty `ancestryId`, `maxHp` ≤ 0, and negative `maxMana`; allows `maxMana: null`.
- `createHero` posts to `/heroes` with the JSON body and returns the parsed `{ id }`; `updateHero` PUTs to `/heroes/{id}` and resolves on 204. (Mirror the existing `client.test.ts` mock-fetch pattern.)

Components are not unit-tested (no component harness in this project), consistent with prior slices.

## Files

**New**
- `NS.Client/src/lib/sheet/build/model.ts`
- `NS.Client/src/lib/sheet/build/validate.ts`
- `NS.Client/src/lib/sheet/build/HeroBuildForm.svelte`
- `NS.Client/src/lib/sheet/build/IdentitySection.svelte`
- `NS.Client/src/lib/sheet/build/VitalsSection.svelte`
- `NS.Client/src/lib/sheet/build/CombatSection.svelte`
- `NS.Client/src/lib/sheet/build/StatsSection.svelte`
- `NS.Client/src/lib/sheet/build/SavesSection.svelte`
- `NS.Client/src/lib/sheet/build/SkillsSection.svelte`
- `NS.Client/src/lib/sheet/build/ClassResourcesSection.svelte`
- `NS.Client/src/routes/(app)/heroes/new/+page.ts`
- `NS.Client/src/routes/(app)/heroes/new/+page.svelte`
- `NS.Client/src/routes/(app)/heroes/[id]/edit/+page.ts`
- `NS.Client/src/routes/(app)/heroes/[id]/edit/+page.svelte`
- `NS.Client/src/routes/(app)/heroes/[id]/edit/+error.svelte`
- `NS.Client/src/lib/sheet/build/model.test.ts`, `validate.test.ts` (and wrapper tests appended to `client.test.ts`)

**Modified**
- `NS.Client/src/lib/api/client.ts` — `createHero`, `updateHero`
- `NS.Client/src/routes/(app)/heroes/+page.svelte` — "New hero" entry point
- The hero sheet at `/heroes/[id]` — "Edit" entry point (exact host component decided in the plan)

## Risks / open items

- **Enum option lists** are maintained as TS string arrays mirroring the `api/types.ts` unions; if the domain enums change, these must be updated (low risk, small surface).
- **Edit "Save" semantics:** `UpdateBuild` clamps `currentHp`/`currentMana` to lowered maxima and preserves level/subclass/collections — the form only sends build fields, matching that contract; no extra client handling needed.
- **"Edit" entry-point host:** which component carries the Edit link on the sheet is finalized in the plan (candidate: the `[id]` page chrome or `HeroBanner`).
