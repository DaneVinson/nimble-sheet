namespace NSFastEndpoints;

/// <summary>Permanently deletes a hero.</summary>
public sealed class DeleteHeroEndpoint : Endpoint<HeroIdRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public DeleteHeroEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Delete("heroes/{heroId}");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(HeroIdRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetOwnedByIdAsync(req.HeroId, User.GetUserId());
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        await _heroes.DeleteAsync(req.HeroId);
        await Send.NoContentAsync(ct);
    }
}
