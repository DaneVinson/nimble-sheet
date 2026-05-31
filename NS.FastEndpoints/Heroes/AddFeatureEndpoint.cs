namespace NSFastEndpoints;

/// <summary>Grants a class feature to the hero.</summary>
public sealed class AddFeatureEndpoint : Endpoint<AddFeatureRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public AddFeatureEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/add-feature");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(AddFeatureRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetByIdAsync(req.HeroId);
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.AddFeature(new HeroFeature(req.Choices, req.FeatureId, req.HeroId, req.LevelGained));
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for granting a feature to a hero.</summary>
/// <param name="HeroId">The hero's unique identifier (route).</param>
/// <param name="Choices">The selections made for any selectable options on the feature.</param>
/// <param name="FeatureId">The identifier of the feature being granted.</param>
/// <param name="LevelGained">The level at which the feature was gained.</param>
public sealed record AddFeatureRequest(Guid HeroId, IReadOnlyList<string> Choices, Guid FeatureId, int LevelGained);
