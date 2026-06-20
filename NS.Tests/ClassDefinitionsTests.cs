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
