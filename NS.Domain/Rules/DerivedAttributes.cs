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
