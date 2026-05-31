namespace NSWebApp;

/// <summary>JWT authentication configuration settings.</summary>
public sealed class JwtSettings
{
    /// <summary>The expected token audience.</summary>
    public string Audience { get; init; } = string.Empty;

    /// <summary>The number of hours before issued tokens expire.</summary>
    public int ExpiryHours { get; init; } = 24;

    /// <summary>The token issuer identifier.</summary>
    public string Issuer { get; init; } = string.Empty;

    /// <summary>The HMAC-SHA-256 signing key; must be at least 32 characters.</summary>
    public string SigningKey { get; init; } = string.Empty;
}
