# Hero Mutation Validation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Reject negative/invalid amounts on the hero play-mutation endpoints — via FastEndpoints request validators (400 + message) at the API boundary and `ArgumentOutOfRangeException` guards in the `Hero` domain methods (defense-in-depth).

**Architecture:** Two enforcement layers. API validators (`Validator<TRequest>`, co-located in each endpoint file like `HeroBuildValidator`) enforce the UX-level bound and short-circuit to a 400 before `HandleAsync`. Domain guards (`ArgumentOutOfRangeException.ThrowIfNegative`) at the top of five `Hero` methods enforce the corruption invariant (no negatives) for any caller. Existing clamping stays.

**Tech Stack:** C# 14 / .NET 10, FastEndpoints 8.x, FluentValidation, xUnit. Spec: `docs/superpowers/specs/2026-06-14-mutation-validation-design.md`.

**Project conventions:** `sealed` classes, XML docs on public types/members, explicit access modifiers, `var` for locals, braces always, no per-file `using` directives (use `_GlobalUsings.cs`). Tests come AFTER implementation (Tasks 3–4), not TDD. Build with `dotnet build NimbleSheets.slnx`; test with `dotnet test` — both from the repo root `C:\Development\repos\GitHub\nimble-sheet`. `FluentValidation` is already a global using in NS.FastEndpoints.

---

## File Structure

**Modify:**
- `NS.Domain/Heroes/Hero.cs` — guard clauses in `TakeDamage`, `Heal`, `GrantTempHp`, `SpendMana`, `SpendHitDice`.
- `NS.FastEndpoints/Heroes/{TakeDamage,Heal,GrantTempHp,SpendMana,SpendHitDice}Endpoint.cs` — one `Validator<TRequest>` each, appended after the request record.
- `NS.Tests/NS.Tests.csproj` — add a project reference to NS.FastEndpoints (for validator tests).
- `NS.Tests/_GlobalUsings.cs` — add `global using NSFastEndpoints;`.
- `NS.Tests/HeroTests.cs` — domain guard tests.

**Create:**
- `NS.Tests/MutationValidationTests.cs` — validator unit tests.

---

### Task 1: Domain guards

**Files:**
- Modify: `NS.Domain/Heroes/Hero.cs`

The five methods currently clamp but don't reject negatives. Add `ArgumentOutOfRangeException.ThrowIfNegative(...)` as the first statement of each. Two methods (`GrantTempHp`, `Heal`) are expression-bodied and become block-bodied.

- [ ] **Step 1: Guard `GrantTempHp`**

Replace:
```csharp
    /// <summary>Grants temporary hit points. Temp HP does not stack; the greater of the current and granted values is kept.</summary>
    public void GrantTempHp(int amount) => TempHp = Math.Max(TempHp, amount);
```
with:
```csharp
    /// <summary>Grants temporary hit points. Temp HP does not stack; the greater of the current and granted values is kept.</summary>
    public void GrantTempHp(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        TempHp = Math.Max(TempHp, amount);
    }
```

- [ ] **Step 2: Guard `Heal`**

Replace:
```csharp
    /// <summary>Restores the specified amount of hit points, up to the hero's maximum.</summary>
    public void Heal(int amount) => CurrentHp = Math.Min(CurrentHp + amount, MaxHp);
```
with:
```csharp
    /// <summary>Restores the specified amount of hit points, up to the hero's maximum.</summary>
    public void Heal(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        CurrentHp = Math.Min(CurrentHp + amount, MaxHp);
    }
```

- [ ] **Step 3: Guard `SpendHitDice`**

Replace:
```csharp
    public void SpendHitDice(int count, int healingAmount)
    {
        HitDiceAvailable = Math.Max(HitDiceAvailable - count, 0);
        Heal(healingAmount);
    }
```
with:
```csharp
    public void SpendHitDice(int count, int healingAmount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        ArgumentOutOfRangeException.ThrowIfNegative(healingAmount);
        HitDiceAvailable = Math.Max(HitDiceAvailable - count, 0);
        Heal(healingAmount);
    }
```

- [ ] **Step 4: Guard `SpendMana`**

Replace:
```csharp
    public void SpendMana(int amount)
    {
        if (CurrentMana.HasValue)
            CurrentMana = Math.Max(CurrentMana.Value - amount, 0);
    }
```
with:
```csharp
    public void SpendMana(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        if (CurrentMana.HasValue)
            CurrentMana = Math.Max(CurrentMana.Value - amount, 0);
    }
```

- [ ] **Step 5: Guard `TakeDamage`**

Replace:
```csharp
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
with:
```csharp
    public void TakeDamage(int amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(amount);
        if (TempHp > 0)
        {
            var absorbed = Math.Min(TempHp, amount);
            TempHp -= absorbed;
            amount -= absorbed;
        }
        CurrentHp = Math.Max(CurrentHp - amount, 0);
    }
