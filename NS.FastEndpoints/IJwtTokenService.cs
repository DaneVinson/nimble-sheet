namespace NSFastEndpoints;

/// <summary>Defines JWT token creation operations.</summary>
public interface IJwtTokenService
{
    /// <summary>Creates a signed JWT for the specified user.</summary>
    /// <param name="user">The authenticated user to create a token for.</param>
    string CreateToken(User user);
}
