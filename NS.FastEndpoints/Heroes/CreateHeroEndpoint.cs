namespace NSFastEndpoints;

/// <summary>Creates a new level-1 hero.</summary>
public sealed class CreateHeroEndpoint : Endpoint<CreateHeroRequest, CreateHeroResponse>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public CreateHeroEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(CreateHeroRequest req, CancellationToken ct)
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
            req.Stats);

        await _heroes.SaveAsync(hero);
        await Send.ResponseAsync(new CreateHeroResponse(hero.Id), 201, ct);
    }
}

/// <summary>Request payload for creating a new hero.</summary>
/// <param name="AncestryId">The identifier of the hero's ancestry.</param>
/// <param name="BackgroundId">The optional identifier of the hero's background.</param>
/// <param name="CombatStats">The hero's initial combat statistics.</param>
/// <param name="HeroClass">The hero's class.</param>
/// <param name="MaxHp">The hero's starting maximum hit points.</param>
/// <param name="MaxMana">The hero's starting maximum mana; <see langword="null"/> for non-casters.</param>
/// <param name="Name">The hero's name.</param>
/// <param name="Resources">The hero's class-specific resource pools.</param>
/// <param name="Saves">The hero's save advantage and disadvantage types.</param>
/// <param name="Skills">The hero's initial skill bonuses.</param>
/// <param name="Stats">The hero's base stats.</param>
public sealed record CreateHeroRequest(
    Guid AncestryId,
    Guid? BackgroundId,
    HeroCombatStats CombatStats,
    HeroClass HeroClass,
    int MaxHp,
    int? MaxMana,
    string Name,
    ClassResources Resources,
    HeroSaves Saves,
    HeroSkills Skills,
    HeroStats Stats);

/// <summary>Response returned after successfully creating a hero.</summary>
/// <param name="Id">The newly created hero's unique identifier.</param>
public sealed record CreateHeroResponse(Guid Id);
