namespace NS.Tests;

/// <summary>
/// Cross-type isolation tests for the SoloDB data services. Each entity type must occupy its
/// own physical collection; storing one type must never make its documents surface through a
/// service for a different type. These guard against the regression where every
/// <c>SoloDocument&lt;T&gt;</c> shared a single collection (because SoloDB derives the default
/// collection name from the generic wrapper type, which is identical for every <c>T</c>).
/// </summary>
public sealed class SoloCollectionIsolationTests
{
    /// <summary>A stored user does not surface as a hero through the hero service.</summary>
    [Fact]
    public async Task CreatingUser_DoesNotAppearInHeroes()
    {
        using var db = new SoloDB($"memory:test-{Guid.CreateVersion7()}");
        var userService = new SoloUserDataService(db);
        await userService.CreateAsync(NewUser());

        var heroes = await new SoloHeroDataService(db).GetAllAsync();

        Assert.Empty(heroes);
    }

    /// <summary>A stored user does not surface as reference data through a reference service.</summary>
    [Fact]
    public async Task CreatingUser_DoesNotAppearInReferenceData()
    {
        using var db = new SoloDB($"memory:test-{Guid.CreateVersion7()}");
        var userService = new SoloUserDataService(db);
        await userService.CreateAsync(NewUser());

        var ancestries = await new SoloReferenceDataService<Ancestry>(db).GetAllAsync();

        Assert.Empty(ancestries);
    }

    /// <summary>A user and a hero saved to the same database stay in isolated collections.</summary>
    [Fact]
    public async Task UserAndHero_AreStoredInSeparateCollections()
    {
        using var db = new SoloDB($"memory:test-{Guid.CreateVersion7()}");
        var userService = new SoloUserDataService(db);
        var heroService = new SoloHeroDataService(db);
        var user = NewUser();
        var hero = TestHero.Create();
        await userService.CreateAsync(user);
        await heroService.SaveAsync(hero);

        var loadedUser = await userService.GetByIdAsync(user.Id);
        var heroes = await heroService.GetAllAsync();

        Assert.NotNull(loadedUser);
        Assert.Equal(user.Id, loadedUser!.Id);
        Assert.Single(heroes);
        Assert.Equal(hero.Id, heroes[0].Id);
    }

    private static User NewUser() =>
        new(DateTimeOffset.UtcNow, "caldra@example.com", Guid.CreateVersion7(), "Caldra");
}
