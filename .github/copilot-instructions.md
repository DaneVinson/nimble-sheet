# NimbleSheets — Copilot Instructions

NimbleSheets is a web API for managing player characters in the **Nimble** tabletop RPG system. It is a C# 14 / .NET 10 solution using FastEndpoints 8.x for the API layer and SoloDB (SQLite-backed JSON document store) for persistence.

---

## Solution Structure

```
NimbleSheets.slnx
Directory.Build.props          ← Nullable, ImplicitUsings, LangVersion=14 for all projects
NS.Domain/                     ← Pure domain model; no framework dependencies
NS.SoloDB/                     ← SoloDB persistence implementations
NS.FastEndpoints/              ← All API endpoints (discovered by NS.WebApp at startup)
NS.WebApp/                     ← Entry point; wires DI and middleware
```

---

## C# Coding Conventions

These conventions apply across the entire codebase.

- **`sealed`** on every class without exception
- **Positional records**: `public sealed record Foo(Type Param);` — `<param>` docs on the declaration line
- **Member ordering** (alphabetical within each group): private fields → constructors → properties → methods
- **XML docs** on all public types and members
- **`_GlobalUsings.cs`** per project holds all `global using` directives; no `using` directives in individual files
- **`Directory.Build.props`** holds `<Nullable>`, `<ImplicitUsings>`, and `<LangVersion>` — individual `.csproj` files must **not** repeat these
- `.slnx` solution format (not `.sln`)
- Acronyms of 3+ letters use Pascal casing: `Xml`, `Cqrs`. Two-letter acronyms stay uppercase: `Id`

---

## NS.Domain

**Namespace**: `NS.Domain` (flat, regardless of subfolder)

### Enums

| File | Values |
|---|---|
| `ActionType` | `Free`, `Heroic`, `Reaction` |
| `ArmorType` | `Cloth`, `Leather`, `Mail`, `Plate`, `Shield` |
| `DamageType` | `Bludgeoning`, `Cold`, `Fire`, `Lightning`, `Piercing`, `Psychic`, `Radiant`, `Slashing` |
| `DieType` | `D4=4`, `D6=6`, `D8=8`, `D10=10`, `D12=12` |
| `HeroClass` | `Berserker`, `Cheat`, `Commander`, `Hunter`, `Mage`, `Oathsworn`, `Shadowmancer`, `Shepherd`, `Songweaver`, `Stormshifter`, `Zephyr` |
| `RuleCategory` | `Combat`, `Conditions`, `LevelUp`, `Movement`, `Resting`, `Spellcasting` |
| `SpellSchool` | `Fire`, `Ice`, `Lightning`, `Radiant` |
| `StatType` | `Dexterity`, `Intelligence`, `Strength`, `Will` |

### Hero Value Objects (positional records)

```csharp
ClassResources(int? JudgmentDiceCount, DieType? JudgmentDiceType, int? LayOnHandsPool, int? ThrillCharges)
HeroCombatStats(int Armor, DieType HitDieType, int InitiativeBonus, int Speed)
HeroSaves(StatType AdvantageOn, StatType DisadvantageOn)
HeroSkills(int Arcana, int Examination, int Finesse, int Influence, int Insight, int Lore, int Might, int Naturecraft, int Perception, int Stealth)
HeroStats(int Dexterity, int Intelligence, int Strength, int Will)
HeroArmor(Guid ArmorId, Guid HeroId, bool IsEquipped)
HeroCondition(Guid ConditionId, string? ExpiresAtEndOf, Guid HeroId)
HeroFeature(IReadOnlyList<string> Choices, Guid FeatureId, Guid HeroId, int LevelGained)
HeroGearItem(Guid HeroId, string Name, int Quantity)
HeroMagicItem(int? ChargesRemaining, Guid HeroId, bool IsEquipped, Guid MagicItemId)
HeroSpell(Guid HeroId, string? Notes, Guid SpellId, int TierUnlocked)
HeroWeapon(Guid HeroId, bool IsEquipped, string? Notes, Guid WeaponId)
```

### Hero Aggregate

`Hero` is a `sealed class` (not a record) with:

- **Constructor**: sets `Level=1`, `MaxHitDice=1`, `HitDiceAvailable=1`, `CurrentHp=maxHp`, `CurrentMana=maxMana`, `CurrentWounds=0`, `PendingStatIncrease=false`, `UnspentSkillPoints=0`. Constructor parameter `userId` (alphabetically last) is stored as `UserId`.
- **`private Hero()`** parameterless constructor for deserializers — sets all reference-type properties to `null!`
- Scalar mutable properties use `private set`
- Collection properties use the `init => _field.AddRange(value)` pattern to support deserializer reconstruction while backing the field with `private readonly List<T>`
- `IsDead => CurrentWounds >= 6`; `IsDying => CurrentHp == 0`
- `UserId` links a hero to its owning `User`

