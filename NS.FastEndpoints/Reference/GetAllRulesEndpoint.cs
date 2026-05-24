namespace NSFastEndpoints;

/// <summary>Returns all rule references.</summary>
public sealed class GetAllRulesEndpoint : EndpointWithoutRequest<List<RuleReference>>
{
    private readonly IReferenceDataService<RuleReference> _rules;

    /// <summary>Initializes the endpoint with the rule reference data service.</summary>
    public GetAllRulesEndpoint(IReferenceDataService<RuleReference> rules) => _rules = rules;

    /// <inheritdoc/>
    public override void Configure()
    {
        Get("reference/rules");
        AllowAnonymous();
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(CancellationToken ct)
    {
        var items = await _rules.GetAllAsync();
        await Send.OkAsync(items.ToList(), ct);
    }
}
