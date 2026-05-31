namespace NSFastEndpoints;

/// <summary>Returns all action references.</summary>
public sealed class GetAllActionsEndpoint : EndpointWithoutRequest<List<ActionReference>>
{
    private readonly IReferenceDataService<ActionReference> _actions;

    /// <summary>Initializes the endpoint with the action reference data service.</summary>
    public GetAllActionsEndpoint(IReferenceDataService<ActionReference> actions) => _actions = actions;

    /// <inheritdoc/>
    public override void Configure()
    {
        Get("reference/actions");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(CancellationToken ct)
    {
        var items = await _actions.GetAllAsync();
        await Send.OkAsync(items.ToList(), ct);
    }
}
