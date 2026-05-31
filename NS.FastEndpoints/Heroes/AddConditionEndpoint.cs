namespace NSFastEndpoints;

/// <summary>Applies a condition to the hero.</summary>
public sealed class AddConditionEndpoint : Endpoint<AddConditionRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public AddConditionEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/add-condition");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(AddConditionRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetByIdAsync(req.HeroId);
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.AddCondition(new HeroCondition(req.ConditionId, req.ExpiresAtEndOf, req.HeroId));
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for applying a condition to a hero.</summary>
/// <param name="HeroId">The hero's unique identifier (route).</param>
/// <param name="ConditionId">The identifier of the condition to apply.</param>
/// <param name="ExpiresAtEndOf">Optional description of when the condition expires (e.g. "your next turn").</param>
public sealed record AddConditionRequest(Guid HeroId, Guid ConditionId, string? ExpiresAtEndOf);
