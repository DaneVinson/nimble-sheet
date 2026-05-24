namespace NS.Domain;

/// <summary>A status condition that can affect a hero or creature during play.</summary>
/// <param name="Description">The full mechanical description of what the condition does.</param>
/// <param name="Id">The unique identifier.</param>
/// <param name="Name">The condition name, e.g. "Prone", "Grappled", "Dying", "Smoldering".</param>
public sealed record Condition(
    string Description,
    Guid Id,
    string Name);
