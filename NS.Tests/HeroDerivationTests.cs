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
