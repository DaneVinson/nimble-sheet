namespace NSFastEndpoints;

/// <summary>Adds a weapon to the hero's inventory.</summary>
public sealed class AddWeaponEndpoint : Endpoint<AddWeaponRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public AddWeaponEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/add-weapon");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(AddWeaponRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetByIdAsync(req.HeroId);
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.AddWeapon(new HeroWeapon(req.HeroId, req.IsEquipped, req.Notes, req.WeaponId));
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for adding a weapon to a hero.</summary>
/// <param name="HeroId">The hero's unique identifier (route).</param>
/// <param name="WeaponId">The identifier of the weapon to add.</param>
/// <param name="IsEquipped">Whether the weapon is currently equipped.</param>
/// <param name="Notes">Optional personal notes about the weapon.</param>
public sealed record AddWeaponRequest(Guid HeroId, Guid WeaponId, bool IsEquipped, string? Notes);
