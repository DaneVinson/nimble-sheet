# NimbleSheets — Claude Instructions

NimbleSheets is a web API for managing player characters in the **Nimble** tabletop RPG system. It is a C# 14 / .NET 10 solution using FastEndpoints 8.x for the API layer and SoloDB (SQLite-backed JSON document store) for persistence.

This file gives Claude an overview of the repository and records learnings accumulated while developing the project. Keep it up to date as the codebase and our shared understanding evolve.

---

## Solution Structure

```
NimbleSheets.slnx
Directory.Build.props          ← Nullable, ImplicitUsings, LangVersion=14 for all projects
NS.Domain/                     ← Pure domain model; no framework dependencies
NS.SoloDB/                     ← SoloDB persistence implementations
NS.FastEndpoints/              ← All API endpoints (discovered by NS.WebApp at startup)
NS.WebApp/                     ← Entry point; wires DI and middleware
NS.Client/                     ← SvelteKit SPA front-end (TypeScript); consumes the API over HTTP
NS.Tests/                      ← xUnit tests (domain units, SoloDB round-trips, seeding, request validators); references NS.Domain, NS.SoloDB, NS.FastEndpoints
docs/rules/                    ← Game-rules reference (source of truth for the domain/seed data)
```

**Game rules reference**: `docs/rules/nimble-basic-rules.md` is a faithful Markdown transcription of the *Nimble Quickstart Rules* PDF (stats, skills, combat, the 4 starter classes with full stat blocks/subclasses, monster stat blocks, Control/Chaos tables, items). **Use it as the source of truth** when modeling domain data or writing seed data. It now includes the full quickstart **spell lists** (Fire/Ice/Lightning/Radiant — 16 spells) which were originally graphical cards that didn't text-extract and were recovered via OCR (rendering PDF pages 11/13 to images with PyMuPDF and reading them). `NS.SoloDB/SeedData.cs` now seeds **authentic** quickstart content for the entities the rules define — all 16 spells, full L1–4 features for the 4 quickstart classes (incl. subclass features and selectable ability lists), the referenced conditions, the combat actions, and rules references — and keeps **honest, labeled placeholders** for entities the quickstart does not define (ancestries, backgrounds, armor, weapons). See [the reseed spec](docs/superpowers/specs/2026-06-18-reseed-reference-data-design.md).

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
- **`private Hero()`** parameterless constructor that sets reference-type properties to `null!`; retained as a safety net but **not** used by SoloDB (which rehydrates via uninitialized objects — see Known Caveats)
- Scalar properties use `private set` (including `Id`, so SoloDB can rehydrate it)
- Collection properties expose `IReadOnlyList<T>` over a non-`readonly` `List<T>` field; the `init` accessor **assigns** a null-tolerant new list (`init => _field = value is null ? [] : [.. value];`) so SoloDB can rehydrate them (see Known Caveats)
- `IsDead => CurrentWounds >= 6`; `IsDying => CurrentHp == 0`
- `UserId` links a hero to its owning `User`

**Mutation methods** (alphabetical): `AddArmor`, `AddCondition`, `AddFeature`, `AddGearItem`, `AddMagicItem`, `AddSpell`, `AddWeapon`, `ApplyHpIncrease`, `ApplyStatIncrease`, `CompletePendingChoice`, `FinalizeSkillAllocation`, `GainWound`, `GrantTempHp`, `Heal`, `HealWound`, `LevelUp`, `RecoverAllResources`, `RemoveArmor`, `RemoveCondition`, `RemoveFeature`, `RemoveGearItem`, `RemoveMagicItem`, `RemoveSpell`, `RemoveWeapon`, `SetArmorEquipped`, `SetMagicItemEquipped`, `SetSubclass`, `SetWeaponEquipped`, `SpendHitDice`, `SpendMana`, `TakeDamage`, `UpdateBuild`, `UpdateCombatStats`

- `SetArmorEquipped`/`SetMagicItemEquipped`/`SetWeaponEquipped(referenceId, isEquipped)` locate the matching collection record by its reference id and replace it with `record with { IsEquipped = ... }`; they no-op when the id is absent (consistent with the `RemoveAll`-by-id removals).

