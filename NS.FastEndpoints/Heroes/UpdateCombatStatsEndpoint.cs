namespace NSFastEndpoints;

/// <summary>Updates the hero's combat statistics.</summary>
public sealed class UpdateCombatStatsEndpoint : Endpoint<UpdateCombatStatsRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public UpdateCombatStatsEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/update-combat-stats");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(UpdateCombatStatsRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetOwnedByIdAsync(req.HeroId, User.GetUserId());
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.UpdateCombatStats(new HeroCombatStats(req.Armor, req.HitDieType, req.InitiativeBonus, req.Speed));
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for updating a hero's combat statistics.</summary>
/// <param name="HeroId">The hero's unique identifier (route).</param>
/// <param name="Armor">The hero's armor value.</param>
/// <param name="HitDieType">The die type used when rolling hit points.</param>
/// <param name="InitiativeBonus">The hero's initiative bonus.</param>
/// <param name="Speed">The hero's movement speed in feet.</param>
public sealed record UpdateCombatStatsRequest(Guid HeroId, int Armor, DieType HitDieType, int InitiativeBonus, int Speed);
