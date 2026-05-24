namespace NS.Domain;

/// <summary>A non-weapon magical item a hero can carry or equip.</summary>
/// <param name="ContainedSpellId">For wands and spell scrolls, the identifier of the contained <see cref="Spell"/>; <see langword="null"/> for all other items.</param>
/// <param name="Description">The narrative description of the item.</param>
/// <param name="Effect">The full mechanical effect of the item.</param>
/// <param name="Id">The unique identifier.</param>
/// <param name="MaxCharges">The maximum number of charges the item can hold; <see langword="null"/> if the item has no charges.</param>
/// <param name="Name">The item's name, e.g. "Cloak of Lesser Windform", "Golden Acorn".</param>
/// <param name="Rarity">The rarity of the item, e.g. "Common", "Rare", "Legendary".</param>
public sealed record MagicItem(
    Guid? ContainedSpellId,
    string Description,
    string Effect,
    Guid Id,
    int? MaxCharges,
    string Name,
    string Rarity);
