namespace NS.Domain;

/// <summary>A weapon carried or wielded by a hero.</summary>
/// <param name="HeroId">The identifier of the owning hero.</param>
/// <param name="IsEquipped">Whether the weapon is currently being wielded.</param>
/// <param name="Notes">Optional notes about the weapon, e.g. a name or story significance.</param>
/// <param name="WeaponId">The identifier of the referenced <see cref="Weapon"/> entity.</param>
public sealed record HeroWeapon(
    Guid HeroId,
    bool IsEquipped,
    string? Notes,
    Guid WeaponId);
