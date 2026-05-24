namespace NSFastEndpoints;

/// <summary>Returns a single action reference by identifier.</summary>
public sealed class GetActionEndpoint : Endpoint<ReferenceIdRequest, ActionReference>
{
    private readonly IReferenceDataService<ActionReference> _actions;

    /// <summary>Initializes the endpoint with the action reference data service.</summary>
    public GetActionEndpoint(IReferenceDataService<ActionReference> actions) => _actions = actions;

    /// <inheritdoc/>
    public override void Configure()
    {
        Get("reference/actions/{id}");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(ReferenceIdRequest req, CancellationToken ct)
    {
        var item = await _actions.GetByIdAsync(req.Id);
        if (item is null) { await Send.NotFoundAsync(ct); return; }
        await Send.OkAsync(item, ct);
    }
}
