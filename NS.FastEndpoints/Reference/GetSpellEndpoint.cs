namespace NSFastEndpoints;

/// <summary>Returns a single spell by identifier.</summary>
public sealed class GetSpellEndpoint : Endpoint<ReferenceIdRequest, Spell>
{
    private readonly IReferenceDataService<Spell> _spells;

    /// <summary>Initializes the endpoint with the spell reference data service.</summary>
    public GetSpellEndpoint(IReferenceDataService<Spell> spells) => _spells = spells;

    /// <inheritdoc/>
    public override void Configure()
    {
        Get("reference/spells/{id}");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(ReferenceIdRequest req, CancellationToken ct)
    {
        var item = await _spells.GetByIdAsync(req.Id);
        if (item is null) { await Send.NotFoundAsync(ct); return; }
        await Send.OkAsync(item, ct);
    }
}
