namespace NS.Domain;

/// <summary>A background representing a hero's history before adventuring and what it grants them.</summary>
/// <param name="Description">The narrative description of the background.</param>
/// <param name="Grants">A summary of the mechanical benefit or starting equipment the background provides.</param>
/// <param name="Id">The unique identifier.</param>
/// <param name="Name">The background name.</param>
public sealed record Background(
    string Description,
    string Grants,
    Guid Id,
    string Name);
