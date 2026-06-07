namespace NSFastEndpoints;

/// <summary>Updates an existing hero's build attributes, preserving level, subclass, play state, and collections.</summary>
public sealed class UpdateHeroEndpoint : Endpoint<HeroBuildRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public UpdateHeroEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Put("heroes/{heroId}");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(HeroBuildRequest req, CancellationToken ct)
    {
        var heroId = Route<Guid>("heroId");
        var hero = await _heroes.GetOwnedByIdAsync(heroId, User.GetUserId());
        if (hero is null) { await Send.NotFoundAsync(ct); return; }

        hero.UpdateBuild(
            req.AncestryId,
            req.BackgroundId,
            req.CombatStats,
            req.HeroClass,
            req.MaxHp,
            req.MaxMana,
            req.Name,
            req.Resources,
            req.Saves,
            req.Skills,
            req.Stats);

        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}