- `TempHp` absorbs damage before `CurrentHp` (`TakeDamage`), is non-stacking (`GrantTempHp` keeps the higher value), and is cleared by `RecoverAllResources`.
- `UpdateBuild(...)` overwrites the character-build fields (the `HeroBuildRequest` set) while preserving level, subclass, play state, and collections; `CurrentHp`/`CurrentMana` clamp to lowered maximums.
- The numeric play-mutation methods (`TakeDamage`, `Heal`, `GrantTempHp`, `SpendMana`, `SpendHitDice`) guard their amount arguments with `ArgumentOutOfRangeException.ThrowIfNegative(...)` as the first statement (uniformly reject negatives; existing `Math.Max/Min` clamping follows). The API validators (NS.FastEndpoints) are the friendlier first line; these guards make the domain authoritative for any caller.

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
Task<IReadOnlyList<Hero>> GetByUserAsync(Guid userId);
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
- `SoloCollections.Of<T>(db)` — the **only** way services should resolve a collection. It calls `db.GetCollection<SoloDocument<T>>(typeof(T).Name)`, naming each collection after the domain type. **Never call `db.GetCollection<SoloDocument<T>>()` without a name**: SoloDB derives the default name from the generic wrapper type, and a closed generic's `Type.Name` is identical for every `T` (`SoloDocument`1`), so every entity type would collide in one physical collection (Users/Heroes/reference data bleeding into each other). See Known Caveats.
- `SoloHeroDataService` — loads full collection then filters in-memory (SoloDB has no native LINQ over JSON)
- `SoloReferenceDataService<T>` — uses a cached reflection delegate `Func<T, Guid> _getId` to read `Id` without a domain interface
- `SoloUserDataService` — same in-memory pattern as `SoloHeroDataService`; implements `IUserDataService`
- All services are registered as **Singletons** because SoloDB is thread-safe via its internal connection pool
- `ServiceCollectionExtensions.AddSoloDBDataServices(services, databasePath)` registers everything (including the seeder)
- **Reference-data seeding**: `IReferenceDataSeeder` (public) / `SoloReferenceDataSeeder` (public sealed) — `SeedAsync` inserts the starter set **only into collections that are currently empty** (idempotent; **editing seed data later needs a fresh DB** — delete `nimble-sheet.db` to reseed). The rows live in `SeedData` (internal static) as positional-record literals with **fixed hand-written GUIDs** (never `CreateVersion7()`) so heroes can reference them; the Caldra-overlapping rows reuse the client `fixtures/caldra.ts` GUIDs (Human `a…0001`, Mace `b…0001`, Rusty Mail `c…0001`, Wooden Buckler `c…0002`, Radiant Judgment `d…0001`, Lay on Hands `d…0002` — do not change these). The content is authentic quickstart data where the rules define it, with labeled placeholders elsewhere (see the Game rules reference note above). NS.WebApp invokes `SeedAsync()` at startup after `app.Build()`.

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
global using System.Security.Claims;
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
- The amount-bearing play-mutation endpoints have validators (co-located in their endpoint file) rejecting invalid amounts → **400**: `TakeDamageValidator`/`HealValidator`/`SpendManaValidator` use `GreaterThan(0)`; `GrantTempHpValidator` and `SpendHitDiceValidator.HealingAmount` use `GreaterThanOrEqualTo(0)` (0 allowed); `SpendHitDiceValidator.Count` uses `GreaterThan(0)`. The no-body mutation endpoints take no numeric input. Domain guards back these up (see NS.Domain).

### Assembly discovery

NS.FastEndpoints exposes `AssemblyMarker` (a public empty class). NS.WebApp discovers endpoints via:
```csharp
builder.Services.AddFastEndpoints(o =>
    o.Assemblies = [typeof(AssemblyMarker).Assembly]);
```

