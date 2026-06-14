# Reference-Data Seeding Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Seed a curated starter set of reference data into SoloDB at application startup so the live `/heroes/[id]` sheet can resolve real reference names end-to-end.

**Architecture:** A dedicated seeder lives entirely in NS.SoloDB (Approach A), preserving the GET-only reference contract. `SeedData` holds curated positional-record rows with fixed GUIDs; `SoloReferenceDataSeeder` inserts each reference type's rows **only when that collection is empty** (idempotent). `NS.WebApp/Program.cs` invokes `SeedAsync()` once after `app.Build()` and before `app.Run()`.

**Tech Stack:** C# 14 / .NET 10, SoloDB, FastEndpoints (host), xUnit (tests). Spec: `docs/superpowers/specs/2026-06-14-reference-data-seeding-design.md`.

**Project conventions:** `sealed` classes, positional records, alphabetical member ordering, XML docs on public types/members, no per-file `using` directives (use `_GlobalUsings.cs`), `var` for locals, explicit access modifiers, braces always. **Reference seed identity uses fixed hand-written GUIDs — never `Guid.CreateVersion7()`** (so heroes can reference rows by a known id and re-seeding is deterministic). Tests come AFTER implementation (Task 5), not TDD. Build with `dotnet build NimbleSheets.slnx`; test with `dotnet test` — both from the repo root `C:\Development\repos\GitHub\nimble-sheet`.

---

## File Structure

**Create:**
- `NS.SoloDB/SeedData.cs` — internal static class; fixed-GUID positional-record rows, one `IReadOnlyList<T>` per reference type.
- `NS.SoloDB/IReferenceDataSeeder.cs` — public seeder interface.
- `NS.SoloDB/SoloReferenceDataSeeder.cs` — public sealed implementation (per-collection seed-when-empty).
- `NS.Tests/SeedingTests.cs` — xUnit tests (populates all collections, idempotent, known id resolves).

**Modify:**
- `NS.SoloDB/ServiceCollectionExtensions.cs` — register `IReferenceDataSeeder`.
- `NS.WebApp/Program.cs` — invoke `SeedAsync()` at startup.

No `_GlobalUsings.cs` changes are required: NS.SoloDB already globally uses `NS.Domain`, `SoloDatabase`, and `Microsoft.Extensions.DependencyInjection`; NS.WebApp already globally uses `NSSoloDB` and the Web SDK implicitly imports `Microsoft.Extensions.DependencyInjection` (for `GetRequiredService`).

---

### Task 1: SeedData — curated rows

**Files:**
- Create: `NS.SoloDB/SeedData.cs`

- [ ] **Step 1: Create the seed data file**

Create `NS.SoloDB/SeedData.cs` with exactly this content. Each record is constructed with target-typed `new(...)` in the positional order defined by the domain records (see CLAUDE.md). The six rows referenced by the client `caldra.ts` fixture reuse the fixture's exact GUIDs (Human `a0…01`; Rusty Mail `c0…01`; Wooden Buckler `c0…02`; Mace `b0…01`; Radiant Judgment `d0…01`; Lay on Hands `d0…02`).

