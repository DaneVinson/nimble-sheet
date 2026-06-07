# Phase A — Backend Enablement Implementation Plan

> **For agentic workers:** Use superpowers:executing-plans or superpowers:subagent-driven-development to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.
>
> **Note on testing:** Per project preference there is **no TDD** here — implement first, verify by build + manual checks, and write unit tests in the final (deferred) task. Do not write tests before implementation.

**Goal:** Add the backend capabilities the `NS.Client` SPA needs — a hero build-update endpoint, per-user scoping + ownership on all hero endpoints, and temporary hit points.

**Architecture:** Create and update share one `HeroBuildRequest` DTO; `UserId` always comes from the JWT `sub` claim. A new `Hero.UpdateBuild(...)` domain method overwrites build fields while preserving play-state and collections. Ownership is enforced by a `GetOwnedByIdAsync` extension used by every hero-by-id endpoint. `TempHp` absorbs damage before `CurrentHp`.

**Tech Stack:** C# 14 / .NET 10, FastEndpoints 8.1, SoloDB, FluentValidation.

**Spec:** `docs/superpowers/specs/2026-06-06-phase-a-backend-enablement-design.md`

**Conventions (apply to every task):** `sealed`; positional records; XML docs on all public types/members; members ordered constants → fields → constructors → properties → methods, alphabetical within each group; no per-file `using` directives (use `_GlobalUsings.cs`); `var`; explicit access modifiers; braces on all control flow.

**Pre-existing uncommitted work:** The tree already has uncommitted Phase-0 work (NS.Client scaffold, SPA build integration, CLAUDE.md, .gitignore). Each task below uses **targeted `git add`** of only its own files so those changes are not swept in.

---

## File Structure

| File | Responsibility |
|---|---|
| `NS.Domain/Heroes/Hero.cs` | (modify) `TempHp` + `GrantTempHp` + `UpdateBuild`; absorb-first `TakeDamage`; clear TempHp on rest |
| `NS.Domain/Abstractions/IHeroDataService.cs` | (modify) add `GetByUserAsync` |
| `NS.SoloDB/SoloHeroDataService.cs` | (modify) implement `GetByUserAsync` |
| `NS.FastEndpoints/_GlobalUsings.cs` | (modify) add `System.Security.Claims` |
| `NS.FastEndpoints/ClaimsPrincipalExtensions.cs` | (create) `GetUserId()` from `sub` claim |
| `NS.FastEndpoints/IHeroDataServiceExtensions.cs` | (create) `GetOwnedByIdAsync()` ownership check |
| `NS.FastEndpoints/Heroes/HeroBuildRequest.cs` | (create) shared create/update DTO + validator |
| `NS.FastEndpoints/Heroes/CreateHeroEndpoint.cs` | (modify) use `HeroBuildRequest`, derive `UserId` |
| `NS.FastEndpoints/Heroes/UpdateHeroEndpoint.cs` | (create) `PUT /heroes/{heroId}` |
| `NS.FastEndpoints/Heroes/GrantTempHpEndpoint.cs` | (create) `POST /heroes/{heroId}/grant-temp-hp` |
| `NS.FastEndpoints/Heroes/GetAllHeroesEndpoint.cs` | (modify) user-scoped list |
| `NS.FastEndpoints/Heroes/GetHeroEndpoint.cs` | (modify) ownership |
| `NS.FastEndpoints/Heroes/DeleteHeroEndpoint.cs` | (modify) ownership (load then delete) |
| 28 granular mutation endpoints | (modify) ownership via `GetOwnedByIdAsync` |
| `CLAUDE.md` | (modify) document new routes/behavior |
| `NS.Domain.Tests/` | (create, deferred) unit tests |

---

## Task 1: Hero domain — TempHp, GrantTempHp, UpdateBuild, absorb-first damage

**Files:**
- Modify: `NS.Domain/Heroes/Hero.cs`

- [ ] **Step 1: Add `TempHp` property**

