namespace NSFastEndpoints;

/// <summary>Returns a single weapon by identifier.</summary>
public sealed class GetWeaponEndpoint : Endpoint<ReferenceIdRequest, Weapon>
{
    private readonly IReferenceDataService<Weapon> _weapons;

    /// <summary>Initializes the endpoint with the weapon reference data service.</summary>
    public GetWeaponEndpoint(IReferenceDataService<Weapon> weapons) => _weapons = weapons;

    /// <inheritdoc/>
    public override void Configure()
    {
        Get("reference/weapons/{id}");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(ReferenceIdRequest req, CancellationToken ct)
    {
        var item = await _weapons.GetByIdAsync(req.Id);
        if (item is null) { await Send.NotFoundAsync(ct); return; }
        await Send.OkAsync(item, ct);
    }
}
