namespace NSFastEndpoints;

/// <summary>Inflicts one wound on the hero.</summary>
public sealed class GainWoundEndpoint : Endpoint<HeroIdRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public GainWoundEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/gain-wound");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(HeroIdRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetByIdAsync(req.HeroId);
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.GainWound();
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}
