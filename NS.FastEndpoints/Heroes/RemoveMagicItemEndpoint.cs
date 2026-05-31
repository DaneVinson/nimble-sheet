namespace NSFastEndpoints;

/// <summary>Removes a magic item from the hero's inventory.</summary>
public sealed class RemoveMagicItemEndpoint : Endpoint<RemoveMagicItemRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public RemoveMagicItemEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/remove-magic-item");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(RemoveMagicItemRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetByIdAsync(req.HeroId);
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.RemoveMagicItem(req.MagicItemId);
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for removing a magic item from a hero.</summary>
/// <param name="HeroId">The hero's unique identifier (route).</param>
/// <param name="MagicItemId">The identifier of the magic item to remove.</param>
public sealed record RemoveMagicItemRequest(Guid HeroId, Guid MagicItemId);
