namespace NSFastEndpoints;

/// <summary>Sets whether an armor item the hero carries is equipped.</summary>
public sealed class SetArmorEquippedEndpoint : Endpoint<SetArmorEquippedRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public SetArmorEquippedEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/set-armor-equipped");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(SetArmorEquippedRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetOwnedByIdAsync(req.HeroId, User.GetUserId());
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.SetArmorEquipped(req.ArmorId, req.IsEquipped);
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for setting an armor item's equipped state.</summary>
/// <param name="HeroId">The hero's unique identifier (route).</param>
/// <param name="ArmorId">The identifier of the armor item to update.</param>
/// <param name="IsEquipped">Whether the armor should be equipped.</param>
public sealed record SetArmorEquippedRequest(Guid HeroId, Guid ArmorId, bool IsEquipped);
