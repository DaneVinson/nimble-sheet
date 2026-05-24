namespace NS.Domain;

/// <summary>The thematic category of a quick-reference rule entry.</summary>
public enum RuleCategory
{
    /// <summary>Rules governing combat actions, attacks, reactions, and damage.</summary>
    Combat,
    /// <summary>Rules for specific status conditions such as Prone, Dying, or Grappled.</summary>
    Conditions,
    /// <summary>Rules for advancing a hero to the next level.</summary>
    LevelUp,
    /// <summary>Rules for movement, speed, range, reach, and falling.</summary>
    Movement,
    /// <summary>Rules for Field Rests and Safe Rests.</summary>
    Resting,
    /// <summary>Rules for casting, upcasting, mana costs, and spell requirements.</summary>
    Spellcasting
}
