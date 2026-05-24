namespace NSFastEndpoints;

/// <summary>Returns all backgrounds.</summary>
public sealed class GetAllBackgroundsEndpoint : EndpointWithoutRequest<List<Background>>
{
    private readonly IReferenceDataService<Background> _backgrounds;

    /// <summary>Initializes the endpoint with the background reference data service.</summary>
    public GetAllBackgroundsEndpoint(IReferenceDataService<Background> backgrounds) => _backgrounds = backgrounds;

    /// <inheritdoc/>
    public override void Configure()
    {
        Get("reference/backgrounds");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(CancellationToken ct)
    {
        var items = await _backgrounds.GetAllAsync();
        await Send.OkAsync(items.ToList(), ct);
    }
}
