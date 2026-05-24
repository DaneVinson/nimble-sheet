namespace NSSoloDB;

/// <summary>
/// Internal SoloDB envelope that provides a <see cref="long"/> primary key
/// while storing a domain entity as a nested JSON document.
/// </summary>
/// <remarks>
/// Domain entities use <see cref="System.Guid"/> identifiers, which SoloDB does
/// not recognise as a primary key without the <c>[SoloId]</c> attribute. This
/// wrapper gives SoloDB its required <c>long Id</c> and keeps the domain model
/// free of any persistence concerns.
/// </remarks>
internal sealed class SoloDocument<T>
{
    /// <summary>The SoloDB-managed auto-increment primary key.</summary>
    public long Id { get; set; }

    /// <summary>The stored domain entity.</summary>
    public T Data { get; set; } = default!;
}
