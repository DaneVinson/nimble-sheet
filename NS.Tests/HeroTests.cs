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

    /// <summary>Equipping a weapon the hero owns flips its equipped flag.</summary>
    [Fact]
    public void SetWeaponEquipped_WhenWeaponPresent_UpdatesFlag()
    {
        var hero = TestHero.Create();
        var weaponId = Guid.CreateVersion7();
        hero.AddWeapon(new HeroWeapon(hero.Id, false, null, weaponId));

        hero.SetWeaponEquipped(weaponId, true);

        Assert.True(hero.Weapons.Single().IsEquipped);
    }

    /// <summary>Setting equipped on an unknown weapon id changes nothing.</summary>
    [Fact]
    public void SetWeaponEquipped_WhenWeaponAbsent_IsNoOp()
    {
        var hero = TestHero.Create();
        hero.AddWeapon(new HeroWeapon(hero.Id, true, null, Guid.CreateVersion7()));

        hero.SetWeaponEquipped(Guid.CreateVersion7(), false);

        Assert.True(hero.Weapons.Single().IsEquipped);
    }

    /// <summary>Equipping armor the hero owns flips its equipped flag.</summary>
    [Fact]
    public void SetArmorEquipped_WhenArmorPresent_UpdatesFlag()
    {
        var hero = TestHero.Create();
        var armorId = Guid.CreateVersion7();
        hero.AddArmor(new HeroArmor(armorId, hero.Id, false));

        hero.SetArmorEquipped(armorId, true);

        Assert.True(hero.Armor.Single().IsEquipped);
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
