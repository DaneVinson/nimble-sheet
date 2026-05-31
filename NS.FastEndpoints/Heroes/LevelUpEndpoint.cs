namespace NSFastEndpoints;

/// <summary>Levels up the hero, incrementing level, refreshing hit dice, and recording pending feature choices.</summary>
public sealed class LevelUpEndpoint : Endpoint<LevelUpRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public LevelUpEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/level-up");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(LevelUpRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetByIdAsync(req.HeroId);
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.LevelUp(req.PendingChoices);
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for leveling up a hero.</summary>
/// <param name="HeroId">The hero's unique identifier (route).</param>
/// <param name="PendingChoices">The list of pending choice labels that must be resolved before the next session.</param>
public sealed record LevelUpRequest(Guid HeroId, IReadOnlyList<string> PendingChoices);
