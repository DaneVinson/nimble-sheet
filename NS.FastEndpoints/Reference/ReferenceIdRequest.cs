namespace NSFastEndpoints;

/// <summary>A request that identifies a reference entity by its unique identifier, bound from the route.</summary>
/// <param name="Id">The entity's unique identifier.</param>
public sealed record ReferenceIdRequest(Guid Id);
