namespace NSFastEndpoints;

/// <summary>Returns all armor.</summary>
public sealed class GetAllArmorEndpoint : EndpointWithoutRequest<List<Armor>>
{
    private readonly IReferenceDataService<Armor> _armor;

    /// <summary>Initializes the endpoint with the armor reference data service.</summary>
    public GetAllArmorEndpoint(IReferenceDataService<Armor> armor) => _armor = armor;

    /// <inheritdoc/>
    public override void Configure()
    {
        Get("reference/armor");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(CancellationToken ct)
    {
        var items = await _armor.GetAllAsync();
        await Send.OkAsync(items.ToList(), ct);
    }
}
