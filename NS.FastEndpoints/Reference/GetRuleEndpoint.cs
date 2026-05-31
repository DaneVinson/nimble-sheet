namespace NSFastEndpoints;

/// <summary>Returns a single rule reference by identifier.</summary>
public sealed class GetRuleEndpoint : Endpoint<ReferenceIdRequest, RuleReference>
{
    private readonly IReferenceDataService<RuleReference> _rules;

    /// <summary>Initializes the endpoint with the rule reference data service.</summary>
    public GetRuleEndpoint(IReferenceDataService<RuleReference> rules) => _rules = rules;

    /// <inheritdoc/>
    public override void Configure()
    {
        Get("reference/rules/{id}");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(ReferenceIdRequest req, CancellationToken ct)
    {
        var item = await _rules.GetByIdAsync(req.Id);
        if (item is null) { await Send.NotFoundAsync(ct); return; }
        await Send.OkAsync(item, ct);
    }
}
