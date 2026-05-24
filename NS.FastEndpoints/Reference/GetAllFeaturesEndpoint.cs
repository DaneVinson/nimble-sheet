namespace NSFastEndpoints;

/// <summary>Returns features, optionally filtered by hero class and level.</summary>
public sealed class GetAllFeaturesEndpoint : Endpoint<GetAllFeaturesRequest, List<Feature>>
{
    private readonly IReferenceDataService<Feature> _features;

    /// <summary>Initializes the endpoint with the feature reference data service.</summary>
    public GetAllFeaturesEndpoint(IReferenceDataService<Feature> features) => _features = features;

    /// <inheritdoc/>
    public override void Configure()
    {
        Get("reference/features");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(GetAllFeaturesRequest req, CancellationToken ct)
    {
        IReadOnlyList<Feature> items;
        if (req.HeroClass.HasValue || req.Level.HasValue)
        {
            items = await _features.FindAsync(f =>
                (!req.HeroClass.HasValue || f.Class == req.HeroClass.Value) &&
                (!req.Level.HasValue || f.Level == req.Level.Value));
        }
        else
        {
            items = await _features.GetAllAsync();
        }

        await Send.OkAsync(items.ToList(), ct);
    }
}

/// <summary>Request for retrieving features with optional filters.</summary>
/// <param name="HeroClass">Optional class filter.</param>
/// <param name="Level">Optional level filter.</param>
public sealed record GetAllFeaturesRequest(HeroClass? HeroClass, int? Level);
