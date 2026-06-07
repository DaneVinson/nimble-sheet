namespace NSFastEndpoints;

/// <summary>Resolves a pending feature choice and grants the associated feature.</summary>
public sealed class CompletePendingChoiceEndpoint : Endpoint<CompletePendingChoiceRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public CompletePendingChoiceEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/complete-pending-choice");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(CompletePendingChoiceRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetOwnedByIdAsync(req.HeroId, User.GetUserId());
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        var feature = new HeroFeature(req.Choices, req.FeatureId, req.HeroId, req.LevelGained);
        hero.CompletePendingChoice(req.ChoiceLabel, feature);
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for completing a pending feature choice.</summary>
/// <param name="HeroId">The hero's unique identifier (route).</param>
/// <param name="ChoiceLabel">The pending choice label being resolved.</param>
/// <param name="Choices">The selections made for any selectable options on the feature.</param>
/// <param name="FeatureId">The identifier of the feature being granted.</param>
/// <param name="LevelGained">The level at which the feature was gained.</param>
public sealed record CompletePendingChoiceRequest(
    Guid HeroId,
    string ChoiceLabel,
    IReadOnlyList<string> Choices,
    Guid FeatureId,
    int LevelGained);
