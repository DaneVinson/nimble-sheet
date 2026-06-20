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
