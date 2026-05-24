namespace NS.Domain;

/// <summary>A spell known by a hero.</summary>
/// <param name="HeroId">The identifier of the owning hero.</param>
/// <param name="Notes">Optional personal notes the player has recorded about this spell.</param>
/// <param name="SpellId">The identifier of the referenced <see cref="Spell"/> entity.</param>
/// <param name="TierUnlocked">The highest tier the hero can currently cast from this spell's school.</param>
public sealed record HeroSpell(
    Guid HeroId,
    string? Notes,
    Guid SpellId,
    int TierUnlocked);
