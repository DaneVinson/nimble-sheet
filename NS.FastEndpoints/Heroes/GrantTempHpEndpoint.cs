namespace NSFastEndpoints;

/// <summary>Grants temporary hit points to the hero.</summary>
public sealed class GrantTempHpEndpoint : Endpoint<GrantTempHpRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public GrantTempHpEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/grant-temp-hp");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(GrantTempHpRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetOwnedByIdAsync(req.HeroId, User.GetUserId());
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.GrantTempHp(req.Amount);
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for granting temporary hit points to a hero.</summary>
/// <param name="HeroId">The hero's unique identifier (route).</param>
/// <param name="Amount">The amount of temporary hit points to grant.</param>
public sealed record GrantTempHpRequest(Guid HeroId, int Amount);

/// <summary>Validates <see cref="GrantTempHpRequest"/>.</summary>
public sealed class GrantTempHpValidator : Validator<GrantTempHpRequest>
{
    /// <summary>Initializes validation rules for granting temporary hit points.</summary>
    public GrantTempHpValidator()
    {
        RuleFor(r => r.Amount).GreaterThanOrEqualTo(0);
    }
}
