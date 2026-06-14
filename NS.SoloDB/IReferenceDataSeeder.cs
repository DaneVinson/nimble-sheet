namespace NSSoloDB;

/// <summary>Populates reference collections with the curated starter data set.</summary>
public interface IReferenceDataSeeder
{
    /// <summary>Seeds each reference collection that is currently empty. Idempotent across restarts.</summary>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    Task SeedAsync(CancellationToken cancellationToken = default);
}
