namespace NS.Domain;

/// <summary>A quick-reference rule summary accessible during play so the player never needs the rulebook.</summary>
/// <param name="Category">The thematic category of the rule.</param>
/// <param name="Description">The full text of the rule summary.</param>
/// <param name="Id">The unique identifier.</param>
/// <param name="Name">The rule's name, e.g. "Dying", "Safe Rest", "Leveling Up".</param>
public sealed record RuleReference(
    RuleCategory Category,
    string Description,
    Guid Id,
    string Name);
