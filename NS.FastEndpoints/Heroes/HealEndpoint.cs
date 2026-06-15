namespace NSFastEndpoints;

/// <summary>Restores the specified amount of hit points, up to the hero's maximum.</summary>
public sealed class HealEndpoint : Endpoint<HealRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public HealEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/heal");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(HealRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetOwnedByIdAsync(req.HeroId, User.GetUserId());
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.Heal(req.Amount);
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for healing a hero.</summary>
/// <param name="HeroId">The hero's unique identifier (route).</param>
/// <param name="Amount">The amount of hit points to restore.</param>
public sealed record HealRequest(Guid HeroId, int Amount);

/// <summary>Validates <see cref="HealRequest"/>.</summary>
public sealed class HealValidator : Validator<HealRequest>
{
    /// <summary>Initializes validation rules for healing.</summary>
    public HealValidator()
    {
        RuleFor(r => r.Amount).GreaterThan(0);
    }
}
