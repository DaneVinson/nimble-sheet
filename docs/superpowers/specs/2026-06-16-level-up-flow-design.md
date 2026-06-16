# Level-Up & Pending-Choice Flow — Design

**Date:** 2026-06-16
**Status:** Approved (brainstorming) — ready for implementation plan

## Problem

The six level-up domain methods and endpoints exist (`level-up`, `apply-hp-increase`, `apply-stat-increase`, `finalize-skill-allocation`, `set-subclass`, `complete-pending-choice`) but there is **no client UI** for them, so leveling a hero requires raw API calls. This slice adds inline affordances on the sheet to level a hero and resolve the mechanical consequences.

## Domain mechanics (existing, unchanged)

- `LevelUp(pendingChoices)` — `Level++`, `MaxHitDice = Level`, `PendingStatIncrease = true`, `UnspentSkillPoints++`, appends `pendingChoices` to `pendingFeatureChoices`. Does **not** touch HP.
- `ApplyHpIncrease(amount)` — raises `MaxHp` and `CurrentHp` by the caller-supplied amount.
- `ApplyStatIncrease(stat)` — +1 to the stat, clears `PendingStatIncrease`.
- `FinalizeSkillAllocation(updatedSkills)` — replaces `Skills` wholesale, sets `UnspentSkillPoints = 0`.
- `SetSubclass(subclass)` — sets the `Subclass` string.
- `CompletePendingChoice(label, feature)` — resolves a pending feature-choice label (out of scope here).

## Goals

- Inline, server-authoritative affordances (POST → `invalidateAll()`, no optimistic updates, no client wizard state) that consume the `HERO_ACTIONS` context optionally — same pattern as play mutations and collection editors.
- A **Level Up** control that captures the manually-entered HP gained, applies it, and increments the level in one user action.
- Resolve the resulting pending state via independent affordances: choose the stat increase, allocate the skill point(s), and choose the subclass at level 3+.

## Non-Goals

- **Pending feature choices** (`pendingFeatureChoices` / `CompletePendingChoice`) — out of scope; nothing populates the choice labels in the data, and features for the new level are added via the existing `FeatureEditor`. `LevelUp` is called with an empty list.
- **Server/domain changes** — none. All endpoints/methods exist.
- **Dice rolling** — HP gained is entered manually (the player rolls their own hit die with advantage). No client-side dice logic.
- **Aligning stat-increase / skill-point cadence to exact Nimble rules** — the domain grants a pending stat increase and +1 skill point on *every* level-up; the UI reflects that. Changing the cadence is separate domain work.

## Decisions (from brainstorming)

| Question | Decision |
|---|---|
| Pending feature choices | Out of scope (empty `pendingChoices`; use `FeatureEditor` for new features) |
| UX shape | Inline pending affordances on the sheet (not a multi-step wizard) |
| HP increase | Manual number entry (no dice rolling) |
| Level Up action | Bundles HP entry: enter rolled HP → applies HP **then** increments level, one refresh |
| Skill allocation | Requires spending **all** unspent points before Finalize (the endpoint clears the pool, so under-spending loses points); enforce the **+12** per-skill cap |
| Subclass | Free-text input (no clean subclass list in the data) |

## Architecture

All client-side, mirroring the established inline-mutation pattern. The new affordances live in a `LevelUpControls.svelte` placed near the **Rest** button in the sheet's pinned region.

### `api/client.ts` — wrappers (POST under `/api`, return `void` on 204)

- `levelUp(heroId)` → `/heroes/{id}/level-up`, body `{ pendingChoices: [] }`
- `applyHpIncrease(heroId, amount)` → `/apply-hp-increase`, body `{ amount }`
- `applyStatIncrease(heroId, stat)` → `/apply-stat-increase`, body `{ stat }`
- `finalizeSkillAllocation(heroId, skills)` → `/finalize-skill-allocation`, body `{ updatedSkills: skills }` (a full `HeroSkills`)
- `setSubclass(heroId, subclass)` → `/set-subclass`, body `{ subclass }`

(Field names match the server request records: `LevelUpRequest.PendingChoices`, `FinalizeSkillAllocationRequest.UpdatedSkills`, `ApplyStatIncreaseRequest.Stat`, etc.)