```csharp
namespace NSSoloDB;

/// <summary>The curated starter set of reference data inserted by the seeder.</summary>
/// <remarks>
/// GUIDs are fixed and hand-written (never <see cref="System.Guid.CreateVersion7"/>) so heroes can
/// reference rows by a known id and seeding stays deterministic across restarts. The rows that the
/// client <c>caldra.ts</c> fixture references reuse that fixture's exact GUIDs.
/// </remarks>
internal static class SeedData
{
    /// <summary>Common combat/movement actions.</summary>
    internal static IReadOnlyList<ActionReference> Actions { get; } =
    [
        new(ActionType.Heroic, 1, "Make a weapon or spell attack.", null,
            new Guid("ac000000-0000-0000-0000-000000000001"), "Attack"),
        new(ActionType.Free, 0, "Move up to your speed.", null,
            new Guid("ac000000-0000-0000-0000-000000000002"), "Dash"),
        new(ActionType.Reaction, 1, "Impose disadvantage on an attack against you.", "Once per round",
            new Guid("ac000000-0000-0000-0000-000000000003"), "Defend"),
    ];

    /// <summary>Playable ancestries.</summary>
    internal static IReadOnlyList<Ancestry> Ancestries { get; } =
    [
        new("Versatile and ambitious.", new Guid("a0000000-0000-0000-0000-000000000001"), "Human", ["Adaptable"]),
        new("Graceful and long-lived.", new Guid("a0000000-0000-0000-0000-000000000002"), "Elf", ["Keen Senses", "Trance"]),
        new("Stout and steadfast.", new Guid("a0000000-0000-0000-0000-000000000003"), "Dwarf", ["Darkvision", "Stonecunning"]),
    ];

    /// <summary>Wearable armor and shields.</summary>
    internal static IReadOnlyList<Armor> Armor { get; } =
    [
        new(ArmorType.Mail, 6, "6 + DEX armor.", new Guid("c0000000-0000-0000-0000-000000000001"), "Rusty Mail"),
        new(ArmorType.Shield, 2, "+2 armor.", new Guid("c0000000-0000-0000-0000-000000000002"), "Wooden Buckler"),
        new(ArmorType.Cloth, 3, "3 + DEX armor.", new Guid("c0000000-0000-0000-0000-000000000003"), "Padded Cloth"),
        new(ArmorType.Leather, 4, "4 + DEX armor.", new Guid("c0000000-0000-0000-0000-000000000004"), "Leather Jerkin"),
    ];

    /// <summary>Character backgrounds.</summary>
    internal static IReadOnlyList<Background> Backgrounds { get; } =
    [
        new("Raised in a temple.", "Advantage on Insight checks about religion.",
            new Guid("ba000000-0000-0000-0000-000000000001"), "Acolyte"),
        new("Trained in a fighting company.", "Proficiency with martial drills.",
            new Guid("ba000000-0000-0000-0000-000000000002"), "Soldier"),
    ];

    /// <summary>Status conditions.</summary>
    internal static IReadOnlyList<Condition> Conditions { get; } =
    [
        new("You are lying down; melee attacks against you have advantage.",
            new Guid("f0000000-0000-0000-0000-000000000001"), "Prone"),
        new("You take 1d4 damage at the start of each of your turns until healed.",
            new Guid("f0000000-0000-0000-0000-000000000002"), "Bleeding"),
        new("You cannot take Reactions and your speed is halved.",
            new Guid("f0000000-0000-0000-0000-000000000003"), "Dazed"),
    ];

    /// <summary>Class features.</summary>
    internal static IReadOnlyList<Feature> Features { get; } =
    [
        new(HeroClass.Oathsworn,
            "When an enemy attacks you, if you have no Judgment Dice, roll your Judgment Dice (2d6). On your next melee hit this encounter, deal that much additional radiant damage.",
            null, new Guid("d0000000-0000-0000-0000-000000000001"), 1, "Radiant Judgment", null, null),
        new(HeroClass.Oathsworn,
            "A magical pool of healing power equal to 5 x LVL. Action: touch a target and spend any amount to restore that many HP.",
            null, new Guid("d0000000-0000-0000-0000-000000000002"), 1, "Lay on Hands", null, null),
        new(HeroClass.Mage,
            "Once per day on a Field Rest, recover mana equal to your level.",
            "Once per day", new Guid("d0000000-0000-0000-0000-000000000003"), 1, "Arcane Recovery", null, null),
    ];

    /// <summary>Magic items.</summary>
    internal static IReadOnlyList<MagicItem> MagicItems { get; } =
    [
        new(new Guid("e0000000-0000-0000-0000-000000000001"), "A slender wand humming with heat.",
            "Cast Firebolt without spending mana.", new Guid("da000000-0000-0000-0000-000000000001"),
            3, "Wand of Firebolt", "Uncommon"),
        new(null, "A traveler's cloak.", "+1 to saving throws while worn.",
            new Guid("da000000-0000-0000-0000-000000000002"), null, "Cloak of Resolve", "Common"),
    ];

    /// <summary>Rules references across categories.</summary>
    internal static IReadOnlyList<RuleReference> Rules { get; } =
    [
        new(RuleCategory.Combat, "At 0 HP you are Dying; make death saves until stabilized or healed.",
            new Guid("ce000000-0000-0000-0000-000000000001"), "Dying"),
        new(RuleCategory.Resting, "A short rest that recovers Hit Dice and some resources.",
            new Guid("ce000000-0000-0000-0000-000000000002"), "Field Rest"),
        new(RuleCategory.Conditions, "Each Wound is permanent until removed; 6 Wounds means death.",
            new Guid("ce000000-0000-0000-0000-000000000003"), "Wounds"),
    ];

    /// <summary>Spells, one Mage spell per school.</summary>
    internal static IReadOnlyList<Spell> Spells { get; } =
    [
        new(1, null, "1d8", DamageType.Fire, "Hurl a bolt of fire at one target.", null,
            new Guid("e0000000-0000-0000-0000-000000000001"), false, false, 1, "Firebolt", 12,
            StatType.Dexterity, SpellSchool.Fire, 1, "Deal +1d8 per extra mana."),
        new(1, null, "1d6", DamageType.Cold, "A shard of ice slows and chills a target.", null,
            new Guid("e0000000-0000-0000-0000-000000000002"), false, false, 1, "Frost Shard", 10,
            StatType.Strength, SpellSchool.Ice, 1, null),
        new(1, "10-ft line", "1d6", DamageType.Lightning, "A line of crackling lightning.", null,
            new Guid("e0000000-0000-0000-0000-000000000003"), false, false, 2, "Spark", 8,
            StatType.Dexterity, SpellSchool.Lightning, 1, null),
        new(1, null, "1d8", DamageType.Radiant, "A searing beam of holy light.", null,
            new Guid("e0000000-0000-0000-0000-000000000004"), false, false, 2, "Radiant Beam", 12,
            StatType.Will, SpellSchool.Radiant, 1, null),
    ];

    /// <summary>Weapons.</summary>
    internal static IReadOnlyList<Weapon> Weapons { get; } =
    [
        new("1d6+2", DamageType.Bludgeoning, "A simple bludgeoning weapon.",
            new Guid("b0000000-0000-0000-0000-000000000001"), false, false, "Mace", null, 1, null, StatType.Strength),
        new("1d6", DamageType.Piercing, "A light ranged bow.",
            new Guid("b0000000-0000-0000-0000-000000000002"), false, true, "Shortbow", 12, 0, "Ranged.", StatType.Dexterity),
        new("1d12", DamageType.Slashing, "A massive two-handed blade.",
            new Guid("b0000000-0000-0000-0000-000000000003"), false, true, "Greatsword", null, 1, null, StatType.Strength),
    ];
}
```

