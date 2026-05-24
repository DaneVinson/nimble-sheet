namespace NS.Domain;

/// <summary>The category of armor, determining how damage modifiers are applied.</summary>
public enum ArmorType
{
    /// <summary>Cloth or no armor; all damage modifiers apply normally.</summary>
    Cloth,
    /// <summary>Light armor such as leather hides.</summary>
    Leather,
    /// <summary>Medium armor such as chain or ring mail.</summary>
    Mail,
    /// <summary>Heavy plate armor.</summary>
    Plate,
    /// <summary>A held shield providing additional protection.</summary>
    Shield
}
