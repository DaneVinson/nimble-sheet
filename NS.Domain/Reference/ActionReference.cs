namespace NS.Domain;

/// <summary>A quick-reference entry describing a standard combat action or reaction a hero can take.</summary>
/// <param name="ActionType">Whether the action is heroic, a reaction, or free.</param>
/// <param name="Cost">The number of actions required to perform this action.</param>
/// <param name="Description">The full mechanical description of the action.</param>
/// <param name="FrequencyLimit">How often the action may be used, e.g. "1/round"; <see langword="null"/> if unlimited.</param>
/// <param name="Id">The unique identifier.</param>
/// <param name="Name">The name of the action, e.g. "Attack", "Defend", "Assess".</param>
public sealed record ActionReference(
    ActionType ActionType,
    int Cost,
    string Description,
    string? FrequencyLimit,
    Guid Id,
    string Name);
