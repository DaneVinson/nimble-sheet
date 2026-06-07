namespace NSFastEndpoints;

/// <summary>Extension methods layering ownership checks over <see cref="IHeroDataService"/>.</summary>
public static class IHeroDataServiceExtensions
{
    /// <summary>Returns the hero with the specified identifier only if it is owned by the specified user; otherwise <see langword="null"/>.</summary>
    public static async Task<Hero?> GetOwnedByIdAsync(this IHeroDataService heroes, Guid id, Guid userId)
    {
        var hero = await heroes.GetByIdAsync(id);
        return hero is not null && hero.UserId == userId ? hero : null;
    }
}
