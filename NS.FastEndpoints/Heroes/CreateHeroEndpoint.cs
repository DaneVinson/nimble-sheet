namespace NSFastEndpoints;

/// <summary>Creates a new level-1 hero owned by the authenticated user.</summary>
public sealed class CreateHeroEndpoint : Endpoint<HeroBuildRequest, CreateHeroResponse>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public CreateHeroEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(HeroBuildRequest req, CancellationToken ct)
    {
        var hero = new Hero(
            req.AncestryId,
            req.BackgroundId,
            req.CombatStats,
            req.HeroClass,
            req.MaxHp,
            req.MaxMana,
            req.Name,
            req.Resources,
            req.Saves,
            req.Skills,
            req.Stats,
            User.GetUserId());

        await _heroes.SaveAsync(hero);
        await Send.ResponseAsync(new CreateHeroResponse(hero.Id), 201, ct);
    }
}

/// <summary>Response returned after successfully creating a hero.</summary>
/// <param name="Id">The newly created hero's unique identifier.</param>
public sealed record CreateHeroResponse(Guid Id);
