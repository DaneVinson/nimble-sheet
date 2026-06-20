namespace NS.Domain;

/// <summary>A playable ancestry (species) a hero can belong to.</summary>
/// <param name="AbilityBonuses">Ability score bonuses this ancestry grants to a hero's base scores.</param>
/// <param name="Description">The narrative and mechanical description of the ancestry.</param>
/// <param name="Id">The unique identifier.</param>
/// <param name="Name">The ancestry name, e.g. "Human", "Elf".</param>
/// <param name="Traits">The ancestry's passive traits or abilities.</param>
public sealed record Ancestry(
    AbilityScores AbilityBonuses,
    string Description,
    Guid Id,
    string Name,
    IReadOnlyList<string> Traits);
