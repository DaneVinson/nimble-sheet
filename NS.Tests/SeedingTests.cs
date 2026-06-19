namespace NS.Tests;

/// <summary>Tests for the SoloDB reference-data seeder.</summary>
public sealed class SeedingTests
{
    /// <summary>Seeding a fresh database populates every reference collection.</summary>
    [Fact]
    public async Task SeedAsync_PopulatesEveryReferenceCollection()
    {
        using var db = new SoloDB($"memory:seed-{Guid.CreateVersion7()}");

        await new SoloReferenceDataSeeder(db).SeedAsync();

        Assert.NotEmpty(await new SoloReferenceDataService<ActionReference>(db).GetAllAsync());
        Assert.NotEmpty(await new SoloReferenceDataService<Ancestry>(db).GetAllAsync());
        Assert.NotEmpty(await new SoloReferenceDataService<Armor>(db).GetAllAsync());
        Assert.NotEmpty(await new SoloReferenceDataService<Background>(db).GetAllAsync());
        Assert.NotEmpty(await new SoloReferenceDataService<Condition>(db).GetAllAsync());
        Assert.NotEmpty(await new SoloReferenceDataService<Feature>(db).GetAllAsync());
        Assert.NotEmpty(await new SoloReferenceDataService<MagicItem>(db).GetAllAsync());
        Assert.NotEmpty(await new SoloReferenceDataService<RuleReference>(db).GetAllAsync());
        Assert.NotEmpty(await new SoloReferenceDataService<Spell>(db).GetAllAsync());
        Assert.NotEmpty(await new SoloReferenceDataService<Weapon>(db).GetAllAsync());
    }

    /// <summary>Seeding twice does not duplicate rows (the empty-check makes it idempotent).</summary>
    [Fact]
    public async Task SeedAsync_IsIdempotent()
    {
        using var db = new SoloDB($"memory:seed-{Guid.CreateVersion7()}");
        var seeder = new SoloReferenceDataSeeder(db);

        await seeder.SeedAsync();
        var afterFirst = (await new SoloReferenceDataService<Ancestry>(db).GetAllAsync()).Count;
        await seeder.SeedAsync();
        var afterSecond = (await new SoloReferenceDataService<Ancestry>(db).GetAllAsync()).Count;

        Assert.Equal(afterFirst, afterSecond);
    }

    /// <summary>The Human ancestry is seeded under the fixed fixture GUID.</summary>
    [Fact]
    public async Task SeedAsync_SeedsHumanAncestryWithKnownId()
    {
        using var db = new SoloDB($"memory:seed-{Guid.CreateVersion7()}");
        await new SoloReferenceDataSeeder(db).SeedAsync();

        var human = await new SoloReferenceDataService<Ancestry>(db)
            .GetByIdAsync(new Guid("a0000000-0000-0000-0000-000000000001"));

        Assert.NotNull(human);
        Assert.Equal("Human", human!.Name);
    }

    /// <summary>The full quickstart spell set is seeded with all four schools represented.</summary>
    [Fact]
    public async Task SeedAsync_SeedsAllSixteenQuickstartSpellsAcrossFourSchools()
    {
        using var db = new SoloDB($"memory:seed-{Guid.CreateVersion7()}");
        await new SoloReferenceDataSeeder(db).SeedAsync();

        var spells = await new SoloReferenceDataService<Spell>(db).GetAllAsync();

        Assert.Equal(16, spells.Count);
        Assert.Equal(
            new[] { SpellSchool.Fire, SpellSchool.Ice, SpellSchool.Lightning, SpellSchool.Radiant },
            spells.Select(spell => spell.School).Distinct().Order());
    }

    /// <summary>The Flame Dart cantrip is seeded as an authentic Fire cantrip.</summary>
    [Fact]
    public async Task SeedAsync_SeedsFlameDartAsAFireCantrip()
    {
        using var db = new SoloDB($"memory:seed-{Guid.CreateVersion7()}");
        await new SoloReferenceDataSeeder(db).SeedAsync();

        var flameDart = await new SoloReferenceDataService<Spell>(db)
            .GetByIdAsync(new Guid("e0000000-0000-0000-0000-000000000001"));

        Assert.NotNull(flameDart);
        Assert.Equal("Flame Dart", flameDart!.Name);
        Assert.Equal(SpellSchool.Fire, flameDart.School);
        Assert.Equal(0, flameDart.Tier);
        Assert.Equal(0, flameDart.ManaCost);
        Assert.Equal("1d10", flameDart.DamageExpression);
    }

    /// <summary>Radiant Judgment is seeded as an Oathsworn level-1 feature under the fixed fixture GUID.</summary>
    [Fact]
    public async Task SeedAsync_SeedsRadiantJudgmentAsOathswornLevelOneFeature()
    {
        using var db = new SoloDB($"memory:seed-{Guid.CreateVersion7()}");
        await new SoloReferenceDataSeeder(db).SeedAsync();

        var feature = await new SoloReferenceDataService<Feature>(db)
            .GetByIdAsync(new Guid("d0000000-0000-0000-0000-000000000001"));

        Assert.NotNull(feature);
        Assert.Equal("Radiant Judgment", feature!.Name);
        Assert.Equal(HeroClass.Oathsworn, feature.Class);
        Assert.Equal(1, feature.Level);
    }
}
