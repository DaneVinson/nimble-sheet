namespace NSFastEndpoints;

/// <summary>Spends hit dice and heals the hero by the rolled amount.</summary>
public sealed class SpendHitDiceEndpoint : Endpoint<SpendHitDiceRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public SpendHitDiceEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/spend-hit-dice");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(SpendHitDiceRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetOwnedByIdAsync(req.HeroId, User.GetUserId());
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.SpendHitDice(req.Count, req.HealingAmount);
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for spending hit dice.</summary>
/// <param name="HeroId">The hero's unique identifier (route).</param>
/// <param name="Count">The number of hit dice to spend.</param>
/// <param name="HealingAmount">The total healing rolled on the hit dice.</param>
public sealed record SpendHitDiceRequest(Guid HeroId, int Count, int HealingAmount);

/// <summary>Validates <see cref="SpendHitDiceRequest"/>.</summary>
public sealed class SpendHitDiceValidator : Validator<SpendHitDiceRequest>
{
    /// <summary>Initializes validation rules for spending hit dice.</summary>
    public SpendHitDiceValidator()
    {
        RuleFor(r => r.Count).GreaterThan(0);
        RuleFor(r => r.HealingAmount).GreaterThanOrEqualTo(0);
    }
}
