namespace NSFastEndpoints;

/// <summary>Performs a Safe Rest, restoring all hit points, mana, hit dice, and healing one wound.</summary>
public sealed class RecoverAllResourcesEndpoint : Endpoint<HeroIdRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public RecoverAllResourcesEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/recover-all-resources");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(HeroIdRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetOwnedByIdAsync(req.HeroId, User.GetUserId());
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.RecoverAllResources();
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}
