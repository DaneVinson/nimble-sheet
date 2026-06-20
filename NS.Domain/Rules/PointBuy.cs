namespace NS.Domain;

/// <summary>Point-buy rules for purchasing a hero's base ability scores at creation.</summary>
public static class PointBuy
{
    /// <summary>The total points available to spend across all ability scores.</summary>
    public const int Budget = 27;

    /// <summary>The maximum purchasable score.</summary>
    public const int MaxScore = 15;

    /// <summary>The minimum (free) score.</summary>
    public const int MinScore = 8;

    private static readonly IReadOnlyDictionary<int, int> _costByScore = new Dictionary<int, int>
    {
        [8] = 0, [9] = 1, [10] = 2, [11] = 3, [12] = 4, [13] = 5, [14] = 7, [15] = 9,
    };

    /// <summary>The point cost of a single score. Throws when the score is outside 8–15.</summary>
    /// <param name="score">The ability score.</param>
    public static int CostOf(int score)
    {
        return _costByScore.TryGetValue(score, out var cost)
            ? cost
            : throw new ArgumentOutOfRangeException(nameof(score), score, "Score must be between 8 and 15.");
    }

    /// <summary>Whether a set of base ability scores is a legal point-buy purchase.</summary>
    /// <param name="scores">The base ability scores.</param>
    public static bool IsValid(AbilityScores scores)
    {
        return InRange(scores.Dexterity) && InRange(scores.Intelligence)
            && InRange(scores.Strength) && InRange(scores.Will)
            && TotalCost(scores) <= Budget;
    }

    /// <summary>The total point cost of a full set of ability scores.</summary>
    /// <param name="scores">The base ability scores.</param>
    public static int TotalCost(AbilityScores scores)
    {
        return CostOf(scores.Dexterity) + CostOf(scores.Intelligence)
            + CostOf(scores.Strength) + CostOf(scores.Will);
    }

    private static bool InRange(int score)
    {
        return score is >= MinScore and <= MaxScore;
    }
}
