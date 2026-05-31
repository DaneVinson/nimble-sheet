namespace NSFastEndpoints;

/// <summary>Returns spells, optionally filtered by tier and school.</summary>
public sealed class GetAllSpellsEndpoint : Endpoint<GetAllSpellsRequest, List<Spell>>
{
    private readonly IReferenceDataService<Spell> _spells;

    /// <summary>Initializes the endpoint with the spell reference data service.</summary>
    public GetAllSpellsEndpoint(IReferenceDataService<Spell> spells) => _spells = spells;

    /// <inheritdoc/>
    public override void Configure()
    {
        Get("reference/spells");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(GetAllSpellsRequest req, CancellationToken ct)
    {
        IReadOnlyList<Spell> items;
        if (req.Tier.HasValue || req.School.HasValue)
        {
            items = await _spells.FindAsync(s =>
                (!req.Tier.HasValue || s.Tier == req.Tier.Value) &&
                (!req.School.HasValue || s.School == req.School.Value));
        }
        else
        {
            items = await _spells.GetAllAsync();
        }

        await Send.OkAsync(items.ToList(), ct);
    }
}

/// <summary>Request for retrieving spells with optional filters.</summary>
/// <param name="Tier">Optional mana tier filter (1–3).</param>
/// <param name="School">Optional spell school filter.</param>
public sealed record GetAllSpellsRequest(int? Tier, SpellSchool? School);
