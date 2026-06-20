# Hero Build: Player-Set Inputs vs. Auto-Derived Attributes

**Date:** 2026-06-19
**Status:** Approved (design)
**Scope:** The hero **create/edit view** and the domain/derivation foundation it needs. The level-up
flow is explicitly a follow-up (see [Out of Scope](#out-of-scope--follow-ups)).

---

## Problem

The current create/edit view lets the player set almost everything directly — ability/stat values,
skills, saves, combat stats, class resources, max HP, max mana. In Nimble most of these are **not**
player choices; they are determined by the player's real choices (class, ancestry, background, base
ability scores) plus rules. This makes the form misleading and unvalidated.

This change makes the create/edit view expose only the genuinely player-set inputs and **derives**
everything else from them, with server-authoritative validation.

### Player-set vs. auto-determined

| Element | Player sets? | Determined by |
|---|---|---|
| Ancestry | Yes | — |
| Background | Yes | — |
| Class | Yes (create only) | — |
| Subclass | Yes, when unlocked | Class level (handled on the sheet, **not** this form) |
| Base ability scores | Yes (create only, point-buy) | — |
| Final ability scores | No | Base + ancestry bonuses |
| Ability modifiers | No | Final ability scores |
| Hit points (max) | No (create) / bounded (edit) | Class HD + level |
| Combat (armor/init/speed) | No | Class (+ equipment, later) |
| Skills | No | Ability modifier of the keyed stat |
| Class features / subclass features | No | Class/subclass progression |
| Class resources / mana | No | Class + level |
| Equipment | Yes (within options) | Edited on the sheet, **not** this form |

---

## Decisions (from brainstorming)

1. **Ability model is D&D-style three-tier.** Store **base** scores (8–15, point-buy). **Final** score
   = base + ancestry bonus. **Modifier** = `floor((final − 10) / 2)`. The *modifier* is what feeds
   skills/saves/rolls and is what the existing `HeroStats` now represents.
2. **Only ancestry adds to starting abilities.** Class does **not** modify starting ability scores;
   class-granted ability increases happen at level-up (a follow-up). So the only new bonus data is an
   **ancestry → ability bonus** field.
3. **Bonus data gap → framework now, zeros initially.** Ancestries/backgrounds are placeholders and
   the quickstart defines no numeric ability/skill bonuses. We add the ancestry bonus field and seed
   it all-zero; real numbers can be dropped in later as pure data. Skills derive purely from the
   associated ability's modifier (no separate skill-bonus data).
4. **Architecture: compute-and-persist at write time (Approach A).** Store `BaseAbilityScores`; on
   create/update the server runs one pure derivation step and persists the derived attributes into the
   hero's existing fields. The sheet, resolver, and untouched level-up flow keep reading stored
   values. Staleness if rules/data later change is accepted (a re-save fixes it).
5. **Skill/save/HP/mana mapping is rules-backed** (`docs/rules/nimble-basic-rules.md`): 4 stats
   (STR/DEX/INT/**WIL**); WIL governs Influence/Insight/Naturecraft/Perception. The four quickstart
   classes are **Cheat, Hunter, Mage, Oathsworn** (the ones with full stat blocks and seeded
   features). Saves and Starting HP/Hit-Die are per class; mana is a per-class, **level-gated**
   formula (Mage casts from L1 = INT×3 + level; Oathsworn casts from L2 = WIL + level; Cheat/Hunter
   never). Oathsworn also has class resources (Judgment Dice, Lay on Hands).
6. **Form field behavior:** Max HP — auto at create, editable-within-class+level-bounds on edit. Mana
   — hidden for non-casters, derived read-only for casters. Subclass — not in this form. Equipment —
   not in this form.

---

## Domain model (`NS.Domain`)

### New value object

```csharp
/// Ability scores or score adjustments, by stat. Used for base scores (8–15), ancestry bonuses
/// (default 0), and computed final scores.
public sealed record AbilityScores(int Dexterity, int Intelligence, int Strength, int Will);
```

### `Hero`

- Add stored property `AbilityScores BaseAbilityScores` with `private set` and the same
  null/init-tolerance pattern used by other reference-type properties (SoloDB rehydrates via
  uninitialized objects).
- `Stats` (`HeroStats`) now holds **ability modifiers**; `Skills`, `Saves`, `CombatStats`,
  `Resources`, `MaxHp`, `MaxMana` remain stored but are **server-derived** (no longer client-supplied).
- **Creation** — replace the build-input constructor usage with:
  ```csharp
  public static Hero Create(
      string name, HeroClass heroClass, Guid ancestryId, Guid? backgroundId,
      AbilityScores baseScores, AbilityScores ancestryBonuses, Guid userId);
  ```
  Computes the derived bundle via `HeroDerivation.Derive(heroClass, baseScores, ancestryBonuses,
  level: 1)`, sets level 1 and the existing play-state defaults (CurrentHp = MaxHp, etc.).
- **`UpdateBuild`** new signature:
  ```csharp
  public void UpdateBuild(
      string name, Guid ancestryId, Guid? backgroundId,
      AbilityScores ancestryBonuses, int maxHp);
  ```
  Recomputes ancestry-dependent derived attributes (ancestry may change → final scores → modifiers →
  skills → mana), clamps `maxHp` to the class+level bounds, and preserves **class, base scores, level,
  subclass, play-state, and all collections**. (`CurrentHp`/`CurrentMana` clamp to lowered maxima, as
  today.)

### `Ancestry`

Add `AbilityScores AbilityBonuses` to the positional record (alphabetical position in params).
Seeded all-zero (see [Seed](#seed-data-nssolodb)).

---

## Derivation (pure, `NS.Domain`)

### `HeroDerivation`

```csharp
public sealed record DerivedAttributes(
    HeroCombatStats CombatStats, int MaxHp, int? MaxMana,
    ClassResources Resources, HeroSaves Saves, HeroSkills Skills, HeroStats Stats);

public static class HeroDerivation
{
    public static int AbilityModifier(int finalScore);              // floor((score - 10) / 2)
    public static AbilityScores FinalScores(AbilityScores baseScores, AbilityScores ancestryBonuses);
    public static DerivedAttributes Derive(
        HeroClass heroClass, AbilityScores baseScores, AbilityScores ancestryBonuses, int level);
}
```

Derivation rules:

- **Final scores** = base + ancestry bonuses (per stat).
- **Modifiers** (`Stats`) = `AbilityModifier(finalScore)` per stat. Mapping for 8–15:
  8→−1, 9→−1, 10→0, 11→0, 12→+1, 13→+1, 14→+2, 15→+2.
- **Skills** (`HeroSkills`) = the keyed stat's modifier:
  - STR: Might
  - DEX: Finesse, Stealth
  - INT: Arcana, Examination, Lore
  - WIL: Influence, Insight, Naturecraft, Perception
- **Saves** = class (from `ClassDefinitions`).
- **CombatStats**: `HitDieType` = class; `Speed` = class (default 6); `InitiativeBonus` = DEX modifier;
  `Armor` = class base (0 for quickstart classes — equipment adjusts armor later, on the sheet).
- **MaxHp**: at `level == 1` → class Starting HP. The edit-time bounds (used by the endpoint, not
  re-derived) are `[StartingHp, StartingHp + HitDieFace × (level − 1)]`. (`MaxHp` is the one derived
  value that is **not** recomputed on edit — level-up adds a *rolled* amount, so it is taken from the
  request and only clamped to these bounds.)
- **MaxMana**: per-class, level-gated. Mage (caster from L1) → `INTmod × 3 + level`; Oathsworn (caster
  from L2) → `WILmod + level` when `level ≥ 2`, else `null`; Cheat/Hunter → `null`.
- **Resources** (`ClassResources`): Oathsworn → `JudgmentDiceCount = 2`, `JudgmentDiceType = D6`
  (becomes `D8` at `level ≥ 3`), `LayOnHandsPool = 5 × level`. Cheat/Hunter/Mage → all null.

### `ClassDefinitions`

Static, keyed by `HeroClass`, covering the **four quickstart classes** only (values verbatim from the
class stat blocks in `docs/rules/nimble-basic-rules.md`):

| Class | Hit Die | Starting HP | Saves (▲/▼) | Speed | Mana (caster onset) | Resources |
|---|---|---|---|---|---|---|
| Cheat | d6 | 10 | DEX ▲ / WIL ▼ | 6 | none | none |
| Hunter | d8 | 13 | DEX ▲ / INT ▼ | 6 | none | none |
| Mage | d6 | 10 | INT ▲ / STR ▼ | 6 | INTmod×3 + level (L1) | none |
| Oathsworn | d10 | 17 | STR ▲ / DEX ▼ | 6 | WILmod + level (L2) | Judgment 2×d6 (d8 @L3), Lay on Hands 5×level |

> Classes without a definition (the non-quickstart `HeroClass` values) are **not offered at create**
> (see Client). Speed is 6 for all four (the rules' default movement); refine if a class block states
> otherwise.

### `PointBuy`

Static helper:

- `IReadOnlyDictionary<int,int> Cost` = `{8:0, 9:1, 10:2, 11:3, 12:4, 13:5, 14:7, 15:9}`.
- `Budget = 27`, `MinScore = 8`, `MaxScore = 15`.
- `int CostOf(int score)`, `int TotalCost(AbilityScores scores)`.
- `bool IsValid(AbilityScores scores)` = every score ∈ [8,15] **and** `TotalCost ≤ Budget`
  (under-spend allowed; leftover points are simply unspent).

---

## API (`NS.FastEndpoints`)

The create and update inputs now diverge, so `HeroBuildRequest` is split:

```csharp
public sealed record CreateHeroRequest(
    Guid AncestryId, Guid? BackgroundId, AbilityScores BaseAbilityScores,
    HeroClass HeroClass, string Name);

public sealed record UpdateHeroRequest(
    Guid AncestryId, Guid? BackgroundId, int MaxHp, string Name);
```

- Class and base ability scores are **immutable after create** and are absent from the update request.
- Owner (`UserId`) still comes from the token, never the body.

### Validators

- **`CreateHeroValidator`**: `Name` not empty; `AncestryId` not empty; `HeroClass` must be a class
  present in `ClassDefinitions`; each base score ∈ [8,15]; `PointBuy.TotalCost ≤ 27`.
- **`UpdateHeroValidator`**: `Name` not empty; `AncestryId` not empty; `MaxHp > 0` (format). The
  class+level **bounds** check needs the stored hero, so it runs in the handler via `AddError` +
  `ThrowIfAnyErrors()` (→ 400) when `MaxHp` is outside `[StartingHp, StartingHp + HitDieFace ×
  (level−1)]`.

### Endpoints

- **`CreateHeroEndpoint`**: load the ancestry (for `AbilityBonuses`) via `IReferenceDataService<Ancestry>`
  → `Hero.Create(...)` → save → 201 `CreateHeroResponse(Id)`.
- **`UpdateHeroEndpoint`**: load the owned hero via `GetOwnedByIdAsync` (404 if missing/not owned) →
  load ancestry bonuses → validate `MaxHp` bounds → `hero.UpdateBuild(...)` → save → 204.

---

## Seed data (`NS.SoloDB`)

- Add `AbilityBonuses` (all zeros) to every seeded `Ancestry`.
- This is a schema change to `Ancestry`, so a **fresh DB is required** (delete `nimble-sheet.db`).
  Pre-existing heroes have no `BaseAbilityScores` and would rehydrate to zeros (invalid); since the DB
  is a dev artifact, reseeding is the migration. Existing fixture GUIDs are preserved.

---

## Client (`NS.Client`)

### Form structure

`HeroBuildForm` keeps the section-per-slice pattern. Changes:

- **Remove** `CombatSection`, `SavesSection`, `SkillsSection`, `ClassResourcesSection`.
- **Identity** (`IdentitySection`): name, ancestry, background, **class**. The class `<select>` starts
  unset (`— select —`), lists only the **four defined classes** (Cheat, Hunter, Mage, Oathsworn — the
  `classDefs.ts` keys), is **required** on create, and is **disabled** on edit (set once, at create).
- **Ability scores** (`AbilityScoresSection`, replaces `StatsSection`): point-buy steppers per ability
  (8–15) with `+/−` controls, a live **remaining points** readout (27 budget), and a read-only
  **final score + modifier** preview per ability. Editable at create only; on edit it shows the final
  scores and modifiers read-only.
- **Vitals** (`VitalsSection`): `maxHp` is read-only on create (shows the selected class's Starting HP,
  computed live from the class definition mirror); on edit it is an input clamped to the class+level
  bounds (client mirror of the server rule). `maxMana` is shown read-only **only when the derived mana
  is non-null** (Mage at any level; Oathsworn at level ≥ 2; never for Cheat/Hunter) — the form drives
  this off the derived value, not a static caster flag.
- **Derived preview** (small, read-only): saves (▲/▼), max HP, max mana, and the keyed skills, so the
  player sees the result of their choices before saving.

### Supporting modules

- **`model.ts`** — `HeroBuildModel` reshaped to the player-set inputs only:
  `{ name, ancestryId, backgroundId, heroClass: HeroClass | '', baseAbilityScores: AbilityScores,
  maxHp }`. (`heroClass: ''` represents the unset state at create.) `blankBuildModel()` → class unset,
  all base scores 8 (cost 0). `heroToBuildModel(hero)` maps a loaded hero (class, base scores, maxHp).
  `normalizeBuild` simplifies to the new fields.
- **`validate.ts`** — required: name, ancestry, class (create); point-buy: each score ∈ [8,15] and
  total cost ≤ 27; maxHp within bounds (edit). Server remains authoritative.
- **New `pointBuy.ts`** (pure, unit-tested) — mirrors the domain: cost table, `totalCost`, `remaining`,
  `canIncrement`/`canDecrement`, `MIN=8`/`MAX=15`/`BUDGET=27`.
- **`api/types.ts`** — add `AbilityScores`; add `CreateHeroRequest`/`UpdateHeroRequest`; add
  `abilityBonuses` to `Ancestry` and `baseAbilityScores` to `Hero`.
- **`api/client.ts`** — `createHero`/`updateHero` send the split DTOs.
- A small **class-definition mirror** (`classDefs.ts`) for the four classes (starting HP, hit die,
  caster flag, saves) so the form can preview HP/mana/saves and gate the class list without a round
  trip. The server stays authoritative.

---

## Testing

- **Domain**: `HeroDerivation` (modifier table, skill→stat mapping, per-class saves/HP/mana/hit-die),
  `PointBuy` (cost + validity, including the boundary at 27), `Hero.Create`/`UpdateBuild` (derived
  values set; class/base scores immutable on update; maxHp clamping).
- **API**: `CreateHeroValidator`/`UpdateHeroValidator`; create/update produce correctly-derived heroes;
  update rejects out-of-bounds `MaxHp` (400) and a non-quickstart class on create.
- **Client**: `pointBuy.ts`, `model`/`validate`, `createHero`/`updateHero` wrappers. Update the
  existing `fixtures/caldra.ts` and resolver/client tests to the new hero/build shape.

---

## Out of scope / follow-ups

- **Level-up rework** — the existing flow grants a skill point + stat increase every level and lets the
  player allocate skills by hand (writing directly to `Stats`/`Skills`); the new model makes skills
  auto-derived and ability increases class-gated (which should raise `BaseAbilityScores`). Until that
  follow-up, level-up remains as-is and is **temporarily inconsistent** with the derived model.
  Concretely: editing a leveled hero re-derives `Stats`/`Skills`/mana/resources from
  `BaseAbilityScores` + class + ancestry at the current level, which **discards** old-flow level-up
  stat/skill tweaks (those were written to `Stats`/`Skills`, not to `BaseAbilityScores`). `MaxHp` is
  preserved (taken from the request, only clamped). This is an accepted consequence of doing the
  level-up rework separately. Tracked as the next spec.
- Real **ancestry/background/class** bonus numbers (data only).
- **Equipment** in the build form (stays on the sheet).
- Showing **base/final ability scores** on the read-only sheet (the sheet already shows modifiers and
  skills, which become correct automatically).

---

## Migration / rollout

1. Implement domain + derivation + tests.
2. Update API request/validators/endpoints + tests.
3. Reseed `Ancestry` with zero bonuses; **delete `nimble-sheet.db`** before running.
4. Update client form, modules, types, wrappers, fixtures + tests.
5. Rebuild SPA into `wwwroot`; browser-verify create (point-buy → derived sheet) and edit (class
   locked, ancestry change re-derives, maxHp bounds).
