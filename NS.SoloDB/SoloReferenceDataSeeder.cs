namespace NSSoloDB;

/// <summary>SoloDB-backed implementation of <see cref="IReferenceDataSeeder"/>.</summary>
public sealed class SoloReferenceDataSeeder : IReferenceDataSeeder
{
    private readonly SoloDB _db;

    /// <summary>Initializes the seeder with the provided SoloDB instance.</summary>
    public SoloReferenceDataSeeder(SoloDB db) => _db = db;

    /// <inheritdoc/>
    public Task SeedAsync(CancellationToken cancellationToken = default)
    {
        SeedIfEmpty(SeedData.Actions);
        SeedIfEmpty(SeedData.Ancestries);
        SeedIfEmpty(SeedData.Armor);
        SeedIfEmpty(SeedData.Backgrounds);
        SeedIfEmpty(SeedData.Conditions);
        SeedIfEmpty(SeedData.Features);
        SeedIfEmpty(SeedData.MagicItems);
        SeedIfEmpty(SeedData.Rules);
        SeedIfEmpty(SeedData.Spells);
        SeedIfEmpty(SeedData.Weapons);
        return Task.CompletedTask;
    }

    private void SeedIfEmpty<T>(IReadOnlyList<T> rows) where T : class
    {
        var collection = SoloCollections.Of<T>(_db);
        if (collection.ToList().Count > 0)
        {
            return;
        }
        foreach (var row in rows)
        {
            collection.Insert(new SoloDocument<T> { Data = row });
        }
    }
}
