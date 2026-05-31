namespace NS.Domain;

/// <summary>Defines persistence operations for <see cref="User"/> instances.</summary>
public interface IUserDataService
{
    /// <summary>Persists a newly created user.</summary>
    Task CreateAsync(User user);

    /// <summary>Returns all users whose <see cref="User.Name"/> contains the specified value (case-insensitive).</summary>
    Task<IReadOnlyList<User>> FindByNameAsync(string name);

    /// <summary>Returns the user with the specified identifier, or <see langword="null"/> if not found.</summary>
    Task<User?> GetByIdAsync(Guid id);

    /// <summary>Persists changes to an existing user.</summary>
    Task UpdateAsync(User user);
}
