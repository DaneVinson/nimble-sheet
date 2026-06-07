namespace NSFastEndpoints;

/// <summary>Extension methods for reading claims from a <see cref="ClaimsPrincipal"/>.</summary>
public static class ClaimsPrincipalExtensions
{
    private const string SubjectClaimType = "sub";

    /// <summary>Returns the authenticated user's identifier, read from the JWT <c>sub</c> claim. The host sets <c>MapInboundClaims = false</c>, so the inbound claim type is the literal "sub".</summary>
    public static Guid GetUserId(this ClaimsPrincipal principal) =>
        Guid.Parse(principal.FindFirstValue(SubjectClaimType)!);
}
