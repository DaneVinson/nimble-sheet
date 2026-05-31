namespace NSFastEndpoints;

/// <summary>Increases the hero's maximum and current hit points, typically applied from a level-up HP roll.</summary>
public sealed class ApplyHpIncreaseEndpoint : Endpoint<ApplyHpIncreaseRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public ApplyHpIncreaseEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/apply-hp-increase");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(ApplyHpIncreaseRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetByIdAsync(req.HeroId);
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.ApplyHpIncrease(req.Amount);
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for applying a hit point increase.</summary>
/// <param name="HeroId">The hero's unique identifier (route).</param>
/// <param name="Amount">The number of hit points to add to both current and maximum HP.</param>
public sealed record ApplyHpIncreaseRequest(Guid HeroId, int Amount);
