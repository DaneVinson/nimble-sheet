namespace NS.Domain;

/// <summary>A mundane gear item in a hero's inventory.</summary>
/// <param name="HeroId">The identifier of the owning hero.</param>
/// <param name="Name">The name of the item, e.g. "Rope", "Chalk", "Rations".</param>
/// <param name="Quantity">The number of this item the hero is carrying.</param>
public sealed record HeroGearItem(
    Guid HeroId,
    string Name,
    int Quantity);
