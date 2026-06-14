# HP Popover + Live-Play Mutations — Design Spec

**Date:** 2026-06-13
**Component:** `NS.Client` (SvelteKit SPA)
**Status:** Approved for planning

## Goal

Make the live character sheet at `/heroes/[id]` interactive: wire the granular hero mutation endpoints to the sheet so a player can adjust HP (damage / heal / temp), wounds, hit dice, mana, and trigger a full resource recovery — directly from the pinned vitals. The server owns every rule (temp-HP absorption, clamping, death/dying, non-stacking temp HP, resource recovery); the client sends amounts and re-fetches. **Front-end only — no backend changes** (the endpoints already exist).

## Scope

**In scope**
- All eight play mutations wired to the UI: `take-damage`, `heal`, `grant-temp-hp`, `gain-wound`, `heal-wound`, `spend-hit-dice`, `spend-mana`, `recover-all-resources`.
- An HP popover with immediate-apply steppers (−5 −1 / +1 +5) plus a Temp HP set field.
- Interactive wound, hit-dice, and mana controls, and a Rest (recover-all) action.
- A hero-actions Svelte context owning the heroId, shared busy/error state, and re-fetch.
- Unit tests for the new API wrappers and the action-orchestration helper (after implementation).

**Out of scope (deferred)**
- Backend changes (endpoints, validation, seeding).
- Optimistic UI — the client always re-fetches after a mutation (server is authoritative).
- Reference-data seeding and full browser-level visual verification (still gated on seeding).
- Condition add/remove, inventory and feature mutations, level-up, build edits — separate slices.

## Decisions (from brainstorming)

| Decision | Choice |
|---|---|
| Slice scope | **All** play mutations (HP, wounds, hit dice, mana, recover-all) |
| HP control | Popover with **immediate-apply** steppers `−5 −1 [live current] +1 +5` + Temp HP set field |
| Stepper semantics | Each click is one mutation + re-fetch; `−` = take-damage, `+` = heal |
| Update strategy | **Re-fetch** via `invalidateAll()` (no optimistic update) |
| Wiring | **Hero-actions Svelte context** (Approach A) — page provides it, tiles consume it; no prop-drilling |
| Context absence | Tiles consume the context **optionally** — render read-only when no provider |
| Mana / Rest | New `ManaTile` (casters only, `vm.mana !== null`); `RestButton` with a confirm step |
| Tests | API wrappers + pure `runAction` orchestration; runes binding + popovers verified manually |

## Architecture

```
+page.ts: getHero → assembleReferenceData → resolveSheet ⟶ { vm, heroId }
                                                              │
+page.svelte: setContext(HERO_ACTIONS, createHeroActions(heroId))
                                                              │
                       ┌──────────────────────────────────────┘
                       ▼
        HeroSheet ▸ VitalsRow ▸ HpTile / WoundTrack / HitDiceTile / ManaTile  (+ RestButton)
                       │  getContext(HERO_ACTIONS) → actions.{takeDamage,…}, actions.busy/error
                       ▼
        actions.takeDamage(5) → POST /heroes/{id}/take-damage {amount:5}
                              → invalidateAll() → +page.ts re-runs → fresh vm → tiles update
```

The resolver (`resolve.ts`) and the `SheetViewModel` shape are unchanged; only the route load gains a `heroId` field and the tiles gain interactivity.

### Modules

**`$lib/api/client.ts` (extend)** — eight wrappers, all POST, all returning `void` (204):
- `takeDamage(heroId, amount)` → `/heroes/{id}/take-damage` `{ amount }`
- `heal(heroId, amount)` → `/heroes/{id}/heal` `{ amount }`
- `grantTempHp(heroId, amount)` → `/heroes/{id}/grant-temp-hp` `{ amount }`
- `gainWound(heroId)` → `/heroes/{id}/gain-wound` (no body)
- `healWound(heroId)` → `/heroes/{id}/heal-wound` (no body)
- `spendHitDice(heroId, count, healingAmount)` → `/heroes/{id}/spend-hit-dice` `{ count, healingAmount }`
- `spendMana(heroId, amount)` → `/heroes/{id}/spend-mana` `{ amount }`
- `recoverAll(heroId)` → `/heroes/{id}/recover-all-resources` (no body)

