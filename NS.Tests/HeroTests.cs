namespace NS.Tests;

/// <summary>Unit tests for <see cref="Hero"/> domain behavior added in Phase A.</summary>
public sealed class HeroTests
{
    /// <summary>Granting temporary hit points above the current value replaces it.</summary>
    [Fact]
    public void GrantTempHp_WhenGrantingHigher_UsesNewValue()
    {
        var hero = TestHero.Create();

        hero.GrantTempHp(3);
        hero.GrantTempHp(7);

        Assert.Equal(7, hero.TempHp);
    }

    /// <summary>Temporary hit points do not stack; granting a lower value keeps the higher one.</summary>
    [Fact]
    public void GrantTempHp_WhenGrantingLower_KeepsHigherValue()
    {
        var hero = TestHero.Create();

        hero.GrantTempHp(7);
        hero.GrantTempHp(3);

        Assert.Equal(7, hero.TempHp);
    }

    /// <summary>A Safe Rest clears any remaining temporary hit points.</summary>
    [Fact]
    public void RecoverAllResources_ClearsTempHp()
    {
        var hero = TestHero.Create();
        hero.GrantTempHp(5);

        hero.RecoverAllResources();

        Assert.Equal(0, hero.TempHp);
    }

    /// <summary>Damage greater than temporary hit points drains them, then reduces current hit points by the remainder.</summary>
    [Fact]
    public void TakeDamage_WhenDamageExceedsTempHp_SpillsToCurrentHp()
    {
        var hero = TestHero.Create(maxHp: 20);
        hero.GrantTempHp(5);

        hero.TakeDamage(8);

        Assert.Equal(0, hero.TempHp);
        Assert.Equal(17, hero.CurrentHp);
    }

    /// <summary>Damage within the temporary hit point pool is absorbed without touching current hit points.</summary>
    [Fact]
    public void TakeDamage_WhenTempHpCoversDamage_PreservesCurrentHp()
    {
        var hero = TestHero.Create(maxHp: 20);
        hero.GrantTempHp(5);

        hero.TakeDamage(3);

        Assert.Equal(2, hero.TempHp);
        Assert.Equal(20, hero.CurrentHp);
    }

    /// <summary>Updating build attributes preserves play state such as accumulated wounds.</summary>
    [Fact]
    public void UpdateBuild_PreservesPlayState()
    {
        var hero = TestHero.Create(maxHp: 20);
        hero.GainWound();

        UpdateBuildTo(hero, maxHp: 20, name: "Caldra the Bold");

        Assert.Equal(1, hero.CurrentWounds);
        Assert.Equal("Caldra the Bold", hero.Name);
    }

    /// <summary>Lowering the maximum hit points below the current value clamps current hit points down.</summary>
    [Fact]
    public void UpdateBuild_WhenMaxHpLowered_ClampsCurrentHp()
    {
        var hero = TestHero.Create(maxHp: 20);
        hero.TakeDamage(5);

        UpdateBuildTo(hero, maxHp: 10, name: "Caldra");

        Assert.Equal(10, hero.MaxHp);
        Assert.Equal(10, hero.CurrentHp);
    }

    private static void UpdateBuildTo(Hero hero, int maxHp, string name) =>
        hero.UpdateBuild(
            ancestryId: Guid.CreateVersion7(),
            backgroundId: null,
            combatStats: new HeroCombatStats(8, DieType.D10, 0, 6),
            heroClass: HeroClass.Oathsworn,
            maxHp: maxHp,
            maxMana: null,
            name: name,
            resources: new ClassResources(null, null, null, null),
            saves: new HeroSaves(StatType.Will, StatType.Dexterity),
            skills: new HeroSkills(0, 0, 0, 0, 0, 0, 0, 0, 0, 0),
            stats: new HeroStats(0, 0, 0, 0));
}