**Mutation methods** (alphabetical): `AddArmor`, `AddCondition`, `AddFeature`, `AddGearItem`, `AddMagicItem`, `AddSpell`, `AddWeapon`, `ApplyHpIncrease`, `ApplyStatIncrease`, `CompletePendingChoice`, `FinalizeSkillAllocation`, `GainWound`, `Heal`, `HealWound`, `LevelUp`, `RecoverAllResources`, `RemoveArmor`, `RemoveCondition`, `RemoveFeature`, `RemoveGearItem`, `RemoveMagicItem`, `RemoveSpell`, `RemoveWeapon`, `SetSubclass`, `SpendHitDice`, `SpendMana`, `TakeDamage`, `UpdateCombatStats`

### Reference Entities (positional records)

```csharp
ActionReference(ActionType ActionType, int Cost, string Description, string? FrequencyLimit, Guid Id, string Name)
Ancestry(string Description, Guid Id, string Name, IReadOnlyList<string> Traits)
Armor(ArmorType ArmorType, int ArmorValue, string Description, Guid Id, string Name)
Background(string Description, string Grants, Guid Id, string Name)
Condition(string Description, Guid Id, string Name)
Feature(HeroClass Class, string Description, string? FrequencyLimit, Guid Id, int Level, string Name, IReadOnlyList<string>? SelectableOptions, string? Subclass)
MagicItem(Guid? ContainedSpellId, string Description, string Effect, Guid Id, int? MaxCharges, string Name, string Rarity)
RuleReference(RuleCategory Category, string Description, Guid Id, string Name)
Spell(int ActionCost, string? AreaOfEffect, string? DamageExpression, DamageType? DamageType, string Description, string? Duration, Guid Id, bool IsConcentration, bool IsSecret, int ManaCost, string Name, int? Range, StatType? SaveType, SpellSchool School, int Tier, string? UpcastEffect)
Weapon(string DamageExpression, DamageType DamageType, string Description, Guid Id, bool IsRare, bool IsTwoHanded, string Name, int? Range, int Reach, string? SpecialEffect, StatType StatUsed)
```

### Abstractions

```csharp
// IHeroDataService
Task DeleteAsync(Guid id);
Task<IReadOnlyList<Hero>> GetAllAsync();
Task<Hero?> GetByIdAsync(Guid id);
Task SaveAsync(Hero hero);

// IReferenceDataService<T> where T : class
Task<IReadOnlyList<T>> FindAsync(Func<T, bool> predicate);
Task<IReadOnlyList<T>> GetAllAsync();
Task<T?> GetByIdAsync(Guid id);

// IUserDataService
Task CreateAsync(User user);
Task<IReadOnlyList<User>> FindByNameAsync(string name);  // case-insensitive contains
Task<User?> GetByIdAsync(Guid id);
Task UpdateAsync(User user);
```

### User Entity

`User` is a `sealed class` (not a record) with properties `Created` (DateTimeOffset), `Email`, `Id` (Guid), `Name` — all `private set`. Follows the same constructor + `private User()` deserializer pattern as `Hero`.

**Mutation methods**: `UpdateEmail(string email)`

Users cannot be deleted.

### GUID creation

All new GUIDs are created with `Guid.CreateVersion7()` (time-ordered, .NET 9+). **Never use `Guid.NewGuid()`.**

---

## NS.SoloDB

**Namespace**: `NSSoloDB` (not `NS.SoloDB` — avoids conflict with `SoloDatabase.SoloDB` class)

