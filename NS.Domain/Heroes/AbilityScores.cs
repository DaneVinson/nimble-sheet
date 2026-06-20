namespace NS.Domain;

/// <summary>A set of ability scores or score adjustments, by stat.</summary>
/// <param name="Dexterity">The Dexterity value.</param>
/// <param name="Intelligence">The Intelligence value.</param>
/// <param name="Strength">The Strength value.</param>
/// <param name="Will">The Will value.</param>
public sealed record AbilityScores(int Dexterity, int Intelligence, int Strength, int Will);
