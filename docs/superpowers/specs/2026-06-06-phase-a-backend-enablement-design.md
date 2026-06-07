# Phase A — Backend Enablement: Design

**Date:** 2026-06-06
**Status:** Approved (pending spec review)
**Part of:** NimbleSheets client build-out (Phase A of A–D). See "Phasing" below.

## Context

We are building the `NS.Client` SPA (auth, character list, interactive sheet, create/edit wizard). Exploring the API surfaced gaps that the client depends on. Rather than one large spec, the work is decomposed into four phases, each with its own spec → plan → implementation cycle. **This spec covers Phase A only** — the backend capabilities the client needs — chosen to be built first.

### Phasing (for reference)

| Phase | Scope |
|---|---|
| **A — Backend enablement** *(this spec)* | Hero update endpoint, user-scoping + ownership, `TempHp` |
| B — Client foundation | App shell, routing, API client, JWT store, auth modal, character list + delete |
| C — Interactive sheet | Sheet view from `nimble_character.png`, responsive, live play actions |
| D — Create/edit wizard | Multi-step wizard; **reference-data seeding lands here** |

## Goals

1. Let the client edit an existing character's build in one transactional save.
2. Scope hero data to its owner — a user only sees and mutates their own heroes.
3. Add temporary hit points to the hero model, behaving correctly under damage.

## Non-goals (explicitly deferred)

- **Reference-data seeding** → Phase D, where the wizard defines exactly which reference shapes it consumes. Seeding now would be premature and likely reworked.
- **Tests** → written *after* implementation, per project preference (no TDD). May be a follow-up pass; not a gate for Phase A.
- **All client work** → Phases B/C/D.

## Key decisions (resolved during brainstorming)

- **Update = shared "build-inputs" DTO, not full-document replace.** The wizard holds the hero 100% client-side until Save, but the *payload* is purely build fields (equipment and play-state are owned by the interactive sheet via granular endpoints, not the wizard). So create and update share one DTO (`HeroBuildRequest`); the server keeps aggregate invariants (Id generation, initial play-state, owner stamping). A literal full-document replace was rejected: it would push aggregate-initialization to the client, invite trusting client-supplied `Id`/`UserId`, and fight `Hero`'s private-setter encapsulation.
- **`UserId` is derived from the JWT `sub` claim**, never from the request body. This also closes an existing hole where `CreateHeroRequest` trusted a client-supplied `UserId`.
- **Ownership violations return 404** (not 403), so the API does not leak the existence of other users' heroes.
- **`TempHp` absorbs damage first**, non-stacking, cleared on Safe Rest.

## Design

### 1. Authenticated-user identity

Add `ClaimsPrincipalExtensions.GetUserId()` in `NS.FastEndpoints`, reading the `sub` claim and parsing a `Guid`. The JWT sets `sub = User.Id` and the host configures `MapInboundClaims = false`, so the inbound claim type is the literal `"sub"` (use a local `const` to avoid a new package dependency on the JWT types). All non-anonymous endpoints sit behind the global `RequireAuthenticatedUser` fallback policy, so `sub` is always present.

- Add `global using System.Security.Claims;` to `NS.FastEndpoints/_GlobalUsings.cs`.

### 2. User-scoping & ownership

**Data service:**
- `IHeroDataService`: add `Task<IReadOnlyList<Hero>> GetByUserAsync(Guid userId)`.
- `SoloHeroDataService.GetByUserAsync`: load the collection and filter `d.Data.UserId == userId` (mirrors the existing in-memory `GetAllAsync`). `GetAllAsync` stays on the interface but is no longer used by endpoints.

**Ownership helper (DRY):**
- Add `IHeroDataServiceExtensions.GetOwnedByIdAsync(this IHeroDataService heroes, Guid id, Guid userId)` in `NS.FastEndpoints` → returns the hero only if it exists **and** `hero.UserId == userId`, else `null`.

**Endpoint changes:**
- `GetAllHeroesEndpoint`: return `await _heroes.GetByUserAsync(User.GetUserId())`.
- **Every hero-by-id endpoint** replaces `GetByIdAsync(req.HeroId)` + null-check with `GetOwnedByIdAsync(req.HeroId, User.GetUserId())` + null-check → 404. This is mechanical but covers the full set: `GetHeroEndpoint`, `DeleteHeroEndpoint`, and **all granular mutation endpoints** (take-damage, heal, gain/heal-wound, spend-mana, spend-hit-dice, recover-all-resources, add/remove condition/armor/weapon/spell/magic-item/gear/feature, level-up, apply-stat-increase, apply-hp-increase, finalize-skill-allocation, complete-pending-choice, set-subclass, update-combat-stats). Without this, any authenticated user could mutate any hero.

### 3. Shared build DTO + create/update endpoints

**`NS.FastEndpoints/Heroes/HeroBuildRequest.cs`** (new, shared):
```
public sealed record HeroBuildRequest(
    Guid AncestryId,
    Guid? BackgroundId,
    HeroCombatStats CombatStats,
    HeroClass HeroClass,
    int MaxHp,
    int? MaxMana,
    string Name,
    ClassResources Resources,
    HeroSaves Saves,
    HeroSkills Skills,
    HeroStats Stats);
```
- Add `HeroBuildValidator : Validator<HeroBuildRequest>` (e.g. `Name` not empty, `MaxHp > 0`). Applies to both create and update via the shared type.