**Key design**:
- `SoloDocument<T>` — internal wrapper with `long Id` (SoloDB's required PK) and `T Data`; keeps domain entities free of persistence attributes
- `SoloHeroDataService` — loads full collection then filters in-memory (SoloDB has no native LINQ over JSON)
- `SoloReferenceDataService<T>` — uses a cached reflection delegate `Func<T, Guid> _getId` to read `Id` without a domain interface
- `SoloUserDataService` — same in-memory pattern as `SoloHeroDataService`; implements `IUserDataService`
- All services are registered as **Singletons** because SoloDB is thread-safe via its internal connection pool
- `ServiceCollectionExtensions.AddSoloDBDataServices(services, databasePath)` registers everything

**`_GlobalUsings.cs`**:
```csharp
global using Microsoft.Extensions.DependencyInjection;
global using NS.Domain;
global using SoloDatabase;
```

---

## NS.FastEndpoints

**Namespace**: `NSFastEndpoints` (flat)  
**Package**: `FastEndpoints` 8.1.0

**`_GlobalUsings.cs`**:
```csharp
global using FastEndpoints;
global using FluentValidation;
global using NS.Domain;
```

### FastEndpoints 8.x API — critical differences from older versions

The `Send*Async` methods (e.g. `SendOkAsync`, `SendNotFoundAsync`) **no longer exist** as direct methods on the endpoint class. In 8.x, response sending is done through the **`Send` property** on `Endpoint<TReq, TRes>`:

```csharp
// ✅ FastEndpoints 8.x correct
await Send.NotFoundAsync(ct);
await Send.NoContentAsync(ct);
await Send.OkAsync(response, ct);
await Send.ResponseAsync(response, statusCode: 201, ct);

// ❌ Old pattern — will not compile in 8.x
await SendNotFoundAsync(ct);
await SendNoContentAsync(ct);
await SendAsync(response, cancellation: ct);
```

### Base class usage

| Scenario | Base class |
|---|---|
| Endpoint with request and typed response | `Endpoint<TRequest, TResponse>` |
| Mutation (no response body) | `Endpoint<TRequest, EmptyResponse>` or `Endpoint<TRequest>` |
| GET all with no request params | `EndpointWithoutRequest<List<T>>` |

**`HandleAsync` signatures**:
- `Endpoint<TReq, TRes>`: `public override async Task HandleAsync(TReq req, CancellationToken ct)`
- `EndpointWithoutRequest<TRes>`: `public override async Task HandleAsync(CancellationToken ct)`

### Route and binding conventions

- Route params are bound by name (case-insensitive): `{heroId}` → `HeroId` property
- JSON body and route params are merged automatically
- All endpoints require an authenticated user by default (via the global fallback policy). Only `CreateUserEndpoint` and `LoginEndpoint` call `AllowAnonymous()` in `Configure()`.
- Validators use FluentValidation via `Validator<TRequest>` and are auto-discovered; format rules go in the validator class, business-rule checks (e.g. uniqueness) go in `HandleAsync` via `AddError` + `ThrowIfAnyErrors()`

### Assembly discovery

NS.FastEndpoints exposes `AssemblyMarker` (a public empty class). NS.WebApp discovers endpoints via:
```csharp
builder.Services.AddFastEndpoints(o =>
    o.Assemblies = [typeof(AssemblyMarker).Assembly]);
```

NS.WebApp must reference the `FastEndpoints` package **directly** and add `global using FastEndpoints;` so `AddFastEndpoints` and `UseFastEndpoints` extension methods resolve.

### Endpoint file conventions

- One class per file; request/response records defined in the same file as their endpoint
- Shared request types (`HeroIdRequest`, `UserIdRequest`, `ReferenceIdRequest`) live in their own files in the same folder
- Mutation endpoints return 204 No Content; clients re-fetch hero state after mutations
- `CreateHeroEndpoint` returns 201 with `CreateHeroResponse(Guid Id)`
- `IJwtTokenService` is defined in NS.FastEndpoints; implementation lives in NS.WebApp

### Hero endpoint routes

| Method | Route | Endpoint |
|---|---|---|
| GET | `/heroes` | `GetAllHeroesEndpoint` |
| GET | `/heroes/{heroId}` | `GetHeroEndpoint` |
| POST | `/heroes` | `CreateHeroEndpoint` |
| DELETE | `/heroes/{heroId}` | `DeleteHeroEndpoint` |
| POST | `/heroes/{heroId}/take-damage` | `TakeDamageEndpoint` |
| POST | `/heroes/{heroId}/heal` | `HealEndpoint` |
| POST | `/heroes/{heroId}/gain-wound` | `GainWoundEndpoint` |
| POST | `/heroes/{heroId}/heal-wound` | `HealWoundEndpoint` |
| POST | `/heroes/{heroId}/spend-mana` | `SpendManaEndpoint` |
| POST | `/heroes/{heroId}/spend-hit-dice` | `SpendHitDiceEndpoint` |
| POST | `/heroes/{heroId}/recover-all-resources` | `RecoverAllResourcesEndpoint` |
| POST | `/heroes/{heroId}/add-condition` | `AddConditionEndpoint` |
| POST | `/heroes/{heroId}/remove-condition` | `RemoveConditionEndpoint` |
| POST | `/heroes/{heroId}/add-armor` | `AddArmorEndpoint` |
| POST | `/heroes/{heroId}/remove-armor` | `RemoveArmorEndpoint` |
| POST | `/heroes/{heroId}/add-weapon` | `AddWeaponEndpoint` |
| POST | `/heroes/{heroId}/remove-weapon` | `RemoveWeaponEndpoint` |
| POST | `/heroes/{heroId}/add-spell` | `AddSpellEndpoint` |
| POST | `/heroes/{heroId}/remove-spell` | `RemoveSpellEndpoint` |
| POST | `/heroes/{heroId}/add-magic-item` | `AddMagicItemEndpoint` |
| POST | `/heroes/{heroId}/remove-magic-item` | `RemoveMagicItemEndpoint` |
| POST | `/heroes/{heroId}/add-gear-item` | `AddGearItemEndpoint` |
| POST | `/heroes/{heroId}/remove-gear-item` | `RemoveGearItemEndpoint` |
| POST | `/heroes/{heroId}/add-feature` | `AddFeatureEndpoint` |
| POST | `/heroes/{heroId}/remove-feature` | `RemoveFeatureEndpoint` |
| POST | `/heroes/{heroId}/level-up` | `LevelUpEndpoint` |
| POST | `/heroes/{heroId}/apply-stat-increase` | `ApplyStatIncreaseEndpoint` |
| POST | `/heroes/{heroId}/apply-hp-increase` | `ApplyHpIncreaseEndpoint` |
| POST | `/heroes/{heroId}/finalize-skill-allocation` | `FinalizeSkillAllocationEndpoint` |
| POST | `/heroes/{heroId}/complete-pending-choice` | `CompletePendingChoiceEndpoint` |
| POST | `/heroes/{heroId}/set-subclass` | `SetSubclassEndpoint` |
| POST | `/heroes/{heroId}/update-combat-stats` | `UpdateCombatStatsEndpoint` |

### Reference data endpoint routes

All reference routes follow `GET /reference/{resource}` and `GET /reference/{resource}/{id}`:

`actions`, `ancestries`, `armor`, `backgrounds`, `conditions`, `features`, `magic-items`, `rules`, `spells`, `weapons`

- `GET /reference/features` supports optional query params: `?heroClass={HeroClass}&level={int}`
- `GET /reference/spells` supports optional query params: `?tier={int}&school={SpellSchool}`

### User endpoint routes

| Method | Route | Endpoint | Notes |
|---|---|---|---|
| POST | `/users` | `CreateUserEndpoint` | Returns 201 with `CreateUserResponse(Guid Id)`; validates email format; checks name uniqueness |
| GET | `/users/{userId}` | `GetUserEndpoint` | Returns 404 if not found |
| POST | `/users/{userId}/update-email` | `UpdateUserEmailEndpoint` | Returns 204; validates email format |
| POST | `/users/login` | `LoginEndpoint` | Returns `LoginResponse(string Token, Guid UserId)`; 401 if name not found |

---

## NS.WebApp

**Packages**: `FastEndpoints` 8.1.0, `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.8

**`_GlobalUsings.cs`**:
```csharp
global using FastEndpoints;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.Extensions.Options;
global using Microsoft.IdentityModel.JsonWebTokens;
global using Microsoft.IdentityModel.Tokens;
global using NS.Domain;
global using NSFastEndpoints;
global using NSSoloDB;
global using NSWebApp;
global using System.Security.Claims;
global using System.Text;
```

**`Program.cs`**:
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSoloDBDataServices(
    builder.Configuration.GetValue<string>("SoloDB:DatabasePath") ?? "nimble-sheet.db");
builder.Services.AddFastEndpoints(o =>
    o.Assemblies = [typeof(AssemblyMarker).Assembly]);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new()
        {
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SigningKey"]!)),
            NameClaimType = JwtRegisteredClaimNames.Name,
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
        };
    });

builder.Services.AddAuthorization(options =>
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

var app = builder.Build();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();
app.Run();
```

Database path is configured via `SoloDB:DatabasePath` in `appsettings.json`; defaults to `"nimble-sheet.db"` in the working directory.

JWT is configured via the `"Jwt"` section (`Audience`, `ExpiryHours`, `Issuer`, `SigningKey`). The signing key must be overridden via environment variables or a secrets store in non-development environments.

**JWT token claims**: `sub` = UserId (Guid string), `name` = user display name, `email`, `jti` = token ID. `MapInboundClaims = false` preserves OIDC-standard claim names. `NameClaimType` is mapped to `"name"` so `User.Identity.Name` resolves correctly.

---

## Known Caveats

- **SoloDB deserialization of `Hero`**: SoloDB's JSON serializer must be able to populate `Hero`'s `private set` scalar properties. The `private Hero()` parameterless constructor and `init`-accessor collection properties were added specifically to support this. If scalar values (level, HP, etc.) come back as zero after a round-trip, a custom JSON converter for `Hero` will be needed.
- **In-memory filtering**: All SoloDB operations load the full collection then filter in-memory. This is fine for small datasets (TTRPG character sheets) but would not scale.
- **Auth is enforced globally**: A fallback `AuthorizationPolicy` of `RequireAuthenticatedUser()` is configured, so every endpoint requires a valid JWT unless it explicitly calls `AllowAnonymous()`. Only `CreateUserEndpoint` and `LoginEndpoint` are anonymous.
- **Login is not secure**: `LoginEndpoint` authenticates by name match only — no password. This is intentional for the POC.
- **No Swagger/OpenAPI**: Not yet added; add `FastEndpoints.Swagger` package if needed.
