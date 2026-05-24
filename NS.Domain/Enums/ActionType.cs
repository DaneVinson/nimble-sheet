namespace NS.Domain;

/// <summary>The type of action a hero can take in combat.</summary>
public enum ActionType
{
    /// <summary>Does not cost an action or any other resource.</summary>
    Free,
    /// <summary>Costs one or more of a hero's three actions per turn.</summary>
    Heroic,
    /// <summary>Costs 1 action and is performed on another creature's turn.</summary>
    Reaction
}
