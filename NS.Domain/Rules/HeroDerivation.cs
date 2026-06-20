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
