namespace NS.Domain;

/// <summary>A complete spell definition in Nimble.</summary>
/// <param name="ActionCost">The number of actions required to cast this spell.</param>
/// <param name="AreaOfEffect">The area covered by the spell, e.g. "3×3 area"; <see langword="null"/> for single-target spells.</param>
/// <param name="DamageExpression">The damage roll expression, e.g. "1d8+INT", "2d6"; <see langword="null"/> for non-damaging spells.</param>
/// <param name="DamageType">The type of damage dealt; <see langword="null"/> for non-damaging spells.</param>
/// <param name="Description">The full mechanical description of the spell.</param>
/// <param name="Duration">The duration of the spell's effect, e.g. "Instant", "Concentration", "1 minute"; <see langword="null"/> for instantaneous effects.</param>
/// <param name="Id">The unique identifier.</param>
/// <param name="IsConcentration">Whether the spell requires concentration to maintain.</param>
/// <param name="IsSecret">Whether the spell is secret or lost knowledge not in general circulation.</param>
/// <param name="ManaCost">The mana required to cast; equals the tier (0 for cantrips).</param>
/// <param name="Name">The spell's name.</param>
/// <param name="Range">The range in spaces; <see langword="null"/> defaults to Reach 1 (melee).</param>
/// <param name="SaveType">The stat the target uses to save against the spell's effect; <see langword="null"/> if no save is required.</param>
/// <param name="School">The school of magic this spell belongs to.</param>
/// <param name="Tier">The spell tier from 0 (cantrip) to 9.</param>
/// <param name="UpcastEffect">The additional effect gained for each extra mana spent above the base cost; <see langword="null"/> if the spell cannot be upcast.</param>
public sealed record Spell(
    int ActionCost,
    string? AreaOfEffect,
    string? DamageExpression,
    DamageType? DamageType,
    string Description,
    string? Duration,
    Guid Id,
    bool IsConcentration,
    bool IsSecret,
    int ManaCost,
    string Name,
    int? Range,
    StatType? SaveType,
    SpellSchool School,
    int Tier,
    string? UpcastEffect);
