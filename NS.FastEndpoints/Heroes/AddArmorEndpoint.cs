namespace NSFastEndpoints;

/// <summary>Adds a piece of armor to the hero's inventory.</summary>
public sealed class AddArmorEndpoint : Endpoint<AddArmorRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public AddArmorEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/add-armor");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(AddArmorRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetByIdAsync(req.HeroId);
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.AddArmor(new HeroArmor(req.ArmorId, req.HeroId, req.IsEquipped));
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for adding armor to a hero.</summary>
/// <param name="HeroId">The hero's unique identifier (route).</param>
/// <param name="ArmorId">The identifier of the armor to add.</param>
/// <param name="IsEquipped">Whether the armor is currently equipped.</param>
public sealed record AddArmorRequest(Guid HeroId, Guid ArmorId, bool IsEquipped);