NS.WebApp must reference the `FastEndpoints` package **directly** and add `global using FastEndpoints;` so `AddFastEndpoints` and `UseFastEndpoints` extension methods resolve.

### Endpoint file conventions

- One class per file; request/response records defined in the same file as their endpoint
- Shared request types (`HeroIdRequest`, `UserIdRequest`, `ReferenceIdRequest`, `HeroBuildRequest`) live in their own files in the same folder
- Mutation endpoints return 204 No Content; clients re-fetch hero state after mutations
- `CreateHeroEndpoint` and `UpdateHeroEndpoint` share the `HeroBuildRequest` DTO (build inputs). Create returns 201 with `CreateHeroResponse(Guid Id)`; update returns 204. Neither trusts a client-supplied owner — `UserId` comes from the token.
- `IJwtTokenService` is defined in NS.FastEndpoints; implementation lives in NS.WebApp

### Hero ownership & scoping

- `UserId` is read from the JWT `sub` claim via `ClaimsPrincipal.GetUserId()` (in NS.FastEndpoints). The host sets `MapInboundClaims = false`, so the claim type is the literal `"sub"`.
- `GET /heroes` is scoped to the caller via `IHeroDataService.GetByUserAsync(userId)`.
- Every hero-by-id endpoint (get, update, delete, and **all** granular mutation endpoints) loads through `IHeroDataServiceExtensions.GetOwnedByIdAsync(id, userId)` and returns **404** when the hero is missing or owned by another user (404, not 403, to avoid leaking existence).

### Hero endpoint routes

> All API routes below are served under the **`/api`** prefix (e.g. `/api/heroes`, `/api/reference/spells`, `/api/users/login`) — see the global `RoutePrefix = "api"` in NS.WebApp. The tables list routes relative to that prefix. The client's `apiFetch` prepends `/api`; the SPA's own client-side routes (`/heroes`, `/login`, …) are unprefixed and resolve to the app shell.