Insert in the properties group, immediately after the `Subclass` property (alphabetical order, before `UnspentSkillPoints`):

```csharp
    /// <summary>The hero's temporary hit points, which absorb damage before current hit points. Lost on a Safe Rest.</summary>
    public int TempHp { get; private set; }
```

- [ ] **Step 2: Initialize `TempHp` in the public constructor**

In the public constructor body, add this line after `Stats = stats;` and before `UnspentSkillPoints = 0;`:

```csharp
        TempHp = 0;
```

- [ ] **Step 3: Add `GrantTempHp` method**

Insert in the methods group, immediately after `GainWound` (alphabetical, before `Heal`):

```csharp
    /// <summary>Grants temporary hit points. Temp HP does not stack; the greater of the current and granted values is kept.</summary>
    public void GrantTempHp(int amount) => TempHp = Math.Max(TempHp, amount);
```

- [ ] **Step 4: Replace `TakeDamage` with the absorb-first version**

Replace the existing `TakeDamage` method:

```csharp
    /// <summary>Reduces the hero's hit points by the specified amount, flooring at zero. Temporary hit points absorb damage first. When reduced to zero the hero enters the dying state.</summary>
    public void TakeDamage(int amount)
    {
        if (TempHp > 0)
        {
            var absorbed = Math.Min(TempHp, amount);
            TempHp -= absorbed;
            amount -= absorbed;
        }
        CurrentHp = Math.Max(CurrentHp - amount, 0);
    }
```

- [ ] **Step 5: Clear TempHp on a Safe Rest**

In `RecoverAllResources`, add `TempHp = 0;` after the `HitDiceAvailable = MaxHitDice;` line:

```csharp
        HitDiceAvailable = MaxHitDice;
        TempHp = 0;
        HealWound();
```

- [ ] **Step 6: Add `UpdateBuild` method**

Insert in the methods group immediately before `UpdateCombatStats` (alphabetical: `UpdateBuild` < `UpdateCombatStats`):

```csharp
    /// <summary>Overwrites the hero's build attributes (those chosen during character creation), preserving level, subclass, play state, and all collections. Current hit points and mana are clamped to the new maximums.</summary>
    public void UpdateBuild(
        Guid ancestryId,
        Guid? backgroundId,
        HeroCombatStats combatStats,
        HeroClass heroClass,
        int maxHp,
        int? maxMana,
        string name,
        ClassResources resources,
        HeroSaves saves,
        HeroSkills skills,
        HeroStats stats)
    {
        AncestryId = ancestryId;
        BackgroundId = backgroundId;
        Class = heroClass;
        CombatStats = combatStats;
        MaxHp = maxHp;
        CurrentHp = Math.Min(CurrentHp, maxHp);
        MaxMana = maxMana;
        CurrentMana = maxMana.HasValue ? Math.Min(CurrentMana ?? maxMana.Value, maxMana.Value) : null;
        Name = name;
        Resources = resources;
        Saves = saves;
        Skills = skills;
        Stats = stats;
    }
```

- [ ] **Step 7: Build**

Run: `dotnet build NS.Domain/NS.Domain.csproj`
Expected: `Build succeeded. 0 Error(s)`.

- [ ] **Step 8: Commit**

```bash
git add NS.Domain/Heroes/Hero.cs
git commit -m "feat(domain): add TempHp and UpdateBuild to Hero; absorb-first damage"
```

---

## Task 2: Data service — GetByUserAsync

**Files:**
- Modify: `NS.Domain/Abstractions/IHeroDataService.cs`
- Modify: `NS.SoloDB/SoloHeroDataService.cs`

- [ ] **Step 1: Add the interface method**

In `IHeroDataService`, insert after `GetAllAsync` and before `GetByIdAsync` (alphabetical):

```csharp
    /// <summary>Returns all heroes owned by the specified user.</summary>
    Task<IReadOnlyList<Hero>> GetByUserAsync(Guid userId);
```