- [ ] **Step 2: Build to verify the literals compile**

Run from `C:\Development\repos\GitHub\nimble-sheet`: `dotnet build NimbleSheets.slnx`
Expected: `Build succeeded`. (A compile error here means a positional-record argument is out of order or an enum value is wrong — fix against the record signatures in CLAUDE.md / `NS.Domain`.)

- [ ] **Step 3: Commit**

```bash
git add NS.SoloDB/SeedData.cs
git commit -m "feat(seed): add curated reference seed data"
```

---

### Task 2: Seeder interface + implementation

**Files:**
- Create: `NS.SoloDB/IReferenceDataSeeder.cs`
- Create: `NS.SoloDB/SoloReferenceDataSeeder.cs`

- [ ] **Step 1: Create the interface**

Create `NS.SoloDB/IReferenceDataSeeder.cs`:

```csharp
namespace NSSoloDB;

/// <summary>Populates reference collections with the curated starter data set.</summary>
public interface IReferenceDataSeeder
{
    /// <summary>Seeds each reference collection that is currently empty. Idempotent across restarts.</summary>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    Task SeedAsync(CancellationToken cancellationToken = default);
}
```

- [ ] **Step 2: Create the implementation**

Create `NS.SoloDB/SoloReferenceDataSeeder.cs`. `SeedAsync` does synchronous SoloDB work and returns a completed task, matching the existing services (`SoloHeroDataService` etc.) — SoloDB operations are synchronous, so an `async` method here would raise CS1998. The private generic helper reads the collection via the internal `SoloCollections.Of<T>` and inserts only when empty (using `.ToList().Count`, the same materialization the other services use).

```csharp
namespace NSSoloDB;

/// <summary>SoloDB-backed implementation of <see cref="IReferenceDataSeeder"/>.</summary>
public sealed class SoloReferenceDataSeeder : IReferenceDataSeeder
{
    private readonly SoloDB _db;

    /// <summary>Initializes the seeder with the provided SoloDB instance.</summary>
    public SoloReferenceDataSeeder(SoloDB db) => _db = db;

    /// <inheritdoc/>
    public Task SeedAsync(CancellationToken cancellationToken = default)
    {
        SeedIfEmpty(SeedData.Actions);
        SeedIfEmpty(SeedData.Ancestries);
        SeedIfEmpty(SeedData.Armor);
        SeedIfEmpty(SeedData.Backgrounds);
        SeedIfEmpty(SeedData.Conditions);
        SeedIfEmpty(SeedData.Features);
        SeedIfEmpty(SeedData.MagicItems);
        SeedIfEmpty(SeedData.Rules);
        SeedIfEmpty(SeedData.Spells);
        SeedIfEmpty(SeedData.Weapons);
        return Task.CompletedTask;
    }

    private void SeedIfEmpty<T>(IReadOnlyList<T> rows) where T : class
    {
        var collection = SoloCollections.Of<T>(_db);
        if (collection.ToList().Count > 0)
        {
            return;
        }
        foreach (var row in rows)
        {
            collection.Insert(new SoloDocument<T> { Data = row });
        }
    }
}
```

