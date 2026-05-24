namespace NS.Domain;

/// <summary>The four base stats that define a hero's capabilities.</summary>
public enum StatType
{
    /// <summary>Agility, reflexes, and precision. Drives DEX weapons, Initiative, and DEX saves.</summary>
    Dexterity,
    /// <summary>Knowledge and reasoning. Drives spellcasting, INT saves, and knowledge skills.</summary>
    Intelligence,
    /// <summary>Raw physical power. Drives STR weapons, HP recovery, STR saves, and Might.</summary>
    Strength,
    /// <summary>Force of personality and wisdom. Drives spellcasting, WIL saves, and social and nature skills.</summary>
    Will
}
