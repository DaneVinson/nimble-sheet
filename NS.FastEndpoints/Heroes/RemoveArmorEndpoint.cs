namespace NSFastEndpoints;

/// <summary>Removes a piece of armor from the hero's inventory.</summary>
public sealed class RemoveArmorEndpoint : Endpoint<RemoveArmorRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public RemoveArmorEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/remove-armor");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(RemoveArmorRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetByIdAsync(req.HeroId);
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.RemoveArmor(req.ArmorId);
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for removing armor from a hero.</summary>
/// <param name="HeroId">The hero's unique identifier (route).</param>
/// <param name="ArmorId">The identifier of the armor to remove.</param>
public sealed record RemoveArmorRequest(Guid HeroId, Guid ArmorId);
