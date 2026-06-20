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
