namespace NS.Domain;

/// <summary>Class-specific resource pools that vary by hero class and level.</summary>
/// <param name="JudgmentDiceCount">The number of Judgment Dice available; applies to Oathsworn.</param>
/// <param name="JudgmentDiceType">The die type used for Judgment Dice; applies to Oathsworn (<see cref="DieType.D6"/> at levels 1–2, <see cref="DieType.D8"/> from level 3).</param>
/// <param name="LayOnHandsPool">The current Lay on Hands healing pool remaining; applies to Oathsworn (max = 5 × level).</param>
/// <param name="ThrillCharges">The current Thrill of the Hunt charges available; applies to Hunter.</param>
public sealed record ClassResources(
    int? JudgmentDiceCount,
    DieType? JudgmentDiceType,
    int? LayOnHandsPool,
    int? ThrillCharges);