The existing `apiFetch` already returns `undefined` for 204 and throws `ApiError` on non-2xx; these wrappers reuse it.

**`$lib/sheet/runAction.ts` (new, pure/plain TS)** — orchestration extracted for testability:
```
runAction(action, refresh, setBusy, setError):
  setBusy(true); setError(null)
  try { await action(); await refresh(); }
  catch (e) { setError(e instanceof ApiError ? e.message : 'Action failed.'); }
  finally { setBusy(false); }
```

**`$lib/sheet/heroActions.svelte.ts` (new, runes)** —
- `HeroActions` interface: reactive `busy: boolean`, `error: string | null`, and the eight methods (parameterless or amount/count args; heroId is captured).
- `createHeroActions(heroId): HeroActions` — `$state` busy/error exposed via getters; each method calls `runAction(() => api.X(heroId, …), invalidateAll, setBusy, setError)`.
- `HERO_ACTIONS` — a unique context key (`Symbol`).

**`$lib/sheet/components/TilePopover.svelte` (new)** — reusable popover: a trigger region (the tile) and a floating dark panel that closes on outside-click and Esc. Opening clears `actions.error`. Used by HP, wounds, hit dice, mana.

### Components

| Component | Change | Interaction |
|---|---|---|
| `HpTile` | add popover | `−5/−1` → `takeDamage`, `+1/+5` → `heal`, center = live `vm.hp.current`; Temp field + **Set** → `grantTempHp` |
| `WoundTrack` | add popover | current `/6`; **Heal wound** (`healWound`, disabled at 0) / **Gain wound** (`gainWound`) |
| `HitDiceTile` | **extract** from VitalsRow inline div, add popover | count stepper `1…available` + healing-amount field + **Spend** → `spendHitDice`; disabled at 0 available |
| `ManaTile` | **new**, rendered only when `vm.mana !== null` | amount field + **Spend** → `spendMana` |
| `RestButton` | **new**, pinned region | confirm ("Rest and recover all resources?") → `recoverAll` |
| `VitalsRow` | render `HitDiceTile`/`ManaTile`; keep armor/init inline | — |
| `HeroSheet` | render `RestButton` in the pinned region | — |

All interactive controls read `actions.busy` to disable while any mutation is in flight, and the active popover shows `actions.error` on failure. Each interactive tile uses `getContext(HERO_ACTIONS)` **optionally**: when no provider is present it renders read-only (no triggers), so the tiles remain usable outside the live route.

### Error / race / busy handling

- A single shared `busy` flag serializes mutations: because one mutation is in flight at a time and all controls disable on `busy`, immediate-apply steppers cannot race.
- `error` is set from `ApiError.message` (e.g. a 400 "insufficient mana" validation message, or 404) and cleared when a popover opens or a subsequent action starts.
- A 401 mid-session is still handled centrally by `apiFetch` (clears session, redirects to `/login`).

## Testing

Written after implementation (project convention):
- `client.test.ts` (extend): each of the eight wrappers issues the right method/path and JSON body; 204 resolves to `void`; non-2xx throws `ApiError`.
- `runAction.test.ts` (new): `busy` goes true→false; `refresh` runs only after `action` resolves; `error` is set from an `ApiError` and from a generic error, and cleared on a following success.
- Existing resolver/cache/client/session tests stay green; the resolver and `SheetViewModel` are unchanged.

## Constraints

- No backend changes.
- `resolve.ts` and the `SheetViewModel` shape stay unchanged (only the route load adds `heroId`).
- Svelte 5 runes idioms; runes-using module logic lives in `.svelte.ts`, with pure logic factored into plain `.ts` for testability.
- Verifying the full play loop visually still depends on a seeded hero (deferred); this slice is verified via `npm run check`, the unit tests, and an HTTP smoke of the mutation endpoints.
