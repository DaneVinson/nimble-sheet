namespace NS.Domain;

/// <summary>The type of damage dealt by an attack, spell, or effect.</summary>
public enum DamageType
{
    /// <summary>Blunt force trauma from clubs, mauls, etc.</summary>
    Bludgeoning,
    /// <summary>Cold or ice damage.</summary>
    Cold,
    /// <summary>Fire and heat damage.</summary>
    Fire,
    /// <summary>Electrical damage.</summary>
    Lightning,
    /// <summary>Piercing damage from arrows, spears, etc.</summary>
    Piercing,
    /// <summary>Mental or psychic damage.</summary>
    Psychic,
    /// <summary>Holy or radiant energy damage.</summary>
    Radiant,
    /// <summary>Cutting damage from blades and claws.</summary>
    Slashing
}
