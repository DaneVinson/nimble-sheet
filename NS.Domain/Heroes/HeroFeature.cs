namespace NS.Domain;

/// <summary>A class feature unlocked by a hero, including any choices made when it was gained.</summary>
/// <param name="Choices">The options selected when this feature was gained, e.g. which Underhanded Ability or Thrill of the Hunt ability was chosen.</param>
/// <param name="FeatureId">The identifier of the referenced <see cref="Feature"/> entity.</param>
/// <param name="HeroId">The identifier of the owning hero.</param>
/// <param name="LevelGained">The level at which the hero unlocked this feature.</param>
public sealed record HeroFeature(
    IReadOnlyList<string> Choices,
    Guid FeatureId,
    Guid HeroId,
    int LevelGained);
