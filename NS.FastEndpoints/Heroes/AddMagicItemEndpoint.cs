namespace NSFastEndpoints;

/// <summary>Adds a magic item to the hero's inventory.</summary>
public sealed class AddMagicItemEndpoint : Endpoint<AddMagicItemRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public AddMagicItemEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/add-magic-item");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(AddMagicItemRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetByIdAsync(req.HeroId);
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.AddMagicItem(new HeroMagicItem(req.ChargesRemaining, req.HeroId, req.IsEquipped, req.MagicItemId));
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for adding a magic item to a hero.</summary>
/// <param name="HeroId">The hero's unique identifier (route).</param>
/// <param name="MagicItemId">The identifier of the magic item to add.</param>
/// <param name="IsEquipped">Whether the item is currently equipped.</param>
/// <param name="ChargesRemaining">Current charges remaining; <see langword="null"/> for items without charges.</param>
public sealed record AddMagicItemRequest(Guid HeroId, Guid MagicItemId, bool IsEquipped, int? ChargesRemaining);