### `heroActions.svelte.ts`

- `levelUp(hpIncrease: number)` — composite: `run(async () => { if (hpIncrease > 0) await applyHpIncrease(getHeroId(), hpIncrease); await levelUp(getHeroId()); })`. `runAction` invalidates once after both POSTs.
- `applyStatIncrease(stat: StatType)`, `finalizeSkillAllocation(skills: HeroSkills)`, `setSubclass(subclass: string)` — one-to-one via `run(...)`.

### View model (`viewmodel.ts` + `resolve.ts`)

Add to `SheetViewModel`:
- `pendingStatIncrease: boolean` ← `hero.pendingStatIncrease`
- `unspentSkillPoints: number` ← `hero.unspentSkillPoints`
- `needsSubclass: boolean` ← `hero.level >= 3 && hero.subclass === null`
- `skillValues: HeroSkills` ← `{ ...hero.skills }` (the raw editable map the allocator finalizes from; the display `skills: SkillViewModel[]` stays)

### UI

**`LevelUpControls.svelte`** — props derived from the view model (`level`, `pendingStatIncrease`, `unspentSkillPoints`, `needsSubclass`, `skillValues`, `hitDie`); reads `HERO_ACTIONS` optionally (renders nothing interactive when absent). Composes:

- **Level Up** `TilePopover` — header "Level up to {level + 1}", a manual "HP gained" number input (min 0), confirm → `actions.levelUp(hp)`.
- **Choose stat +1** popover (shown when `pendingStatIncrease`) — four buttons STR/DEX/INT/WIL → `actions.applyStatIncrease(stat)`.
- **Allocate skills** popover (shown when `unspentSkillPoints > 0`) — a row per skill (label + current value + −/+ buttons) seeded from `skillValues`; a derived "spent X of N" counter; +/− disabled at the bounds (can't go below the starting value; can't exceed `+12`); Finalize disabled until `spent === unspentSkillPoints` → `actions.finalizeSkillAllocation(updatedSkills)`.
- **Choose subclass** popover (shown when `needsSubclass`) — a free-text input + confirm → `actions.setSubclass(name)`.

Shared `editorButton` styling; `actions.error` shown in each popover; `actions.busy` disables confirms.

**Pure helper (`levelUp/skillAllocation.ts` or co-located)** — the allocation math extracted as pure functions for testability: given starting `HeroSkills`, a working `HeroSkills`, and `unspentSkillPoints`, compute `spent` and `canFinalize` (all spent) and `canIncrement(skill)` (respects +12 and budget). Keeps the component thin and the rules unit-testable.

**Placement** — `HeroSheet.svelte` renders `<LevelUpControls … />` adjacent to the existing Rest button (pass the needed view-model fields). `RestButton` is unaffected.

## Error handling

Mutations flow through `apiFetch` → `runAction`: 401 clears session + redirects; other non-2xx surface `ApiError` into `actions.error` (shown in the open popover). The composite `levelUp` surfaces an error from either POST; if `applyHpIncrease` fails, `level-up` is not attempted (the `await` throws first). No new error paths.

## Testing

- **Vitest** — client wrapper tests: `levelUp` composite issues `apply-hp-increase` then `level-up` (and skips HP when 0); `finalizeSkillAllocation` posts `{ updatedSkills }`; `applyStatIncrease` posts `{ stat }`; `setSubclass` posts `{ subclass }`. Resolver tests for `pendingStatIncrease`/`unspentSkillPoints`/`needsSubclass`/`skillValues`. Pure skill-allocation helper tests: spent count, `canFinalize`, `+12` cap, budget exhaustion.
- **No domain tests** — no domain change.
- **Browser verification** — level a hero (enter HP), confirm level/MaxHitDice/HP updated; choose a stat (pending flag clears, stat +1); allocate the skill point and finalize (skill +1, pending clears); at level 3 set a subclass (banner appears under the level-3 condition and clears after). Rebuild SPA into `wwwroot` first.

## Implementation order

1. Client wrappers + actions (incl. composite `levelUp`).
2. View-model fields + resolver.
3. Pure skill-allocation helper + tests.
4. `LevelUpControls.svelte` + the four affordances + wire into `HeroSheet`.
5. Browser verification + docs.
