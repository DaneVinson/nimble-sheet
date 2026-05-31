namespace NSFastEndpoints;

/// <summary>Returns a single ancestry by identifier.</summary>
public sealed class GetAncestryEndpoint : Endpoint<ReferenceIdRequest, Ancestry>
{
    private readonly IReferenceDataService<Ancestry> _ancestries;

    /// <summary>Initializes the endpoint with the ancestry reference data service.</summary>
    public GetAncestryEndpoint(IReferenceDataService<Ancestry> ancestries) => _ancestries = ancestries;

    /// <inheritdoc/>
    public override void Configure()
    {
        Get("reference/ancestries/{id}");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(ReferenceIdRequest req, CancellationToken ct)
    {
        var item = await _ancestries.GetByIdAsync(req.Id);
        if (item is null) { await Send.NotFoundAsync(ct); return; }
        await Send.OkAsync(item, ct);
    }
}