```

- [ ] **Step 6: Build**

Run from `C:\Development\repos\GitHub\nimble-sheet`: `dotnet build NimbleSheets.slnx`
Expected: `Build succeeded`, 0 errors, 0 warnings.

- [ ] **Step 7: Commit**

```bash
git add NS.Domain/Heroes/Hero.cs
git commit -m "feat(domain): reject negative amounts in hero mutation methods"
```

---

### Task 2: Request validators

**Files:**
- Modify: `NS.FastEndpoints/Heroes/TakeDamageEndpoint.cs`
- Modify: `NS.FastEndpoints/Heroes/HealEndpoint.cs`
- Modify: `NS.FastEndpoints/Heroes/GrantTempHpEndpoint.cs`
- Modify: `NS.FastEndpoints/Heroes/SpendManaEndpoint.cs`
- Modify: `NS.FastEndpoints/Heroes/SpendHitDiceEndpoint.cs`

Append a validator class at the END of each file (after the request record). `Validator<T>` and `RuleFor` come from FluentValidation (already a global using). Validate only the numeric field(s) — never `HeroId` (route-bound).

- [ ] **Step 1: `TakeDamageEndpoint.cs`** — append:

```csharp

/// <summary>Validates <see cref="TakeDamageRequest"/>.</summary>
public sealed class TakeDamageValidator : Validator<TakeDamageRequest>
{
    /// <summary>Initializes validation rules for applying damage.</summary>
    public TakeDamageValidator()
    {
        RuleFor(r => r.Amount).GreaterThan(0);
    }
}
```

- [ ] **Step 2: `HealEndpoint.cs`** — append:

```csharp

/// <summary>Validates <see cref="HealRequest"/>.</summary>
public sealed class HealValidator : Validator<HealRequest>
{
    /// <summary>Initializes validation rules for healing.</summary>
    public HealValidator()
    {
        RuleFor(r => r.Amount).GreaterThan(0);
    }
}
```

- [ ] **Step 3: `GrantTempHpEndpoint.cs`** — append:

```csharp

/// <summary>Validates <see cref="GrantTempHpRequest"/>.</summary>
public sealed class GrantTempHpValidator : Validator<GrantTempHpRequest>
{
    /// <summary>Initializes validation rules for granting temporary hit points.</summary>
    public GrantTempHpValidator()
    {
        RuleFor(r => r.Amount).GreaterThanOrEqualTo(0);
    }
}
```

- [ ] **Step 4: `SpendManaEndpoint.cs`** — append:

```csharp

/// <summary>Validates <see cref="SpendManaRequest"/>.</summary>
public sealed class SpendManaValidator : Validator<SpendManaRequest>
{
    /// <summary>Initializes validation rules for spending mana.</summary>
    public SpendManaValidator()
    {
        RuleFor(r => r.Amount).GreaterThan(0);
    }
}
```

- [ ] **Step 5: `SpendHitDiceEndpoint.cs`** — append:

```csharp