- [ ] **Step 2: Implement it in SoloHeroDataService**

Insert after `GetByIdAsync` and before `SaveAsync`:

```csharp
    /// <inheritdoc/>
    public Task<IReadOnlyList<Hero>> GetByUserAsync(Guid userId)
    {
        IReadOnlyList<Hero> heroes = _db.GetCollection<SoloDocument<Hero>>()
            .ToList()
            .Where(d => d.Data.UserId == userId)
            .Select(d => d.Data)
            .ToList();
        return Task.FromResult(heroes);
    }
```

- [ ] **Step 3: Build**

Run: `dotnet build NS.SoloDB/NS.SoloDB.csproj`
Expected: `Build succeeded. 0 Error(s)`.

- [ ] **Step 4: Commit**

```bash
git add NS.Domain/Abstractions/IHeroDataService.cs NS.SoloDB/SoloHeroDataService.cs
git commit -m "feat(data): add GetByUserAsync for user-scoped hero queries"
```

---

## Task 3: FastEndpoints helpers — user id + ownership

**Files:**
- Modify: `NS.FastEndpoints/_GlobalUsings.cs`
- Create: `NS.FastEndpoints/ClaimsPrincipalExtensions.cs`
- Create: `NS.FastEndpoints/IHeroDataServiceExtensions.cs`

- [ ] **Step 1: Add the Claims global using**

Add this line to `NS.FastEndpoints/_GlobalUsings.cs`:

```csharp
global using System.Security.Claims;
```

- [ ] **Step 2: Create `ClaimsPrincipalExtensions.cs`**

```csharp
namespace NSFastEndpoints;

/// <summary>Extension methods for reading claims from a <see cref="ClaimsPrincipal"/>.</summary>
public static class ClaimsPrincipalExtensions
{
    private const string SubjectClaimType = "sub";

    /// <summary>Returns the authenticated user's identifier, read from the JWT <c>sub</c> claim. The host sets <c>MapInboundClaims = false</c>, so the inbound claim type is the literal "sub".</summary>
    public static Guid GetUserId(this ClaimsPrincipal principal) =>
        Guid.Parse(principal.FindFirstValue(SubjectClaimType)!);
}
```

- [ ] **Step 3: Create `IHeroDataServiceExtensions.cs`**

```csharp
namespace NSFastEndpoints;

/// <summary>Extension methods layering ownership checks over <see cref="IHeroDataService"/>.</summary>
public static class IHeroDataServiceExtensions
{
    /// <summary>Returns the hero with the specified identifier only if it is owned by the specified user; otherwise <see langword="null"/>.</summary>
    public static async Task<Hero?> GetOwnedByIdAsync(this IHeroDataService heroes, Guid id, Guid userId)
    {
        var hero = await heroes.GetByIdAsync(id);
        return hero is not null && hero.UserId == userId ? hero : null;
    }
}
```

- [ ] **Step 4: Build**

Run: `dotnet build NS.FastEndpoints/NS.FastEndpoints.csproj`
Expected: `Build succeeded. 0 Error(s)`.

- [ ] **Step 5: Commit**

```bash
git add NS.FastEndpoints/_GlobalUsings.cs NS.FastEndpoints/ClaimsPrincipalExtensions.cs NS.FastEndpoints/IHeroDataServiceExtensions.cs
git commit -m "feat(api): add GetUserId and GetOwnedByIdAsync helpers"
```

---

## Task 4: Shared HeroBuildRequest + rework CreateHeroEndpoint

**Files:**
- Create: `NS.FastEndpoints/Heroes/HeroBuildRequest.cs`
- Modify: `NS.FastEndpoints/Heroes/CreateHeroEndpoint.cs`

- [ ] **Step 1: Create `HeroBuildRequest.cs`**

