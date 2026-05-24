namespace NSFastEndpoints;

/// <summary>Returns all weapons.</summary>
public sealed class GetAllWeaponsEndpoint : EndpointWithoutRequest<List<Weapon>>
{
    private readonly IReferenceDataService<Weapon> _weapons;

    /// <summary>Initializes the endpoint with the weapon reference data service.</summary>
    public GetAllWeaponsEndpoint(IReferenceDataService<Weapon> weapons) => _weapons = weapons;

    /// <inheritdoc/>
    public override void Configure()
    {
        Get("reference/weapons");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(CancellationToken ct)
    {
        var items = await _weapons.GetAllAsync();
        await Send.OkAsync(items.ToList(), ct);
    }
}
