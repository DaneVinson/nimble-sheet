namespace NSFastEndpoints;

/// <summary>Returns a single user by identifier.</summary>
public sealed class GetUserEndpoint : Endpoint<UserIdRequest, User>
{
    private readonly IUserDataService _users;

    /// <summary>Initializes the endpoint with the user data service.</summary>
    public GetUserEndpoint(IUserDataService users) => _users = users;

    /// <inheritdoc/>
    public override void Configure()
    {
        Get("users/{userId}");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(UserIdRequest req, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(req.UserId);
        if (user is null) { await Send.NotFoundAsync(ct); return; }
        await Send.OkAsync(user, ct);
    }
}
