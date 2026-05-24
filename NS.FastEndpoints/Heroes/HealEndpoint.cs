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
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(HealRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetByIdAsync(req.HeroId);
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