```csharp
namespace NSFastEndpoints;

/// <summary>The character-build attributes shared by hero creation and update. The owning user is taken from the authenticated token, never the request body.</summary>
/// <param name="AncestryId">The identifier of the hero's ancestry.</param>
/// <param name="BackgroundId">The optional identifier of the hero's background.</param>
/// <param name="CombatStats">The hero's combat statistics.</param>
/// <param name="HeroClass">The hero's class.</param>
/// <param name="MaxHp">The hero's maximum hit points.</param>
/// <param name="MaxMana">The hero's maximum mana; <see langword="null"/> for non-casters.</param>
/// <param name="Name">The hero's name.</param>
/// <param name="Resources">The hero's class-specific resource pools.</param>
/// <param name="Saves">The hero's save advantage and disadvantage types.</param>
/// <param name="Skills">The hero's skill bonuses.</param>
/// <param name="Stats">The hero's base stats.</param>
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

/// <summary>Validates <see cref="HeroBuildRequest"/>.</summary>
public sealed class HeroBuildValidator : Validator<HeroBuildRequest>
{
    /// <summary>Initializes validation rules for a hero build.</summary>
    public HeroBuildValidator()
    {
        RuleFor(r => r.Name).NotEmpty();
        RuleFor(r => r.MaxHp).GreaterThan(0);
    }
}
```

- [ ] **Step 2: Rewrite `CreateHeroEndpoint.cs`**

Replace the entire file (removes the old `CreateHeroRequest` record; keeps `CreateHeroResponse`):

```csharp
namespace NSFastEndpoints;

/// <summary>Creates a new level-1 hero owned by the authenticated user.</summary>
public sealed class CreateHeroEndpoint : Endpoint<HeroBuildRequest, CreateHeroResponse>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public CreateHeroEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(HeroBuildRequest req, CancellationToken ct)
    {
        var hero = new Hero(
            req.AncestryId,
            req.BackgroundId,
            req.CombatStats,
            req.HeroClass,
            req.MaxHp,
            req.MaxMana,
            req.Name,
            req.Resources,
            req.Saves,
            req.Skills,
            req.Stats,
            User.GetUserId());

        await _heroes.SaveAsync(hero);
        await Send.ResponseAsync(new CreateHeroResponse(hero.Id), 201, ct);
    }
}

/// <summary>Response returned after successfully creating a hero.</summary>
/// <param name="Id">The newly created hero's unique identifier.</param>
public sealed record CreateHeroResponse(Guid Id);
```

- [ ] **Step 3: Build**

Run: `dotnet build NS.FastEndpoints/NS.FastEndpoints.csproj`
Expected: `Build succeeded. 0 Error(s)`. (If anything else referenced `CreateHeroRequest`, it would fail here — nothing does.)

- [ ] **Step 4: Commit**

```bash
git add NS.FastEndpoints/Heroes/HeroBuildRequest.cs NS.FastEndpoints/Heroes/CreateHeroEndpoint.cs
git commit -m "feat(api): share HeroBuildRequest; create derives UserId from token"
```

---

## Task 5: UpdateHeroEndpoint (PUT)

**Files:**
- Create: `NS.FastEndpoints/Heroes/UpdateHeroEndpoint.cs`

- [ ] **Step 1: Create `UpdateHeroEndpoint.cs`**

```csharp
namespace NSFastEndpoints;

/// <summary>Updates an existing hero's build attributes, preserving level, subclass, play state, and collections.</summary>
public sealed class UpdateHeroEndpoint : Endpoint<HeroBuildRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public UpdateHeroEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Put("heroes/{heroId}");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(HeroBuildRequest req, CancellationToken ct)
    {
        var heroId = Route<Guid>("heroId");
        var hero = await _heroes.GetOwnedByIdAsync(heroId, User.GetUserId());
        if (hero is null) { await Send.NotFoundAsync(ct); return; }

        hero.UpdateBuild(
            req.AncestryId,
            req.BackgroundId,
            req.CombatStats,
            req.HeroClass,
            req.MaxHp,
            req.MaxMana,
            req.Name,
            req.Resources,
            req.Saves,
            req.Skills,
            req.Stats);

        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}
```

