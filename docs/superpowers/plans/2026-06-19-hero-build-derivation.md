# Hero Build: Player-Set Inputs vs. Auto-Derived Attributes — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Rework the hero create/edit view so only genuinely player-set inputs are settable (ancestry, background, class, base ability scores via 27-point point-buy) and everything else (ability modifiers, skills, saves, HP, mana, class resources) is server-derived.

**Architecture:** Approach A — *compute-and-persist at write time*. A pure `HeroDerivation` function computes the derived attributes from class + base scores + ancestry bonuses + level; the create/update endpoints persist them into the hero's existing stored fields. The sheet, resolver, and the (untouched) level-up flow keep reading stored values.

**Tech Stack:** C# 14 / .NET 10, FastEndpoints 8.x, SoloDB; SvelteKit 2 / Svelte 5 (runes), TypeScript, Vitest.

**Spec:** `docs/superpowers/specs/2026-06-19-hero-build-derivation-design.md`

## Global Constraints

- **C#:** `sealed` on every class; positional records with `<param>` XML docs; member ordering (constants → private fields → constructors → properties → methods), alphabetical within group; XML docs on all public types/members; `var` for locals; explicit access modifiers; braces always; `_camelCase` private fields; 3+-letter acronyms Pascal-cased; **no per-file `using`** (use `_GlobalUsings.cs`); GUIDs via `Guid.CreateVersion7()` (never `NewGuid`); FastEndpoints 8.x uses `Send.*` (e.g. `Send.NoContentAsync(ct)`), never `SendAsync`.
- **TS/Svelte:** standard SvelteKit idioms (the C# conventions do **not** apply here); keep `npm run check` at 0 errors / 0 warnings.
- **The four playable classes are exactly:** Cheat, Hunter, Mage, Oathsworn.
- **Point-buy:** scores ∈ [8,15]; costs `{8:0,9:1,10:2,11:3,12:4,13:5,14:7,15:9}`; budget = 27 (under-spend allowed).
- **Modifier** = `floor((finalScore − 10) / 2)`; final = base + ancestry bonus.
- **Tests:** `dotnet test` (from repo root) for C#; `npm test` + `npm run check` (from `NS.Client/`) for the client. Commit after each task.

---

## File Structure

**New (domain):**
- `NS.Domain/Heroes/AbilityScores.cs` — the ability-score value object.
- `NS.Domain/Rules/PointBuy.cs` — point-buy cost/validation.
- `NS.Domain/Rules/ClassDefinition.cs` + `NS.Domain/Rules/ClassDefinitions.cs` — per-class stat blocks.
- `NS.Domain/Rules/DerivedAttributes.cs` + `NS.Domain/Rules/HeroDerivation.cs` — derivation.

**Modified (domain/API/seed):**
- `NS.Domain/Reference/Ancestry.cs` — add `AbilityBonuses`.
- `NS.Domain/Heroes/Hero.cs` — add `BaseAbilityScores`, `Hero.Create`, new `UpdateBuild`; remove the old constructor + old `UpdateBuild`.
- `NS.FastEndpoints/Heroes/HeroBuildRequest.cs` → split into `CreateHeroRequest`/`UpdateHeroRequest` + validators.
- `NS.FastEndpoints/Heroes/CreateHeroEndpoint.cs`, `UpdateHeroEndpoint.cs`.
- `NS.SoloDB/SeedData.cs` — ancestry bonuses (zeros).
- `NS.Tests/TestHero.cs`, `NS.Tests/HeroTests.cs` — adapt to `Hero.Create`/new `UpdateBuild`.

**New (client):**
- `NS.Client/src/lib/sheet/build/pointBuy.ts` (+ `.test.ts`).
- `NS.Client/src/lib/sheet/build/classDefs.ts` (+ `.test.ts`) — class mirror + pure preview helpers.
- `NS.Client/src/lib/sheet/build/AbilityScoresSection.svelte` — replaces `StatsSection`.

**Modified (client):**
- `NS.Client/src/lib/api/types.ts`, `NS.Client/src/lib/api/client.ts` (+ `client.test.ts`).
- `NS.Client/src/lib/sheet/build/model.ts` (+ `model.test.ts`), `validate.ts` (+ `validate.test.ts`), `options.ts`.
- `NS.Client/src/lib/sheet/build/IdentitySection.svelte`, `VitalsSection.svelte`, `HeroBuildForm.svelte`.
- `NS.Client/src/routes/(app)/heroes/new/+page.svelte`, `.../[id]/edit/+page.svelte`.
- `NS.Client/src/lib/fixtures/caldra.ts`.

**Deleted (client):** `CombatSection.svelte`, `SavesSection.svelte`, `SkillsSection.svelte`, `ClassResourcesSection.svelte`.

---

## Task 1: AbilityScores + PointBuy (domain)

**Files:**
- Create: `NS.Domain/Heroes/AbilityScores.cs`, `NS.Domain/Rules/PointBuy.cs`
- Test: `NS.Tests/PointBuyTests.cs`

**Interfaces:**
- Produces: `AbilityScores(int Dexterity, int Intelligence, int Strength, int Will)`; `PointBuy.Budget=27`, `PointBuy.MinScore=8`, `PointBuy.MaxScore=15`, `int PointBuy.CostOf(int)`, `int PointBuy.TotalCost(AbilityScores)`, `bool PointBuy.IsValid(AbilityScores)`.

- [ ] **Step 1: Write the failing test** — `NS.Tests/PointBuyTests.cs`:

```csharp
namespace NS.Tests;

public sealed class PointBuyTests
{
    [Theory]
    [InlineData(8, 0)]
    [InlineData(10, 2)]
    [InlineData(13, 5)]
    [InlineData(14, 7)]
    [InlineData(15, 9)]
    public void CostOf_ReturnsTableCost(int score, int expected)
    {
        Assert.Equal(expected, PointBuy.CostOf(score));
    }

    [Fact]
    public void CostOf_ThrowsForOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PointBuy.CostOf(16));
    }

    [Fact]
    public void TotalCost_SumsAllFourScores()
    {
        // 14(7) + 13(5) + 12(4) + 8(0) = 16
        var scores = new AbilityScores(Dexterity: 14, Intelligence: 13, Strength: 12, Will: 8);
        Assert.Equal(16, PointBuy.TotalCost(scores));
    }

    [Fact]
    public void IsValid_TrueWhenWithinBudgetAndRange()
    {
        // 15(9) + 14(7) + 13(5) + 13(5) = 26 <= 27
        Assert.True(PointBuy.IsValid(new AbilityScores(15, 14, 13, 13)));
    }

    [Fact]
    public void IsValid_FalseWhenOverBudget()
    {
        // 15(9) + 15(9) + 15(9) + 8(0) = 27 ok; bump Will to 9 -> 28 over budget
        Assert.False(PointBuy.IsValid(new AbilityScores(15, 15, 15, 9)));
    }

    [Fact]
    public void IsValid_FalseWhenOutOfRange()
    {
        Assert.False(PointBuy.IsValid(new AbilityScores(7, 10, 10, 10)));
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test --filter "FullyQualifiedName~PointBuyTests"`
Expected: FAIL — `AbilityScores`/`PointBuy` do not exist (compile error).

- [ ] **Step 3: Write `NS.Domain/Heroes/AbilityScores.cs`**

```csharp
namespace NS.Domain;

/// <summary>A set of ability scores or score adjustments, by stat.</summary>
/// <param name="Dexterity">The Dexterity value.</param>
/// <param name="Intelligence">The Intelligence value.</param>
/// <param name="Strength">The Strength value.</param>
/// <param name="Will">The Will value.</param>
public sealed record AbilityScores(int Dexterity, int Intelligence, int Strength, int Will);
```

- [ ] **Step 4: Write `NS.Domain/Rules/PointBuy.cs`**

```csharp
namespace NS.Domain;

/// <summary>Point-buy rules for purchasing a hero's base ability scores at creation.</summary>
public static class PointBuy
{
    /// <summary>The total points available to spend across all ability scores.</summary>
    public const int Budget = 27;

    /// <summary>The maximum purchasable score.</summary>
    public const int MaxScore = 15;

    /// <summary>The minimum (free) score.</summary>
    public const int MinScore = 8;

    private static readonly IReadOnlyDictionary<int, int> _costByScore = new Dictionary<int, int>
    {
        [8] = 0, [9] = 1, [10] = 2, [11] = 3, [12] = 4, [13] = 5, [14] = 7, [15] = 9,
    };

    /// <summary>The point cost of a single score. Throws when the score is outside 8–15.</summary>
    /// <param name="score">The ability score.</param>
    public static int CostOf(int score)
    {
        return _costByScore.TryGetValue(score, out var cost)
            ? cost
            : throw new ArgumentOutOfRangeException(nameof(score), score, "Score must be between 8 and 15.");
    }

    /// <summary>Whether a set of base ability scores is a legal point-buy purchase.</summary>
    /// <param name="scores">The base ability scores.</param>
    public static bool IsValid(AbilityScores scores)
    {
        return InRange(scores.Dexterity) && InRange(scores.Intelligence)
            && InRange(scores.Strength) && InRange(scores.Will)
            && TotalCost(scores) <= Budget;
    }

    /// <summary>The total point cost of a full set of ability scores.</summary>
    /// <param name="scores">The base ability scores.</param>
    public static int TotalCost(AbilityScores scores)
    {
        return CostOf(scores.Dexterity) + CostOf(scores.Intelligence)
            + CostOf(scores.Strength) + CostOf(scores.Will);
    }

    private static bool InRange(int score)
    {
        return score is >= MinScore and <= MaxScore;
    }
}
```

- [ ] **Step 5: Run test to verify it passes**

Run: `dotnet test --filter "FullyQualifiedName~PointBuyTests"`
Expected: PASS (all).

- [ ] **Step 6: Commit**

```bash
git add NS.Domain/Heroes/AbilityScores.cs NS.Domain/Rules/PointBuy.cs NS.Tests/PointBuyTests.cs
git commit -m "feat(domain): add AbilityScores value object and PointBuy rules"
```

---

## Task 2: ClassDefinitions (domain)

**Files:**
- Create: `NS.Domain/Rules/ClassDefinition.cs`, `NS.Domain/Rules/ClassDefinitions.cs`
- Test: `NS.Tests/ClassDefinitionsTests.cs`

**Interfaces:**
- Produces: `ClassDefinition(StatType SaveAdvantage, StatType SaveDisadvantage, int Speed, int StartingHitDie, int StartingHp)` — **note:** `StartingHitDie` is a `DieType`. Final signature is `ClassDefinition(StatType SaveAdvantage, StatType SaveDisadvantage, int Speed, DieType StartingHitDie, int StartingHp)`. `ClassDefinitions.For(HeroClass) → ClassDefinition?`, `ClassDefinitions.IsPlayable(HeroClass) → bool`, `ClassDefinitions.PlayableClasses → IReadOnlyCollection<HeroClass>`.

- [ ] **Step 1: Write the failing test** — `NS.Tests/ClassDefinitionsTests.cs`:

```csharp
namespace NS.Tests;

public sealed class ClassDefinitionsTests
{
    [Fact]
    public void For_Oathsworn_ReturnsRulesStatBlock()
    {
        var def = ClassDefinitions.For(HeroClass.Oathsworn);

        Assert.NotNull(def);
        Assert.Equal(DieType.D10, def!.StartingHitDie);
        Assert.Equal(17, def.StartingHp);
        Assert.Equal(StatType.Strength, def.SaveAdvantage);
        Assert.Equal(StatType.Dexterity, def.SaveDisadvantage);
        Assert.Equal(6, def.Speed);
    }

    [Fact]
    public void For_Mage_HasD6AndIntStrSaves()
    {
        var def = ClassDefinitions.For(HeroClass.Mage);
        Assert.Equal(DieType.D6, def!.StartingHitDie);
        Assert.Equal(StatType.Intelligence, def.SaveAdvantage);
        Assert.Equal(StatType.Strength, def.SaveDisadvantage);
    }

    [Fact]
    public void For_NonQuickstartClass_ReturnsNull()
    {
        Assert.Null(ClassDefinitions.For(HeroClass.Berserker));
    }

    [Fact]
    public void PlayableClasses_AreTheFourQuickstartClasses()
    {
        Assert.Equal(
            new[] { HeroClass.Cheat, HeroClass.Hunter, HeroClass.Mage, HeroClass.Oathsworn }.OrderBy(c => c),
            ClassDefinitions.PlayableClasses.OrderBy(c => c));
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test --filter "FullyQualifiedName~ClassDefinitionsTests"`
Expected: FAIL — types do not exist.

- [ ] **Step 3: Write `NS.Domain/Rules/ClassDefinition.cs`**

```csharp
namespace NS.Domain;

/// <summary>The level-1 stat block for a playable class.</summary>
/// <param name="SaveAdvantage">The stat whose saves are rolled with advantage.</param>
/// <param name="SaveDisadvantage">The stat whose saves are rolled with disadvantage.</param>
/// <param name="Speed">The class's base movement speed in spaces.</param>
/// <param name="StartingHitDie">The class's hit die type.</param>
/// <param name="StartingHp">The class's level-1 starting maximum hit points.</param>
public sealed record ClassDefinition(
    StatType SaveAdvantage,
    StatType SaveDisadvantage,
    int Speed,
    DieType StartingHitDie,
    int StartingHp);
```

- [ ] **Step 4: Write `NS.Domain/Rules/ClassDefinitions.cs`**

```csharp
namespace NS.Domain;

/// <summary>The level-1 stat blocks for the playable (quickstart) classes.</summary>
public static class ClassDefinitions
{
    private static readonly IReadOnlyDictionary<HeroClass, ClassDefinition> _byClass =
        new Dictionary<HeroClass, ClassDefinition>
        {
            [HeroClass.Cheat] = new(StatType.Dexterity, StatType.Will, 6, DieType.D6, 10),
            [HeroClass.Hunter] = new(StatType.Dexterity, StatType.Intelligence, 6, DieType.D8, 13),
            [HeroClass.Mage] = new(StatType.Intelligence, StatType.Strength, 6, DieType.D6, 10),
            [HeroClass.Oathsworn] = new(StatType.Strength, StatType.Dexterity, 6, DieType.D10, 17),
        };

    /// <summary>The classes that can be chosen at hero creation (those with a defined stat block).</summary>
    public static IReadOnlyCollection<HeroClass> PlayableClasses => [.. _byClass.Keys];

    /// <summary>Gets the stat block for a class, or <see langword="null"/> when the class has no definition.</summary>
    /// <param name="heroClass">The class to look up.</param>
    public static ClassDefinition? For(HeroClass heroClass)
    {
        return _byClass.TryGetValue(heroClass, out var definition) ? definition : null;
    }

    /// <summary>Whether a class has a defined stat block (and is therefore playable).</summary>
    /// <param name="heroClass">The class to check.</param>
    public static bool IsPlayable(HeroClass heroClass)
    {
        return _byClass.ContainsKey(heroClass);
    }
}
```

- [ ] **Step 5: Run test to verify it passes**

Run: `dotnet test --filter "FullyQualifiedName~ClassDefinitionsTests"`
Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add NS.Domain/Rules/ClassDefinition.cs NS.Domain/Rules/ClassDefinitions.cs NS.Tests/ClassDefinitionsTests.cs
git commit -m "feat(domain): add ClassDefinitions stat blocks for the four quickstart classes"
```

---

## Task 3: HeroDerivation + DerivedAttributes (domain)

**Files:**
- Create: `NS.Domain/Rules/DerivedAttributes.cs`, `NS.Domain/Rules/HeroDerivation.cs`
- Test: `NS.Tests/HeroDerivationTests.cs`

**Interfaces:**
- Consumes: `AbilityScores`, `ClassDefinitions`, `HeroStats`, `HeroSkills`, `HeroSaves`, `HeroCombatStats`, `ClassResources`.
- Produces: `DerivedAttributes(HeroCombatStats CombatStats, int MaxHp, int? MaxMana, ClassResources Resources, HeroSaves Saves, HeroSkills Skills, HeroStats Stats)`; `HeroDerivation.AbilityModifier(int) → int`; `HeroDerivation.FinalScores(AbilityScores, AbilityScores) → AbilityScores`; `HeroDerivation.Derive(HeroClass, AbilityScores baseScores, AbilityScores ancestryBonuses, int level) → DerivedAttributes`; `HeroDerivation.MaxHpBounds(HeroClass, int level) → (int Min, int Max)`.

- [ ] **Step 1: Write the failing test** — `NS.Tests/HeroDerivationTests.cs`:

```csharp
namespace NS.Tests;

public sealed class HeroDerivationTests
{
    [Theory]
    [InlineData(8, -1)]
    [InlineData(10, 0)]
    [InlineData(11, 0)]
    [InlineData(12, 1)]
    [InlineData(14, 2)]
    [InlineData(15, 2)]
    public void AbilityModifier_FollowsFloorRule(int finalScore, int expected)
    {
        Assert.Equal(expected, HeroDerivation.AbilityModifier(finalScore));
    }

    [Fact]
    public void FinalScores_AddAncestryBonuses()
    {
        var final = HeroDerivation.FinalScores(
            new AbilityScores(12, 10, 14, 8),
            new AbilityScores(0, 2, 0, 1));
        Assert.Equal(new AbilityScores(12, 12, 14, 9), final);
    }

    [Fact]
    public void Derive_Oathsworn_Level1_HasClassHpSavesResourcesAndNoMana()
    {
        var d = HeroDerivation.Derive(
            HeroClass.Oathsworn,
            baseScores: new AbilityScores(10, 10, 14, 12),
            ancestryBonuses: new AbilityScores(0, 0, 0, 0),
            level: 1);

        Assert.Equal(17, d.MaxHp);
        Assert.Null(d.MaxMana);                                   // caster only from level 2
        Assert.Equal(StatType.Strength, d.Saves.AdvantageOn);
        Assert.Equal(StatType.Dexterity, d.Saves.DisadvantageOn);
        Assert.Equal(DieType.D10, d.CombatStats.HitDieType);
        Assert.Equal(0, d.CombatStats.InitiativeBonus);          // DEX 10 -> mod 0
        Assert.Equal(2, d.Stats.Strength);                       // STR 14 -> mod 2
        Assert.Equal(2, d.Skills.Might);                         // Might keyed to STR
        Assert.Equal(1, d.Skills.Influence);                     // Influence keyed to WIL (12 -> mod 1)
        Assert.Equal(2, d.Resources.JudgmentDiceCount);
        Assert.Equal(DieType.D6, d.Resources.JudgmentDiceType);
        Assert.Equal(5, d.Resources.LayOnHandsPool);             // 5 * level
    }

    [Fact]
    public void Derive_Oathsworn_Level3_HasD8JudgmentManaAndScaledLayOnHands()
    {
        var d = HeroDerivation.Derive(
            HeroClass.Oathsworn,
            baseScores: new AbilityScores(10, 10, 10, 14),
            ancestryBonuses: new AbilityScores(0, 0, 0, 0),
            level: 3);

        Assert.Equal(DieType.D8, d.Resources.JudgmentDiceType);
        Assert.Equal(15, d.Resources.LayOnHandsPool);            // 5 * 3
        Assert.Equal(5, d.MaxMana);                              // WIL 14 -> mod 2; 2 + 3
    }

    [Fact]
    public void Derive_Mage_Level1_HasIntMana()
    {
        var d = HeroDerivation.Derive(
            HeroClass.Mage,
            baseScores: new AbilityScores(10, 14, 10, 10),
            ancestryBonuses: new AbilityScores(0, 0, 0, 0),
            level: 1);

        Assert.Equal(7, d.MaxMana);                              // INT 14 -> mod 2; 2*3 + 1
        Assert.Null(d.Resources.JudgmentDiceCount);
    }

    [Fact]
    public void Derive_Hunter_IsNonCaster()
    {
        var d = HeroDerivation.Derive(
            HeroClass.Hunter, new AbilityScores(10, 10, 10, 10), new AbilityScores(0, 0, 0, 0), level: 5);
        Assert.Null(d.MaxMana);
    }

    [Fact]
    public void MaxHpBounds_AreStartingHpToStartingPlusHitDiePerExtraLevel()
    {
        var (min, max) = HeroDerivation.MaxHpBounds(HeroClass.Oathsworn, level: 3);
        Assert.Equal(17, min);
        Assert.Equal(17 + 10 * 2, max);                          // d10 face 10, (3-1) extra levels
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test --filter "FullyQualifiedName~HeroDerivationTests"`
Expected: FAIL — types do not exist.

- [ ] **Step 3: Write `NS.Domain/Rules/DerivedAttributes.cs`**

```csharp
namespace NS.Domain;

/// <summary>The bundle of attributes derived from a hero's class, ability scores, and level.</summary>
/// <param name="CombatStats">The derived combat statistics.</param>
/// <param name="MaxHp">The derived maximum hit points (the create-time / level-1 value).</param>
/// <param name="MaxMana">The derived maximum mana; <see langword="null"/> for non-casters.</param>
/// <param name="Resources">The derived class resource pools.</param>
/// <param name="Saves">The derived advantaged/disadvantaged saves.</param>
/// <param name="Skills">The derived skill bonuses.</param>
/// <param name="Stats">The derived ability modifiers.</param>
public sealed record DerivedAttributes(
    HeroCombatStats CombatStats,
    int MaxHp,
    int? MaxMana,
    ClassResources Resources,
    HeroSaves Saves,
    HeroSkills Skills,
    HeroStats Stats);
```

- [ ] **Step 4: Write `NS.Domain/Rules/HeroDerivation.cs`**

```csharp
namespace NS.Domain;

/// <summary>Computes a hero's derived attributes from its player-set inputs and level.</summary>
public static class HeroDerivation
{
    /// <summary>The ability modifier for a final ability score: floor((score − 10) / 2).</summary>
    /// <param name="finalScore">The final (base + ancestry bonus) ability score.</param>
    public static int AbilityModifier(int finalScore)
    {
        return (int)Math.Floor((finalScore - 10) / 2.0);
    }

    /// <summary>Computes all derived attributes for a hero.</summary>
    /// <param name="heroClass">The hero's class (must be a playable class).</param>
    /// <param name="baseScores">The player-bought base ability scores.</param>
    /// <param name="ancestryBonuses">The hero's ancestry ability bonuses.</param>
    /// <param name="level">The hero's current level.</param>
    public static DerivedAttributes Derive(
        HeroClass heroClass, AbilityScores baseScores, AbilityScores ancestryBonuses, int level)
    {
        var definition = Require(heroClass);
        var final = FinalScores(baseScores, ancestryBonuses);

        var dexterity = AbilityModifier(final.Dexterity);
        var intelligence = AbilityModifier(final.Intelligence);
        var strength = AbilityModifier(final.Strength);
        var will = AbilityModifier(final.Will);

        var stats = new HeroStats(dexterity, intelligence, strength, will);
        var skills = new HeroSkills(
            Arcana: intelligence,
            Examination: intelligence,
            Finesse: dexterity,
            Influence: will,
            Insight: will,
            Lore: intelligence,
            Might: strength,
            Naturecraft: will,
            Perception: will,
            Stealth: dexterity);
        var saves = new HeroSaves(definition.SaveAdvantage, definition.SaveDisadvantage);
        var combatStats = new HeroCombatStats(
            Armor: 0, HitDieType: definition.StartingHitDie, InitiativeBonus: dexterity, Speed: definition.Speed);

        return new DerivedAttributes(
            combatStats,
            definition.StartingHp,
            MaxManaFor(heroClass, intelligence, will, level),
            ResourcesFor(heroClass, level),
            saves,
            skills,
            stats);
    }

    /// <summary>Computes a hero's final ability scores (base + ancestry bonuses).</summary>
    /// <param name="baseScores">The base ability scores.</param>
    /// <param name="ancestryBonuses">The ancestry ability bonuses.</param>
    public static AbilityScores FinalScores(AbilityScores baseScores, AbilityScores ancestryBonuses)
    {
        return new AbilityScores(
            baseScores.Dexterity + ancestryBonuses.Dexterity,
            baseScores.Intelligence + ancestryBonuses.Intelligence,
            baseScores.Strength + ancestryBonuses.Strength,
            baseScores.Will + ancestryBonuses.Will);
    }

    /// <summary>The inclusive lower/upper bounds for a hero's max HP at a given level.</summary>
    /// <param name="heroClass">The hero's class.</param>
    /// <param name="level">The hero's current level.</param>
    public static (int Min, int Max) MaxHpBounds(HeroClass heroClass, int level)
    {
        var definition = Require(heroClass);
        var hitDieFace = (int)definition.StartingHitDie;
        return (definition.StartingHp, definition.StartingHp + hitDieFace * (level - 1));
    }

    private static int? MaxManaFor(HeroClass heroClass, int intelligenceModifier, int willModifier, int level)
    {
        return heroClass switch
        {
            HeroClass.Mage => intelligenceModifier * 3 + level,
            HeroClass.Oathsworn when level >= 2 => willModifier + level,
            _ => null,
        };
    }

    private static ClassDefinition Require(HeroClass heroClass)
    {
        return ClassDefinitions.For(heroClass)
            ?? throw new ArgumentOutOfRangeException(nameof(heroClass), heroClass, "No definition for class.");
    }

    private static ClassResources ResourcesFor(HeroClass heroClass, int level)
    {
        if (heroClass == HeroClass.Oathsworn)
        {
            return new ClassResources(
                JudgmentDiceCount: 2,
                JudgmentDiceType: level >= 3 ? DieType.D8 : DieType.D6,
                LayOnHandsPool: 5 * level,
                ThrillCharges: null);
        }
        return new ClassResources(null, null, null, null);
    }
}
```

- [ ] **Step 5: Run test to verify it passes**

Run: `dotnet test --filter "FullyQualifiedName~HeroDerivationTests"`
Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add NS.Domain/Rules/DerivedAttributes.cs NS.Domain/Rules/HeroDerivation.cs NS.Tests/HeroDerivationTests.cs
git commit -m "feat(domain): add HeroDerivation (modifiers, skills, saves, HP, mana, resources)"
```

---

## Task 4: Ancestry ability bonuses + seed (domain/seed)

**Files:**
- Modify: `NS.Domain/Reference/Ancestry.cs`, `NS.SoloDB/SeedData.cs:52-60`
- Test: existing suite must stay green (`dotnet test`).

**Interfaces:**
- Produces: `Ancestry(AbilityScores AbilityBonuses, string Description, Guid Id, string Name, IReadOnlyList<string> Traits)`.

- [ ] **Step 1: Modify `NS.Domain/Reference/Ancestry.cs`** — add `AbilityBonuses` (alphabetically first):

```csharp
namespace NS.Domain;

/// <summary>A playable ancestry (species) a hero can belong to.</summary>
/// <param name="AbilityBonuses">Ability score bonuses this ancestry grants to a hero's base scores.</param>
/// <param name="Description">A short description of the ancestry.</param>
/// <param name="Id">The unique identifier of the ancestry.</param>
/// <param name="Name">The ancestry's display name.</param>
/// <param name="Traits">The ancestry's notable traits.</param>
public sealed record Ancestry(
    AbilityScores AbilityBonuses,
    string Description,
    Guid Id,
    string Name,
    IReadOnlyList<string> Traits);
```

> If the existing `Ancestry.cs` has different param docs, preserve the existing ones and only add the `AbilityBonuses` line and parameter.

- [ ] **Step 2: Modify `NS.SoloDB/SeedData.cs:52-60`** — add zero bonuses as the first positional arg of each ancestry:

```csharp
    internal static IReadOnlyList<Ancestry> Ancestries { get; } =
    [
        new(new AbilityScores(0, 0, 0, 0),
            "Versatile and ambitious. (Placeholder — the quickstart rules do not define ancestries; the full game has 5 common and 19 exotic ancestries.)",
            new Guid("a0000000-0000-0000-0000-000000000001"), "Human", ["Adaptable"]),
        new(new AbilityScores(0, 0, 0, 0),
            "Graceful and long-lived. (Placeholder — not defined in the quickstart rules.)",
            new Guid("a0000000-0000-0000-0000-000000000002"), "Elf", ["Keen Senses"]),
        new(new AbilityScores(0, 0, 0, 0),
            "Stout and steadfast. (Placeholder — not defined in the quickstart rules.)",
            new Guid("a0000000-0000-0000-0000-000000000003"), "Dwarf", ["Stonecunning"]),
    ];
```

- [ ] **Step 3: Find any other `new Ancestry`/positional ancestry construction and fix it**

Run: `grep -rn "new Ancestry(" NS.Domain NS.SoloDB NS.FastEndpoints NS.Tests`
If any other call sites exist, add `new AbilityScores(0, 0, 0, 0)` as the first argument. (As of writing, only `SeedData.cs` constructs ancestries.)

- [ ] **Step 4: Build and run the full suite**

Run: `dotnet test`
Expected: PASS (no behavioral change; only the new field, defaulted to zero).

- [ ] **Step 5: Commit**

```bash
git add NS.Domain/Reference/Ancestry.cs NS.SoloDB/SeedData.cs
git commit -m "feat(domain): add ancestry ability bonuses (seeded zero)"
```

---

## Task 5: Hero.Create + new UpdateBuild (domain)

Add the new derivation-aware creation/update path **alongside** the existing constructor and `UpdateBuild` (kept temporarily so the solution keeps compiling; removed in Task 8).

**Files:**
- Modify: `NS.Domain/Heroes/Hero.cs`
- Test: `NS.Tests/HeroBuildTests.cs`

**Interfaces:**
- Produces: `Hero BaseAbilityScores { get; }` (AbilityScores); `static Hero Hero.Create(string name, HeroClass heroClass, Guid ancestryId, Guid? backgroundId, AbilityScores baseScores, AbilityScores ancestryBonuses, Guid userId)`; `void Hero.UpdateBuild(string name, Guid ancestryId, Guid? backgroundId, AbilityScores ancestryBonuses, int maxHp)` (new overload).

- [ ] **Step 1: Write the failing test** — `NS.Tests/HeroBuildTests.cs`:

```csharp
namespace NS.Tests;

public sealed class HeroBuildTests
{
    [Fact]
    public void Create_DerivesStoredAttributesFromClassAndScores()
    {
        var hero = Hero.Create(
            name: "Caldra",
            heroClass: HeroClass.Oathsworn,
            ancestryId: Guid.CreateVersion7(),
            backgroundId: null,
            baseScores: new AbilityScores(10, 10, 14, 12),
            ancestryBonuses: new AbilityScores(0, 0, 0, 0),
            userId: Guid.CreateVersion7());

        Assert.Equal(1, hero.Level);
        Assert.Equal(new AbilityScores(10, 10, 14, 12), hero.BaseAbilityScores);
        Assert.Equal(17, hero.MaxHp);
        Assert.Equal(17, hero.CurrentHp);
        Assert.Equal(2, hero.Stats.Strength);                 // STR 14 -> mod 2
        Assert.Equal(2, hero.Skills.Might);
        Assert.Equal(StatType.Strength, hero.Saves.AdvantageOn);
        Assert.Equal(5, hero.Resources.LayOnHandsPool);
        Assert.Null(hero.MaxMana);                            // Oathsworn casts from level 2
    }

    [Fact]
    public void UpdateBuild_RecomputesFromAncestryChangeButKeepsClassAndBaseScores()
    {
        var hero = Hero.Create(
            "Caldra", HeroClass.Mage, Guid.CreateVersion7(), null,
            new AbilityScores(10, 14, 10, 10), new AbilityScores(0, 0, 0, 0), Guid.CreateVersion7());
        var newAncestry = Guid.CreateVersion7();

        // ancestry now grants +2 INT -> final INT 16 -> mod 3 -> mana 3*3 + 1 = 10
        hero.UpdateBuild("Caldra II", newAncestry, null, new AbilityScores(0, 2, 0, 0), maxHp: 10);

        Assert.Equal("Caldra II", hero.Name);
        Assert.Equal(newAncestry, hero.AncestryId);
        Assert.Equal(HeroClass.Mage, hero.Class);                       // class unchanged
        Assert.Equal(new AbilityScores(10, 14, 10, 10), hero.BaseAbilityScores); // base unchanged
        Assert.Equal(3, hero.Stats.Intelligence);
        Assert.Equal(10, hero.MaxMana);
        Assert.Equal(10, hero.MaxHp);
    }

    [Fact]
    public void UpdateBuild_PreservesLevelSubclassAndCollections()
    {
        var hero = Hero.Create(
            "Caldra", HeroClass.Oathsworn, Guid.CreateVersion7(), null,
            new AbilityScores(10, 10, 12, 12), new AbilityScores(0, 0, 0, 0), Guid.CreateVersion7());
        hero.LevelUp([]);
        hero.LevelUp([]);                                               // level 3
        hero.SetSubclass("Oath of Vengeance");
        hero.AddGearItem(new HeroGearItem(hero.Id, "Torch", 2));

        hero.UpdateBuild("Caldra", hero.AncestryId, null, new AbilityScores(0, 0, 0, 0), maxHp: 30);

        Assert.Equal(3, hero.Level);
        Assert.Equal("Oath of Vengeance", hero.Subclass);
        Assert.Single(hero.Gear);
        Assert.Equal(30, hero.MaxHp);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test --filter "FullyQualifiedName~HeroBuildTests"`
Expected: FAIL — `Hero.Create` / new `UpdateBuild` / `BaseAbilityScores` do not exist.

- [ ] **Step 3: Add `BaseAbilityScores` property to `Hero`** — in the properties group (alphabetical, after `BackgroundId`), in `NS.Domain/Heroes/Hero.cs`:

```csharp
    /// <summary>The hero's player-bought base ability scores (before ancestry bonuses).</summary>
    public AbilityScores BaseAbilityScores { get; private set; } = null!;
```

Also add `BaseAbilityScores = null!;` to the `private Hero()` deserializer constructor (alongside the other reference-type fields).

- [ ] **Step 4: Add the `Hero.Create` factory** — place it in the methods group (alphabetical: `Create` comes before the instance methods; a static factory at the top of the methods region is acceptable — match the file's existing ordering by putting it right before `AddArmor`):

```csharp
    /// <summary>Creates a new level-1 hero, deriving all non-player-set attributes from the class,
    /// base ability scores, ancestry bonuses, and level 1.</summary>
    /// <param name="name">The hero's name.</param>
    /// <param name="heroClass">The hero's class (must be a playable class).</param>
    /// <param name="ancestryId">The identifier of the hero's ancestry.</param>
    /// <param name="backgroundId">The optional identifier of the hero's background.</param>
    /// <param name="baseScores">The player-bought base ability scores.</param>
    /// <param name="ancestryBonuses">The hero's ancestry ability bonuses.</param>
    /// <param name="userId">The identifier of the owning <see cref="User"/>.</param>
    public static Hero Create(
        string name,
        HeroClass heroClass,
        Guid ancestryId,
        Guid? backgroundId,
        AbilityScores baseScores,
        AbilityScores ancestryBonuses,
        Guid userId)
    {
        var derived = HeroDerivation.Derive(heroClass, baseScores, ancestryBonuses, level: 1);
        return new Hero
        {
            AncestryId = ancestryId,
            BackgroundId = backgroundId,
            BaseAbilityScores = baseScores,
            Class = heroClass,
            CombatStats = derived.CombatStats,
            CurrentHp = derived.MaxHp,
            CurrentMana = derived.MaxMana,
            CurrentWounds = 0,
            HitDiceAvailable = 1,
            Id = Guid.CreateVersion7(),
            Level = 1,
            MaxHitDice = 1,
            MaxHp = derived.MaxHp,
            MaxMana = derived.MaxMana,
            Name = name,
            PendingStatIncrease = false,
            Resources = derived.Resources,
            Saves = derived.Saves,
            Skills = derived.Skills,
            Stats = derived.Stats,
            TempHp = 0,
            UnspentSkillPoints = 0,
            UserId = userId,
        };
    }
```

> This uses object-initializer syntax via the `private Hero()` constructor, so every assigned property must have an accessible setter — they already do (`private set` / `init`). Scalars use `private set`, which object initializers can set from inside the class.

- [ ] **Step 5: Add the new `UpdateBuild` overload** — in the methods group near the existing `UpdateBuild`:

```csharp
    /// <summary>Overwrites the player-set build attributes, re-deriving ancestry-dependent attributes
    /// (modifiers, skills, mana, resources) at the hero's current level while preserving class, base
    /// ability scores, level, subclass, play state, and collections. Max HP is taken from the caller
    /// (clamped by the API to the class+level bounds) since level-up adds a rolled amount. Current HP
    /// and mana are clamped to the new maxima.</summary>
    /// <param name="name">The hero's name.</param>
    /// <param name="ancestryId">The identifier of the hero's ancestry.</param>
    /// <param name="backgroundId">The optional identifier of the hero's background.</param>
    /// <param name="ancestryBonuses">The hero's ancestry ability bonuses.</param>
    /// <param name="maxHp">The hero's maximum hit points.</param>
    public void UpdateBuild(
        string name,
        Guid ancestryId,
        Guid? backgroundId,
        AbilityScores ancestryBonuses,
        int maxHp)
    {
        var derived = HeroDerivation.Derive(Class, BaseAbilityScores, ancestryBonuses, Level);
        AncestryId = ancestryId;
        BackgroundId = backgroundId;
        CombatStats = derived.CombatStats;
        MaxHp = maxHp;
        CurrentHp = Math.Min(CurrentHp, maxHp);
        MaxMana = derived.MaxMana;
        CurrentMana = derived.MaxMana.HasValue
            ? Math.Min(CurrentMana ?? derived.MaxMana.Value, derived.MaxMana.Value)
            : null;
        Name = name;
        Resources = derived.Resources;
        Saves = derived.Saves;
        Skills = derived.Skills;
        Stats = derived.Stats;
    }
```

- [ ] **Step 6: Run test to verify it passes**

Run: `dotnet test --filter "FullyQualifiedName~HeroBuildTests"`
Expected: PASS. Then `dotnet test` to confirm nothing else broke (old constructor/UpdateBuild still present).

- [ ] **Step 7: Commit**

```bash
git add NS.Domain/Heroes/Hero.cs NS.Tests/HeroBuildTests.cs
git commit -m "feat(domain): add Hero.Create and derivation-aware UpdateBuild"
```

---

## Task 6: Split request DTOs + validators (API)

**Files:**
- Modify/replace: `NS.FastEndpoints/Heroes/HeroBuildRequest.cs` → contains `CreateHeroRequest`, `CreateHeroValidator`, `UpdateHeroRequest`, `UpdateHeroValidator`
- Test: `NS.Tests/HeroBuildValidatorTests.cs`

**Interfaces:**
- Produces: `CreateHeroRequest(Guid AncestryId, Guid? BackgroundId, AbilityScores BaseAbilityScores, HeroClass HeroClass, string Name)`; `UpdateHeroRequest(Guid AncestryId, Guid? BackgroundId, int MaxHp, string Name)`.

- [ ] **Step 1: Write the failing test** — `NS.Tests/HeroBuildValidatorTests.cs`:

```csharp
using FluentValidation.TestHelper;

namespace NS.Tests;

public sealed class HeroBuildValidatorTests
{
    private static CreateHeroRequest ValidCreate() => new(
        AncestryId: Guid.CreateVersion7(),
        BackgroundId: null,
        BaseAbilityScores: new AbilityScores(10, 10, 10, 10),
        HeroClass: HeroClass.Oathsworn,
        Name: "Caldra");

    [Fact]
    public void Create_Valid_PassesValidation()
    {
        var result = new CreateHeroValidator().TestValidate(ValidCreate());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Create_EmptyName_Fails()
    {
        var result = new CreateHeroValidator().TestValidate(ValidCreate() with { Name = "" });
        result.ShouldHaveValidationErrorFor(r => r.Name);
    }

    [Fact]
    public void Create_NonPlayableClass_Fails()
    {
        var result = new CreateHeroValidator().TestValidate(ValidCreate() with { HeroClass = HeroClass.Berserker });
        result.ShouldHaveValidationErrorFor(r => r.HeroClass);
    }

    [Fact]
    public void Create_OverBudgetScores_Fails()
    {
        var result = new CreateHeroValidator().TestValidate(
            ValidCreate() with { BaseAbilityScores = new AbilityScores(15, 15, 15, 9) });
        result.ShouldHaveValidationErrorFor(r => r.BaseAbilityScores);
    }

    [Fact]
    public void Update_NonPositiveMaxHp_Fails()
    {
        var request = new UpdateHeroRequest(Guid.CreateVersion7(), null, 0, "Caldra");
        var result = new UpdateHeroValidator().TestValidate(request);
        result.ShouldHaveValidationErrorFor(r => r.MaxHp);
    }
}
```

> If `FluentValidation.TestHelper` is not already referenced, replace each assertion with a direct `new CreateHeroValidator().Validate(req).IsValid` check. Confirm with `grep -rn "TestValidate" NS.Tests` whether the helper is already used in this codebase; if not, use the `.Validate(...).IsValid` form and the per-error `.Errors` collection.

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test --filter "FullyQualifiedName~HeroBuildValidatorTests"`
Expected: FAIL — new request types / validators do not exist.

- [ ] **Step 3: Replace `NS.FastEndpoints/Heroes/HeroBuildRequest.cs`** with the split DTOs + validators:

```csharp
namespace NSFastEndpoints;

/// <summary>The inputs for creating a hero. Owner is taken from the token, never the body.</summary>
/// <param name="AncestryId">The identifier of the hero's ancestry.</param>
/// <param name="BackgroundId">The optional identifier of the hero's background.</param>
/// <param name="BaseAbilityScores">The player-bought base ability scores (point-buy).</param>
/// <param name="HeroClass">The hero's class (chosen once, at creation).</param>
/// <param name="Name">The hero's name.</param>
public sealed record CreateHeroRequest(
    Guid AncestryId,
    Guid? BackgroundId,
    AbilityScores BaseAbilityScores,
    HeroClass HeroClass,
    string Name);

/// <summary>Validates <see cref="CreateHeroRequest"/>.</summary>
public sealed class CreateHeroValidator : Validator<CreateHeroRequest>
{
    /// <summary>Initializes validation rules for hero creation.</summary>
    public CreateHeroValidator()
    {
        RuleFor(r => r.Name).NotEmpty();
        RuleFor(r => r.AncestryId).NotEmpty();
        RuleFor(r => r.HeroClass)
            .Must(ClassDefinitions.IsPlayable)
            .WithMessage("Class is not a playable class.");
        RuleFor(r => r.BaseAbilityScores)
            .Must(PointBuy.IsValid)
            .WithMessage("Ability scores must be between 8 and 15 and cost at most 27 points.");
    }
}

/// <summary>The inputs for updating a hero. Class and base ability scores are immutable after creation.</summary>
/// <param name="AncestryId">The identifier of the hero's ancestry.</param>
/// <param name="BackgroundId">The optional identifier of the hero's background.</param>
/// <param name="MaxHp">The hero's maximum hit points (bounds-checked against class and level).</param>
/// <param name="Name">The hero's name.</param>
public sealed record UpdateHeroRequest(
    Guid AncestryId,
    Guid? BackgroundId,
    int MaxHp,
    string Name);

/// <summary>Validates <see cref="UpdateHeroRequest"/>. The class+level bounds for <c>MaxHp</c> are
/// checked in the endpoint, which has access to the stored hero.</summary>
public sealed class UpdateHeroValidator : Validator<UpdateHeroRequest>
{
    /// <summary>Initializes validation rules for a hero update.</summary>
    public UpdateHeroValidator()
    {
        RuleFor(r => r.Name).NotEmpty();
        RuleFor(r => r.AncestryId).NotEmpty();
        RuleFor(r => r.MaxHp).GreaterThan(0);
    }
}
```

> `AbilityScores`, `ClassDefinitions`, `PointBuy` resolve via the existing `global using NS.Domain;` in `NS.FastEndpoints/_GlobalUsings.cs`.

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test --filter "FullyQualifiedName~HeroBuildValidatorTests"`
Expected: PASS. (The endpoints still reference the now-deleted `HeroBuildRequest` and will not compile yet — that is fixed in Task 7; run the *filtered* build/test which compiles the whole solution, so expect a compile error from the endpoints. If the filtered run fails to compile, proceed directly to Task 7 and run the tests at the end of Task 7. To keep this task self-contained, do Steps 3 of Task 7 before re-running.)

> **Note:** Because the endpoints depend on the old `HeroBuildRequest`, the solution will not compile between Step 3 here and Task 7 Step 1. Treat Tasks 6 and 7 as a single compile unit: implement Task 7's endpoint edits, then run the validator + endpoint tests together.

- [ ] **Step 5: Commit (after Task 7 compiles)** — defer the commit; commit at the end of Task 7.

---

## Task 7: Rewire create/update endpoints (API)

**Files:**
- Modify: `NS.FastEndpoints/Heroes/CreateHeroEndpoint.cs`, `NS.FastEndpoints/Heroes/UpdateHeroEndpoint.cs`
- Test: `NS.Tests/HeroBuildValidatorTests.cs` (from Task 6) + full `dotnet test`.

**Interfaces:**
- Consumes: `CreateHeroRequest`, `UpdateHeroRequest`, `Hero.Create`, `Hero.UpdateBuild` (new), `IReferenceDataService<Ancestry>`, `HeroDerivation.MaxHpBounds`.

- [ ] **Step 1: Rewrite `CreateHeroEndpoint.cs`**:

```csharp
namespace NSFastEndpoints;

/// <summary>Creates a new level-1 hero owned by the authenticated user.</summary>
public sealed class CreateHeroEndpoint : Endpoint<CreateHeroRequest, CreateHeroResponse>
{
    private readonly IReferenceDataService<Ancestry> _ancestries;
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero and ancestry data services.</summary>
    /// <param name="ancestries">The ancestry reference-data service.</param>
    /// <param name="heroes">The hero data service.</param>
    public CreateHeroEndpoint(IReferenceDataService<Ancestry> ancestries, IHeroDataService heroes)
    {
        _ancestries = ancestries;
        _heroes = heroes;
    }

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(CreateHeroRequest req, CancellationToken ct)
    {
        var ancestry = await _ancestries.GetByIdAsync(req.AncestryId);
        if (ancestry is null)
        {
            AddError(r => r.AncestryId, "Ancestry not found.");
            await Send.ErrorsAsync(400, ct);
            return;
        }

        var hero = Hero.Create(
            req.Name,
            req.HeroClass,
            req.AncestryId,
            req.BackgroundId,
            req.BaseAbilityScores,
            ancestry.AbilityBonuses,
            User.GetUserId());

        await _heroes.SaveAsync(hero);
        await Send.ResponseAsync(new CreateHeroResponse(hero.Id), 201, ct);
    }
}

/// <summary>Response returned after successfully creating a hero.</summary>
/// <param name="Id">The newly created hero's unique identifier.</param>
public sealed record CreateHeroResponse(Guid Id);
```

> Confirm `Send.ErrorsAsync(int, CancellationToken)` exists in this FastEndpoints version with `grep -rn "Send.Errors" NS.FastEndpoints`; if the codebase instead uses `ThrowError`/`AddError(...); ThrowIfAnyErrors();`, follow that established pattern (see the validators note in CLAUDE.md: "business-rule checks go in `HandleAsync` via `AddError` + `ThrowIfAnyErrors()`"). Prefer: `AddError(r => r.AncestryId, "Ancestry not found."); ThrowIfAnyErrors();`.

- [ ] **Step 2: Rewrite `UpdateHeroEndpoint.cs`**:

```csharp
namespace NSFastEndpoints;

/// <summary>Updates an existing hero's player-set build attributes, preserving level, subclass,
/// play state, and collections.</summary>
public sealed class UpdateHeroEndpoint : Endpoint<UpdateHeroRequest>
{
    private readonly IReferenceDataService<Ancestry> _ancestries;
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero and ancestry data services.</summary>
    /// <param name="ancestries">The ancestry reference-data service.</param>
    /// <param name="heroes">The hero data service.</param>
    public UpdateHeroEndpoint(IReferenceDataService<Ancestry> ancestries, IHeroDataService heroes)
    {
        _ancestries = ancestries;
        _heroes = heroes;
    }

    /// <inheritdoc/>
    public override void Configure()
    {
        Put("heroes/{heroId}");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(UpdateHeroRequest req, CancellationToken ct)
    {
        var heroId = Route<Guid>("heroId");
        var hero = await _heroes.GetOwnedByIdAsync(heroId, User.GetUserId());
        if (hero is null) { await Send.NotFoundAsync(ct); return; }

        var ancestry = await _ancestries.GetByIdAsync(req.AncestryId);
        if (ancestry is null)
        {
            AddError(r => r.AncestryId, "Ancestry not found.");
        }

        var (minHp, maxHp) = HeroDerivation.MaxHpBounds(hero.Class, hero.Level);
        if (req.MaxHp < minHp || req.MaxHp > maxHp)
        {
            AddError(r => r.MaxHp, $"Max HP must be between {minHp} and {maxHp} for this class and level.");
        }

        ThrowIfAnyErrors();

        hero.UpdateBuild(req.Name, req.AncestryId, req.BackgroundId, ancestry!.AbilityBonuses, req.MaxHp);
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}
```

- [ ] **Step 3: Run the full suite**

Run: `dotnet test`
Expected: PASS — including `HeroBuildValidatorTests`. (The old `Hero` constructor and old `UpdateBuild` are still present and now unused by production code; cleaned up in Task 8.)

- [ ] **Step 4: Commit (covers Tasks 6 + 7)**

```bash
git add NS.FastEndpoints/Heroes/HeroBuildRequest.cs NS.FastEndpoints/Heroes/CreateHeroEndpoint.cs NS.FastEndpoints/Heroes/UpdateHeroEndpoint.cs NS.Tests/HeroBuildValidatorTests.cs
git commit -m "feat(api): split create/update hero requests; derive attributes server-side"
```

---

## Task 8: Remove the legacy Hero constructor + old UpdateBuild (domain cleanup)

Now that production code uses `Hero.Create` and the new `UpdateBuild`, remove the dead members and adapt the two remaining test call sites.

**Files:**
- Modify: `NS.Domain/Heroes/Hero.cs` (remove old public constructor + old `UpdateBuild` overload), `NS.Tests/TestHero.cs`, `NS.Tests/HeroTests.cs`

- [ ] **Step 1: Update `NS.Tests/TestHero.cs`** to build via `Hero.Create`:

```csharp
namespace NS.Tests;

/// <summary>Factory helpers for constructing <see cref="Hero"/> instances in tests.</summary>
internal static class TestHero
{
    /// <summary>Creates a valid level-1 Oathsworn hero with the specified maximum hit points and owner.
    /// Base scores are all 10 (modifiers 0); HP above the class starting value is applied as an increase.</summary>
    /// <param name="maxHp">The hero's starting maximum (and current) hit points.</param>
    /// <param name="userId">The owning user's identifier; a new identifier is generated when omitted.</param>
    internal static Hero Create(int maxHp = 17, Guid? userId = null)
    {
        var hero = Hero.Create(
            name: "Caldra",
            heroClass: HeroClass.Oathsworn,
            ancestryId: Guid.CreateVersion7(),
            backgroundId: null,
            baseScores: new AbilityScores(10, 10, 10, 10),
            ancestryBonuses: new AbilityScores(0, 0, 0, 0),
            userId: userId ?? Guid.CreateVersion7());
        if (maxHp > hero.MaxHp)
        {
            hero.ApplyHpIncrease(maxHp - hero.MaxHp);
        }
        return hero;
    }
}
```

> The default Oathsworn `StartingHp` is 17 (matching the previous default), and the only non-default value used by tests is `maxHp: 20` (→ `ApplyHpIncrease(3)`). No test passes a `maxHp` below 17.

- [ ] **Step 2: Update `NS.Tests/HeroTests.cs:245-260`** — replace the `UpdateBuildTo` helper with the new signature:

```csharp
    private static void UpdateBuildTo(Hero hero, int maxHp, string name) =>
        hero.UpdateBuild(
            name: name,
            ancestryId: Guid.CreateVersion7(),
            backgroundId: null,
            ancestryBonuses: new AbilityScores(0, 0, 0, 0),
            maxHp: maxHp);
```

- [ ] **Step 3: Remove the legacy members from `NS.Domain/Heroes/Hero.cs`**
  - Delete the public `Hero(Guid ancestryId, … Guid userId)` constructor (the big build-input constructor).
  - Delete the old `UpdateBuild(Guid ancestryId, Guid? backgroundId, HeroCombatStats combatStats, … HeroStats stats)` overload.
  - Keep the `private Hero()` deserializer constructor and the new `Hero.Create` / new `UpdateBuild`.

- [ ] **Step 4: Build and run the full suite**

Run: `dotnet test`
Expected: PASS. If any assertion fails because it encoded the *old* arbitrary `TestHero` build values, update it to the **derived Oathsworn baseline** (base all 10, level 1): `Stats` all 0; all `Skills` 0; `Saves` = `Strength`▲ / `Dexterity`▼; `CombatStats` = `Armor 0, HitDieType D10, InitiativeBonus 0, Speed 6`; `MaxHp` 17; `MaxMana` null; `Resources` = `JudgmentDiceCount 2, JudgmentDiceType D6, LayOnHandsPool 5, ThrillCharges null`. (Previously `TestHero` used `combatStats armor 8`, `saves Will/Dexterity`, and null resources — any test asserting those exact values must move to the derived baseline.)

- [ ] **Step 5: Commit**

```bash
git add NS.Domain/Heroes/Hero.cs NS.Tests/TestHero.cs NS.Tests/HeroTests.cs
git commit -m "refactor(domain): remove legacy Hero build constructor and UpdateBuild overload"
```

---

## Task 9: Client API types + wrappers

**Files:**
- Modify: `NS.Client/src/lib/api/types.ts`, `NS.Client/src/lib/api/client.ts`
- Test: `NS.Client/src/lib/api/client.test.ts`

**Interfaces:**
- Produces (TS): `AbilityScores`, `CreateHeroRequest`, `UpdateHeroRequest`; `Ancestry.abilityBonuses`; `Hero.baseAbilityScores`; `createHero(model)`, `updateHero(id, model)` send the split DTOs.

- [ ] **Step 1: Edit `NS.Client/src/lib/api/types.ts`**
  - Add after the `HeroStats` interface:

```ts
export interface AbilityScores {
  dexterity: number;
  intelligence: number;
  strength: number;
  will: number;
}
```

  - Add `baseAbilityScores: AbilityScores;` to the `Hero` interface (alphabetical, after `backgroundId`).
  - Change the `Ancestry` interface to include bonuses:

```ts
export interface Ancestry { abilityBonuses: AbilityScores; description: string; id: string; name: string; traits: string[]; }
```

  - Add the request DTOs (near the bottom, by the other request/response types):

```ts
export interface CreateHeroRequest {
  ancestryId: string;
  backgroundId: string | null;
  baseAbilityScores: AbilityScores;
  heroClass: HeroClass;
  name: string;
}
export interface UpdateHeroRequest {
  ancestryId: string;
  backgroundId: string | null;
  maxHp: number;
  name: string;
}
```

- [ ] **Step 2: Write the failing test** — replace the create/update wrapper tests in `NS.Client/src/lib/api/client.test.ts`. First add `CreateHeroRequest`/`UpdateHeroRequest` handling. Add this test block (and update imports to include `createHero, updateHero` if not already imported):

```ts
describe('hero build wrappers', () => {
  it('createHero posts the create DTO (class + base scores, no maxHp)', async () => {
    const fetchMock = captureFetch(201);
    // Response body for 201 create:
    vi.stubGlobal('fetch', vi.fn(() =>
      Promise.resolve(new Response(JSON.stringify({ id: 'h1' }), { status: 201 }))));
    const model = {
      name: 'Caldra', ancestryId: 'a1', backgroundId: null,
      heroClass: 'Oathsworn' as const,
      baseAbilityScores: { dexterity: 10, intelligence: 10, strength: 14, will: 12 },
      maxHp: 0
    };
    await createHero(model);
    const [path, init] = (globalThis.fetch as unknown as { mock: { calls: [string, RequestInit][] } }).mock.calls[0];
    expect(path).toBe('/api/heroes');
    expect(init.method).toBe('POST');
    expect(JSON.parse(init.body as string)).toEqual({
      name: 'Caldra', ancestryId: 'a1', backgroundId: null,
      heroClass: 'Oathsworn',
      baseAbilityScores: { dexterity: 10, intelligence: 10, strength: 14, will: 12 }
    });
  });

  it('updateHero puts the update DTO (name/ancestry/background/maxHp only)', async () => {
    const fetchMock = captureFetch(204);
    const model = {
      name: 'Caldra', ancestryId: 'a2', backgroundId: 'b1',
      heroClass: 'Oathsworn' as const,
      baseAbilityScores: { dexterity: 10, intelligence: 10, strength: 14, will: 12 },
      maxHp: 25
    };
    await updateHero('h1', model);
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/heroes/h1',
      expect.objectContaining({ method: 'PUT', body: JSON.stringify({ name: 'Caldra', ancestryId: 'a2', backgroundId: 'b1', maxHp: 25 }) })
    );
  });
});
```

> If `captureFetch` is defined once at the top of `client.test.ts`, reuse it. The `createHero` 201 case needs a JSON body, so it stubs `fetch` directly. Remove any pre-existing create/update wrapper tests that asserted the whole model was sent.

- [ ] **Step 3: Run test to verify it fails**

Run (from `NS.Client/`): `npm test -- client.test`
Expected: FAIL — wrappers still send the full model.

- [ ] **Step 4: Edit `NS.Client/src/lib/api/client.ts`** — rewrite the wrappers (keep the `HeroBuildModel` import):

```ts
/** POST /heroes — create a hero from the build model's create-time inputs. */
export function createHero(build: HeroBuildModel): Promise<{ id: string }> {
	const body: CreateHeroRequest = {
		ancestryId: build.ancestryId,
		backgroundId: build.backgroundId,
		baseAbilityScores: build.baseAbilityScores,
		heroClass: build.heroClass as HeroClass,
		name: build.name
	};
	return apiFetch<{ id: string }>('/heroes', { method: 'POST', body: JSON.stringify(body) });
}

/** PUT /heroes/{id} — update a hero's editable build attributes (class and base scores are immutable). */
export function updateHero(id: string, build: HeroBuildModel): Promise<void> {
	const body: UpdateHeroRequest = {
		ancestryId: build.ancestryId,
		backgroundId: build.backgroundId,
		maxHp: build.maxHp,
		name: build.name
	};
	return apiFetch<void>(`/heroes/${id}`, { method: 'PUT', body: JSON.stringify(body) });
}
```

  Add to the type import at the top of `client.ts` (the existing `import type { … } from './types';`): `AbilityScores, CreateHeroRequest, UpdateHeroRequest, HeroClass` (only those not already imported).

- [ ] **Step 5: Run test to verify it passes**

Run: `npm test -- client.test`
Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add NS.Client/src/lib/api/types.ts NS.Client/src/lib/api/client.ts NS.Client/src/lib/api/client.test.ts
git commit -m "feat(client): split create/update hero DTOs and ability-score types"
```

---

## Task 10: Point-buy helper (client)

**Files:**
- Create: `NS.Client/src/lib/sheet/build/pointBuy.ts`, `NS.Client/src/lib/sheet/build/pointBuy.test.ts`

**Interfaces:**
- Produces: `POINT_BUY_MIN=8`, `POINT_BUY_MAX=15`, `POINT_BUY_BUDGET=27`; `costOf(score)`, `totalCost(scores)`, `remaining(scores)`, `canIncrement(scores, key)`, `canDecrement(scores, key)`; `type AbilityKey = 'dexterity'|'intelligence'|'strength'|'will'`.

- [ ] **Step 1: Write the failing test** — `pointBuy.test.ts`:

```ts
import { describe, expect, it } from 'vitest';
import { canDecrement, canIncrement, costOf, remaining, totalCost } from './pointBuy';
import type { AbilityScores } from '$lib/api/types';

const scores = (d: number, i: number, s: number, w: number): AbilityScores =>
  ({ dexterity: d, intelligence: i, strength: s, will: w });

describe('pointBuy', () => {
  it('costOf follows the table', () => {
    expect(costOf(8)).toBe(0);
    expect(costOf(13)).toBe(5);
    expect(costOf(15)).toBe(9);
  });

  it('totalCost sums all four', () => {
    expect(totalCost(scores(14, 13, 12, 8))).toBe(7 + 5 + 4 + 0);
  });

  it('remaining is budget minus spent', () => {
    expect(remaining(scores(8, 8, 8, 8))).toBe(27);
    expect(remaining(scores(15, 14, 13, 13))).toBe(27 - 26);
  });

  it('canIncrement is false at max or when unaffordable', () => {
    expect(canIncrement(scores(8, 8, 8, 8), 'strength')).toBe(true);
    expect(canIncrement(scores(15, 8, 8, 8), 'dexterity')).toBe(false); // at max
    // 15(9)+15(9)+8(0)+8(0)=18 spent, 9 left; raising will 8->9 costs 1 -> affordable
    expect(canIncrement(scores(15, 15, 8, 8), 'will')).toBe(true);
    // 15(9)+15(9)+14(7)=25 spent for d/i/s, 2 left; will 13->14 costs 7-5=2 -> affordable; ->15 costs 2 more, only after
    expect(canIncrement(scores(15, 15, 14, 13), 'will')).toBe(false); // 25+5=30 already... guard via remaining
  });

  it('canDecrement is false at min', () => {
    expect(canDecrement(scores(8, 10, 10, 10), 'dexterity')).toBe(false);
    expect(canDecrement(scores(9, 10, 10, 10), 'dexterity')).toBe(true);
  });
});
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npm test -- pointBuy`
Expected: FAIL — module not found.

- [ ] **Step 3: Write `pointBuy.ts`**:

```ts
import type { AbilityScores } from '$lib/api/types';

export const POINT_BUY_MIN = 8;
export const POINT_BUY_MAX = 15;
export const POINT_BUY_BUDGET = 27;

export type AbilityKey = 'dexterity' | 'intelligence' | 'strength' | 'will';

const COST: Record<number, number> = { 8: 0, 9: 1, 10: 2, 11: 3, 12: 4, 13: 5, 14: 7, 15: 9 };

export function costOf(score: number): number {
  return COST[score] ?? Number.POSITIVE_INFINITY;
}

export function totalCost(scores: AbilityScores): number {
  return costOf(scores.dexterity) + costOf(scores.intelligence) + costOf(scores.strength) + costOf(scores.will);
}

export function remaining(scores: AbilityScores): number {
  return POINT_BUY_BUDGET - totalCost(scores);
}

export function canIncrement(scores: AbilityScores, key: AbilityKey): boolean {
  const current = scores[key];
  if (current >= POINT_BUY_MAX) return false;
  const delta = costOf(current + 1) - costOf(current);
  return remaining(scores) >= delta;
}

export function canDecrement(scores: AbilityScores, key: AbilityKey): boolean {
  return scores[key] > POINT_BUY_MIN;
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `npm test -- pointBuy`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add NS.Client/src/lib/sheet/build/pointBuy.ts NS.Client/src/lib/sheet/build/pointBuy.test.ts
git commit -m "feat(client): add point-buy helper"
```

---

## Task 11: Class-definition mirror + build preview helpers (client)

**Files:**
- Create: `NS.Client/src/lib/sheet/build/classDefs.ts`, `NS.Client/src/lib/sheet/build/classDefs.test.ts`

**Interfaces:**
- Produces: `playableClasses: HeroClass[]`; `classDefs: Record<string, ClassDef>`; `abilityModifier(score)`; `finalScores(base, bonuses)`; `previewMaxMana(heroClass, finalScores, level)`; `maxHpBounds(heroClass, level)`; `startingHp(heroClass)`.

- [ ] **Step 1: Write the failing test** — `classDefs.test.ts`:

```ts
import { describe, expect, it } from 'vitest';
import { abilityModifier, finalScores, maxHpBounds, playableClasses, previewMaxMana, startingHp } from './classDefs';

describe('classDefs', () => {
  it('lists exactly the four playable classes', () => {
    expect([...playableClasses].sort()).toEqual(['Cheat', 'Hunter', 'Mage', 'Oathsworn']);
  });

  it('abilityModifier floors (score-10)/2', () => {
    expect(abilityModifier(8)).toBe(-1);
    expect(abilityModifier(14)).toBe(2);
    expect(abilityModifier(11)).toBe(0);
  });

  it('finalScores add ancestry bonuses', () => {
    expect(finalScores({ dexterity: 10, intelligence: 12, strength: 14, will: 8 }, { dexterity: 0, intelligence: 2, strength: 0, will: 1 }))
      .toEqual({ dexterity: 10, intelligence: 14, strength: 14, will: 9 });
  });

  it('previewMaxMana matches per-class rules', () => {
    const f = { dexterity: 10, intelligence: 14, strength: 10, will: 14 };
    expect(previewMaxMana('Mage', f, 1)).toBe(7);          // intMod 2 *3 +1
    expect(previewMaxMana('Oathsworn', f, 1)).toBeNull();  // caster from level 2
    expect(previewMaxMana('Oathsworn', f, 3)).toBe(5);     // wilMod 2 + 3
    expect(previewMaxMana('Hunter', f, 5)).toBeNull();
  });

  it('startingHp and maxHpBounds come from the class block', () => {
    expect(startingHp('Oathsworn')).toBe(17);
    expect(maxHpBounds('Oathsworn', 3)).toEqual({ min: 17, max: 17 + 10 * 2 });
  });
});
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npm test -- classDefs`
Expected: FAIL — module not found.

- [ ] **Step 3: Write `classDefs.ts`**:

```ts
import type { AbilityScores, DieType, HeroClass, StatType } from '$lib/api/types';

export interface ClassDef {
  casterFromLevel: number | null;
  hitDie: DieType;
  manaFormula: 'mageInt' | 'oathswornWil' | null;
  saveAdvantage: StatType;
  saveDisadvantage: StatType;
  speed: number;
  startingHp: number;
}

export const classDefs: Record<string, ClassDef> = {
  Cheat: { casterFromLevel: null, hitDie: 'D6', manaFormula: null, saveAdvantage: 'Dexterity', saveDisadvantage: 'Will', speed: 6, startingHp: 10 },
  Hunter: { casterFromLevel: null, hitDie: 'D8', manaFormula: null, saveAdvantage: 'Dexterity', saveDisadvantage: 'Intelligence', speed: 6, startingHp: 13 },
  Mage: { casterFromLevel: 1, hitDie: 'D6', manaFormula: 'mageInt', saveAdvantage: 'Intelligence', saveDisadvantage: 'Strength', speed: 6, startingHp: 10 },
  Oathsworn: { casterFromLevel: 2, hitDie: 'D10', manaFormula: 'oathswornWil', saveAdvantage: 'Strength', saveDisadvantage: 'Dexterity', speed: 6, startingHp: 17 }
};

export const playableClasses = Object.keys(classDefs) as HeroClass[];

const dieFace = (die: DieType): number => Number(die.slice(1));

export function abilityModifier(finalScore: number): number {
  return Math.floor((finalScore - 10) / 2);
}

export function finalScores(base: AbilityScores, bonuses: AbilityScores): AbilityScores {
  return {
    dexterity: base.dexterity + bonuses.dexterity,
    intelligence: base.intelligence + bonuses.intelligence,
    strength: base.strength + bonuses.strength,
    will: base.will + bonuses.will
  };
}

export function startingHp(heroClass: HeroClass): number {
  return classDefs[heroClass]?.startingHp ?? 0;
}

export function maxHpBounds(heroClass: HeroClass, level: number): { min: number; max: number } {
  const def = classDefs[heroClass];
  if (!def) return { min: 1, max: Number.MAX_SAFE_INTEGER };
  return { min: def.startingHp, max: def.startingHp + dieFace(def.hitDie) * (level - 1) };
}

export function previewMaxMana(heroClass: HeroClass, final: AbilityScores, level: number): number | null {
  const def = classDefs[heroClass];
  if (!def || def.casterFromLevel === null || level < def.casterFromLevel) return null;
  if (def.manaFormula === 'mageInt') return abilityModifier(final.intelligence) * 3 + level;
  if (def.manaFormula === 'oathswornWil') return abilityModifier(final.will) + level;
  return null;
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `npm test -- classDefs`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add NS.Client/src/lib/sheet/build/classDefs.ts NS.Client/src/lib/sheet/build/classDefs.test.ts
git commit -m "feat(client): add class-definition mirror and build preview helpers"
```

---

## Task 12: Build model + validation (client)

**Files:**
- Modify: `NS.Client/src/lib/sheet/build/model.ts`, `validate.ts`, `options.ts`
- Test: `model.test.ts`, `validate.test.ts`

**Interfaces:**
- Produces: `HeroBuildModel { name; ancestryId; backgroundId; heroClass: HeroClass | ''; baseAbilityScores: AbilityScores; maxHp }`; `blankBuildModel()`; `heroToBuildModel(hero)`; `normalizeBuild(model)`; `validateBuild(model, opts: { mode: 'create'|'edit'; level: number })`.

- [ ] **Step 1: Replace `NS.Client/src/lib/sheet/build/model.ts`**:

```ts
import type { AbilityScores, Hero, HeroClass } from '$lib/api/types';
import { POINT_BUY_MIN } from './pointBuy';
import { startingHp } from './classDefs';

/** The client-side editable shape of a hero's player-set build inputs. */
export interface HeroBuildModel {
  name: string;
  ancestryId: string;
  backgroundId: string | null;
  heroClass: HeroClass | '';
  baseAbilityScores: AbilityScores;
  maxHp: number;
}

/** A level-1 default build for the create form (class unset, all scores at the point-buy minimum). */
export function blankBuildModel(): HeroBuildModel {
  return {
    name: '',
    ancestryId: '',
    backgroundId: null,
    heroClass: '',
    baseAbilityScores: {
      dexterity: POINT_BUY_MIN,
      intelligence: POINT_BUY_MIN,
      strength: POINT_BUY_MIN,
      will: POINT_BUY_MIN
    },
    maxHp: 0
  };
}

/** Map a loaded hero onto an editable build model for the edit form. */
export function heroToBuildModel(hero: Hero): HeroBuildModel {
  return {
    name: hero.name,
    ancestryId: hero.ancestryId,
    backgroundId: hero.backgroundId,
    heroClass: hero.class,
    baseAbilityScores: { ...hero.baseAbilityScores },
    maxHp: hero.maxHp
  };
}

function coerceNumber(value: number): number {
  return Number.isFinite(value) ? value : 0;
}

/** Coerce cleared numeric inputs back to numbers before submit. */
export function normalizeBuild(model: HeroBuildModel): HeroBuildModel {
  return {
    ...model,
    maxHp: coerceNumber(model.maxHp),
    baseAbilityScores: {
      dexterity: coerceNumber(model.baseAbilityScores.dexterity),
      intelligence: coerceNumber(model.baseAbilityScores.intelligence),
      strength: coerceNumber(model.baseAbilityScores.strength),
      will: coerceNumber(model.baseAbilityScores.will)
    }
  };
}

/** The default Max HP shown for a chosen class at create (the class's starting HP). */
export function defaultMaxHpForClass(heroClass: HeroClass | ''): number {
  return heroClass === '' ? 0 : startingHp(heroClass);
}
```

- [ ] **Step 2: Replace `NS.Client/src/lib/sheet/build/validate.ts`**:

```ts
import type { HeroBuildModel } from './model';
import { POINT_BUY_BUDGET, POINT_BUY_MAX, POINT_BUY_MIN, totalCost } from './pointBuy';
import { maxHpBounds, playableClasses } from './classDefs';

/** Field-keyed validation messages for the build form. */
export type BuildErrors = Partial<Record<'name' | 'ancestryId' | 'heroClass' | 'baseAbilityScores' | 'maxHp', string>>;

/** Validate the build model. The server remains authoritative. */
export function validateBuild(model: HeroBuildModel, opts: { mode: 'create' | 'edit'; level: number }): BuildErrors {
  const errors: BuildErrors = {};

  if (model.name.trim() === '') {
    errors.name = 'Name is required.';
  }
  if (model.ancestryId === '') {
    errors.ancestryId = 'Select an ancestry.';
  }

  if (opts.mode === 'create') {
    if (model.heroClass === '' || !playableClasses.includes(model.heroClass)) {
      errors.heroClass = 'Select a class.';
    }
    const scores = model.baseAbilityScores;
    const inRange = [scores.dexterity, scores.intelligence, scores.strength, scores.will]
      .every((s) => s >= POINT_BUY_MIN && s <= POINT_BUY_MAX);
    if (!inRange) {
      errors.baseAbilityScores = `Each ability must be between ${POINT_BUY_MIN} and ${POINT_BUY_MAX}.`;
    } else if (totalCost(scores) > POINT_BUY_BUDGET) {
      errors.baseAbilityScores = `Ability scores cost more than ${POINT_BUY_BUDGET} points.`;
    }
  } else {
    if (model.heroClass !== '') {
      const { min, max } = maxHpBounds(model.heroClass, opts.level);
      if (model.maxHp < min || model.maxHp > max) {
        errors.maxHp = `Max HP must be between ${min} and ${max}.`;
      }
    }
  }

  return errors;
}
```

- [ ] **Step 3: Edit `NS.Client/src/lib/sheet/build/options.ts`** — remove the unused `heroClasses`/`statTypes` if no longer referenced after the form rework (keep `dieTypes` only if still used). At minimum, leave the file compiling. Run `grep -rn "heroClasses\|statTypes\|dieTypes" NS.Client/src` to confirm references before deleting any export. (The class list now comes from `playableClasses` in `classDefs.ts`.)

- [ ] **Step 4: Replace `model.test.ts` and `validate.test.ts`** with tests for the new shapes:

`model.test.ts`:

```ts
import { describe, expect, it } from 'vitest';
import { blankBuildModel, defaultMaxHpForClass, heroToBuildModel, normalizeBuild } from './model';

describe('build model', () => {
  it('blank model has class unset and all scores at 8', () => {
    const m = blankBuildModel();
    expect(m.heroClass).toBe('');
    expect(m.baseAbilityScores).toEqual({ dexterity: 8, intelligence: 8, strength: 8, will: 8 });
  });

  it('defaultMaxHpForClass returns the class starting HP', () => {
    expect(defaultMaxHpForClass('Oathsworn')).toBe(17);
    expect(defaultMaxHpForClass('')).toBe(0);
  });

  it('normalizeBuild coerces NaN scores to 0', () => {
    const m = blankBuildModel();
    m.baseAbilityScores.strength = NaN as unknown as number;
    expect(normalizeBuild(m).baseAbilityScores.strength).toBe(0);
  });

  it('heroToBuildModel copies class, base scores and maxHp', () => {
    const hero = {
      name: 'Caldra', ancestryId: 'a1', backgroundId: null, class: 'Mage',
      baseAbilityScores: { dexterity: 10, intelligence: 14, strength: 8, will: 12 }, maxHp: 10
    } as unknown as Parameters<typeof heroToBuildModel>[0];
    const m = heroToBuildModel(hero);
    expect(m.heroClass).toBe('Mage');
    expect(m.baseAbilityScores.intelligence).toBe(14);
    expect(m.maxHp).toBe(10);
  });
});
```

`validate.test.ts`:

```ts
import { describe, expect, it } from 'vitest';
import { validateBuild } from './validate';
import { blankBuildModel } from './model';

describe('validateBuild', () => {
  it('create requires name, ancestry, class', () => {
    const e = validateBuild(blankBuildModel(), { mode: 'create', level: 1 });
    expect(e.name).toBeDefined();
    expect(e.ancestryId).toBeDefined();
    expect(e.heroClass).toBeDefined();
  });

  it('create passes with valid inputs', () => {
    const m = { ...blankBuildModel(), name: 'Caldra', ancestryId: 'a1', heroClass: 'Oathsworn' as const };
    expect(validateBuild(m, { mode: 'create', level: 1 })).toEqual({});
  });

  it('create rejects over-budget scores', () => {
    const m = {
      ...blankBuildModel(), name: 'Caldra', ancestryId: 'a1', heroClass: 'Mage' as const,
      baseAbilityScores: { dexterity: 15, intelligence: 15, strength: 15, will: 9 }
    };
    expect(validateBuild(m, { mode: 'create', level: 1 }).baseAbilityScores).toBeDefined();
  });

  it('edit checks maxHp bounds', () => {
    const m = { ...blankBuildModel(), name: 'Caldra', ancestryId: 'a1', heroClass: 'Oathsworn' as const, maxHp: 100 };
    expect(validateBuild(m, { mode: 'edit', level: 1 }).maxHp).toBeDefined();
    expect(validateBuild({ ...m, maxHp: 17 }, { mode: 'edit', level: 1 }).maxHp).toBeUndefined();
  });
});
```

- [ ] **Step 5: Run tests**

Run: `npm test -- model validate`
Expected: PASS for both files.

- [ ] **Step 6: Commit**

```bash
git add NS.Client/src/lib/sheet/build/model.ts NS.Client/src/lib/sheet/build/validate.ts NS.Client/src/lib/sheet/build/options.ts NS.Client/src/lib/sheet/build/model.test.ts NS.Client/src/lib/sheet/build/validate.test.ts
git commit -m "feat(client): reshape build model and validation to player-set inputs"
```

---

## Task 13: Form sections + HeroBuildForm rework (client)

This task is browser-verified (Svelte components are not unit-tested per project convention). It ends with `npm run check` clean and a build.

**Files:**
- Create: `NS.Client/src/lib/sheet/build/AbilityScoresSection.svelte`
- Modify: `IdentitySection.svelte`, `VitalsSection.svelte`, `HeroBuildForm.svelte`, `new/+page.svelte`, `[id]/edit/+page.svelte`
- Delete: `CombatSection.svelte`, `SavesSection.svelte`, `SkillsSection.svelte`, `ClassResourcesSection.svelte`

- [ ] **Step 1: Rewrite `IdentitySection.svelte`** — class limited to the four, disabled on edit:

```svelte
<script lang="ts">
	import type { Ancestry, Background, HeroClass } from '$lib/api/types';
	import { playableClasses } from './classDefs';

	let {
		name = $bindable(),
		ancestryId = $bindable(),
		backgroundId = $bindable(),
		heroClass = $bindable(),
		ancestries,
		backgrounds,
		classLocked,
		errors
	}: {
		name: string;
		ancestryId: string;
		backgroundId: string | null;
		heroClass: HeroClass | '';
		ancestries: Ancestry[];
		backgrounds: Background[];
		classLocked: boolean;
		errors: { name?: string; ancestryId?: string; heroClass?: string };
	} = $props();

	const field = 'mt-1 w-full rounded bg-slate-900 px-2 py-1 text-sm text-white disabled:opacity-60';
	const lbl = 'block text-xs text-slate-400';
</script>

<section class="rounded-lg bg-slate-800 p-4">
	<h2 class="mb-3 text-sm font-semibold uppercase tracking-wide text-slate-300">Identity</h2>
	<div class="grid gap-3 sm:grid-cols-2">
		<label class={lbl}>
			Name
			<input type="text" bind:value={name} class={field} />
			{#if errors.name}<span class="mt-1 block text-[11px] text-red-400">{errors.name}</span>{/if}
		</label>
		<label class={lbl}>
			Class
			<select bind:value={heroClass} class={field} disabled={classLocked}>
				<option value="">— select —</option>
				{#each playableClasses as c (c)}<option value={c}>{c}</option>{/each}
			</select>
			{#if classLocked}<span class="mt-1 block text-[11px] text-slate-500">Class is set at creation and cannot be changed.</span>{/if}
			{#if errors.heroClass}<span class="mt-1 block text-[11px] text-red-400">{errors.heroClass}</span>{/if}
		</label>
		<label class={lbl}>
			Ancestry
			<select bind:value={ancestryId} class={field}>
				<option value="">— select —</option>
				{#each ancestries as a (a.id)}<option value={a.id}>{a.name}</option>{/each}
			</select>
			{#if errors.ancestryId}<span class="mt-1 block text-[11px] text-red-400">{errors.ancestryId}</span>{/if}
		</label>
		<label class={lbl}>
			Background
			<select bind:value={backgroundId} class={field}>
				<option value={null}>— none —</option>
				{#each backgrounds as b (b.id)}<option value={b.id}>{b.name}</option>{/each}
			</select>
		</label>
	</div>
</section>
```

- [ ] **Step 2: Create `AbilityScoresSection.svelte`** — point-buy steppers + live final/modifier preview:

```svelte
<script lang="ts">
	import type { AbilityScores, Ancestry } from '$lib/api/types';
	import { abilityModifier, finalScores } from './classDefs';
	import { canDecrement, canIncrement, remaining, type AbilityKey } from './pointBuy';

	let {
		baseAbilityScores = $bindable(),
		ancestry,
		editable
	}: {
		baseAbilityScores: AbilityScores;
		ancestry: Ancestry | undefined;
		editable: boolean;
	} = $props();

	const rows: { key: AbilityKey; label: string }[] = [
		{ key: 'strength', label: 'STR' },
		{ key: 'dexterity', label: 'DEX' },
		{ key: 'intelligence', label: 'INT' },
		{ key: 'will', label: 'WIL' }
	];

	const zero: AbilityScores = { dexterity: 0, intelligence: 0, strength: 0, will: 0 };
	const bonuses = $derived(ancestry?.abilityBonuses ?? zero);
	const final = $derived(finalScores(baseAbilityScores, bonuses));
	const left = $derived(remaining(baseAbilityScores));

	function inc(key: AbilityKey) {
		if (editable && canIncrement(baseAbilityScores, key)) {
			baseAbilityScores = { ...baseAbilityScores, [key]: baseAbilityScores[key] + 1 };
		}
	}
	function dec(key: AbilityKey) {
		if (editable && canDecrement(baseAbilityScores, key)) {
			baseAbilityScores = { ...baseAbilityScores, [key]: baseAbilityScores[key] - 1 };
		}
	}
	const sign = (n: number) => (n >= 0 ? `+${n}` : `${n}`);
	const btn = 'h-6 w-6 rounded bg-slate-700 text-sm font-bold text-white hover:bg-slate-600 disabled:opacity-40';
</script>

<section class="rounded-lg bg-slate-800 p-4">
	<div class="mb-3 flex items-center justify-between">
		<h2 class="text-sm font-semibold uppercase tracking-wide text-slate-300">Ability Scores</h2>
		{#if editable}
			<span class="text-xs {left < 0 ? 'text-red-400' : 'text-slate-400'}">Points left: {left}</span>
		{/if}
	</div>
	<div class="grid grid-cols-1 gap-2 sm:grid-cols-2">
		{#each rows as row (row.key)}
			<div class="flex items-center justify-between rounded bg-slate-900 px-3 py-2">
				<span class="text-xs font-semibold text-slate-300">{row.label}</span>
				<div class="flex items-center gap-2">
					{#if editable}
						<button type="button" class={btn} aria-label={`Decrease ${row.label}`} disabled={!canDecrement(baseAbilityScores, row.key)} onclick={() => dec(row.key)}>−</button>
					{/if}
					<span class="w-6 text-center text-sm font-bold text-white">{baseAbilityScores[row.key]}</span>
					{#if editable}
						<button type="button" class={btn} aria-label={`Increase ${row.label}`} disabled={!canIncrement(baseAbilityScores, row.key)} onclick={() => inc(row.key)}>+</button>
					{/if}
					<span class="ml-2 w-20 text-right text-[11px] text-slate-400">
						final {final[row.key]} ({sign(abilityModifier(final[row.key]))})
					</span>
				</div>
			</div>
		{/each}
	</div>
	{#if !editable}
		<p class="mt-2 text-[11px] text-slate-500">Ability scores are set at creation.</p>
	{/if}
</section>
```

- [ ] **Step 3: Rewrite `VitalsSection.svelte`** — derived HP/mana display, bounded HP on edit:

```svelte
<script lang="ts">
	import type { AbilityScores, HeroClass } from '$lib/api/types';
	import { maxHpBounds, previewMaxMana, startingHp } from './classDefs';

	let {
		maxHp = $bindable(),
		heroClass,
		finalScores: final,
		mode,
		level,
		errors
	}: {
		maxHp: number;
		heroClass: HeroClass | '';
		finalScores: AbilityScores;
		mode: 'create' | 'edit';
		level: number;
		errors: { maxHp?: string };
	} = $props();

	const hasClass = $derived(heroClass !== '');
	const createHp = $derived(hasClass ? startingHp(heroClass as HeroClass) : 0);
	const mana = $derived(hasClass ? previewMaxMana(heroClass as HeroClass, final, level) : null);
	const bounds = $derived(hasClass ? maxHpBounds(heroClass as HeroClass, level) : { min: 1, max: 1 });

	const field = 'mt-1 w-full rounded bg-slate-900 px-2 py-1 text-sm text-white';
	const lbl = 'block text-xs text-slate-400';
</script>

<section class="rounded-lg bg-slate-800 p-4">
	<h2 class="mb-3 text-sm font-semibold uppercase tracking-wide text-slate-300">Vitals</h2>
	<div class="grid gap-3 sm:grid-cols-2">
		<div class={lbl}>
			Max HP
			{#if mode === 'create'}
				<div class="mt-1 rounded bg-slate-900 px-2 py-1 text-sm text-white">{createHp || '—'}</div>
				<span class="mt-1 block text-[11px] text-slate-500">Set by class.</span>
			{:else}
				<input type="number" min={bounds.min} max={bounds.max} bind:value={maxHp} class={field} />
				<span class="mt-1 block text-[11px] text-slate-500">Allowed {bounds.min}–{bounds.max} at level {level}.</span>
				{#if errors.maxHp}<span class="mt-1 block text-[11px] text-red-400">{errors.maxHp}</span>{/if}
			{/if}
		</div>
		{#if mana !== null}
			<div class={lbl}>
				Max mana
				<div class="mt-1 rounded bg-slate-900 px-2 py-1 text-sm text-white">{mana}</div>
				<span class="mt-1 block text-[11px] text-slate-500">Set by class.</span>
			</div>
		{/if}
	</div>
</section>
```

- [ ] **Step 4: Rewrite `HeroBuildForm.svelte`** — new sections, `mode`/`level`, derived preview, ancestry lookup:

```svelte
<script lang="ts">
	import type { Ancestry, Background } from '$lib/api/types';
	import { ApiError } from '$lib/api/client';
	import { normalizeBuild, type HeroBuildModel } from './model';
	import { validateBuild, type BuildErrors } from './validate';
	import { finalScores } from './classDefs';
	import IdentitySection from './IdentitySection.svelte';
	import AbilityScoresSection from './AbilityScoresSection.svelte';
	import VitalsSection from './VitalsSection.svelte';

	let {
		initial,
		ancestries,
		backgrounds,
		submitLabel,
		mode,
		level = 1,
		onsubmit
	}: {
		initial: HeroBuildModel;
		ancestries: Ancestry[];
		backgrounds: Background[];
		submitLabel: string;
		mode: 'create' | 'edit';
		level?: number;
		onsubmit: (model: HeroBuildModel) => Promise<void>;
	} = $props();

	// svelte-ignore state_referenced_locally
	let model = $state<HeroBuildModel>(structuredClone(initial));
	let errors = $state<BuildErrors>({});
	let busy = $state(false);
	let formError = $state<string | null>(null);

	const zero = { dexterity: 0, intelligence: 0, strength: 0, will: 0 };
	const selectedAncestry = $derived(ancestries.find((a) => a.id === model.ancestryId));
	const previewFinal = $derived(finalScores(model.baseAbilityScores, selectedAncestry?.abilityBonuses ?? zero));

	async function handleSubmit(event: SubmitEvent) {
		event.preventDefault();
		errors = validateBuild(model, { mode, level });
		if (Object.keys(errors).length > 0) {
			return;
		}
		busy = true;
		formError = null;
		try {
			await onsubmit(normalizeBuild($state.snapshot(model) as HeroBuildModel));
		} catch (e) {
			formError = e instanceof ApiError ? e.message : 'Save failed.';
		} finally {
			busy = false;
		}
	}
</script>

<form onsubmit={handleSubmit} class="mx-auto max-w-3xl space-y-4 px-4 py-8">
	<IdentitySection
		bind:name={model.name}
		bind:ancestryId={model.ancestryId}
		bind:backgroundId={model.backgroundId}
		bind:heroClass={model.heroClass}
		{ancestries}
		{backgrounds}
		classLocked={mode === 'edit'}
		{errors}
	/>
	<AbilityScoresSection
		bind:baseAbilityScores={model.baseAbilityScores}
		ancestry={selectedAncestry}
		editable={mode === 'create'}
	/>
	<VitalsSection
		bind:maxHp={model.maxHp}
		heroClass={model.heroClass}
		finalScores={previewFinal}
		{mode}
		{level}
		{errors}
	/>

	{#if errors.baseAbilityScores}<p class="text-sm text-red-400">{errors.baseAbilityScores}</p>{/if}
	{#if formError}<p class="text-sm text-red-400">{formError}</p>{/if}
	<button
		type="submit"
		disabled={busy}
		class="rounded bg-blue-700 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-600 disabled:opacity-50"
	>
		{submitLabel}
	</button>
</form>
```

- [ ] **Step 5: Update the create page** `NS.Client/src/routes/(app)/heroes/new/+page.svelte` — add `mode="create"`:

Change the `<HeroBuildForm ... />` usage to include `mode="create"`:

```svelte
<HeroBuildForm
	initial={blankBuildModel()}
	ancestries={data.ancestries}
	backgrounds={data.backgrounds}
	submitLabel="Create hero"
	mode="create"
	onsubmit={submit}
/>
```

- [ ] **Step 6: Update the edit page** `NS.Client/src/routes/(app)/heroes/[id]/edit/+page.svelte` — add `mode="edit"` and `level`:

```svelte
<HeroBuildForm
	initial={heroToBuildModel(data.hero)}
	ancestries={data.ancestries}
	backgrounds={data.backgrounds}
	submitLabel="Save changes"
	mode="edit"
	level={data.hero.level}
	onsubmit={submit}
/>
```

- [ ] **Step 7: Delete the removed sections**

```bash
git rm NS.Client/src/lib/sheet/build/CombatSection.svelte NS.Client/src/lib/sheet/build/SavesSection.svelte NS.Client/src/lib/sheet/build/SkillsSection.svelte NS.Client/src/lib/sheet/build/ClassResourcesSection.svelte
```

- [ ] **Step 8: Type-check**

Run (from `NS.Client/`): `npm run check`
Expected: 0 errors, 0 warnings. Fix any remaining references to the deleted sections or the old model fields.

- [ ] **Step 9: Commit**

```bash
git add NS.Client/src/lib/sheet/build/ NS.Client/src/routes/
git commit -m "feat(client): rework build form to player-set inputs with point-buy"
```

---

## Task 14: Update the Caldra fixture (client)

**Files:**
- Modify: `NS.Client/src/lib/fixtures/caldra.ts`
- Test: `npm test` (resolver + any tests using the fixture must pass)

- [ ] **Step 1: Edit `caldra.ts`**
  - Add `baseAbilityScores` to the hero object (alphabetical, after `backgroundId`). Choose scores consistent with the fixture's existing modifiers (`stats: { dexterity: 0, intelligence: -1, strength: 2, will: 2 }`): base that yields those modifiers with zero ancestry bonus → DEX 10 (0), INT 8 (−1), STR 14 (2), WIL 14 (2):

```ts
  baseAbilityScores: { dexterity: 10, intelligence: 8, strength: 14, will: 14 },
```

  - Add `abilityBonuses` to the fixture's `Human` ancestry object:

```ts
    { id: ancestryHumanId, name: 'Human', description: 'Versatile and ambitious.', traits: ['Adaptable'], abilityBonuses: { dexterity: 0, intelligence: 0, strength: 0, will: 0 } }
```

- [ ] **Step 2: Run the client suite**

Run: `npm test`
Expected: PASS (all files). Fix any type errors the fixture surfaces.

- [ ] **Step 3: Commit**

```bash
git add NS.Client/src/lib/fixtures/caldra.ts
git commit -m "test(client): add base ability scores and ancestry bonuses to the Caldra fixture"
```

---

## Task 15: Reseed, build, and browser-verify

**Files:** none (verification + deploy).

- [ ] **Step 1: Full backend + client test sweep**

Run: `dotnet test` (expect green) and, from `NS.Client/`, `npm test` and `npm run check` (expect green).

- [ ] **Step 2: Reseed the dev database** (ancestry schema changed)

```bash
rm -f NS.WebApp/nimble-sheet.db NS.WebApp/nimble-sheet.db-shm NS.WebApp/nimble-sheet.db-wal
```

- [ ] **Step 3: Rebuild the SPA into wwwroot and run the server**

```bash
rm -rf NS.WebApp/wwwroot
cd NS.Client && npm run build && cd ..
mkdir -p NS.WebApp/wwwroot && cp -r NS.Client/build/* NS.WebApp/wwwroot/
ASPNETCORE_URLS=http://localhost:5197 ASPNETCORE_ENVIRONMENT=Development dotnet run --project NS.WebApp
```

- [ ] **Step 4: Browser-verify (manual or Playwright via the installed Edge channel)** at `http://localhost:5197`:
  - **Create:** class selector starts unset and offers only Cheat/Hunter/Mage/Oathsworn; point-buy enforces 27 points and 8–15; Combat/Saves/Skills/Class-Resources sections are gone; Max HP shows the class starting HP read-only; mana appears only for Mage (and not for a level-1 Oathsworn). Submit → the sheet shows derived modifiers, skills (keyed to abilities), saves, HP, and (for Oathsworn) Judgment Dice / Lay on Hands.
  - **Edit:** class is disabled; ability scores read-only with final+modifier shown; changing ancestry re-derives on save; Max HP input is clamped to the class+level bounds and rejects out-of-range with a 400 message.

- [ ] **Step 5: Commit** any verification fixups (none expected). Then summarize results.

---

## Self-Review Notes (for the implementer)

- **Spec coverage:** Player-set vs. derived (Tasks 5–7, 13); point-buy (Tasks 1, 10, 12, 13); ancestry bonuses (Tasks 4, 9, 14); per-class HP/saves/mana/resources incl. Oathsworn & Mage onset levels (Tasks 2–3); split create/update with immutable class & base scores (Tasks 6–7, 9, 13); Max-HP bounds on edit (Tasks 3, 7, 12, 13); removed form sections + class list scoping (Tasks 12–13); seed reseed (Task 15); level-up left untouched (no task touches `LevelUpControls`/level-up endpoints — the accepted temporary inconsistency).
- **FastEndpoints API:** confirm `Send.ResponseAsync`/`Send.NoContentAsync`/`Send.NotFoundAsync` and the `AddError(...) + ThrowIfAnyErrors()` pattern match the codebase (CLAUDE.md documents both); adjust `Send.ErrorsAsync` usage in Task 7 Step 1 accordingly.
- **Type consistency:** `AbilityScores` (C# `Dexterity/Intelligence/Strength/Will`; TS `dexterity/intelligence/strength/will`); `Hero.Create`/`UpdateBuild` new signatures used identically in endpoints and tests; `CreateHeroRequest`/`UpdateHeroRequest` fields match between server records and client interfaces and the wrapper bodies.
