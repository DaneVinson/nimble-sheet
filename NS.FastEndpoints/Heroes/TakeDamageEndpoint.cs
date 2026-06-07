namespace NSFastEndpoints;

/// <summary>Reduces the hero's hit points by the specified amount.</summary>
public sealed class TakeDamageEndpoint : Endpoint<TakeDamageRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public TakeDamageEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/take-damage");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(TakeDamageRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetOwnedByIdAsync(req.HeroId, User.GetUserId());
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.TakeDamage(req.Amount);
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for applying damage to a hero.</summary>
/// <param name="HeroId">The hero's unique identifier (route).</param>
/// <param name="Amount">The amount of damage to apply.</param>
public sealed record TakeDamageRequest(Guid HeroId, int Amount);