- [ ] **Step 2: Build**

Run: `dotnet build NS.FastEndpoints/NS.FastEndpoints.csproj`
Expected: `Build succeeded. 0 Error(s)`.

- [ ] **Step 3: Commit**

```bash
git add NS.FastEndpoints/Heroes/UpdateHeroEndpoint.cs
git commit -m "feat(api): add PUT /heroes/{heroId} build-update endpoint"
```

---

## Task 6: GrantTempHpEndpoint

**Files:**
- Create: `NS.FastEndpoints/Heroes/GrantTempHpEndpoint.cs`

- [ ] **Step 1: Create `GrantTempHpEndpoint.cs`**

```csharp
namespace NSFastEndpoints;

/// <summary>Grants temporary hit points to the hero.</summary>
public sealed class GrantTempHpEndpoint : Endpoint<GrantTempHpRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public GrantTempHpEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/grant-temp-hp");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(GrantTempHpRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetOwnedByIdAsync(req.HeroId, User.GetUserId());
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.GrantTempHp(req.Amount);
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for granting temporary hit points to a hero.</summary>
/// <param name="HeroId">The hero's unique identifier (route).</param>
/// <param name="Amount">The amount of temporary hit points to grant.</param>
public sealed record GrantTempHpRequest(Guid HeroId, int Amount);
```

- [ ] **Step 2: Build**

Run: `dotnet build NS.FastEndpoints/NS.FastEndpoints.csproj`
Expected: `Build succeeded. 0 Error(s)`.

- [ ] **Step 3: Commit**

```bash
git add NS.FastEndpoints/Heroes/GrantTempHpEndpoint.cs
git commit -m "feat(api): add POST /heroes/{heroId}/grant-temp-hp"
```

---

## Task 7: User-scope list + ownership on get/delete

**Files:**
- Modify: `NS.FastEndpoints/Heroes/GetAllHeroesEndpoint.cs`
- Modify: `NS.FastEndpoints/Heroes/GetHeroEndpoint.cs`
- Modify: `NS.FastEndpoints/Heroes/DeleteHeroEndpoint.cs`

- [ ] **Step 1: Scope the list to the caller**

In `GetAllHeroesEndpoint.HandleAsync`, replace the body:

```csharp
    public override async Task HandleAsync(CancellationToken ct)
    {
        var heroes = await _heroes.GetByUserAsync(User.GetUserId());
        await Send.OkAsync(heroes.ToList(), ct);
    }
```

- [ ] **Step 2: Ownership on GetHero**

In `GetHeroEndpoint.HandleAsync`, change the load line:

```csharp
        var hero = await _heroes.GetOwnedByIdAsync(req.HeroId, User.GetUserId());
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        await Send.OkAsync(hero, ct);
```

- [ ] **Step 3: Ownership on DeleteHero (load, then delete)**

Replace `DeleteHeroEndpoint.HandleAsync`:

```csharp
    public override async Task HandleAsync(HeroIdRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetOwnedByIdAsync(req.HeroId, User.GetUserId());
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        await _heroes.DeleteAsync(req.HeroId);
        await Send.NoContentAsync(ct);
    }
```

- [ ] **Step 4: Build**

Run: `dotnet build NS.FastEndpoints/NS.FastEndpoints.csproj`
Expected: `Build succeeded. 0 Error(s)`.

- [ ] **Step 5: Commit**

```bash
git add NS.FastEndpoints/Heroes/GetAllHeroesEndpoint.cs NS.FastEndpoints/Heroes/GetHeroEndpoint.cs NS.FastEndpoints/Heroes/DeleteHeroEndpoint.cs
git commit -m "feat(api): user-scope hero list; enforce ownership on get/delete"
```

---

## Task 8: Ownership on all granular mutation endpoints

Each of these endpoints contains exactly this line:

```csharp
        var hero = await _heroes.GetByIdAsync(req.HeroId);
```

