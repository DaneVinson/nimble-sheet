namespace NS.Domain;

/// <summary>The level-1 stat block for a playable class.</summary>
/// <param name="SaveAdvantage">The stat whose saves are rolled with advantage.</param>
/// <param name="SaveDisadvantage">The stat whose saves are rolled with disadvantage.</param>
/// <param name="Speed">The class's base movement speed in spaces.</param>
/// <param name="StartingHitDie">The class's hit die type.</param>
/// <param name="StartingHp">The class's level-1 starting maximum hit points.</param>
public sealed record ClassDefinition(
    StatType SaveAdvantage,
    StatType SaveDisadvantage,
    int Speed,
    DieType StartingHitDie,
    int StartingHp);
