namespace NSWebApp;

/// <summary>Creates signed JWT tokens for authenticated users.</summary>
public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;

    /// <summary>Initializes the service with JWT configuration.</summary>
    public JwtTokenService(IOptions<JwtSettings> settings) => _settings = settings.Value;

    /// <inheritdoc/>
    public string CreateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SigningKey));
        var descriptor = new SecurityTokenDescriptor
        {
            Audience = _settings.Audience,
            Expires = DateTime.UtcNow.AddHours(_settings.ExpiryHours),
            Issuer = _settings.Issuer,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
            Subject = new ClaimsIdentity([
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.Name),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            ]),
        };
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
