namespace NSFastEndpoints;

/// <summary>Heals one wound from the hero.</summary>
public sealed class HealWoundEndpoint : Endpoint<HeroIdRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public HealWoundEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/heal-wound");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(HeroIdRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetOwnedByIdAsync(req.HeroId, User.GetUserId());
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.HealWound();
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}