- [ ] **Step 3: Build**

Run: `dotnet build NimbleSheets.slnx`
Expected: `Build succeeded`.

- [ ] **Step 4: Commit**

```bash
git add NS.SoloDB/IReferenceDataSeeder.cs NS.SoloDB/SoloReferenceDataSeeder.cs
git commit -m "feat(seed): add reference-data seeder (seed-when-empty)"
```

---

### Task 3: Register the seeder in DI

**Files:**
- Modify: `NS.SoloDB/ServiceCollectionExtensions.cs`

- [ ] **Step 1: Register the seeder**

In `NS.SoloDB/ServiceCollectionExtensions.cs`, add the seeder registration immediately after the `IUserDataService` line (before the reference-service registrations):

```csharp
        services.AddSingleton<IUserDataService, SoloUserDataService>();
        services.AddSingleton<IReferenceDataSeeder, SoloReferenceDataSeeder>();
```

Leave every other registration unchanged.

- [ ] **Step 2: Build**

Run: `dotnet build NimbleSheets.slnx`
Expected: `Build succeeded`.

- [ ] **Step 3: Commit**

```bash
git add NS.SoloDB/ServiceCollectionExtensions.cs
git commit -m "feat(seed): register reference-data seeder as singleton"
```

---

### Task 4: Invoke the seeder at startup

**Files:**
- Modify: `NS.WebApp/Program.cs`

- [ ] **Step 1: Seed after Build, before the middleware/run**

In `NS.WebApp/Program.cs`, insert the seeding call on the line directly after `var app = builder.Build();` and before `app.UseHttpsRedirection();`:

```csharp
var app = builder.Build();

await app.Services.GetRequiredService<IReferenceDataSeeder>().SeedAsync();

app.UseHttpsRedirection();
```

