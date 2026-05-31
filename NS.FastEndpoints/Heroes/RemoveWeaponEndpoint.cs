namespace NSFastEndpoints;

/// <summary>Removes a weapon from the hero's inventory.</summary>
public sealed class RemoveWeaponEndpoint : Endpoint<RemoveWeaponRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public RemoveWeaponEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/remove-weapon");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(RemoveWeaponRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetByIdAsync(req.HeroId);
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.RemoveWeapon(req.WeaponId);
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for removing a weapon from a hero.</summary>
/// <param name="HeroId">The hero's unique identifier (route).</param>
/// <param name="WeaponId">The identifier of the weapon to remove.</param>
public sealed record RemoveWeaponRequest(Guid HeroId, Guid WeaponId);
