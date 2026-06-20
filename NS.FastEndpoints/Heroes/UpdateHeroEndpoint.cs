namespace NSFastEndpoints;

/// <summary>Updates an existing hero's player-set build attributes, preserving level, subclass,
/// play state, and collections.</summary>
public sealed class UpdateHeroEndpoint : Endpoint<UpdateHeroRequest>
{
    private readonly IReferenceDataService<Ancestry> _ancestries;
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero and ancestry data services.</summary>
    /// <param name="ancestries">The ancestry reference-data service.</param>
    /// <param name="heroes">The hero data service.</param>
    public UpdateHeroEndpoint(IReferenceDataService<Ancestry> ancestries, IHeroDataService heroes)
    {
        _ancestries = ancestries;
        _heroes = heroes;
    }

    /// <inheritdoc/>
    public override void Configure()
    {
        Put("heroes/{heroId}");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(UpdateHeroRequest req, CancellationToken ct)
    {
        var heroId = Route<Guid>("heroId");
        var hero = await _heroes.GetOwnedByIdAsync(heroId, User.GetUserId());
        if (hero is null) { await Send.NotFoundAsync(ct); return; }

        var ancestry = await _ancestries.GetByIdAsync(req.AncestryId);
        if (ancestry is null)
        {
            AddError(r => r.AncestryId, "Ancestry not found.");
        }

        var (minHp, maxHp) = HeroDerivation.MaxHpBounds(hero.Class, hero.Level);
        if (req.MaxHp < minHp || req.MaxHp > maxHp)
        {
            AddError(r => r.MaxHp, $"Max HP must be between {minHp} and {maxHp} for this class and level.");
        }

        ThrowIfAnyErrors();

        hero.UpdateBuild(req.Name, req.AncestryId, req.BackgroundId, ancestry!.AbilityBonuses, req.MaxHp);
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}