Change it in **every** file below to:

```csharp
        var hero = await _heroes.GetOwnedByIdAsync(req.HeroId, User.GetUserId());
```

Nothing else in these files changes. The `if (hero is null) { await Send.NotFoundAsync(ct); return; }` line directly below stays as-is.

**Files (all under `NS.FastEndpoints/Heroes/`):**

- [ ] AddArmorEndpoint.cs
- [ ] AddConditionEndpoint.cs
- [ ] AddFeatureEndpoint.cs
- [ ] AddGearItemEndpoint.cs
- [ ] AddMagicItemEndpoint.cs
- [ ] AddSpellEndpoint.cs
- [ ] AddWeaponEndpoint.cs
- [ ] ApplyHpIncreaseEndpoint.cs
- [ ] ApplyStatIncreaseEndpoint.cs
- [ ] CompletePendingChoiceEndpoint.cs
- [ ] FinalizeSkillAllocationEndpoint.cs
- [ ] GainWoundEndpoint.cs
- [ ] HealEndpoint.cs
- [ ] HealWoundEndpoint.cs
- [ ] LevelUpEndpoint.cs
- [ ] RecoverAllResourcesEndpoint.cs
- [ ] RemoveArmorEndpoint.cs
- [ ] RemoveConditionEndpoint.cs
- [ ] RemoveFeatureEndpoint.cs
- [ ] RemoveGearItemEndpoint.cs
- [ ] RemoveMagicItemEndpoint.cs
- [ ] RemoveSpellEndpoint.cs
- [ ] RemoveWeaponEndpoint.cs
- [ ] SetSubclassEndpoint.cs
- [ ] SpendHitDiceEndpoint.cs
- [ ] SpendManaEndpoint.cs
- [ ] TakeDamageEndpoint.cs
- [ ] UpdateCombatStatsEndpoint.cs

- [ ] **Build**

Run: `dotnet build NS.FastEndpoints/NS.FastEndpoints.csproj`
Expected: `Build succeeded. 0 Error(s)`.

- [ ] **Verify none were missed**

Run: `git grep -n "GetByIdAsync(req.HeroId)" NS.FastEndpoints/Heroes/`
Expected: no output (every hero-by-id endpoint now uses `GetOwnedByIdAsync`). The only remaining `GetByIdAsync` reference should be inside `IHeroDataServiceExtensions.GetOwnedByIdAsync` itself.

- [ ] **Commit**

```bash
git add NS.FastEndpoints/Heroes/
git commit -m "feat(api): enforce hero ownership on all granular mutation endpoints"
```

---

## Task 9: Manual end-to-end verification

**Files:** none (verification only).

- [ ] **Step 1: Full solution build**

Run: `dotnet build NS.WebApp/NS.WebApp.csproj`
Expected: `Build succeeded. 0 Error(s)`. (SPA build is skipped because `wwwroot/index.html` already exists.)

- [ ] **Step 2: Run the API**

Run (background): `ASPNETCORE_ENVIRONMENT=Development DOTNET_URLS="http://localhost:5080" dotnet run --project NS.WebApp/NS.WebApp.csproj --no-build`

- [ ] **Step 3: Create two users and log in**

```bash
curl -s -X POST http://localhost:5080/users -H "Content-Type: application/json" -d '{"email":"a@x.com","name":"Alice"}'
curl -s -X POST http://localhost:5080/users -H "Content-Type: application/json" -d '{"email":"b@x.com","name":"Bob"}'
# capture tokens:
curl -s -X POST http://localhost:5080/users/login -H "Content-Type: application/json" -d '{"name":"Alice"}'
curl -s -X POST http://localhost:5080/users/login -H "Content-Type: application/json" -d '{"name":"Bob"}'
```
Save Alice's and Bob's `Token` values as `$A` and `$B`.

- [ ] **Step 4: Alice creates a hero (no UserId in body)**