/// <summary>Validates <see cref="SpendHitDiceRequest"/>.</summary>
public sealed class SpendHitDiceValidator : Validator<SpendHitDiceRequest>
{
    /// <summary>Initializes validation rules for spending hit dice.</summary>
    public SpendHitDiceValidator()
    {
        RuleFor(r => r.Count).GreaterThan(0);
        RuleFor(r => r.HealingAmount).GreaterThanOrEqualTo(0);
    }
}
```

- [ ] **Step 6: Build**

Run: `dotnet build NimbleSheets.slnx`
Expected: `Build succeeded`, 0 errors, 0 warnings.

- [ ] **Step 7: Commit**

```bash
git add NS.FastEndpoints/Heroes/TakeDamageEndpoint.cs NS.FastEndpoints/Heroes/HealEndpoint.cs NS.FastEndpoints/Heroes/GrantTempHpEndpoint.cs NS.FastEndpoints/Heroes/SpendManaEndpoint.cs NS.FastEndpoints/Heroes/SpendHitDiceEndpoint.cs
git commit -m "feat(api): add validators rejecting invalid mutation amounts"
```

---

### Task 3: Domain guard tests (tests-after)

**Files:**
- Modify: `NS.Tests/HeroTests.cs`

- [ ] **Step 1: Append the guard tests**

Add these `[Fact]` methods inside the `HeroTests` class in `NS.Tests/HeroTests.cs` (before the closing brace). `TestHero.Create()` makes a level-1 Oathsworn with `MaxHitDice`/`HitDiceAvailable` = 1 and `MaxMana` null; the negative guards run before any `HasValue` check, so they throw even for a non-caster.

```csharp
    /// <summary>Negative damage is rejected rather than silently healing the hero.</summary>
    [Fact]
    public void TakeDamage_WhenAmountNegative_Throws()
    {
        var hero = TestHero.Create();

        Assert.Throws<ArgumentOutOfRangeException>(() => hero.TakeDamage(-1));
    }

    /// <summary>Negative healing is rejected.</summary>
    [Fact]
    public void Heal_WhenAmountNegative_Throws()
    {
        var hero = TestHero.Create();

        Assert.Throws<ArgumentOutOfRangeException>(() => hero.Heal(-1));
    }

    /// <summary>Negative temporary hit points are rejected.</summary>
    [Fact]
    public void GrantTempHp_WhenAmountNegative_Throws()
    {
        var hero = TestHero.Create();

        Assert.Throws<ArgumentOutOfRangeException>(() => hero.GrantTempHp(-1));
    }

    /// <summary>Granting zero temporary hit points is allowed (a no-op).</summary>
    [Fact]
    public void GrantTempHp_WhenAmountZero_DoesNotThrow()
    {
        var hero = TestHero.Create();

        hero.GrantTempHp(0);

        Assert.Equal(0, hero.TempHp);
    }

    /// <summary>Spending negative mana is rejected.</summary>
    [Fact]
    public void SpendMana_WhenAmountNegative_Throws()
    {
        var hero = TestHero.Create();

        Assert.Throws<ArgumentOutOfRangeException>(() => hero.SpendMana(-1));
    }

    /// <summary>A negative hit-dice count is rejected.</summary>
    [Fact]
    public void SpendHitDice_WhenCountNegative_Throws()
    {
        var hero = TestHero.Create();

        Assert.Throws<ArgumentOutOfRangeException>(() => hero.SpendHitDice(-1, 5));
    }

    /// <summary>A negative healing amount on a hit-dice spend is rejected.</summary>
    [Fact]
    public void SpendHitDice_WhenHealingNegative_Throws()
    {
        var hero = TestHero.Create();

        Assert.Throws<ArgumentOutOfRangeException>(() => hero.SpendHitDice(1, -1));
    }

    /// <summary>Spending hit dice with zero healing is allowed (a no-op heal).</summary>
    [Fact]
    public void SpendHitDice_WhenHealingZero_DoesNotThrow()
    {
        var hero = TestHero.Create();

        hero.SpendHitDice(1, 0);

        Assert.Equal(0, hero.HitDiceAvailable);
    }
```

- [ ] **Step 2: Run the tests**

Run from `C:\Development\repos\GitHub\nimble-sheet`: `dotnet test`
Expected: all tests pass, including the 8 new guard tests.

- [ ] **Step 3: Commit**

```bash
git add NS.Tests/HeroTests.cs
git commit -m "test(domain): cover negative-amount guards"
```

---

### Task 4: Validator tests (tests-after)

**Files:**
- Modify: `NS.Tests/NS.Tests.csproj`
- Modify: `NS.Tests/_GlobalUsings.cs`
- Create: `NS.Tests/MutationValidationTests.cs`

The validators live in NS.FastEndpoints, which NS.Tests does not yet reference. Add the project reference and global using first.

- [ ] **Step 1: Reference NS.FastEndpoints from NS.Tests**

In `NS.Tests/NS.Tests.csproj`, add the reference inside the existing `<ItemGroup>` that holds the other `<ProjectReference>` lines:

```xml
    <ProjectReference Include="..\NS.Domain\NS.Domain.csproj" />
    <ProjectReference Include="..\NS.FastEndpoints\NS.FastEndpoints.csproj" />
    <ProjectReference Include="..\NS.SoloDB\NS.SoloDB.csproj" />
```

(The FastEndpoints package's ASP.NET Core framework dependency flows transitively through this reference, so no extra `<FrameworkReference>` should be needed. If `dotnet test` in Step 4 fails with an error about the `Microsoft.AspNetCore.App` shared framework, add `<FrameworkReference Include="Microsoft.AspNetCore.App" />` to NS.Tests.csproj's first `<ItemGroup>` and rebuild.)

- [ ] **Step 2: Add the global using**

In `NS.Tests/_GlobalUsings.cs`, add `global using NSFastEndpoints;` (keep the existing lines):

```csharp
global using NS.Domain;
global using NSFastEndpoints;
global using NSSoloDB;
global using SoloDatabase;
global using Xunit;
```

- [ ] **Step 3: Create the validator tests**

`NS.Tests/MutationValidationTests.cs`. Each validator is instantiated directly and `Validate(...)` returns a result whose `IsValid` is asserted — no HTTP harness. The request records and validators resolve via the `global using NSFastEndpoints;`.

```csharp
namespace NS.Tests;

/// <summary>Unit tests for the hero play-mutation request validators.</summary>
public sealed class MutationValidationTests
{
    private static readonly Guid HeroId = Guid.CreateVersion7();

