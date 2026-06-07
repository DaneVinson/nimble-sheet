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
        var col = _db.GetCollection<SoloDocument<Hero>>();
        var doc = col.ToList().FirstOrDefault(d => d.Data.Id == id);
        if (doc != null)
            col.Delete(doc.Id);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<Hero>> GetAllAsync()
    {
        IReadOnlyList<Hero> heroes = _db.GetCollection<SoloDocument<Hero>>()
            .ToList()
            .ConvertAll(d => d.Data);
        return Task.FromResult(heroes);
    }

    /// <inheritdoc/>
    public Task<Hero?> GetByIdAsync(Guid id)
    {
        Hero? hero = _db.GetCollection<SoloDocument<Hero>>()
            .ToList()
            .FirstOrDefault(d => d.Data.Id == id)
            ?.Data;
        return Task.FromResult(hero);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<Hero>> GetByUserAsync(Guid userId)
    {
        IReadOnlyList<Hero> heroes = _db.GetCollection<SoloDocument<Hero>>()
            .ToList()
            .Where(d => d.Data.UserId == userId)
            .Select(d => d.Data)
            .ToList();
        return Task.FromResult(heroes);
    }

    /// <inheritdoc/>
    public Task SaveAsync(Hero hero)
    {
        var col = _db.GetCollection<SoloDocument<Hero>>();
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
