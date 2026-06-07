namespace NS.Tests;

/// <summary>
/// Round-trip tests for <see cref="SoloHeroDataService"/> against an in-memory SoloDB.
/// These guard the Phase A deserialization fixes: SoloDB rehydrates entities as uninitialized
/// objects, which previously left <see cref="Hero.Id"/> as <see cref="Guid.Empty"/> and threw on
/// the collection accessors.
/// </summary>
public sealed class HeroPersistenceTests
{
    /// <summary>The user-scoped query returns only heroes owned by the requested user.</summary>
    [Fact]
    public async Task GetByUserAsync_ReturnsOnlyHeroesOwnedByUser()
    {
        using var db = new SoloDB($"memory:test-{Guid.CreateVersion7()}");
        var service = new SoloHeroDataService(db);
        var alice = Guid.CreateVersion7();
        var bob = Guid.CreateVersion7();
        await service.SaveAsync(TestHero.Create(userId: alice));
        await service.SaveAsync(TestHero.Create(userId: alice));
        await service.SaveAsync(TestHero.Create(userId: bob));

        var aliceHeroes = await service.GetByUserAsync(alice);

        Assert.Equal(2, aliceHeroes.Count);
        Assert.All(aliceHeroes, h => Assert.Equal(alice, h.UserId));
    }

    /// <summary>A saved hero round-trips with its identifier, owner, scalars, and collections intact.</summary>
    [Fact]
    public async Task SaveAndGetById_RoundTripsHeroThroughSoloDb()
    {
        using var db = new SoloDB($"memory:test-{Guid.CreateVersion7()}");
        var service = new SoloHeroDataService(db);
        var userId = Guid.CreateVersion7();
        var hero = TestHero.Create(maxHp: 17, userId: userId);
        hero.AddWeapon(new HeroWeapon(hero.Id, true, "sharp", Guid.CreateVersion7()));
        await service.SaveAsync(hero);

        var loaded = await service.GetByIdAsync(hero.Id);

        Assert.NotNull(loaded);
        Assert.NotEqual(Guid.Empty, loaded!.Id);
        Assert.Equal(hero.Id, loaded.Id);
        Assert.Equal(userId, loaded.UserId);
        Assert.Equal(17, loaded.MaxHp);
        Assert.Equal("Caldra", loaded.Name);
        Assert.Single(loaded.Weapons);
    }
}
