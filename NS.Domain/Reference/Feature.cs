namespace NS.Domain;

/// <summary>A class feature or ability that a hero unlocks through leveling or subclass selection.</summary>
/// <param name="Class">The hero class this feature belongs to.</param>
/// <param name="Description">The full mechanical description of the feature.</param>
/// <param name="FrequencyLimit">How often the feature can be used, e.g. "1/turn", "1/encounter"; <see langword="null"/> if passive or unlimited.</param>
/// <param name="Id">The unique identifier.</param>
/// <param name="Level">The hero level at which this feature is gained.</param>
/// <param name="Name">The feature's name.</param>
/// <param name="SelectableOptions">Named options the player chooses from when gaining this feature, e.g. Underhanded Abilities or Thrill of the Hunt abilities; <see langword="null"/> if no choice is required.</param>
/// <param name="Subclass">The subclass this feature belongs to; <see langword="null"/> for base class features available to all.</param>
public sealed record Feature(
    HeroClass Class,
    string Description,
    string? FrequencyLimit,
    Guid Id,
    int Level,
    string Name,
    IReadOnlyList<string>? SelectableOptions,
    string? Subclass);
