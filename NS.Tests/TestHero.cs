namespace NS.Tests;

/// <summary>Factory helpers for constructing <see cref="Hero"/> instances in tests.</summary>
internal static class TestHero
{
    /// <summary>Creates a valid level-1 hero with the specified maximum hit points and owner.</summary>
    /// <param name="maxHp">The hero's starting maximum (and current) hit points.</param>
    /// <param name="userId">The owning user's identifier; a new identifier is generated when omitted.</param>
    internal static Hero Create(int maxHp = 17, Guid? userId = null) =>
        new(
            ancestryId: Guid.CreateVersion7(),
            backgroundId: null,
            combatStats: new HeroCombatStats(8, DieType.D10, 0, 6),
            heroClass: HeroClass.Oathsworn,
            maxHp: maxHp,
            maxMana: null,
            name: "Caldra",
            resources: new ClassResources(null, null, null, null),
            saves: new HeroSaves(StatType.Will, StatType.Dexterity),
            skills: new HeroSkills(0, 0, 0, 0, 0, 0, 0, 0, 0, 0),
            stats: new HeroStats(0, 0, 0, 0),
            userId: userId ?? Guid.CreateVersion7());
}
