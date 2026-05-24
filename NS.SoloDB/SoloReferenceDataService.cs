namespace NSSoloDB;

/// <summary>SoloDB-backed implementation of <see cref="IReferenceDataService{T}"/>.</summary>
/// <typeparam name="T">
/// The reference entity type. Must expose a public <c>Guid Id</c> property.
/// </typeparam>
public sealed class SoloReferenceDataService<T> : IReferenceDataService<T> where T : class
{
    // Cached delegate that reads the Guid Id property from T via reflection.
    // Computed once per closed generic type the first time the class is used.
    private static readonly Func<T, Guid> _getId = BuildIdGetter();

    private readonly SoloDB _db;

    /// <summary>Initializes the service with the provided SoloDB instance.</summary>
    public SoloReferenceDataService(SoloDB db) => _db = db;

    private static Func<T, Guid> BuildIdGetter()
    {
        var prop = typeof(T).GetProperty("Id")
            ?? throw new InvalidOperationException(
                $"{typeof(T).Name} does not expose a public 'Id' property.");
        return item => (Guid)prop.GetValue(item)!;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<T>> FindAsync(Func<T, bool> predicate)
    {
        IReadOnlyList<T> results = _db.GetCollection<SoloDocument<T>>()
            .ToList()
            .Where(d => predicate(d.Data))
            .Select(d => d.Data)
            .ToList();
        return Task.FromResult(results);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<T>> GetAllAsync()
    {
        IReadOnlyList<T> results = _db.GetCollection<SoloDocument<T>>()
            .ToList()
            .ConvertAll(d => d.Data);
        return Task.FromResult(results);
    }

    /// <inheritdoc/>
    public Task<T?> GetByIdAsync(Guid id)
    {
        T? item = _db.GetCollection<SoloDocument<T>>()
            .ToList()
            .FirstOrDefault(d => _getId(d.Data) == id)
            ?.Data;
        return Task.FromResult(item);
    }
}