    /// <summary>Take-damage rejects a non-positive amount.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void TakeDamage_RejectsNonPositiveAmount(int amount) =>
        Assert.False(new TakeDamageValidator().Validate(new TakeDamageRequest(HeroId, amount)).IsValid);

    /// <summary>Take-damage accepts a positive amount.</summary>
    [Fact]
    public void TakeDamage_AcceptsPositiveAmount() =>
        Assert.True(new TakeDamageValidator().Validate(new TakeDamageRequest(HeroId, 1)).IsValid);

    /// <summary>Heal rejects a non-positive amount.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Heal_RejectsNonPositiveAmount(int amount) =>
        Assert.False(new HealValidator().Validate(new HealRequest(HeroId, amount)).IsValid);

    /// <summary>Spend-mana rejects a non-positive amount.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void SpendMana_RejectsNonPositiveAmount(int amount) =>
        Assert.False(new SpendManaValidator().Validate(new SpendManaRequest(HeroId, amount)).IsValid);

    /// <summary>Grant-temp-hp allows zero but rejects a negative amount.</summary>
    [Fact]
    public void GrantTempHp_AllowsZero_RejectsNegative()
    {
        Assert.True(new GrantTempHpValidator().Validate(new GrantTempHpRequest(HeroId, 0)).IsValid);
        Assert.False(new GrantTempHpValidator().Validate(new GrantTempHpRequest(HeroId, -1)).IsValid);
    }

    /// <summary>Spend-hit-dice requires a positive count and a non-negative healing amount.</summary>
    [Fact]
    public void SpendHitDice_ValidatesCountAndHealing()
    {
        Assert.True(new SpendHitDiceValidator().Validate(new SpendHitDiceRequest(HeroId, 1, 0)).IsValid);
        Assert.False(new SpendHitDiceValidator().Validate(new SpendHitDiceRequest(HeroId, 0, 0)).IsValid);
        Assert.False(new SpendHitDiceValidator().Validate(new SpendHitDiceRequest(HeroId, 1, -1)).IsValid);
    }
}
```

- [ ] **Step 4: Run the tests**

Run: `dotnet test`
Expected: all tests pass, including the new validator tests.

- [ ] **Step 5: Commit**

```bash
git add NS.Tests/NS.Tests.csproj NS.Tests/_GlobalUsings.cs NS.Tests/MutationValidationTests.cs
git commit -m "test(api): cover mutation request validators"
```

---

### Task 5: Full verification + HTTP smoke

**Files:** none.

- [ ] **Step 1: Build + test the whole solution**

Run from `C:\Development\repos\GitHub\nimble-sheet`:
```bash
dotnet build NimbleSheets.slnx
dotnet test
```
Expected: `Build succeeded`; all tests pass.

- [ ] **Step 2: HTTP smoke of the validation path (recommended)**

Confirm a negative amount now returns 400 instead of silently healing.

1. From `NS.WebApp/`: `dotnet run --launch-profile http` (API on `http://localhost:5197`).
2. Create a user + log in (capture `TOKEN`); create a hero via `POST /heroes` (minimal `HeroBuildRequest`; `ancestryId` may be `a0000000-0000-0000-0000-000000000001`). Capture hero id `H`.
3. `POST /heroes/H/take-damage` with `{"amount":-5}` → expect **400** (not 204).
4. `POST /heroes/H/take-damage` with `{"amount":5}` → expect 204.
5. `POST /heroes/H/spend-hit-dice` with `{"count":0,"healingAmount":0}` → expect 400; `{"count":1,"healingAmount":0}` → expect 204.
6. Stop the server.

Record any deviation.

- [ ] **Step 3: Final commit (only if verification fixups were needed)**

```bash
git add -A
git commit -m "chore: verification fixups for mutation validation"
```

---

## Notes for the implementer

- **Validators are auto-discovered.** FastEndpoints scans the endpoints assembly (already configured in NS.WebApp), and a co-located `Validator<TRequest>` is picked up automatically — no registration. A failing validator returns a 400 before `HandleAsync`, so the endpoint handlers are not touched.
- **Two intentionally-different bounds.** The API forbids no-op zeros where meaningless (damage/heal/mana/count `GreaterThan(0)`); the domain only forbids negatives (`ThrowIfNegative`). `grant-temp-hp` and `HealingAmount` allow 0 in both layers.
- **Guard placement:** `ThrowIfNegative` must be the FIRST statement so the throw precedes any clamping or `HasValue` short-circuit (e.g. `SpendMana` on a non-caster still throws on a negative).
- **`ThrowIfNegative`** is a static `ArgumentOutOfRangeException` helper available on .NET 8+ (this project is .NET 10); it uses the caller-argument-expression to name the parameter in the message.
