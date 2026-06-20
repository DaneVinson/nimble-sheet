namespace NSFastEndpoints;

/// <summary>The inputs for creating a hero. Owner is taken from the token, never the body.</summary>
/// <param name="AncestryId">The identifier of the hero's ancestry.</param>
/// <param name="BackgroundId">The optional identifier of the hero's background.</param>
/// <param name="BaseAbilityScores">The player-bought base ability scores (point-buy).</param>
/// <param name="HeroClass">The hero's class (chosen once, at creation).</param>
/// <param name="Name">The hero's name.</param>
public sealed record CreateHeroRequest(
    Guid AncestryId,
    Guid? BackgroundId,
    AbilityScores BaseAbilityScores,
    HeroClass HeroClass,
    string Name);

/// <summary>Validates <see cref="CreateHeroRequest"/>.</summary>
public sealed class CreateHeroValidator : Validator<CreateHeroRequest>
{
    /// <summary>Initializes validation rules for hero creation.</summary>
    public CreateHeroValidator()
    {
        RuleFor(r => r.Name).NotEmpty();
        RuleFor(r => r.AncestryId).NotEmpty();
        RuleFor(r => r.HeroClass)
            .Must(ClassDefinitions.IsPlayable)
            .WithMessage("Class is not a playable class.");
        RuleFor(r => r.BaseAbilityScores)
            .Must(PointBuy.IsValid)
            .WithMessage("Ability scores must be between 8 and 15 and cost at most 27 points.");
    }
}

/// <summary>The inputs for updating a hero. Class and base ability scores are immutable after creation.</summary>
/// <param name="AncestryId">The identifier of the hero's ancestry.</param>
/// <param name="BackgroundId">The optional identifier of the hero's background.</param>
/// <param name="MaxHp">The hero's maximum hit points (bounds-checked against class and level).</param>
/// <param name="Name">The hero's name.</param>
public sealed record UpdateHeroRequest(
    Guid AncestryId,
    Guid? BackgroundId,
    int MaxHp,
    string Name);

/// <summary>Validates <see cref="UpdateHeroRequest"/>. The class+level bounds for <c>MaxHp</c> are
/// checked in the endpoint, which has access to the stored hero.</summary>
public sealed class UpdateHeroValidator : Validator<UpdateHeroRequest>
{
    /// <summary>Initializes validation rules for a hero update.</summary>
    public UpdateHeroValidator()
    {
        RuleFor(r => r.Name).NotEmpty();
        RuleFor(r => r.AncestryId).NotEmpty();
        RuleFor(r => r.MaxHp).GreaterThan(0);
    }
}
