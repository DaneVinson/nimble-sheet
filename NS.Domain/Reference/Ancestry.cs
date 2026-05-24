namespace NS.Domain;

/// <summary>An ancestry available to heroes in Nimble, providing narrative identity and passive traits.</summary>
/// <param name="Description">The narrative and mechanical description of the ancestry.</param>
/// <param name="Id">The unique identifier.</param>
/// <param name="Name">The ancestry name, e.g. "Human", "Elf".</param>
/// <param name="Traits">The ancestry's passive traits or abilities.</param>
public sealed record Ancestry(
    string Description,
    Guid Id,
    string Name,
    IReadOnlyList<string> Traits);
