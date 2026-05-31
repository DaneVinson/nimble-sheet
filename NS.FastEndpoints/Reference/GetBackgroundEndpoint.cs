namespace NSFastEndpoints;

/// <summary>Returns a single background by identifier.</summary>
public sealed class GetBackgroundEndpoint : Endpoint<ReferenceIdRequest, Background>
{
    private readonly IReferenceDataService<Background> _backgrounds;

    /// <summary>Initializes the endpoint with the background reference data service.</summary>
    public GetBackgroundEndpoint(IReferenceDataService<Background> backgrounds) => _backgrounds = backgrounds;

    /// <inheritdoc/>
    public override void Configure()
    {
        Get("reference/backgrounds/{id}");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(ReferenceIdRequest req, CancellationToken ct)
    {
        var item = await _backgrounds.GetByIdAsync(req.Id);
        if (item is null) { await Send.NotFoundAsync(ct); return; }
        await Send.OkAsync(item, ct);
    }
}
