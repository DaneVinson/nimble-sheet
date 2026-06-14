namespace NSSoloDB;

/// <summary>
/// Resolves the SoloDB collection that stores a given domain entity type.
/// </summary>
/// <remarks>
/// Every entity is persisted wrapped in <see cref="SoloDocument{T}"/>. Without an explicit
/// name, SoloDB derives a collection name from the wrapper type, and a closed generic's type
/// name is identical for every <c>T</c> (<c>SoloDocument`1</c>) — so all entity types would
/// collide in a single collection. Naming each collection after the domain type keeps every
/// entity type isolated.
/// </remarks>
internal static class SoloCollections
{
    /// <summary>Gets the <see cref="SoloDocument{T}"/> collection for entity type <typeparamref name="T"/>.</summary>
    /// <typeparam name="T">The domain entity type the collection stores.</typeparam>
    /// <param name="database">The SoloDB instance to resolve the collection from.</param>
    /// <returns>The collection dedicated to <typeparamref name="T"/>, named after the domain type.</returns>
    public static ISoloDBCollection<SoloDocument<T>> Of<T>(SoloDB database) =>
        database.GetCollection<SoloDocument<T>>(typeof(T).Name);
}
