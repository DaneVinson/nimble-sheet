namespace NSFastEndpoints;

/// <summary>Applies a +1 increase to the specified stat and clears the pending stat increase flag.</summary>
public sealed class ApplyStatIncreaseEndpoint : Endpoint<ApplyStatIncreaseRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public ApplyStatIncreaseEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/apply-stat-increase");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(ApplyStatIncreaseRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetByIdAsync(req.HeroId);
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.ApplyStatIncrease(req.Stat);
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for applying a stat increase.</summary>
/// <param name="HeroId">The hero's unique identifier (route).</param>
/// <param name="Stat">The stat to increase by +1.</param>
public sealed record ApplyStatIncreaseRequest(Guid HeroId, StatType Stat);
