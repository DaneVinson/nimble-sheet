namespace NS.Domain;

/// <summary>The combat statistics derived from a hero's stats and equipped armor.</summary>
/// <param name="Armor">The total armor value applied when the hero uses the Defend reaction.</param>
/// <param name="HitDieType">The die type used for this hero's hit dice.</param>
/// <param name="InitiativeBonus">The bonus added to Initiative rolls.</param>
/// <param name="Speed">The number of spaces the hero can move per Move action; typically 6.</param>
public sealed record HeroCombatStats(
    int Armor,
    DieType HitDieType,
    int InitiativeBonus,
    int Speed);
