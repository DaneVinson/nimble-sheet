namespace NSFastEndpoints;

/// <summary>Returns all ancestries.</summary>
public sealed class GetAllAncestriesEndpoint : EndpointWithoutRequest<List<Ancestry>>
{
    private readonly IReferenceDataService<Ancestry> _ancestries;

    /// <summary>Initializes the endpoint with the ancestry reference data service.</summary>
    public GetAllAncestriesEndpoint(IReferenceDataService<Ancestry> ancestries) => _ancestries = ancestries;

    /// <inheritdoc/>
    public override void Configure()
    {
        Get("reference/ancestries");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(CancellationToken ct)
    {
        var items = await _ancestries.GetAllAsync();
        await Send.OkAsync(items.ToList(), ct);
    }
}
