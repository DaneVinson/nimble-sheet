namespace NSFastEndpoints;

/// <summary>Returns a single armor entry by identifier.</summary>
public sealed class GetArmorEndpoint : Endpoint<ReferenceIdRequest, Armor>
{
    private readonly IReferenceDataService<Armor> _armor;

    /// <summary>Initializes the endpoint with the armor reference data service.</summary>
    public GetArmorEndpoint(IReferenceDataService<Armor> armor) => _armor = armor;

    /// <inheritdoc/>
    public override void Configure()
    {
        Get("reference/armor/{id}");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(ReferenceIdRequest req, CancellationToken ct)
    {
        var item = await _armor.GetByIdAsync(req.Id);
        if (item is null) { await Send.NotFoundAsync(ct); return; }
        await Send.OkAsync(item, ct);
    }
}
