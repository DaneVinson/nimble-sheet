namespace NSFastEndpoints;

/// <summary>Returns all heroes.</summary>
public sealed class GetAllHeroesEndpoint : EndpointWithoutRequest<List<Hero>>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public GetAllHeroesEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Get("heroes");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(CancellationToken ct)
    {
        var heroes = await _heroes.GetAllAsync();
        await Send.OkAsync(heroes.ToList(), ct);
    }
}
