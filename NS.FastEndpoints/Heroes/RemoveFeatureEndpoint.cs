namespace NSFastEndpoints;

/// <summary>Removes a class feature from the hero.</summary>
public sealed class RemoveFeatureEndpoint : Endpoint<RemoveFeatureRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public RemoveFeatureEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/remove-feature");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(RemoveFeatureRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetOwnedByIdAsync(req.HeroId, User.GetUserId());
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.RemoveFeature(req.FeatureId);
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for removing a feature from a hero.</summary>
/// <param name="HeroId">The hero's unique identifier (route).</param>
/// <param name="FeatureId">The identifier of the feature to remove.</param>
public sealed record RemoveFeatureRequest(Guid HeroId, Guid FeatureId);