(`IReferenceDataSeeder` resolves via the existing `global using NSSoloDB;`; `GetRequiredService` resolves via the Web SDK's implicit `Microsoft.Extensions.DependencyInjection` import. Top-level `await` is already valid in this file.)

- [ ] **Step 2: Build**

Run: `dotnet build NimbleSheets.slnx`
Expected: `Build succeeded`. (If `GetRequiredService` does not resolve, add `global using Microsoft.Extensions.DependencyInjection;` to `NS.WebApp/_GlobalUsings.cs` and rebuild.)

- [ ] **Step 3: Commit**

```bash
git add NS.WebApp/Program.cs
git commit -m "feat(seed): seed reference data on application startup"
```

---

### Task 5: Tests (tests-after)

**Files:**
- Create: `NS.Tests/SeedingTests.cs`

- [ ] **Step 1: Write the tests**

Create `NS.Tests/SeedingTests.cs`. The in-memory-DB pattern (`new SoloDB($"memory:...")`) and direct service construction mirror the existing `SoloCollectionIsolationTests`. `NSSoloDB`, `NS.Domain`, `SoloDatabase`, and `Xunit` are already global usings in NS.Tests.

```csharp
namespace NS.Tests;

/// <summary>Tests for the SoloDB reference-data seeder.</summary>
public sealed class SeedingTests
{
    /// <summary>Seeding a fresh database populates every reference collection.</summary>
    [Fact]
    public async Task SeedAsync_PopulatesEveryReferenceCollection()
    {
        using var db = new SoloDB($"memory:seed-{Guid.CreateVersion7()}");

        await new SoloReferenceDataSeeder(db).SeedAsync();

        Assert.NotEmpty(await new SoloReferenceDataService<ActionReference>(db).GetAllAsync());
        Assert.NotEmpty(await new SoloReferenceDataService<Ancestry>(db).GetAllAsync());
        Assert.NotEmpty(await new SoloReferenceDataService<Armor>(db).GetAllAsync());
        Assert.NotEmpty(await new SoloReferenceDataService<Background>(db).GetAllAsync());
        Assert.NotEmpty(await new SoloReferenceDataService<Condition>(db).GetAllAsync());
        Assert.NotEmpty(await new SoloReferenceDataService<Feature>(db).GetAllAsync());
        Assert.NotEmpty(await new SoloReferenceDataService<MagicItem>(db).GetAllAsync());
        Assert.NotEmpty(await new SoloReferenceDataService<RuleReference>(db).GetAllAsync());
        Assert.NotEmpty(await new SoloReferenceDataService<Spell>(db).GetAllAsync());
        Assert.NotEmpty(await new SoloReferenceDataService<Weapon>(db).GetAllAsync());
    }

    /// <summary>Seeding twice does not duplicate rows (the empty-check makes it idempotent).</summary>
    [Fact]
    public async Task SeedAsync_IsIdempotent()
    {
        using var db = new SoloDB($"memory:seed-{Guid.CreateVersion7()}");
        var seeder = new SoloReferenceDataSeeder(db);

        await seeder.SeedAsync();
        var afterFirst = (await new SoloReferenceDataService<Ancestry>(db).GetAllAsync()).Count;
        await seeder.SeedAsync();
        var afterSecond = (await new SoloReferenceDataService<Ancestry>(db).GetAllAsync()).Count;

        Assert.Equal(afterFirst, afterSecond);
    }

    /// <summary>The Human ancestry is seeded under the fixed fixture GUID.</summary>
    [Fact]
    public async Task SeedAsync_SeedsHumanAncestryWithKnownId()
    {
        using var db = new SoloDB($"memory:seed-{Guid.CreateVersion7()}");
        await new SoloReferenceDataSeeder(db).SeedAsync();

        var human = await new SoloReferenceDataService<Ancestry>(db)
            .GetByIdAsync(new Guid("a0000000-0000-0000-0000-000000000001"));

        Assert.NotNull(human);
        Assert.Equal("Human", human!.Name);
    }
}
```

- [ ] **Step 2: Run the tests**

Run from `C:\Development\repos\GitHub\nimble-sheet`: `dotnet test`
Expected: all tests pass — the existing suites plus the 3 new `SeedingTests` (`PopulatesEveryReferenceCollection`, `IsIdempotent`, `SeedsHumanAncestryWithKnownId`).

- [ ] **Step 3: Commit**

```bash
git add NS.Tests/SeedingTests.cs
git commit -m "test(seed): cover seeding population, idempotency, and known id"
```

---

### Task 6: Full verification + HTTP smoke

**Files:** none (verification only).

- [ ] **Step 1: Build + test the whole solution**

Run from `C:\Development\repos\GitHub\nimble-sheet`:
```bash
dotnet build NimbleSheets.slnx
dotnet test
```
Expected: `Build succeeded`; all tests pass.

- [ ] **Step 2: HTTP smoke of the seeded data (recommended)**

Confirm seeding runs at startup and reference data is served. Reference GET endpoints require authentication, so create a user, log in, and call a reference route:

1. Delete any stale dev database so seeding runs on a fresh DB: from `NS.WebApp/`, remove the file named by `SoloDB:DatabasePath` in `appsettings.json` (default `nimble-sheet.db`) if present.
2. From `NS.WebApp/`: `dotnet run --launch-profile http` (API on `http://localhost:5197`).
3. Create a user: `POST http://localhost:5197/users` with `{"name":"SeedSmoke","email":"seed@example.com"}` → 201.
4. Log in: `POST http://localhost:5197/users/login` with `{"name":"SeedSmoke"}` → capture `token` from the response.
5. `GET http://localhost:5197/reference/ancestries` with header `Authorization: Bearer <token>` → expect a JSON array including `"name":"Human"` with `"id":"a0000000-0000-0000-0000-000000000001"`.
6. `GET http://localhost:5197/reference/spells?school=Fire` with the bearer token → expect `"name":"Firebolt"`.
7. Stop the server.

Record any deviation. (Full browser-level visual verification of a resolved hero sheet — creating a Caldra-shaped hero via the build API and viewing `/heroes/[id]` — is now unblocked by this slice and can follow as its own verification pass.)

- [ ] **Step 3: Final commit (only if verification fixups were needed)**

```bash
git add -A
git commit -m "chore(seed): verification fixups"
```

---

## Notes for the implementer

- **Positional-record order is load-bearing.** Every `new(...)` in `SeedData.cs` must match the exact parameter order of the domain record (see CLAUDE.md "Reference Entities"). A misordered argument that happens to type-check (e.g. two adjacent `string` params) will compile but seed wrong data — double-check `Spell` (16 params) and `Weapon` (11 params) especially.
- **Fixed GUIDs only.** Never substitute `Guid.CreateVersion7()` for a seed id. The six Caldra-overlapping rows must keep the exact fixture GUIDs listed in Task 1.
- **Seed-when-empty, per collection.** The seeder never overwrites existing rows; editing seed data later requires a fresh database. This is intentional (see spec).
- **The seeder is public** (like `SoloHeroDataService`/`SoloReferenceDataService`) so `NS.Tests` can construct it directly; `SeedData` stays `internal`.
- **Member ordering / XML docs** follow the project conventions already used across NS.SoloDB.
```
