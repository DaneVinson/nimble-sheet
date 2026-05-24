namespace NS.Domain;

/// <summary>An armor item carried or worn by a hero.</summary>
/// <param name="ArmorId">The identifier of the referenced <see cref="Armor"/> entity.</param>
/// <param name="HeroId">The identifier of the owning hero.</param>
/// <param name="IsEquipped">Whether the armor is currently being worn.</param>
public sealed record HeroArmor(
    Guid ArmorId,
    Guid HeroId,
    bool IsEquipped);
