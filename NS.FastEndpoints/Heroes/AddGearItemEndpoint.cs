namespace NSFastEndpoints;

/// <summary>Adds a named gear item to the hero's inventory.</summary>
public sealed class AddGearItemEndpoint : Endpoint<AddGearItemRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public AddGearItemEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/add-gear-item");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(AddGearItemRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetOwnedByIdAsync(req.HeroId, User.GetUserId());
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.AddGearItem(new HeroGearItem(req.HeroId, req.Name, req.Quantity));
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for adding a gear item to a hero.</summary>
/// <param name="HeroId">The hero's unique identifier (route).</param>
/// <param name="Name">The name of the gear item.</param>
/// <param name="Quantity">The quantity being added.</param>
public sealed record AddGearItemRequest(Guid HeroId, string Name, int Quantity);
