namespace NSFastEndpoints;

/// <summary>Returns a single condition by identifier.</summary>
public sealed class GetConditionEndpoint : Endpoint<ReferenceIdRequest, Condition>
{
    private readonly IReferenceDataService<Condition> _conditions;

    /// <summary>Initializes the endpoint with the condition reference data service.</summary>
    public GetConditionEndpoint(IReferenceDataService<Condition> conditions) => _conditions = conditions;

    /// <inheritdoc/>
    public override void Configure()
    {
        Get("reference/conditions/{id}");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(ReferenceIdRequest req, CancellationToken ct)
    {
        var item = await _conditions.GetByIdAsync(req.Id);
        if (item is null) { await Send.NotFoundAsync(ct); return; }
        await Send.OkAsync(item, ct);
    }
}
