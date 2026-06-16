namespace NSFastEndpoints;

/// <summary>Sets whether a weapon in the hero's equipment is equipped.</summary>
public sealed class SetWeaponEquippedEndpoint : Endpoint<SetWeaponEquippedRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public SetWeaponEquippedEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/set-weapon-equipped");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(SetWeaponEquippedRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetOwnedByIdAsync(req.HeroId, User.GetUserId());
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.SetWeaponEquipped(req.WeaponId, req.IsEquipped);
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for setting a weapon's equipped state.</summary>
/// <param name="HeroId">The hero's unique identifier (route).</param>
/// <param name="WeaponId">The identifier of the weapon to update.</param>
/// <param name="IsEquipped">Whether the weapon should be equipped.</param>
public sealed record SetWeaponEquippedRequest(Guid HeroId, Guid WeaponId, bool IsEquipped);
