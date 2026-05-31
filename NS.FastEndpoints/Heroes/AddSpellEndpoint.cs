namespace NSFastEndpoints;

/// <summary>Adds a spell to the hero's spell list.</summary>
public sealed class AddSpellEndpoint : Endpoint<AddSpellRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public AddSpellEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/add-spell");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(AddSpellRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetByIdAsync(req.HeroId);
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.AddSpell(new HeroSpell(req.HeroId, req.Notes, req.SpellId, req.TierUnlocked));
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for adding a spell to a hero.</summary>
/// <param name="HeroId">The hero's unique identifier (route).</param>
/// <param name="SpellId">The identifier of the spell to add.</param>
/// <param name="TierUnlocked">The mana tier at which the hero unlocked this spell.</param>
/// <param name="Notes">Optional personal notes about the spell.</param>
public sealed record AddSpellRequest(Guid HeroId, Guid SpellId, int TierUnlocked, string? Notes);