```bash
curl -s -X POST http://localhost:5080/heroes -H "Authorization: Bearer $A" -H "Content-Type: application/json" \
  -d '{"ancestryId":"00000000-0000-0000-0000-000000000001","backgroundId":null,"heroClass":"Oathsworn","maxHp":17,"maxMana":null,"name":"Caldra","combatStats":{"armor":8,"hitDieType":"D10","initiativeBonus":0,"speed":6},"resources":{"judgmentDiceCount":null,"judgmentDiceType":null,"layOnHandsPool":null,"thrillCharges":null},"saves":{"advantageOn":"Will","disadvantageOn":"Dexterity"},"skills":{"arcana":-1,"examination":-1,"finesse":0,"influence":4,"insight":4,"lore":-1,"might":2,"naturecraft":2,"perception":2,"stealth":0},"stats":{"dexterity":0,"intelligence":-1,"strength":2,"will":2}}'
```
Expected: `201` with `{"id":"..."}`. Save as `$HID`.

- [ ] **Step 5: Verify scoping & ownership**

```bash
curl -s http://localhost:5080/heroes -H "Authorization: Bearer $A"   # Alice sees Caldra
curl -s http://localhost:5080/heroes -H "Authorization: Bearer $B"   # Bob sees []  (empty)
curl -s -o /dev/null -w "%{http_code}\n" http://localhost:5080/heroes/$HID -H "Authorization: Bearer $B"  # 404
curl -s -o /dev/null -w "%{http_code}\n" -X DELETE http://localhost:5080/heroes/$HID -H "Authorization: Bearer $B"  # 404
```
Expected: Alice's list has 1 hero, Bob's is empty, Bob's get/delete return 404.

- [ ] **Step 6: Verify update preserves play state + clamps HP**

```bash
curl -s -X POST http://localhost:5080/heroes/$HID/take-damage -H "Authorization: Bearer $A" -H "Content-Type: application/json" -d '{"amount":5}'   # CurrentHp 17 -> 12
# PUT same build but maxHp lowered to 10:
curl -s -X PUT http://localhost:5080/heroes/$HID -H "Authorization: Bearer $A" -H "Content-Type: application/json" \
  -d '{"ancestryId":"00000000-0000-0000-0000-000000000001","backgroundId":null,"heroClass":"Oathsworn","maxHp":10,"maxMana":null,"name":"Caldra the Bold","combatStats":{"armor":8,"hitDieType":"D10","initiativeBonus":0,"speed":6},"resources":{"judgmentDiceCount":null,"judgmentDiceType":null,"layOnHandsPool":null,"thrillCharges":null},"saves":{"advantageOn":"Will","disadvantageOn":"Dexterity"},"skills":{"arcana":-1,"examination":-1,"finesse":0,"influence":4,"insight":4,"lore":-1,"might":2,"naturecraft":2,"perception":2,"stealth":0},"stats":{"dexterity":0,"intelligence":-1,"strength":2,"will":2}}'
curl -s http://localhost:5080/heroes/$HID -H "Authorization: Bearer $A"
```
Expected: 204 on PUT; GET shows `name` = "Caldra the Bold", `maxHp` = 10, `currentHp` clamped to 10 (was 12), `currentWounds` still 0.

- [ ] **Step 7: Verify TempHp absorb-first**

```bash
curl -s -X POST http://localhost:5080/heroes/$HID/grant-temp-hp -H "Authorization: Bearer $A" -H "Content-Type: application/json" -d '{"amount":4}'
curl -s -X POST http://localhost:5080/heroes/$HID/take-damage   -H "Authorization: Bearer $A" -H "Content-Type: application/json" -d '{"amount":3}'
curl -s http://localhost:5080/heroes/$HID -H "Authorization: Bearer $A"
```
Expected: after grant `tempHp`=4; after 3 damage `tempHp`=1 and `currentHp` unchanged (10).

- [ ] **Step 8: Stop the API.**

---

## Task 10: Update CLAUDE.md

