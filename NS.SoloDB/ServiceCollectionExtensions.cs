namespace NSSoloDB;

/// <summary>Extension methods for registering NimbleSheets SoloDB data services with the DI container.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the SoloDB database and all NimbleSheets data services as singletons.
    /// </summary>
    /// <remarks>
    /// SoloDB is thread-safe via its internal connection pool, making <see cref="ServiceLifetime.Singleton"/>
    /// the correct lifetime for both the database instance and the services that wrap it.
    /// </remarks>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="databasePath">
    /// Path to the SoloDB database file, e.g. <c>"nimble-sheet.db"</c>,
    /// or a named in-memory URI, e.g. <c>"memory:nimble"</c>.
    /// </param>
    public static IServiceCollection AddSoloDBDataServices(
        this IServiceCollection services,
        string databasePath)
    {
        services.AddSingleton<SoloDB>(_ => new SoloDB(databasePath));
        services.AddSingleton<IHeroDataService, SoloHeroDataService>();
        services.AddSingleton<IUserDataService, SoloUserDataService>();
        services.AddSingleton<IReferenceDataSeeder, SoloReferenceDataSeeder>();
        services.AddSingleton<IReferenceDataService<ActionReference>, SoloReferenceDataService<ActionReference>>();
        services.AddSingleton<IReferenceDataService<Ancestry>, SoloReferenceDataService<Ancestry>>();
        services.AddSingleton<IReferenceDataService<Armor>, SoloReferenceDataService<Armor>>();
        services.AddSingleton<IReferenceDataService<Background>, SoloReferenceDataService<Background>>();
        services.AddSingleton<IReferenceDataService<Condition>, SoloReferenceDataService<Condition>>();
        services.AddSingleton<IReferenceDataService<Feature>, SoloReferenceDataService<Feature>>();
        services.AddSingleton<IReferenceDataService<MagicItem>, SoloReferenceDataService<MagicItem>>();
        services.AddSingleton<IReferenceDataService<RuleReference>, SoloReferenceDataService<RuleReference>>();
        services.AddSingleton<IReferenceDataService<Spell>, SoloReferenceDataService<Spell>>();
        services.AddSingleton<IReferenceDataService<Weapon>, SoloReferenceDataService<Weapon>>();
        return services;
    }
}
