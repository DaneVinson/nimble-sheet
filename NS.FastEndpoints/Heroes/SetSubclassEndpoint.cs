namespace NSFastEndpoints;

/// <summary>Sets the hero's subclass, chosen at level 3.</summary>
public sealed class SetSubclassEndpoint : Endpoint<SetSubclassRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public SetSubclassEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/set-subclass");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(SetSubclassRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetOwnedByIdAsync(req.HeroId, User.GetUserId());
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.SetSubclass(req.Subclass);
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for setting a hero's subclass.</summary>
/// <param name="HeroId">The hero's unique identifier (route).</param>
/// <param name="Subclass">The chosen subclass name.</param>
public sealed record SetSubclassRequest(Guid HeroId, string Subclass);
