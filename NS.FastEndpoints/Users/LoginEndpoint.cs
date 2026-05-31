namespace NSFastEndpoints;

/// <summary>Authenticates a user by name and returns a signed JWT.</summary>
public sealed class LoginEndpoint : Endpoint<LoginRequest, LoginResponse>
{
    private readonly IJwtTokenService _jwt;
    private readonly IUserDataService _users;

    /// <summary>Initializes the endpoint with required services.</summary>
    public LoginEndpoint(IJwtTokenService jwt, IUserDataService users)
    {
        _jwt = jwt;
        _users = users;
    }

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("users/login");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var matches = await _users.FindByNameAsync(req.Name);
        var user = matches.FirstOrDefault(u => u.Name.Equals(req.Name, StringComparison.OrdinalIgnoreCase));
        if (user is null) { await Send.UnauthorizedAsync(ct); return; }
        await Send.OkAsync(new LoginResponse(_jwt.CreateToken(user), user.Id), ct);
    }
}

/// <summary>Request payload for authenticating a user.</summary>
/// <param name="Name">The user's display name.</param>
public sealed record LoginRequest(string Name);

/// <summary>Response returned after successful authentication.</summary>
/// <param name="Token">The JWT bearer token.</param>
/// <param name="UserId">The authenticated user's unique identifier.</param>
public sealed record LoginResponse(string Token, Guid UserId);

/// <summary>Validates <see cref="LoginRequest"/>.</summary>
public sealed class LoginValidator : Validator<LoginRequest>
{
    /// <summary>Initializes login validation rules.</summary>
    public LoginValidator()
    {
        RuleFor(r => r.Name).NotEmpty();
    }
}
