namespace NSFastEndpoints;

/// <summary>Spends the specified amount of mana.</summary>
public sealed class SpendManaEndpoint : Endpoint<SpendManaRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public SpendManaEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/spend-mana");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(SpendManaRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetOwnedByIdAsync(req.HeroId, User.GetUserId());
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.SpendMana(req.Amount);
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for spending mana.</summary>
/// <param name="HeroId">The hero's unique identifier (route).</param>
/// <param name="Amount">The amount of mana to spend.</param>
public sealed record SpendManaRequest(Guid HeroId, int Amount);

/// <summary>Validates <see cref="SpendManaRequest"/>.</summary>
public sealed class SpendManaValidator : Validator<SpendManaRequest>
{
    /// <summary>Initializes validation rules for spending mana.</summary>
    public SpendManaValidator()
    {
        RuleFor(r => r.Amount).GreaterThan(0);
    }
}
