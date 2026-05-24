namespace NSFastEndpoints;

/// <summary>Returns all magic items.</summary>
public sealed class GetAllMagicItemsEndpoint : EndpointWithoutRequest<List<MagicItem>>
{
    private readonly IReferenceDataService<MagicItem> _magicItems;

    /// <summary>Initializes the endpoint with the magic item reference data service.</summary>
    public GetAllMagicItemsEndpoint(IReferenceDataService<MagicItem> magicItems) => _magicItems = magicItems;

    /// <inheritdoc/>
    public override void Configure()
    {
        Get("reference/magic-items");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(CancellationToken ct)
    {
        var items = await _magicItems.GetAllAsync();
        await Send.OkAsync(items.ToList(), ct);
    }
}
