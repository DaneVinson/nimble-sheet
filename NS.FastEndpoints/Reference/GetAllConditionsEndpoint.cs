namespace NSFastEndpoints;

/// <summary>Returns all conditions.</summary>
public sealed class GetAllConditionsEndpoint : EndpointWithoutRequest<List<Condition>>
{
    private readonly IReferenceDataService<Condition> _conditions;

    /// <summary>Initializes the endpoint with the condition reference data service.</summary>
    public GetAllConditionsEndpoint(IReferenceDataService<Condition> conditions) => _conditions = conditions;

    /// <inheritdoc/>
    public override void Configure()
    {
        Get("reference/conditions");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(CancellationToken ct)
    {
        var items = await _conditions.GetAllAsync();
        await Send.OkAsync(items.ToList(), ct);
    }
}
