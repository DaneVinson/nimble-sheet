namespace NSFastEndpoints;

/// <summary>Removes a spell from the hero's spell list.</summary>
public sealed class RemoveSpellEndpoint : Endpoint<RemoveSpellRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public RemoveSpellEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/remove-spell");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(RemoveSpellRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetOwnedByIdAsync(req.HeroId, User.GetUserId());
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.RemoveSpell(req.SpellId);
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for removing a spell from a hero.</summary>
/// <param name="HeroId">The hero's unique identifier (route).</param>
/// <param name="SpellId">The identifier of the spell to remove.</param>
public sealed record RemoveSpellRequest(Guid HeroId, Guid SpellId);
