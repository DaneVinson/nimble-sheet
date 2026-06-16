# Features Editing — Design

**Date:** 2026-06-15
**Status:** Approved (brainstorming) — ready for implementation plan

## Problem

The collection-editing work (2026-06-15) added inline add/remove for weapons, armor, magic items, gear, spells, and conditions, but **features** were deliberately deferred because they carry extra semantics: each `Feature` has a `class`, a `level`, optional `selectableOptions`, and a `subclass`, and a hero's `HeroFeature` records the player's `choices` plus the `levelGained`. The Features panel is still display-only. This slice adds inline add/remove of features on the sheet, capturing selectable-option choices at add time.

## Goals

- Inline **add/remove** of class features on the sheet's Features panel, using the established editor pattern (optional `HERO_ACTIONS` context, read-only when absent, server-authoritative, POST → `invalidateAll()`).
- The "+ Add" picker is filtered to features matching the **hero's class** with **`level ≤ hero.level`**, excluding already-owned features.
- When a selected feature has `selectableOptions`, the player picks them as **multi-select checkboxes** (not required); the picks are sent as `choices`. Features without options add with empty choices.
- `levelGained` defaults to the selected feature's own `level`, editable.

## Non-Goals

- **Server changes** — `AddFeature`/`RemoveFeature` endpoints and the `HeroFeature` domain record already exist and are unchanged.
- **Editing choices after add** — there is no edit-choices endpoint; choices are set at add time. To change them, remove and re-add. (Consistent with the other collections.)
- Level-up flow, subclass selection, or pending-choice resolution — separate concerns with their own endpoints.

## Decisions (from brainstorming)

| Question | Decision |
|---|---|
| Picker scope | Hero's class **and** `feature.level ≤ hero.level`; exclude owned |
| Selectable options | Multi-select checkboxes on add, **not required**; features without options add with `[]` |
| `levelGained` | Defaults to the selected feature's `level`, editable number input |

## Architecture

Mirrors the six existing collection editors (closest analog: `ConditionEditor`, a reference-backed picker), with two feature-specific wrinkles: a two-axis catalog filter (class + level) and the choices checkboxes. All client-side; no domain/API changes.

### Client changes (NS.Client only)

**`src/lib/api/client.ts`** — two wrappers (POST under `/api`, return `void` on 204):
- `addFeature(heroId, featureId, choices, levelGained)` → `POST /heroes/{id}/add-feature`, body `{ featureId, choices, levelGained }`
- `removeFeature(heroId, featureId)` → `POST /heroes/{id}/remove-feature`, body `{ featureId }`

(The server's `AddFeatureRequest(HeroId, Choices, FeatureId, LevelGained)` binds by name, so JSON field order is irrelevant; `HeroId` comes from the route.)

**`src/lib/sheet/heroActions.svelte.ts`** — extend `HeroActions` and `createHeroActions`:
- `addFeature(featureId: string, choices: string[], levelGained: number): Promise<void>`
- `removeFeature(featureId: string): Promise<void>`
Each via the existing `run(() => …)`.

**`src/lib/sheet/viewmodel.ts` + `resolve.ts`** — add `featureId: string` as the first field of `FeatureViewModel`, populated in `buildFeatures` in **both** the resolved branch and the "Unknown feature" fallback (so a feature with a missing reference is still removable).

**`src/lib/sheet/components/FeatureEditor.svelte`** (new) — props `features: FeatureLevelGroup[]`, `heroClass: HeroClass`, `heroLevel: number`. Renders the level-grouped list (as `FeaturesPanel` does today) with a ✕ remove button per feature when `actions` is present. "+ Add" `TilePopover`:
- on open, lazily `getCollection<Feature>('features')` (session-cached); `catalogError` try/catch.
- `available = catalog.filter(f => f.class === heroClass && f.level <= heroLevel && !ownedIds.has(f.id))`.
- a `<select>` of `available` (label e.g. `{name} (L{level})`).
- an `onSelect` handler sets `levelGained = selected.level` and resets `choices = []`.
- when the selected feature's `selectableOptions` is non-empty, render a checkbox per option bound into a `choices` string array.
- a "Level gained" number input (`bind:value={levelGained}`, min 1).
- Add button (disabled while `actions.busy` or no selection) → `actions.addFeature(selectedId, choices, levelGained)`, then reset.
- Reuses `editorButton` from `./styles`; reads `HERO_ACTIONS` optionally (read-only list when absent).

**`src/lib/sheet/components/FeaturesPanel.svelte`** — replace the inline grouped list with `<FeatureEditor features={vm.features} heroClass={vm.className} heroLevel={vm.level} />`; remove the now-unused `Panel` import.

## Error handling

Mutations flow through `apiFetch` → `runAction`: 401 clears session + redirects to `/login`; other non-2xx surface `ApiError` into the shared `actions.error` (shown in the popover); catalog-fetch failures show `catalogError`. No new error paths.

## Testing

- **Vitest** — `addFeature` wrapper test (asserts `/api/heroes/{id}/add-feature` + body `{ featureId, choices, levelGained }`); a resolver test asserting `featureId` is carried onto the view model.
- **No domain tests** — no domain change.
- **Browser verification** — add a feature that has `selectableOptions` (check a choice), confirm it renders with "Chosen: …" under the right level group, then remove it. Same Playwright approach as the prior verification (rebuild SPA into `wwwroot` first).

## Implementation order

Single cohesive slice (one editor, no server work): client wrappers + actions → view-model `featureId` + resolver → `FeatureEditor` + `FeaturesPanel` wiring → tests → browser verification + docs.
