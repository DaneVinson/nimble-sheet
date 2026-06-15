namespace NSFastEndpoints;

/// <summary>Sets whether a magic item the hero carries is equipped.</summary>
public sealed class SetMagicItemEquippedEndpoint : Endpoint<SetMagicItemEquippedRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public SetMagicItemEquippedEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/set-magic-item-equipped");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(SetMagicItemEquippedRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetOwnedByIdAsync(req.HeroId, User.GetUserId());
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.SetMagicItemEquipped(req.MagicItemId, req.IsEquipped);
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for setting a magic item's equipped state.</summary>
/// <param name="HeroId">The hero's unique identifier (route).</param>
/// <param name="MagicItemId">The identifier of the magic item to update.</param>
/// <param name="IsEquipped">Whether the magic item should be equipped.</param>
public sealed record SetMagicItemEquippedRequest(Guid HeroId, Guid MagicItemId, bool IsEquipped);
