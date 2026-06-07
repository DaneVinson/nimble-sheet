namespace NSFastEndpoints;

/// <summary>Removes a condition from the hero.</summary>
public sealed class RemoveConditionEndpoint : Endpoint<RemoveConditionRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public RemoveConditionEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/remove-condition");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(RemoveConditionRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetOwnedByIdAsync(req.HeroId, User.GetUserId());
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.RemoveCondition(req.ConditionId);
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for removing a condition from a hero.</summary>
/// <param name="HeroId">The hero's unique identifier (route).</param>
/// <param name="ConditionId">The identifier of the condition to remove.</param>
public sealed record RemoveConditionRequest(Guid HeroId, Guid ConditionId);
