namespace NS.Domain;

/// <summary>An armor or shield item that can be equipped by a hero.</summary>
/// <param name="ArmorType">The category of armor.</param>
/// <param name="ArmorValue">The flat damage reduction provided when the hero uses the Defend reaction.</param>
/// <param name="Description">A description of the armor.</param>
/// <param name="Id">The unique identifier.</param>
/// <param name="Name">The armor's name, e.g. "Leather Armor", "Rusty Mail".</param>
public sealed record Armor(
    ArmorType ArmorType,
    int ArmorValue,
    string Description,
    Guid Id,
    string Name);
