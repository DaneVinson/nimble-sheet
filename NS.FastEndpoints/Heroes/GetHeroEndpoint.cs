namespace NSFastEndpoints;

/// <summary>Returns a single hero by identifier.</summary>
public sealed class GetHeroEndpoint : Endpoint<GetHeroRequest, Hero>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public GetHeroEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Get("heroes/{heroId}");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(GetHeroRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetByIdAsync(req.HeroId);
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        await Send.OkAsync(hero, ct);
    }
}

/// <summary>Request for retrieving a hero by identifier.</summary>
/// <param name="HeroId">The hero's unique identifier.</param>
public sealed record GetHeroRequest(Guid HeroId);
