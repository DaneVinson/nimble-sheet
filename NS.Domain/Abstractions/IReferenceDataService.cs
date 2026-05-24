namespace NS.Domain;

/// <summary>Defines read-only persistence operations for immutable reference data of type <typeparamref name="T"/>.</summary>
/// <typeparam name="T">The reference entity type.</typeparam>
public interface IReferenceDataService<T> where T : class
{
    /// <summary>Returns all entities of type <typeparamref name="T"/> that satisfy the predicate.</summary>
    Task<IReadOnlyList<T>> FindAsync(Func<T, bool> predicate);

    /// <summary>Returns all entities of type <typeparamref name="T"/>.</summary>
    Task<IReadOnlyList<T>> GetAllAsync();

    /// <summary>Returns the entity with the specified identifier, or <see langword="null"/> if not found.</summary>
    Task<T?> GetByIdAsync(Guid id);
}
