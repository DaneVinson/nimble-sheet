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
