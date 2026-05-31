namespace NSFastEndpoints;

/// <summary>Updates the email address of an existing user.</summary>
public sealed class UpdateUserEmailEndpoint : Endpoint<UpdateUserEmailRequest>
{
    private readonly IUserDataService _users;

    /// <summary>Initializes the endpoint with the user data service.</summary>
    public UpdateUserEmailEndpoint(IUserDataService users) => _users = users;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("users/{userId}/update-email");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(UpdateUserEmailRequest req, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(req.UserId);
        if (user is null) { await Send.NotFoundAsync(ct); return; }
        user.UpdateEmail(req.Email);
        await _users.UpdateAsync(user);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for updating a user's email address.</summary>
/// <param name="Email">The new email address.</param>
/// <param name="UserId">The user's unique identifier (route).</param>
public sealed record UpdateUserEmailRequest(string Email, Guid UserId);

/// <summary>Validates <see cref="UpdateUserEmailRequest"/>.</summary>
public sealed class UpdateUserEmailValidator : Validator<UpdateUserEmailRequest>
{
    /// <summary>Initializes validation rules for updating a user's email.</summary>
    public UpdateUserEmailValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty()
            .EmailAddress();
    }
}
