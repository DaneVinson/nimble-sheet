namespace NSFastEndpoints;

/// <summary>Creates a new user.</summary>
public sealed class CreateUserEndpoint : Endpoint<CreateUserRequest, CreateUserResponse>
{
    private readonly IUserDataService _users;

    /// <summary>Initializes the endpoint with the user data service.</summary>
    public CreateUserEndpoint(IUserDataService users) => _users = users;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("users");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(CreateUserRequest req, CancellationToken ct)
    {
        var existing = await _users.FindByNameAsync(req.Name);
        if (existing.Any(u => u.Name.Equals(req.Name, StringComparison.OrdinalIgnoreCase)))
        {
            AddError(r => r.Name, "A user with this name already exists.");
            ThrowIfAnyErrors();
        }

        var user = new User(DateTimeOffset.UtcNow, req.Email, Guid.CreateVersion7(), req.Name);
        await _users.CreateAsync(user);
        await Send.ResponseAsync(new CreateUserResponse(user.Id), 201, ct);
    }
}

/// <summary>Request payload for creating a new user.</summary>
/// <param name="Email">The user's email address.</param>
/// <param name="Name">The user's display name.</param>
public sealed record CreateUserRequest(string Email, string Name);

/// <summary>Response returned after successfully creating a user.</summary>
/// <param name="Id">The newly created user's unique identifier.</param>
public sealed record CreateUserResponse(Guid Id);

/// <summary>Validates <see cref="CreateUserRequest"/>.</summary>
public sealed class CreateUserValidator : Validator<CreateUserRequest>
{
    /// <summary>Initializes validation rules for creating a user.</summary>
    public CreateUserValidator()
    {
        RuleFor(r => r.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(r => r.Name)
            .NotEmpty();
    }
}
