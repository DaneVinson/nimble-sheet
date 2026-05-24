namespace NS.Domain;

/// <summary>The four base stats of a hero.</summary>
/// <param name="Dexterity">Agility, reflexes, and precision. Affects DEX weapon damage, Initiative, DEX saves, and the Stealth and Finesse skills.</param>
/// <param name="Intelligence">Knowledge and reasoning. Affects spellcasting, INT saves, and the Arcana, Examination, and Lore skills.</param>
/// <param name="Strength">Raw physical power. Affects STR weapon damage, HP recovery, STR saves, and the Might skill.</param>
/// <param name="Will">Force of personality and wisdom. Affects spellcasting, WIL saves, and the Insight, Influence, Naturecraft, and Perception skills.</param>
public sealed record HeroStats(
    int Dexterity,
    int Intelligence,
    int Strength,
    int Will);