**Files:**
- Modify: `CLAUDE.md`

- [ ] **Step 1: Add the new hero routes**

In the Hero endpoint routes table (NS.FastEndpoints section), add rows:

```markdown
| PUT | `/heroes/{heroId}` | `UpdateHeroEndpoint` |
| POST | `/heroes/{heroId}/grant-temp-hp` | `GrantTempHpEndpoint` |
```

- [ ] **Step 2: Document the new domain + auth behavior**

Add a short note under the NS.Domain Hero Aggregate description:

```markdown
- `TempHp` absorbs damage before `CurrentHp` (`TakeDamage`), is non-stacking (`GrantTempHp` keeps the higher value), and is cleared by `RecoverAllResources`.
- `UpdateBuild(...)` overwrites the character-build fields (the `HeroBuildRequest` set) while preserving level, subclass, play state, and collections; `CurrentHp`/`CurrentMana` clamp to lowered maximums.
```

And update the Known Caveats / auth notes to record:

```markdown
- **Hero ownership is enforced**: every hero-by-id endpoint loads via `GetOwnedByIdAsync` and returns 404 if the hero is missing or owned by another user. `GET /heroes` is scoped to the caller via `IHeroDataService.GetByUserAsync`. `UserId` is always taken from the JWT `sub` claim (via `ClaimsPrincipal.GetUserId()`); create and update share the `HeroBuildRequest` DTO and never trust a client-supplied owner.
```

- [ ] **Step 3: Commit**

```bash
git add CLAUDE.md
git commit -m "docs: record Phase A backend changes in CLAUDE.md"
```

---

## Task 11 (DEFERRED — tests after implementation)

> Per project preference, write these **after** the implementation above is working. Not a gate for Phase A completion. Skip until the user asks.

**Files:**
- Create: `NS.Domain.Tests/NS.Domain.Tests.csproj`
- Create: `NS.Domain.Tests/HeroTests.cs`

- [ ] **Step 1: Create the test project and add to the solution**

```bash
dotnet new xunit -o NS.Domain.Tests
dotnet add NS.Domain.Tests/NS.Domain.Tests.csproj reference NS.Domain/NS.Domain.csproj
dotnet sln NimbleSheets.slnx add NS.Domain.Tests/NS.Domain.Tests.csproj
```

- [ ] **Step 2: Add `HeroTests.cs`** covering: `TakeDamage` drains `TempHp` before `CurrentHp`; `GrantTempHp` is non-stacking; `RecoverAllResources` clears `TempHp`; `UpdateBuild` clamps `CurrentHp` to a lowered `MaxHp` and preserves `CurrentWounds`. Build a level-1 `Hero` via its public constructor in each test (Arrange), call the method (Act), assert the property (Assert).

- [ ] **Step 3: Run** `dotnet test NS.Domain.Tests/NS.Domain.Tests.csproj` — expected: all pass.

- [ ] **Step 4: Commit.**

---

## Self-Review

**Spec coverage:** Update endpoint (T4/T5), shared DTO + token-derived UserId (T3/T4), user-scoping (T2/T7), ownership 404 on all hero-by-id endpoints (T3/T7/T8), TempHp absorb-first + grant + rest-clear + endpoint (T1/T6), docs (T10), tests deferred (T11). Seeding correctly absent (Phase D). ✅ All spec sections mapped.

**Placeholder scan:** No TBD/TODO; every code step shows complete code; T8's repeated edit is one exact line across an enumerated file list (not a vague "similar to"). Runtime values in T9 curl (tokens/ids) are expected user-supplied, not plan gaps. ✅

**Type consistency:** `HeroBuildRequest`, `GetUserId()`, `GetOwnedByIdAsync()`, `UpdateBuild(...)`, `GrantTempHp()`, `GetByUserAsync()`, `GrantTempHpRequest` used identically across tasks. `UpdateBuild` parameter order matches `HeroBuildRequest`/constructor order. ✅
