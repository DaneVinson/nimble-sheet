namespace NSFastEndpoints;

/// <summary>Removes a gear item from the hero's inventory by name.</summary>
public sealed class RemoveGearItemEndpoint : Endpoint<RemoveGearItemRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public RemoveGearItemEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/remove-gear-item");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(RemoveGearItemRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetOwnedByIdAsync(req.HeroId, User.GetUserId());
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.RemoveGearItem(req.Name);
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for removing a gear item from a hero.</summary>
/// <param name="HeroId">The hero's unique identifier (route).</param>
/// <param name="Name">The name of the gear item to remove.</param>
public sealed record RemoveGearItemRequest(Guid HeroId, string Name);
