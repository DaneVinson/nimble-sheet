# Character Sheet UI — Design Spec

**Date:** 2026-06-12
**Component:** `NS.Client` (SvelteKit SPA)
**Status:** Approved for planning

## Goal

Build a **display-only**, dark-mode character sheet in `NS.Client` that renders **all** sections of a `Hero` from a typed fixture. This is the first real front-end feature on top of the Phase A backend. It is a **pure front-end slice — no backend changes**.

## Scope

**In scope**
- A read-only character sheet rendering every section of a full hero.
- A typed data layer (DTO types + fixture + resolver) that mirrors real API responses so a later slice can swap the fixture for live `fetch()` calls with no component changes.
- Dark-mode visual design ("modern dark app" direction), responsive to mobile.
- Resolver unit tests (written after implementation).

**Out of scope (deferred to later slices)**
- Any mutation/interaction. The HP damage/heal popover is **designed and documented here but not built**.
- Authentication, hero list, create/edit (build) form.
- Live API calls and reference-data fetching.
- Backend seeding (the DB is empty; we use a client fixture instead).

## Decisions (from brainstorming)

| Decision | Choice |
|---|---|
| First-slice scope | Display only, no interactions |
| Data source | Client TypeScript fixture (no backend work) |
| Sections | Full hero — every section |
| Art direction | Modern dark app, dark mode |
| Layout | Prominent banner + featured vitals + stats + skills **always visible**; the rest in tabs |
| Tabs | Combat · Magic · Class Resources · Inventory · Features (5 tabs; Class Resources is its own tab) |
| Header | No crest/avatar badge; large name + Ancestry·Class·Level subtitle; prominent vitals row |
| Save markers | `SAVE▲` (green, advantage) / `SAVE▼` (red, disadvantage) carets on stat blocks, with tooltip |
| HP control | Designed (popover) but **deferred** — not built this slice |
| Data/types architecture | Approach B — mirror API DTOs + resolver layer; fixture shaped like real API responses |
| Route | `/sheet` for this slice (eventual home `/heroes/[id]`) |
| Tests | Vitest resolver unit tests, written after implementation |

## Architecture

Three layers keep the eventual API swap nearly free:

### 1. API DTO types — `src/lib/api/types.ts`
TypeScript interfaces mirroring the C# DTOs exactly:
- `Hero` — scalar properties plus ID-referenced collections: `HeroWeapon { weaponId, isEquipped, notes }`, `HeroArmor { armorId, isEquipped }`, `HeroSpell { spellId, tierUnlocked, notes }`, `HeroMagicItem { magicItemId, isEquipped, chargesRemaining }`, `HeroFeature { featureId, levelGained, choices }`, `HeroCondition { conditionId, expiresAtEndOf }`, `HeroGearItem { name, quantity }`.
- Value objects: `HeroStats`, `HeroSkills`, `HeroCombatStats`, `HeroSaves`, `ClassResources`.
- Reference entities: `Weapon`, `Armor`, `Spell`, `MagicItem`, `Feature`, `Condition`, `Ancestry`, `Background`, `ActionReference`, `RuleReference` (only those the sheet needs are required; others may be added as used).
- Enums as **string-union types matching `JsonStringEnumConverter` names**: e.g. `HeroClass = 'Berserker' | 'Cheat' | … | 'Zephyr'`, `DieType = 'D4' | 'D6' | 'D8' | 'D10' | 'D12'`, `StatType = 'Strength' | 'Dexterity' | 'Intelligence' | 'Will'`, `DamageType`, `ArmorType`, `SpellSchool`, `ActionType`, `RuleCategory`.

> Note: `id` fields are GUID strings. SoloDB/JSON serializes property names in camelCase via FastEndpoints' serializer; types use camelCase to match.

### 2. Fixture — `src/lib/fixtures/caldra.ts`
A `Hero` object plus a `ReferenceData` bundle (`{ weapons, armor, spells, magicItems, features, conditions, ancestries, backgrounds }`) containing exactly the reference entities the hero points to. Shaped identically to API payloads. Content: **Caldra Brightward, Oathsworn 1** (see Fixture Content below).

### 3. Resolver / selectors — `src/lib/sheet/resolve.ts`
Pure functions taking `Hero + ReferenceData` and producing the **view models** the components render:
- `resolveWeapons` — `HeroWeapon` joined with `Weapon` (name, damage expression, damage type, stat used, equipped).
- `resolveArmor` — `HeroArmor` joined with `Armor` (name, type, armor value, equipped).
- `resolveSpells` — `HeroSpell` joined with `Spell`, **grouped by tier then school**.
- `resolveFeatures` — `HeroFeature` joined with `Feature`, **grouped/sorted by level gained**.
- `resolveConditions`, `resolveMagicItems`, `resolveGear`.
- Display helpers: modifier formatting (`+4` / `-1` / `0`), `DieType`→`d10`, save-marker derivation from `HeroSaves` (which stat is advantage/disadvantage), skills list with governing stat.
- **Robustness:** a referenced ID with no matching reference entity yields a fallback label (e.g. "Unknown weapon") rather than throwing.

