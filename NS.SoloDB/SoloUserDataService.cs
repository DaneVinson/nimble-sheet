namespace NSSoloDB;

/// <summary>SoloDB-backed implementation of <see cref="IUserDataService"/>.</summary>
public sealed class SoloUserDataService : IUserDataService
{
    private readonly SoloDB _db;

    /// <summary>Initializes the service with the provided SoloDB instance.</summary>
    public SoloUserDataService(SoloDB db) => _db = db;

    /// <inheritdoc/>
    public Task CreateAsync(User user)
    {
        SoloCollections.Of<User>(_db)
            .Insert(new SoloDocument<User> { Data = user });
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<User>> FindByNameAsync(string name)
    {
        IReadOnlyList<User> users = SoloCollections.Of<User>(_db)
            .ToList()
            .Where(d => d.Data.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            .Select(d => d.Data)
            .ToList();
        return Task.FromResult(users);
    }

    /// <inheritdoc/>
    public Task<User?> GetByIdAsync(Guid id)
    {
        User? user = SoloCollections.Of<User>(_db)
            .ToList()
            .FirstOrDefault(d => d.Data.Id == id)
            ?.Data;
        return Task.FromResult(user);
    }

    /// <inheritdoc/>
    public Task UpdateAsync(User user)
    {
        var col = SoloCollections.Of<User>(_db);
        var existing = col.ToList().FirstOrDefault(d => d.Data.Id == user.Id);
        if (existing is not null)
        {
            existing.Data = user;
            col.Update(existing);
        }
        return Task.CompletedTask;
    }
}
