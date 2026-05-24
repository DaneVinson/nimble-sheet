namespace NS.Domain;

/// <summary>A magic item carried or equipped by a hero.</summary>
/// <param name="ChargesRemaining">The remaining charges on the item; <see langword="null"/> if the item has no charges.</param>
/// <param name="HeroId">The identifier of the owning hero.</param>
/// <param name="IsEquipped">Whether the item is currently equipped or worn.</param>
/// <param name="MagicItemId">The identifier of the referenced <see cref="MagicItem"/> entity.</param>
public sealed record HeroMagicItem(
    int? ChargesRemaining,
    Guid HeroId,
    bool IsEquipped,
    Guid MagicItemId);