Resolver output types live in `src/lib/sheet/viewmodel.ts`.

## Components — `src/lib/sheet/components/`

**Top level**
- `HeroSheet.svelte` — receives the resolved view model; composes the pinned region and tabs.

**Pinned region (always visible)**
- `HeroBanner.svelte` — large name + "Ancestry · Class · Level" subtitle; gradient background; no crest.
- `VitalsRow.svelte` — responsive grid of vital tiles.
  - `HpTile.svelte` — current / temp / max. Structured so the deferred damage/heal popover can be dropped in later with no refactor.
  - `WoundTrack.svelte` — 6 pips + skull; fills from `currentWounds`; dead (≥6) and dying (0 HP) styling.
  - `ArmorTile.svelte`, `InitTile.svelte`, `HitDiceTile.svelte` (`d10`, available / max).
- `StatRow.svelte` → `StatBlock.svelte` — stat value, label, and `SAVE▲`/`SAVE▼` caret + tooltip.
- `SkillsRow.svelte` — 10 skills, each with governing stat label and bonus.

**Tabs**
- `SheetTabs.svelte` — tab navigation + panel switching (Flowbite Tabs).
- `CombatPanel.svelte` — weapons, armor, conditions.
- `MagicPanel.svelte` — spells (grouped by tier/school).
- `ClassResourcesPanel.svelte` — mana, Judgment Dice, Lay on Hands pool, Thrill charges (only the resources present).
- `InventoryPanel.svelte` — magic items, gear.
- `FeaturesPanel.svelte` — class features by level.

**Shared**
- `Panel.svelte` — titled card wrapper used by tab panels.
- Every panel renders a clear **empty state** when its collection is empty ("No spells known", "No conditions", etc.).

## Route & styling

- **Route:** `src/routes/sheet/+page.svelte` (`/sheet`). Loads the fixture, runs the resolver, renders `<HeroSheet>`. The scaffold landing page (`/`) is left intact; the eventual home for a sheet is `/heroes/[id]` once auth/list exist.
- **Styling:** Tailwind v4 utilities (CSS-first config already in `src/app.css`) plus a few Flowbite Svelte components (Tabs, Badge, Tooltip). **Dark mode forced** on the sheet wrapper (apply the `dark` class).
- **Responsive:** vitals, stat, and skill grids and the tab bar collapse to a single column on small screens.

## Fixture content (Caldra Brightward)

Faithful to the reference image (`nimble_character.png`):
- Class `Oathsworn`, level 1, ancestry Human, name "Caldra Brightward".
- Stats: STR 2, DEX 0, INT −1, WIL 2. Saves: advantage STR, disadvantage DEX.
- Combat: armor 8, initiative 0, hit die `D10`, speed 6. HP: current 17 / max 17, temp 0. Wounds 0. Hit dice 1 / 1.
- Skills: Arcana −1, Examination −1, Finesse 0, Influence +4, Insight +4, Lore −1, Might +2, Naturecraft +2, Perception +2, Stealth 0.
- Weapons: Mace (`1d6+2`, STR). Armor: Rusty Mail (Mail, 6+DEX → armorValue 6), Wooden Buckler (Shield, +2).
- Features: Radiant Judgment (L1), Lay on Hands (L1). Class resources: Judgment Dice 2×`D6`, Lay on Hands pool 5.
- Deliberately empty: spells, magic items, conditions, gear — so their panels' empty states are exercised.

This single fixture therefore covers **both rendering paths across the set of panels**: populated panels (weapons, armor, features, class resources) and empty panels (spells, magic items, conditions, gear). It is not a goal to show the same panel both populated and empty.

## Testing & verification

- **Resolver unit tests** (`src/lib/sheet/resolve.test.ts`) with Vitest, written **after** implementation: join correctness, spell grouping by tier/school, feature grouping by level, save-marker derivation, modifier formatting, `DieType` formatting, and missing-reference fallback. Add Vitest as a `devDependency` and a `test` script to `NS.Client/package.json`.
- **Components**: verified visually via `npm run dev` against the fixture.
- **Gates**: `npm run check` at **0 errors / 0 warnings**; `npm run build` succeeds; the existing NS.WebApp static-hosting integration still serves the SPA.

## Future slices (not built now)

- API integration: replace the fixture import with `fetch()` to `GET /heroes/{id}` + reference endpoints; resolver and components unchanged.
- HP damage/heal popover and the other live-play mutations (wired to the granular mutation endpoints).
- Auth (login), hero list, create/edit (build) form; move the sheet to `/heroes/[id]`.
