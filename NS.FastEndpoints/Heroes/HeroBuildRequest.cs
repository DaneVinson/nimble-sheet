namespace NSFastEndpoints;

/// <summary>The character-build attributes shared by hero creation and update. The owning user is taken from the authenticated token, never the request body.</summary>
/// <param name="AncestryId">The identifier of the hero's ancestry.</param>
/// <param name="BackgroundId">The optional identifier of the hero's background.</param>
/// <param name="CombatStats">The hero's combat statistics.</param>
/// <param name="HeroClass">The hero's class.</param>
/// <param name="MaxHp">The hero's maximum hit points.</param>
/// <param name="MaxMana">The hero's maximum mana; <see langword="null"/> for non-casters.</param>
/// <param name="Name">The hero's name.</param>
/// <param name="Resources">The hero's class-specific resource pools.</param>
/// <param name="Saves">The hero's save advantage and disadvantage types.</param>
/// <param name="Skills">The hero's skill bonuses.</param>
/// <param name="Stats">The hero's base stats.</param>
public sealed record HeroBuildRequest(
    Guid AncestryId,
    Guid? BackgroundId,
    HeroCombatStats CombatStats,
    HeroClass HeroClass,
    int MaxHp,
    int? MaxMana,
    string Name,
    ClassResources Resources,
    HeroSaves Saves,
    HeroSkills Skills,
    HeroStats Stats);

/// <summary>Validates <see cref="HeroBuildRequest"/>.</summary>
public sealed class HeroBuildValidator : Validator<HeroBuildRequest>
{
    /// <summary>Initializes validation rules for a hero build.</summary>
    public HeroBuildValidator()
    {
        RuleFor(r => r.Name).NotEmpty();
        RuleFor(r => r.MaxHp).GreaterThan(0);
    }
}
