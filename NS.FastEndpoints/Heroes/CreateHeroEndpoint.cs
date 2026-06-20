namespace NSFastEndpoints;

/// <summary>Creates a new level-1 hero owned by the authenticated user.</summary>
public sealed class CreateHeroEndpoint : Endpoint<CreateHeroRequest, CreateHeroResponse>
{
    private readonly IReferenceDataService<Ancestry> _ancestries;
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero and ancestry data services.</summary>
    /// <param name="ancestries">The ancestry reference-data service.</param>
    /// <param name="heroes">The hero data service.</param>
    public CreateHeroEndpoint(IReferenceDataService<Ancestry> ancestries, IHeroDataService heroes)
    {
        _ancestries = ancestries;
        _heroes = heroes;
    }

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(CreateHeroRequest req, CancellationToken ct)
    {
        var ancestry = await _ancestries.GetByIdAsync(req.AncestryId);
        if (ancestry is null)
        {
            AddError(r => r.AncestryId, "Ancestry not found.");
            ThrowIfAnyErrors();
        }

        var hero = Hero.Create(
            req.Name,
            req.HeroClass,
            req.AncestryId,
            req.BackgroundId,
            req.BaseAbilityScores,
            ancestry!.AbilityBonuses,
            User.GetUserId());

        await _heroes.SaveAsync(hero);
        await Send.ResponseAsync(new CreateHeroResponse(hero.Id), 201, ct);
    }
}

/// <summary>Response returned after successfully creating a hero.</summary>
/// <param name="Id">The newly created hero's unique identifier.</param>
public sealed record CreateHeroResponse(Guid Id);