| Method | Route | Endpoint |
|---|---|---|
| GET | `/heroes` | `GetAllHeroesEndpoint` |
| GET | `/heroes/{heroId}` | `GetHeroEndpoint` |
| POST | `/heroes` | `CreateHeroEndpoint` |
| PUT | `/heroes/{heroId}` | `UpdateHeroEndpoint` |
| DELETE | `/heroes/{heroId}` | `DeleteHeroEndpoint` |
| POST | `/heroes/{heroId}/take-damage` | `TakeDamageEndpoint` |
| POST | `/heroes/{heroId}/grant-temp-hp` | `GrantTempHpEndpoint` |
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
| POST | `/heroes/{heroId}/set-weapon-equipped` | `SetWeaponEquippedEndpoint` |
| POST | `/heroes/{heroId}/set-armor-equipped` | `SetArmorEquippedEndpoint` |
| POST | `/heroes/{heroId}/set-magic-item-equipped` | `SetMagicItemEquippedEndpoint` |
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
global using System.Text.Json.Serialization;
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
await app.Services.GetRequiredService<IReferenceDataSeeder>().SeedAsync();  // seed reference data (idempotent)
app.UseHttpsRedirection();
app.UseDefaultFiles();      // serve the SPA from wwwroot (same-origin)
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints(c =>
{
    c.Endpoints.RoutePrefix = "api";   // all API endpoints served under /api (avoids SPA route collisions)
    c.Serializer.Options.Converters.Add(new JsonStringEnumConverter());
});
app.MapFallbackToFile("index.html").AllowAnonymous();   // SPA deep-link fallback
app.Run();
```

**JSON enum serialization**: `UseFastEndpoints` registers a `JsonStringEnumConverter`, so enums are serialized as their **names** (e.g. `"class":"Oathsworn"`, `"hitDieType":"D10"`, `"advantageOn":"Will"`). Requests accept either names or integers. Prefer names in the client.

Database path is configured via `SoloDB:DatabasePath` in `appsettings.json`; defaults to `"nimble-sheet.db"` in the working directory.

JWT is configured via the `"Jwt"` section (`Audience`, `ExpiryHours`, `Issuer`, `SigningKey`). The signing key must be overridden via environment variables or a secrets store in non-development environments.

**JWT token claims**: `sub` = UserId (Guid string), `name` = user display name, `email`, `jti` = token ID. `MapInboundClaims = false` preserves OIDC-standard claim names. `NameClaimType` is mapped to `"name"` so `User.Identity.Name` resolves correctly.

---

## NS.Client

The front-end SPA.

### Character Sheet & live API integration

A dark-mode character sheet backed by **live API data**, behind a login flow.

- **Auth/session** (`src/lib/auth/session.ts`): a localStorage-backed JWT session store (`Session = { name, token, userId }`) hydrated on load; `setSession` / `clearSession`. Guarded for non-browser (test) environments.
- **API client** (`src/lib/api/client.ts`): `apiFetch` attaches the bearer token, throws `ApiError` on non-2xx, and on **401 clears the session + redirects to `/login`** (centrally). 204 → `void`. `readErrorMessage` extracts FastEndpoints validation messages (`{errors:{field:[msg]}}`) so 400s surface a real message. Typed wrappers: `login`, `createUser`, `getHeroes`, `getHero`, `getReferenceCollection`, `createHero`, `updateHero`, and the hero play-mutation wrappers (`takeDamage`, `heal`, `grantTempHp`, `gainWound`, `healWound`, `spendHitDice`, `spendMana`, `recoverAll`).
- **Reference cache** (`src/lib/reference/cache.ts`): lazily fetches only the reference collections a hero actually references (`neededResources` → `assembleReferenceData`), caches each full collection for the session, and **evicts a rejected fetch** so it retries rather than poisoning the cache.
- **Data layer** (`src/lib/`): `api/types.ts` mirrors the API DTOs (camelCase; enums as string-union types matching the `JsonStringEnumConverter` names). `sheet/resolve.ts` is a pure resolver joining the hero's ID-referenced collections to reference data into a `SheetViewModel` (`sheet/viewmodel.ts`); `sheet/format.ts` holds display helpers. `fixtures/caldra.ts` remains as the resolver test fixture.
- **Components** (`src/lib/sheet/components/`): `HeroSheet` composes a pinned region (banner, vitals, stats with `SAVE▲/▼` save markers, skills) and a tab switcher (`SheetTabs`) over Combat / Magic / Class Resources / Inventory / Features panels. Always dark — dark-tone Tailwind utilities directly, no `dark:` variants.
- **Live-play mutations** (`src/lib/sheet/`): interactive tiles wire the eight play-mutation endpoints. A `heroActions.svelte.ts` runes context (`createHeroActions(getHeroId)` + `HERO_ACTIONS` key, reactive `busy`/`error`, each method POSTs then `invalidateAll()`) is provided by `HeroActionsScope.svelte` (keyed by `heroId` on the `[id]` page so a fresh actions object resets stale error/inputs on navigation). `runAction.ts` is the pure (rune-free, testable) busy/error/refresh orchestrator. Tiles (`HpTile`, `WoundTrack`, `HitDiceTile`, `ManaTile`, `RestButton`) consume the context **optionally** via a reusable `TilePopover` — read-only when no provider is present. Server owns all rules; the client sends amounts and re-fetches (no optimistic updates).
- **Collection editing** (`src/lib/sheet/components/*Editor.svelte`): inline add/remove on the sheet for weapons, armor, magic items, gear, spells, conditions, and **features**, plus equip/unequip for weapons/armor/magic items. Per-collection editor components (`WeaponEditor`, `ArmorEditor`, `MagicItemEditor`, `SpellEditor`, `GearEditor`, `ConditionEditor`, `FeatureEditor`) replace the panels' read-only lists; each consumes the `HERO_ACTIONS` context **optionally** (read-only when absent, same as the play tiles) and uses the same `run()` → POST → `invalidateAll()` flow. The full add/remove/equip client wrappers live in `api/client.ts`; the matching `heroActions` methods reuse `runAction`. "+ Add" pickers lazily fetch the full reference catalog via `getCollection(resource)` (session-cached) on popover open and exclude already-owned ids to avoid duplicates; gear is free-text (name + quantity), keyed by name. `FeatureEditor` additionally filters the picker to the hero's class with `level ≤ hero.level` and captures a feature's `selectableOptions` as checkbox `choices` on add (plus an editable `levelGained` defaulting to the feature's level). The reference id needed for remove/equip is carried on the view models (`weaponId`/`armorId`/`magicItemId`/`spellId`/`conditionId`/`featureId`). Shared button styling is `editorButton` in `components/styles.ts`. Server owns all rules; no optimistic updates.
- **Level-up flow** (`src/lib/sheet/components/LevelUpControls.svelte`): inline controls next to the Rest button. A **Level Up** popover takes a manually-entered "HP gained" and, in one action (`heroActions.levelUp(hp)`), applies the HP increase (when > 0) then POSTs `level-up` with an empty `pendingChoices` — a single re-fetch. The resulting pending state surfaces as separate amber popovers: **choose stat increase** (`pendingStatIncrease`), **allocate skill points** (`unspentSkillPoints > 0`), and **choose subclass** at L3 (`needsSubclass`). The skill allocator is backed by the pure, unit-tested `src/lib/sheet/levelUp/skillAllocation.ts` (`SKILLS`, `SKILL_CAP=12`, `spentPoints`/`canIncrement`/`canDecrement`/`canFinalize`) — it seeds a working copy from `skillValues`, enforces the +12 cap, and disables Finalize until **all** points are spent (the endpoint clears the pool, so under-spending would lose points). The view model exposes `pendingStatIncrease`/`unspentSkillPoints`/`needsSubclass`/`skillValues`. **Out of scope:** pending *feature* choices (`CompletePendingChoice`) — new-level features are added via `FeatureEditor`; and dice rolling — HP is entered manually. Note the domain grants a pending stat increase + skill point on *every* level-up; the UI reflects that.
- **Hero build form** (`src/lib/sheet/build/`): a shared `HeroBuildForm.svelte` + seven `*Section.svelte` (Identity/Vitals/Combat/Stats/Saves/Skills/ClassResources), each a `$bindable()` slice of a `HeroBuildModel` (`model.ts`: `blankBuildModel`, `heroToBuildModel`, `normalizeBuild` — coerces cleared required numerics to 0 before submit). `validate.ts` does required-fields-only client validation; the server is authoritative. Build fields only — equipment/spell collections are not in the form (the server's `UpdateBuild` preserves them); those are edited inline on the sheet (see Collection editing above).
- **Routes**: `/login` (login + create-user, anonymous); a guarded **`(app)` route group** (`(app)/+layout.ts` redirects to `/login` when there's no session; `(app)/+layout.svelte` is the dark app chrome — app name, user name, logout, `$navigating` loading bar) holding `/heroes` (list, with a "New hero" link), `/heroes/new` (create form → `createHero` → navigate to the new sheet), `/heroes/[id]` (the live sheet: load → `getHero` → `assembleReferenceData` → `resolveSheet` → `HeroSheet`, with an "Edit" link and a `+error.svelte` 404 boundary), and `/heroes/[id]/edit` (edit form pre-filled via `heroToBuildModel` → `updateHero` → back to the sheet; own `+error.svelte`). Root `/` redirects to `/heroes`. (SvelteKit route groups don't appear in the URL.)
- **Tests**: Vitest covers the resolver, reference cache, API client (play-mutation, build, collection add/remove/equip, and level-up wrappers), session store, `runAction`, the build `model`/`validate`/`normalizeBuild` logic, and the pure `levelUp/skillAllocation` helper; run with `npm test`. `vitest.config.ts` adds `$lib` and `$app/*` aliases (the latter pointing at `src/test/app-stub.ts`) so these modules import under the Node test env. Runes modules (`heroActions.svelte.ts`) and `.svelte` components are not compiled by the standalone Vitest config, so the inline editors, `LevelUpControls`, and the `heroActions` composites are verified in the browser rather than unit-tested.
- **Shipped (2026-06-14)**: live-play mutations (HP/wounds/hit-dice/mana/rest popovers), reference-data seeding (real names resolve end-to-end), the create/edit-hero build form, and server-side mutation validation. Specs/plans for each are under `docs/superpowers/`.
- **Shipped (2026-06-15)**: the `/api` route prefix (fixes deep-link/refresh on `/heroes/{id}`), a sensible default Max HP for new heroes (hit-die face value), and inline collection editing for all seven hero collections — equipment, spells, conditions, and **features** (with equip toggles for weapons/armor/magic items, and class/level-filtered feature add with selectable-option choices). Browser-level visual verification has now been done end-to-end (login → create → sheet → edit → play mutations → collection editing) via Playwright against the single .NET server.
- **Shipped (2026-06-16)**: the **level-up flow** UI (`LevelUpControls`) — Level Up with manual HP entry, then inline resolution of the pending stat increase, skill allocation (pure helper, +12 cap, all-points-required), and subclass at L3. Browser-verified end-to-end (level 1→3, stat/skill/subclass resolved). Spec/plan under `docs/superpowers/`.
- **Shipped (2026-06-18)**: documentation/data hardening. (1) Suppressed the unfixable **SQLite CVE-2025-6965** (`NU1903`) via a documented risk acceptance — see Known Caveats. (2) Added the **Nimble quickstart rules reference** at `docs/rules/nimble-basic-rules.md` (+ the source PDF), with the Fire/Ice/Lightning/Radiant **spell lists recovered via OCR** (the PDF's graphical spell cards don't text-extract; rendered pages 11/13 to images with PyMuPDF and read them). (3) **Reseeded `SeedData.cs`** with authentic quickstart content — all 16 spells, full L1–4 features for the 4 quickstart classes (incl. subclass features + selectable ability lists), the referenced conditions, combat actions, and rules references — keeping honest **labeled placeholders** where the quickstart has no data (ancestries/backgrounds/armor/weapons); fixture GUIDs preserved, 41 tests pass, and the seeded data was verified flowing end-to-end through the live API. Spec under `docs/superpowers/specs/2026-06-18-reseed-reference-data-design.md`.
- **Shipped (2026-06-19)**: two `TilePopover` fixes for the inline-editor/play-tile popovers, both browser-verified against the live single-server build. (1) **Dismiss on outside `pointerdown` instead of `click`** — a native `<select>`'s option-selection emits a fall-through `click` outside the small popover, which a click listener treated as "clicked outside" and closed it before the user could press "Add", silently breaking every inline editor's add-flow (reported via **Conditions**); native option selection dispatches no page-level `pointerdown`, so the swap is immune to it while still dismissing on genuine outside interaction. (2) **Popover rendered `position: fixed`, anchored to the trigger, with vertical flip + horizontal clamp** — the hero sheet's `<article>` has `overflow-hidden`, which clipped the lower half (the "Add" button) of downward-opening popovers in the bottom editors so they were invisible/unclickable; `fixed` is viewport-relative and escapes ancestor `overflow`, the flip opens the popover above the trigger when there's no room below, and the clamp keeps it on-screen. Neither bug reproduces with Playwright's programmatic `selectOption` (it sets values without the OS dropdown's fall-through click), so verification reproduced the exact event signature directly (a bare outside `click`) and measured the "Add" button's on-screen geometry/hit-test at the failing 720px viewport. Change is `NS.Client/src/lib/sheet/components/TilePopover.svelte`.

**Stack**:
- **SvelteKit 2.x** on **Svelte 5** (runes mode forced for project code), **Vite 8**, **TypeScript**
- **Pure SPA**: `@sveltejs/adapter-static` with `fallback: 'index.html'`; `src/routes/+layout.ts` sets `ssr = false` and `prerender = false`. Builds to static assets under `build/` (git-ignored) — no Node server at runtime
- **Tailwind CSS v4** via the `@tailwindcss/vite` plugin (no `tailwind.config.js`; config is CSS-first in `src/app.css`)
- **Flowbite Svelte 1.x** (`flowbite-svelte`, `flowbite`, `flowbite-svelte-icons`) as the UI component library
- **Vitest** for unit tests (pure TypeScript logic — resolver, reference cache, API client, session store); configured via `vitest.config.ts` (standalone, no SvelteKit plugin) with `$lib`/`$app/*` aliases (`$app/*` → `src/test/app-stub.ts`) so the app modules import under Node

**Key files**:
- `src/app.css` — Tailwind entry: `@import 'tailwindcss'`, `@plugin 'flowbite/plugin'`, the `dark` custom variant, and `@source` directives pointing at the Flowbite Svelte `dist` folders so their classes are scanned. Imported once in `src/routes/+layout.svelte`
- `svelte.config.js` — static adapter + SPA fallback
- `vite.config.ts` — registers `tailwindcss()` before `sveltekit()`; dev `server.proxy` forwards `/api` to the API (`http://localhost:5197`) so `npm run dev` runs against the live backend (production is same-origin and never hits the proxy). All API calls go through `/api` (the client's `apiFetch` prepends the prefix), so client routes like `/heroes` never collide with the API

**Commands** (run from `NS.Client/`):
- `npm run dev` — dev server
- `npm run build` — static SPA build to `build/`
- `npm run check` — `svelte-check` type checking (keep at 0 errors / 0 warnings)
- `npm test` — Vitest unit tests (`vitest run`)

**Conventions**: Flowbite Svelte expects Svelte ≥5.40 and Tailwind ≥4.1. The C# conventions above do **not** apply to this project; follow standard SvelteKit/TypeScript idioms here.

### Build & hosting integration (NS.WebApp ↔ NS.Client)

NS.WebApp hosts the SPA as **same-origin static content**, so the browser never makes cross-origin calls and **no CORS configuration is needed** — and there is **never a second web server**: `dotnet run` on NS.WebApp serves both the API and the front-end. This mirrors the pattern in the `check-mate` repo. Wiring lives in `NS.WebApp/NS.WebApp.csproj` (MSBuild targets) and `NS.WebApp/Program.cs` (middleware).

- **`BuildSpaIfMissing`** (`BeforeTargets="Build"`, condition: `wwwroot\index.html` absent): runs `npm install` + `npm run build` in `NS.Client`, then `Copy`s `NS.Client/build/**` into the **source** `NS.WebApp/wwwroot/`. Runs on the first build (or after `wwwroot` is cleared); **skipped once the output exists**, so subsequent builds are fast (~5s). Because the output lands in source `wwwroot`, `dotnet run` serves the SPA directly.
- **`PublishSpa`** (`BeforeTargets="Publish"`): **always** runs `npm install` + `npm run build` for a fresh production build, then injects `NS.Client/build/**` into the publish output under `wwwroot\` via `ResolvedFileToPublish` (publish directory only).
- **Refreshing the SPA in dev**: because `BuildSpaIfMissing` is skipped when output exists, a plain `dotnet build` does **not** pick up SPA source changes. To refresh, delete `NS.WebApp/wwwroot` (next build rebuilds) or run `npm run build` in `NS.Client`. For active front-end iteration you *can* still run `npm run dev` (Vite HMR), but it is optional — the app fully works from the single .NET server.
- **`wwwroot` is generated**: `NS.WebApp/wwwroot/` is git-ignored (see root `.gitignore`); it is never committed.
- **Serving** (`Program.cs`): `UseDefaultFiles()` + `UseStaticFiles()` (assets served before auth, no token needed) and `MapFallbackToFile("index.html").AllowAnonymous()` so client-side deep links return the app shell. API endpoints keep their own auth requirements; the fallback must stay anonymous or the global `RequireAuthenticatedUser` fallback policy would 401 the shell.
- **API/SPA route collisions (resolved)**: All API endpoints are served under the **`/api`** prefix (`UseFastEndpoints(c => c.Endpoints.RoutePrefix = "api")`), so they never collide with SPA client routes. The client routes (`/heroes`, `/heroes/{id}`, `/login`, …) fall through to `MapFallbackToFile("index.html")`, so **refresh/deep-link/bookmark on any client route returns the app shell** rather than a raw API response. Previously the API was unprefixed and answered `/heroes` and `/heroes/{id}` directly (a hard navigation/refresh returned a 401 instead of the SPA). Confirmed via browser verification. The dev Vite proxy now forwards only `/api`.

---

## Known Caveats

- **SoloDB deserialization of `Hero` (resolved)**: SoloDB rehydrates entities as **uninitialized objects** — it does *not* run any constructor or field initializer, then assigns values through property setters. Two consequences, now fixed:
  - Every persisted scalar/value-object property must have a **settable** accessor (`get; private set;` or `init`). `Hero.Id` was originally get-only, so it could not be rehydrated and came back as `Guid.Empty`, breaking lookups by id — it now has `private set`. (`User` was already fine; all its properties have `private set`.)
  - Collection properties must **not** rely on the backing field being initialized. The original `init => _field.AddRange(value)` accessors threw `NullReferenceException` because the field initializer never ran. They now **assign** a null-tolerant new list: `init => _field = value is null ? [] : [.. value];` and the fields are no longer `readonly`.
  - The `private Hero()` parameterless constructor is **never called** by SoloDB (it uses uninitialized-object construction); it is retained only as a harmless safety net for any future deserializer that does use constructors.
- **SoloDB collection naming (resolved)**: `db.GetCollection<SoloDocument<T>>()` with no name puts **every** entity type in one collection — SoloDB names the collection from the generic wrapper type, and a closed generic's `Type.Name` is `SoloDocument`1` for every `T`. This made `GET /reference/*` return `User`/`Hero` documents coerced onto the reference type. Fixed by routing all services through `SoloCollections.Of<T>` (explicit `typeof(T).Name` collection name). `SoloCollectionIsolationTests` guards it; the original single-type round-trip tests could not catch a cross-type collision because they only stored one type per database.
- **In-memory filtering**: All SoloDB operations load the full collection then filter in-memory. This is fine for small datasets (TTRPG character sheets) but would not scale.
- **Auth is enforced globally**: A fallback `AuthorizationPolicy` of `RequireAuthenticatedUser()` is configured, so every endpoint requires a valid JWT unless it explicitly calls `AllowAnonymous()`. Only `CreateUserEndpoint` and `LoginEndpoint` are anonymous.
- **Login is not secure**: `LoginEndpoint` authenticates by name match only — no password. This is intentional for the POC.
- **No Swagger/OpenAPI**: Not yet added; add `FastEndpoints.Swagger` package if needed.
- **SQLite advisory NU1903 / CVE-2025-6965 (suppressed, accepted)**: SoloDB 1.2.x transitively pins the **SQLitePCLRaw 2.1.11** family, whose native SQLite (< 3.50.2) carries a high-severity flaw (`GHSA-2m69-gcr7-jv3q`). It is **not fixable today**: the only patched engine ships in the SQLitePCLRaw **3.x** family, and forcing it breaks SoloDB's connection-pool transaction logic at runtime (`cannot start a transaction within a transaction`, 8 SoloDB tests fail). Upstream is stuck too — SoloDB 1.2.3 and Microsoft.Data.Sqlite 10.0.9 **both still pin 2.1.11**. The flaw requires executing **crafted SQL**, which this app never does (SoloDB generates all SQL over a fixed schema; the API surface is JSON-only and filters via in-memory predicates), so it is **unreachable** in this threat model. Risk accepted: `NU1903` is suppressed via a scoped `<NoWarn>` in the three affected projects (`NS.SoloDB`, `NS.Tests`, `NS.WebApp`) — full rationale in `NS.SoloDB.csproj`. The suppression only silences the build warning; `dotnet list package --vulnerable --include-transitive` still reports it. **Revert the suppression and bump once SoloDB ships a patched (SQLitePCLRaw 3.x / SQLite ≥ 3.50.2) graph.**
