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
NS.Tests/                      ← xUnit tests (domain unit tests + SoloDB round-trip tests)
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
- **`private Hero()`** parameterless constructor that sets reference-type properties to `null!`; retained as a safety net but **not** used by SoloDB (which rehydrates via uninitialized objects — see Known Caveats)
- Scalar properties use `private set` (including `Id`, so SoloDB can rehydrate it)
- Collection properties expose `IReadOnlyList<T>` over a non-`readonly` `List<T>` field; the `init` accessor **assigns** a null-tolerant new list (`init => _field = value is null ? [] : [.. value];`) so SoloDB can rehydrate them (see Known Caveats)
- `IsDead => CurrentWounds >= 6`; `IsDying => CurrentHp == 0`
- `UserId` links a hero to its owning `User`

**Mutation methods** (alphabetical): `AddArmor`, `AddCondition`, `AddFeature`, `AddGearItem`, `AddMagicItem`, `AddSpell`, `AddWeapon`, `ApplyHpIncrease`, `ApplyStatIncrease`, `CompletePendingChoice`, `FinalizeSkillAllocation`, `GainWound`, `GrantTempHp`, `Heal`, `HealWound`, `LevelUp`, `RecoverAllResources`, `RemoveArmor`, `RemoveCondition`, `RemoveFeature`, `RemoveGearItem`, `RemoveMagicItem`, `RemoveSpell`, `RemoveWeapon`, `SetSubclass`, `SpendHitDice`, `SpendMana`, `TakeDamage`, `UpdateBuild`, `UpdateCombatStats`

- `TempHp` absorbs damage before `CurrentHp` (`TakeDamage`), is non-stacking (`GrantTempHp` keeps the higher value), and is cleared by `RecoverAllResources`.
- `UpdateBuild(...)` overwrites the character-build fields (the `HeroBuildRequest` set) while preserving level, subclass, play state, and collections; `CurrentHp`/`CurrentMana` clamp to lowered maximums.

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
app.UseHttpsRedirection();
app.UseDefaultFiles();      // serve the SPA from wwwroot (same-origin)
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints(c => c.Serializer.Options.Converters.Add(new JsonStringEnumConverter()));
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

### Character Sheet (display-only)

The first feature: a read-only, dark-mode character sheet at route `/sheet`.

- **Data layer** (`src/lib/`): `api/types.ts` mirrors the API DTOs (camelCase; enums as string-union types matching the `JsonStringEnumConverter` names). `fixtures/caldra.ts` is a `Hero` + `ReferenceData` fixture shaped exactly like API responses. `sheet/resolve.ts` is a pure resolver joining the hero's ID-referenced collections to reference data into a `SheetViewModel` (`sheet/viewmodel.ts`); `sheet/format.ts` holds display helpers.
- **Components** (`src/lib/sheet/components/`): `HeroSheet` composes a pinned region (banner, vitals, stats with `SAVE▲/▼` save markers, skills) and a tab switcher (`SheetTabs`) over Combat / Magic / Class Resources / Inventory / Features panels. Always dark — styled with dark-tone Tailwind utilities directly, no `dark:` variants.
- **Tests**: `sheet/resolve.test.ts` (Vitest) covers the resolver; run with `npm test`.
- **Not yet wired**: live API calls (swap the fixture for `fetch()`), the HP damage/heal popover, and other play mutations are deferred to later slices. The sheet's eventual home is `/heroes/[id]` once auth/list exist.

**Stack**:
- **SvelteKit 2.x** on **Svelte 5** (runes mode forced for project code), **Vite 8**, **TypeScript**
- **Pure SPA**: `@sveltejs/adapter-static` with `fallback: 'index.html'`; `src/routes/+layout.ts` sets `ssr = false` and `prerender = false`. Builds to static assets under `build/` (git-ignored) — no Node server at runtime
- **Tailwind CSS v4** via the `@tailwindcss/vite` plugin (no `tailwind.config.js`; config is CSS-first in `src/app.css`)
- **Flowbite Svelte 1.x** (`flowbite-svelte`, `flowbite`, `flowbite-svelte-icons`) as the UI component library
- **Vitest** for unit tests (pure TypeScript logic, e.g. the sheet resolver); configured via `vitest.config.ts` (standalone, no SvelteKit plugin)

**Key files**:
- `src/app.css` — Tailwind entry: `@import 'tailwindcss'`, `@plugin 'flowbite/plugin'`, the `dark` custom variant, and `@source` directives pointing at the Flowbite Svelte `dist` folders so their classes are scanned. Imported once in `src/routes/+layout.svelte`
- `svelte.config.js` — static adapter + SPA fallback
- `vite.config.ts` — registers `tailwindcss()` before `sveltekit()`

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
- **API/SPA route collisions**: API routes are unprefixed (`/heroes`, `/users`, `/reference/...`). A client-side route that exactly matches a real API route (e.g. `/heroes` or `/heroes/{id}`) resolves to the API, not the SPA shell. If/when the SPA needs overlapping paths, prefix the API (e.g. `/api`) or use distinct client routes.

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
