namespace NSFastEndpoints;

/// <summary>Finalizes the hero's skill point allocation after leveling up.</summary>
public sealed class FinalizeSkillAllocationEndpoint : Endpoint<FinalizeSkillAllocationRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public FinalizeSkillAllocationEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/finalize-skill-allocation");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(FinalizeSkillAllocationRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetByIdAsync(req.HeroId);
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.FinalizeSkillAllocation(req.UpdatedSkills);
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for finalizing skill allocation after a level-up.</summary>
/// <param name="HeroId">The hero's unique identifier (route).</param>
/// <param name="UpdatedSkills">The hero's new skill values after spending skill points.</param>
public sealed record FinalizeSkillAllocationRequest(Guid HeroId, HeroSkills UpdatedSkills);
