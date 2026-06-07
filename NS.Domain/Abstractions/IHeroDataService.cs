namespace NS.Domain;

/// <summary>Defines persistence operations for <see cref="Hero"/> aggregate instances.</summary>
public interface IHeroDataService
{
    /// <summary>Deletes the hero with the specified identifier.</summary>
    Task DeleteAsync(Guid id);

    /// <summary>Returns all heroes.</summary>
    Task<IReadOnlyList<Hero>> GetAllAsync();

    /// <summary>Returns the hero with the specified identifier, or <see langword="null"/> if not found.</summary>
    Task<Hero?> GetByIdAsync(Guid id);

    /// <summary>Returns all heroes owned by the specified user.</summary>
    Task<IReadOnlyList<Hero>> GetByUserAsync(Guid userId);

    /// <summary>Creates or updates the specified hero.</summary>
    Task SaveAsync(Hero hero);
}
