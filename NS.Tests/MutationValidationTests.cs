namespace NS.Tests;

/// <summary>Unit tests for the hero play-mutation request validators.</summary>
public sealed class MutationValidationTests
{
    private static readonly Guid HeroId = Guid.CreateVersion7();

    /// <summary>Take-damage rejects a non-positive amount.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void TakeDamage_RejectsNonPositiveAmount(int amount) =>
        Assert.False(new TakeDamageValidator().Validate(new TakeDamageRequest(HeroId, amount)).IsValid);

    /// <summary>Take-damage accepts a positive amount.</summary>
    [Fact]
    public void TakeDamage_AcceptsPositiveAmount() =>
        Assert.True(new TakeDamageValidator().Validate(new TakeDamageRequest(HeroId, 1)).IsValid);

    /// <summary>Heal rejects a non-positive amount.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Heal_RejectsNonPositiveAmount(int amount) =>
        Assert.False(new HealValidator().Validate(new HealRequest(HeroId, amount)).IsValid);

    /// <summary>Spend-mana rejects a non-positive amount.</summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void SpendMana_RejectsNonPositiveAmount(int amount) =>
        Assert.False(new SpendManaValidator().Validate(new SpendManaRequest(HeroId, amount)).IsValid);

    /// <summary>Grant-temp-hp allows zero but rejects a negative amount.</summary>
    [Fact]
    public void GrantTempHp_AllowsZero_RejectsNegative()
    {
        Assert.True(new GrantTempHpValidator().Validate(new GrantTempHpRequest(HeroId, 0)).IsValid);
        Assert.False(new GrantTempHpValidator().Validate(new GrantTempHpRequest(HeroId, -1)).IsValid);
    }

    /// <summary>Spend-hit-dice requires a positive count and a non-negative healing amount.</summary>
    [Fact]
    public void SpendHitDice_ValidatesCountAndHealing()
    {
        Assert.True(new SpendHitDiceValidator().Validate(new SpendHitDiceRequest(HeroId, 1, 0)).IsValid);
        Assert.False(new SpendHitDiceValidator().Validate(new SpendHitDiceRequest(HeroId, 0, 0)).IsValid);
        Assert.False(new SpendHitDiceValidator().Validate(new SpendHitDiceRequest(HeroId, 1, -1)).IsValid);
    }
}
