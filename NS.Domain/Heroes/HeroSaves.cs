namespace NS.Domain;

/// <summary>The save dispositions of a hero, indicating which saves are rolled with advantage or disadvantage.</summary>
/// <param name="AdvantageOn">The stat type this hero rolls with advantage on saves.</param>
/// <param name="DisadvantageOn">The stat type this hero rolls with disadvantage on saves.</param>
public sealed record HeroSaves(
    StatType AdvantageOn,
    StatType DisadvantageOn);
