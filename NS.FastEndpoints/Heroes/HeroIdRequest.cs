namespace NSFastEndpoints;

/// <summary>A request that identifies a hero by its unique identifier, bound from the route.</summary>
/// <param name="HeroId">The hero's unique identifier.</param>
public sealed record HeroIdRequest(Guid HeroId);
