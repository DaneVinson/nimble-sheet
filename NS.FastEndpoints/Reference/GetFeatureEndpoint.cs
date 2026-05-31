namespace NSFastEndpoints;

/// <summary>Returns a single feature by identifier.</summary>
public sealed class GetFeatureEndpoint : Endpoint<ReferenceIdRequest, Feature>
{
    private readonly IReferenceDataService<Feature> _features;

    /// <summary>Initializes the endpoint with the feature reference data service.</summary>
    public GetFeatureEndpoint(IReferenceDataService<Feature> features) => _features = features;

    /// <inheritdoc/>
    public override void Configure()
    {
        Get("reference/features/{id}");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(ReferenceIdRequest req, CancellationToken ct)
    {
        var item = await _features.GetByIdAsync(req.Id);
        if (item is null) { await Send.NotFoundAsync(ct); return; }
        await Send.OkAsync(item, ct);
    }
}