**`CreateHeroEndpoint`** (`Post("heroes")`): request becomes `HeroBuildRequest`; construct `Hero` with `UserId = User.GetUserId()`; return 201 `CreateHeroResponse(Id)`. The old `CreateHeroRequest` (with `UserId`) is removed.

**`UpdateHeroEndpoint`** (new, `Put("heroes/{heroId}")`): request body binds to `HeroBuildRequest`; read the route id via `Route<Guid>("heroId")`. Load with `GetOwnedByIdAsync` → 404 if null; call `hero.UpdateBuild(...)`; `SaveAsync`; return 204.

### 4. `Hero.UpdateBuild(...)` domain method

Overwrites the build fields and preserves everything else (Id, UserId, Level, Subclass, all collections, current play-state):

```
public void UpdateBuild(
    Guid ancestryId, Guid? backgroundId, HeroCombatStats combatStats, HeroClass heroClass,
    int maxHp, int? maxMana, string name, ClassResources resources,
    HeroSaves saves, HeroSkills skills, HeroStats stats)
```
- Sets each build field.
- Clamps `CurrentHp = Min(CurrentHp, maxHp)`.
- Mana: if `maxMana` is null → `CurrentMana = null`; else clamp `CurrentMana` to `maxMana` (treat previously-null current as `maxMana`).
- Placed alphabetically in the methods group (after `TakeDamage`, before `UpdateCombatStats`).

### 5. Temporary hit points

**`Hero` changes:**
- New property `public int TempHp { get; private set; }` (alphabetically after `Subclass`). Defaults to 0; set to 0 in the public constructor.
- `GrantTempHp(int amount)` → `TempHp = Math.Max(TempHp, amount)` (non-stacking). Alphabetically after `GainWound`.
- `TakeDamage(int amount)` modified: drain `TempHp` first, then `CurrentHp`:
  ```
  if (TempHp > 0) { var absorbed = Math.Min(TempHp, amount); TempHp -= absorbed; amount -= absorbed; }
  CurrentHp = Math.Max(CurrentHp - amount, 0);
  ```
- `RecoverAllResources()` additionally sets `TempHp = 0` (temp HP lost on Safe Rest).

**Endpoint** `GrantTempHpEndpoint` (new, `Post("heroes/{heroId}/grant-temp-hp")`): mirrors `TakeDamageEndpoint`. Request `GrantTempHpRequest(Guid HeroId, int Amount)`; `GetOwnedByIdAsync` → 404; `hero.GrantTempHp(req.Amount)`; `SaveAsync`; 204.

**Compatibility:** existing stored heroes deserialize `TempHp` to 0 (absent field). No migration needed.

## Files touched

| File | Change |
|---|---|
| `NS.Domain/Heroes/Hero.cs` | `TempHp` prop; `GrantTempHp`, `UpdateBuild`; modify `TakeDamage`, `RecoverAllResources`; init `TempHp` in ctor |
| `NS.Domain/Abstractions/IHeroDataService.cs` | add `GetByUserAsync` |
| `NS.SoloDB/SoloHeroDataService.cs` | implement `GetByUserAsync` |
| `NS.FastEndpoints/_GlobalUsings.cs` | add `System.Security.Claims` |
| `NS.FastEndpoints/ClaimsPrincipalExtensions.cs` | new — `GetUserId()` |
| `NS.FastEndpoints/IHeroDataServiceExtensions.cs` | new — `GetOwnedByIdAsync()` |
| `NS.FastEndpoints/Heroes/HeroBuildRequest.cs` | new — shared DTO + validator |
| `NS.FastEndpoints/Heroes/CreateHeroEndpoint.cs` | use `HeroBuildRequest`; derive `UserId` |
| `NS.FastEndpoints/Heroes/UpdateHeroEndpoint.cs` | new — `PUT` |
| `NS.FastEndpoints/Heroes/GrantTempHpEndpoint.cs` | new — `POST grant-temp-hp` |
| `NS.FastEndpoints/Heroes/GetAllHeroesEndpoint.cs` | use `GetByUserAsync` |
| `NS.FastEndpoints/Heroes/GetHeroEndpoint.cs`, `DeleteHeroEndpoint.cs`, + all granular mutation endpoints | ownership via `GetOwnedByIdAsync` |
| `CLAUDE.md` | document new routes, `TempHp`, ownership, `GetByUserAsync` |

## Conventions

All C# per `CLAUDE.md` and global instructions: `sealed`, positional records, XML docs on public members, alphabetical member ordering, `_GlobalUsings.cs` only, `var`, explicit access modifiers, braces on all control flow.

## Verification (manual, after build)

1. `dotnet build` clean.
2. User A creates a hero (no `UserId` in body); `GET /heroes` as A returns only A's heroes.
3. User B `GET`/`PUT`/`DELETE` on A's hero → 404. So does any granular mutation.
4. `PUT /heroes/{id}` with edited build → fields change; `CurrentHp`/wounds/equipment preserved; `CurrentHp` clamps when `MaxHp` lowered.
5. `grant-temp-hp` then `take-damage` → TempHp absorbs first; re-granting is non-stacking; Safe Rest clears TempHp.
