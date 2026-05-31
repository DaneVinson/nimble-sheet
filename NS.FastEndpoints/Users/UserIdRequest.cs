namespace NSFastEndpoints;

/// <summary>A request that identifies a user by its unique identifier, bound from the route.</summary>
/// <param name="UserId">The user's unique identifier.</param>
public sealed record UserIdRequest(Guid UserId);
