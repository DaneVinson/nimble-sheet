namespace NS.Domain;

/// <summary>An active condition currently affecting a hero.</summary>
/// <param name="ConditionId">The identifier of the referenced <see cref="Condition"/> entity.</param>
/// <param name="ExpiresAtEndOf">A description of when the condition expires, e.g. "next turn", "encounter"; <see langword="null"/> if indefinite.</param>
/// <param name="HeroId">The identifier of the affected hero.</param>
public sealed record HeroCondition(
    Guid ConditionId,
    string? ExpiresAtEndOf,
    Guid HeroId);
