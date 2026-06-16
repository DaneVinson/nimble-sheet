# Collection Editing (Equipment / Spells / Conditions) — Design

**Date:** 2026-06-15
**Status:** Approved (brainstorming) — ready for implementation plan

## Problem

A hero's reference-backed collections — weapons, armor, magic items, gear, spells, conditions — are **display-only** in the SPA today. The build form (`/heroes/new`, `/heroes/[id]/edit`) deliberately excludes them (the server's `Hero.UpdateBuild(...)` preserves them), and the sheet renders them read-only. The server already exposes add/remove endpoints for every collection, but the client has **no** wrappers or UI for them. There is no way, short of out-of-band API calls, to give a hero a weapon, learn a spell, pick up gear, or apply a condition.

This slice adds **inline editing** of these collections on the sheet, plus proper **equip/unequip** for weapons, armor, and magic items (which today can only have their equipped state set at add-time).

## Goals

- **Add/remove** inline on the sheet for: weapons, armor, magic items, gear, spells, conditions.
- **Equip/unequip** weapons, armor, and magic items as a first-class operation (new server endpoints + domain methods).
- Reuse the established live-mutation pattern: controls consume the `HERO_ACTIONS` context optionally (read-only when absent), POST then `invalidateAll()`, server authoritative, no optimistic updates.
- Keep initial sheet load light: "Add" pickers lazily fetch the full reference collection on demand, cached per session.

## Non-Goals

- **Features** editing — carries level/choices/subclass semantics and overlaps with level-up; separate future work.
- Editing per-item detail after add (e.g. changing a spell's notes or a magic item's remaining charges) beyond equip toggling — out of scope; remove + re-add to change other fields.
- Server-side **dedupe** — the domain appends without dedupe; the client picker excludes already-owned items to prevent duplicates. The server is unchanged here.
- Build-form changes — the `HeroBuildRequest` and build flow are untouched.

## Decisions (from brainstorming)

| Question | Decision |
|---|---|
| Edit location | **Inline on the sheet** (in the existing Combat/Magic/Inventory panels), like the HP/mana play mutations — not a separate editor page |
| Scope | Equipment (weapons, armor, magic items, gear) + spells + **conditions**; features excluded |
| Equipped state | **Proper equip/unequip endpoints** (new domain methods + endpoints), not set-at-add-only and not a client-side remove+re-add hack |

## Architecture

The feature spans all three backend-to-frontend layers but is thin at each: the add/remove endpoints already exist, the orchestration (`runAction` + `heroActions`) already exists, and the display panels already exist. The new work is (a) three equip endpoints, (b) client wrappers + actions for the full set, (c) lazy reference loading for pickers, (d) per-collection editor components that the panels compose.

Server owns all rules; the client sends a mutation and re-fetches. Controls are gated on the presence of the `HERO_ACTIONS` context, so a non-owner / no-actions render stays read-only — identical to how the play tiles behave.

### 1. NS.Domain

Three new `Hero` methods (placed alphabetically among the existing mutators):

```csharp
public void SetArmorEquipped(Guid armorId, bool isEquipped);
public void SetMagicItemEquipped(Guid magicItemId, bool isEquipped);
public void SetWeaponEquipped(Guid weaponId, bool isEquipped);
```

Each locates the matching record in its backing list by reference id and replaces it in place with `item with { IsEquipped = isEquipped }` (the `Hero*` value objects are positional records). No-op if the id is absent — consistent with the existing `RemoveAll`-by-id removal semantics. Gear, spells, and conditions have no equipped concept.

### 2. NS.FastEndpoints

Three new endpoints mirroring the existing add/remove ones (constructor-injected `IHeroDataService`, `GetOwnedByIdAsync(id, userId)` → **404** when missing/not-owned, mutate, `SaveAsync`, **204**):

| Method | Route | Request |
|---|---|---|
| POST | `/heroes/{heroId}/set-weapon-equipped` | `SetWeaponEquippedRequest(Guid HeroId, Guid WeaponId, bool IsEquipped)` |
| POST | `/heroes/{heroId}/set-armor-equipped` | `SetArmorEquippedRequest(Guid HeroId, Guid ArmorId, bool IsEquipped)` |
| POST | `/heroes/{heroId}/set-magic-item-equipped` | `SetMagicItemEquippedRequest(Guid HeroId, Guid MagicItemId, bool IsEquipped)` |

