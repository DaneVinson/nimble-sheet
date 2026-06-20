namespace NS.Tests;

/// <summary>Unit tests for the hero create and update request validators.</summary>
public sealed class HeroBuildValidatorTests
{
    private static CreateHeroRequest ValidCreate() => new(
        AncestryId: Guid.CreateVersion7(),
        BackgroundId: null,
        BaseAbilityScores: new AbilityScores(10, 10, 10, 10),
        HeroClass: HeroClass.Oathsworn,
        Name: "Caldra");

    /// <summary>A valid create request passes validation.</summary>
    [Fact]
    public void Create_Valid_PassesValidation()
    {
        var result = new CreateHeroValidator().Validate(ValidCreate());
        Assert.True(result.IsValid);
    }

    /// <summary>An empty name fails validation on the Name field.</summary>
    [Fact]
    public void Create_EmptyName_Fails()
    {
        var result = new CreateHeroValidator().Validate(ValidCreate() with { Name = "" });
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateHeroRequest.Name));
    }

    /// <summary>A non-playable class fails validation on the HeroClass field.</summary>
    [Fact]
    public void Create_NonPlayableClass_Fails()
    {
        var result = new CreateHeroValidator().Validate(ValidCreate() with { HeroClass = HeroClass.Berserker });
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateHeroRequest.HeroClass));
    }

    /// <summary>Over-budget ability scores fail validation on the BaseAbilityScores field.</summary>
    [Fact]
    public void Create_OverBudgetScores_Fails()
    {
        var result = new CreateHeroValidator().Validate(
            ValidCreate() with { BaseAbilityScores = new AbilityScores(15, 15, 15, 9) });
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateHeroRequest.BaseAbilityScores));
    }

    /// <summary>A non-positive MaxHp fails validation on the MaxHp field.</summary>
    [Fact]
    public void Update_NonPositiveMaxHp_Fails()
    {
        var request = new UpdateHeroRequest(Guid.CreateVersion7(), null, 0, "Caldra");
        var result = new UpdateHeroValidator().Validate(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateHeroRequest.MaxHp));
    }
}
