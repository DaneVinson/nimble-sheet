namespace NS.Domain;

/// <summary>The ten skill bonuses of a hero, each capped at +12.</summary>
/// <param name="Arcana">Bonus to Arcana (INT): understanding of magical phenomena and enchantments.</param>
/// <param name="Examination">Bonus to Examination (INT): uncovering clues, diagnosing injuries, and unraveling traps or devices.</param>
/// <param name="Finesse">Bonus to Finesse (DEX): lockpicking, disarming traps, tinkering, and other careful hand work.</param>
/// <param name="Influence">Bonus to Influence (WIL): persuasion, charm, and captivating performance.</param>
/// <param name="Insight">Bonus to Insight (WIL): understanding people, detecting lies, and making sense of situations.</param>
/// <param name="Lore">Bonus to Lore (INT): history of civilizations, kingdoms, religions, and cultural practices.</param>
/// <param name="Might">Bonus to Might (STR): lifting, breaking obstacles, climbing, swimming, and feats of strength.</param>
/// <param name="Naturecraft">Bonus to Naturecraft (WIL): wilderness survival, navigation, tracking, and animal handling.</param>
/// <param name="Perception">Bonus to Perception (WIL): spotting hidden objects, secret passages, and concealed creatures.</param>
/// <param name="Stealth">Bonus to Stealth (DEX): moving unseen and unheard.</param>
public sealed record HeroSkills(
    int Arcana,
    int Examination,
    int Finesse,
    int Influence,
    int Insight,
    int Lore,
    int Might,
    int Naturecraft,
    int Perception,
    int Stealth);