(Served under the global `/api` prefix like all endpoints.) No numeric validators needed. The 14 existing add/remove endpoints are reused unchanged.

### 3. NS.Client

**`api/client.ts`** — new wrappers, all POST under `/api`, returning `void` on 204:
`addWeapon`/`removeWeapon`/`setWeaponEquipped`, `addArmor`/`removeArmor`/`setArmorEquipped`, `addMagicItem`/`removeMagicItem`/`setMagicItemEquipped`, `addSpell`/`removeSpell`, `addGearItem`/`removeGearItem`, `addCondition`/`removeCondition`. Bodies mirror the request records above and the existing add request DTOs.

**`heroActions.svelte.ts`** — extend the `HeroActions` interface and `createHeroActions` with the new methods, each via the existing `run(() => …)` helper (shared reactive `busy`/`error`, POST → `invalidateAll()`). No new orchestration logic; `runAction` is reused.

**`reference/cache.ts`** — expose a session-cached `loadReferenceCollection(resource)` that returns the full collection for a resource (weapons / armor / magic-items / spells / conditions), fetching once and reusing the existing per-collection cache (and its evict-on-failure behavior). "Add" pickers call this on popover open, so the full lists are not fetched at sheet load.

**View models + `resolve.ts`** — add the reference id to the relevant view models so list rows can target removal/equip:
`WeaponViewModel.weaponId`, `ArmorViewModel.armorId`, `MagicItemViewModel.magicItemId`, `SpellViewModel.spellId`, `ConditionViewModel.conditionId`. Gear already keys by `name`. `resolve.ts` populates them from the source `Hero*` records it already joins.

**UI components** (`src/lib/sheet/components/`) — one editor component per collection:
`WeaponEditor`, `ArmorEditor`, `MagicItemEditor`, `SpellEditor`, `GearEditor`, `ConditionEditor`. Each renders its list with:
- a per-row **✕ remove** → `actions.removeX(id)` (gear: by name),
- a per-row **equip toggle** for weapons/armor/magic items → `actions.setXEquipped(id, !equipped)`,
- a **"+ Add"** `TilePopover` whose content is a reference `<select>` (excluding already-owned ids to prevent duplicates) plus the per-type fields:
  - weapon: equipped checkbox (+ optional notes),
  - armor: equipped checkbox,
  - magic item: equipped checkbox + optional remaining charges,
  - spell: tier unlocked (default the spell's own tier),
  - gear: free-text name + quantity (no reference picker),
  - condition: optional "expires at end of" text.

Each editor reads `HERO_ACTIONS` via `getContext` **optionally**: present → interactive; absent → renders the current read-only list (so existing read-only consumers are unaffected). `CombatPanel` (weapons, armor, conditions), `MagicPanel` (spells), and `InventoryPanel` (magic items, gear) compose these editors in place of their current inline `<ul>`s.

## Error handling

Mutations flow through the existing `apiFetch`: 401 clears the session and redirects to `/login`; other non-2xx throw `ApiError` surfaced by `runAction` into the shared `error` (rendered in the popover, as the play tiles do). A 404 (hero missing/not owned) surfaces the same way. No new error paths.

## Testing

- **NS.Tests** — unit tests for the three equip methods: set equipped true → record reflects it; set false → cleared; absent id → no-op (no throw, collection unchanged).
- **Vitest** — client wrapper tests (path + body) for the new wrappers; resolver tests asserting the new id fields are populated; picker "exclude owned" logic if extracted to a pure helper.
- **Browser verification** (end of implementation) — add a weapon → equip it → remove it; add a spell; add gear with quantity; add and remove a condition. Same Playwright-driven approach used for the prior verification, including a stale-`wwwroot` rebuild.

## Implementation order (vertical slices)

1. **Weapons end-to-end** — domain `SetWeaponEquipped` + endpoint, client wrappers + actions, view-model id, `WeaponEditor`, wire into `CombatPanel`. Establishes the full pattern.
2. **Armor + magic items** — replicate (equip + add/remove), wire into `CombatPanel`/`InventoryPanel`.
3. **Spells** — add/remove (no equip), `SpellEditor` in `MagicPanel`.
4. **Gear** — free-text add/remove, `GearEditor` in `InventoryPanel`.
5. **Conditions** — reference-backed add/remove, `ConditionEditor` in `CombatPanel`.
6. **Browser verification + docs** (CLAUDE.md route table + NS.Client notes).
