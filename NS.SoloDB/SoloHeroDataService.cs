namespace NSSoloDB;

/// <summary>SoloDB-backed implementation of <see cref="IHeroDataService"/>.</summary>
public sealed class SoloHeroDataService : IHeroDataService
{
    private readonly SoloDB _db;

    /// <summary>Initializes the service with the provided SoloDB instance.</summary>
    public SoloHeroDataService(SoloDB db) => _db = db;

    /// <inheritdoc/>
    public Task DeleteAsync(Guid id)
    {
        var col = SoloCollections.Of<Hero>(_db);
        var doc = col.ToList().FirstOrDefault(d => d.Data.Id == id);
        if (doc != null)
            col.Delete(doc.Id);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<Hero>> GetAllAsync()
    {
        IReadOnlyList<Hero> heroes = SoloCollections.Of<Hero>(_db)
            .ToList()
            .ConvertAll(d => d.Data);
        return Task.FromResult(heroes);
    }

    /// <inheritdoc/>
    public Task<Hero?> GetByIdAsync(Guid id)
    {
        Hero? hero = SoloCollections.Of<Hero>(_db)
            .ToList()
            .FirstOrDefault(d => d.Data.Id == id)
            ?.Data;
        return Task.FromResult(hero);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<Hero>> GetByUserAsync(Guid userId)
    {
        IReadOnlyList<Hero> heroes = SoloCollections.Of<Hero>(_db)
            .ToList()
            .Where(d => d.Data.UserId == userId)
            .Select(d => d.Data)
            .ToList();
        return Task.FromResult(heroes);
    }

    /// <inheritdoc/>
    public Task SaveAsync(Hero hero)
    {
        var col = SoloCollections.Of<Hero>(_db);
        var existing = col.ToList().FirstOrDefault(d => d.Data.Id == hero.Id);
        if (existing is null)
        {
            col.Insert(new SoloDocument<Hero> { Data = hero });
        }
        else
        {
            existing.Data = hero;
            col.Update(existing);
        }
        return Task.CompletedTask;
    }
}
