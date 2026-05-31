namespace NSFastEndpoints;

/// <summary>Returns a single magic item by identifier.</summary>
public sealed class GetMagicItemEndpoint : Endpoint<ReferenceIdRequest, MagicItem>
{
    private readonly IReferenceDataService<MagicItem> _magicItems;

    /// <summary>Initializes the endpoint with the magic item reference data service.</summary>
    public GetMagicItemEndpoint(IReferenceDataService<MagicItem> magicItems) => _magicItems = magicItems;

    /// <inheritdoc/>
    public override void Configure()
    {
        Get("reference/magic-items/{id}");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(ReferenceIdRequest req, CancellationToken ct)
    {
        var item = await _magicItems.GetByIdAsync(req.Id);
        if (item is null) { await Send.NotFoundAsync(ct); return; }
        await Send.OkAsync(item, ct);
    }
}
