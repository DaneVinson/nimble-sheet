namespace NS.Domain;

/// <summary>A weapon a hero can equip and use in combat.</summary>
/// <param name="DamageExpression">The damage roll expression, e.g. "1d6+STR", "2d6".</param>
/// <param name="DamageType">The type of damage the weapon deals.</param>
/// <param name="Description">A description of the weapon.</param>
/// <param name="Id">The unique identifier.</param>
/// <param name="IsRare">Whether the weapon is a rare or magical weapon.</param>
/// <param name="IsTwoHanded">Whether the weapon requires two hands to wield.</param>
/// <param name="Name">The weapon's name, e.g. "Dagger", "Shortbow", "Manglemaul".</param>
/// <param name="Range">The range in spaces for ranged weapons; <see langword="null"/> for melee weapons.</param>
/// <param name="Reach">The melee reach of the weapon in spaces; 1 for standard weapons.</param>
/// <param name="SpecialEffect">Any special on-hit or triggered effect; <see langword="null"/> if none.</param>
/// <param name="StatUsed">Whether the weapon uses <see cref="StatType.Strength"/> or <see cref="StatType.Dexterity"/> for attack and damage rolls.</param>
public sealed record Weapon(
    string DamageExpression,
    DamageType DamageType,
    string Description,
    Guid Id,
    bool IsRare,
    bool IsTwoHanded,
    string Name,
    int? Range,
    int Reach,
    string? SpecialEffect,
    StatType StatUsed);
