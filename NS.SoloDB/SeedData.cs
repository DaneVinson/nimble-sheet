namespace NSSoloDB;

/// <summary>The curated starter set of reference data inserted by the seeder.</summary>
/// <remarks>
/// GUIDs are fixed and hand-written (never <see cref="System.Guid.CreateVersion7"/>) so heroes can
/// reference rows by a known id and seeding stays deterministic across restarts. The rows that the
/// client <c>caldra.ts</c> fixture references reuse that fixture's exact GUIDs.
/// </remarks>
internal static class SeedData
{
    /// <summary>Common combat/movement actions.</summary>
    internal static IReadOnlyList<ActionReference> Actions { get; } =
    [
        new(ActionType.Heroic, 1, "Make a weapon or spell attack.", null,
            new Guid("ac000000-0000-0000-0000-000000000001"), "Attack"),
        new(ActionType.Free, 0, "Move up to your speed.", null,
            new Guid("ac000000-0000-0000-0000-000000000002"), "Dash"),
        new(ActionType.Reaction, 1, "Impose disadvantage on an attack against you.", "Once per round",
            new Guid("ac000000-0000-0000-0000-000000000003"), "Defend"),
    ];

    /// <summary>Playable ancestries.</summary>
    internal static IReadOnlyList<Ancestry> Ancestries { get; } =
    [
        new("Versatile and ambitious.", new Guid("a0000000-0000-0000-0000-000000000001"), "Human", ["Adaptable"]),
        new("Graceful and long-lived.", new Guid("a0000000-0000-0000-0000-000000000002"), "Elf", ["Keen Senses", "Trance"]),
        new("Stout and steadfast.", new Guid("a0000000-0000-0000-0000-000000000003"), "Dwarf", ["Darkvision", "Stonecunning"]),
    ];

    /// <summary>Wearable armor and shields.</summary>
    internal static IReadOnlyList<Armor> Armor { get; } =
    [
        new(ArmorType.Mail, 6, "6 + DEX armor.", new Guid("c0000000-0000-0000-0000-000000000001"), "Rusty Mail"),
        new(ArmorType.Shield, 2, "+2 armor.", new Guid("c0000000-0000-0000-0000-000000000002"), "Wooden Buckler"),
        new(ArmorType.Cloth, 3, "3 + DEX armor.", new Guid("c0000000-0000-0000-0000-000000000003"), "Padded Cloth"),
        new(ArmorType.Leather, 4, "4 + DEX armor.", new Guid("c0000000-0000-0000-0000-000000000004"), "Leather Jerkin"),
    ];

    /// <summary>Character backgrounds.</summary>
    internal static IReadOnlyList<Background> Backgrounds { get; } =
    [
        new("Raised in a temple.", "Advantage on Insight checks about religion.",
            new Guid("ba000000-0000-0000-0000-000000000001"), "Acolyte"),
        new("Trained in a fighting company.", "Proficiency with martial drills.",
            new Guid("ba000000-0000-0000-0000-000000000002"), "Soldier"),
    ];

    /// <summary>Status conditions.</summary>
    internal static IReadOnlyList<Condition> Conditions { get; } =
    [
        new("You are lying down; melee attacks against you have advantage.",
            new Guid("f0000000-0000-0000-0000-000000000001"), "Prone"),
        new("You take 1d4 damage at the start of each of your turns until healed.",
            new Guid("f0000000-0000-0000-0000-000000000002"), "Bleeding"),
        new("You cannot take Reactions and your speed is halved.",
            new Guid("f0000000-0000-0000-0000-000000000003"), "Dazed"),
    ];

    /// <summary>Class features.</summary>
    internal static IReadOnlyList<Feature> Features { get; } =
    [
        new(HeroClass.Oathsworn,
            "When an enemy attacks you, if you have no Judgment Dice, roll your Judgment Dice (2d6). On your next melee hit this encounter, deal that much additional radiant damage.",
            null, new Guid("d0000000-0000-0000-0000-000000000001"), 1, "Radiant Judgment", null, null),
        new(HeroClass.Oathsworn,
            "A magical pool of healing power equal to 5 x LVL. Action: touch a target and spend any amount to restore that many HP.",
            null, new Guid("d0000000-0000-0000-0000-000000000002"), 1, "Lay on Hands", null, null),
        new(HeroClass.Mage,
            "Once per day on a Field Rest, recover mana equal to your level.",
            "Once per day", new Guid("d0000000-0000-0000-0000-000000000003"), 1, "Arcane Recovery", null, null),
    ];

    /// <summary>Magic items.</summary>
    internal static IReadOnlyList<MagicItem> MagicItems { get; } =
    [
        new(new Guid("e0000000-0000-0000-0000-000000000001"), "A slender wand humming with heat.",
            "Cast Firebolt without spending mana.", new Guid("da000000-0000-0000-0000-000000000001"),
            3, "Wand of Firebolt", "Uncommon"),
        new(null, "A traveler's cloak.", "+1 to saving throws while worn.",
            new Guid("da000000-0000-0000-0000-000000000002"), null, "Cloak of Resolve", "Common"),
    ];

    /// <summary>Rules references across categories.</summary>
    internal static IReadOnlyList<RuleReference> Rules { get; } =
    [
        new(RuleCategory.Combat, "At 0 HP you are Dying; make death saves until stabilized or healed.",
            new Guid("ce000000-0000-0000-0000-000000000001"), "Dying"),
        new(RuleCategory.Resting, "A short rest that recovers Hit Dice and some resources.",
            new Guid("ce000000-0000-0000-0000-000000000002"), "Field Rest"),
        new(RuleCategory.Conditions, "Each Wound is permanent until removed; 6 Wounds means death.",
            new Guid("ce000000-0000-0000-0000-000000000003"), "Wounds"),
    ];

    /// <summary>Spells, one Mage spell per school.</summary>
    internal static IReadOnlyList<Spell> Spells { get; } =
    [
        new(1, null, "1d8", DamageType.Fire, "Hurl a bolt of fire at one target.", null,
            new Guid("e0000000-0000-0000-0000-000000000001"), false, false, 1, "Firebolt", 12,
            StatType.Dexterity, SpellSchool.Fire, 1, "Deal +1d8 per extra mana."),
        new(1, null, "1d6", DamageType.Cold, "A shard of ice slows and chills a target.", null,
            new Guid("e0000000-0000-0000-0000-000000000002"), false, false, 1, "Frost Shard", 10,
            StatType.Strength, SpellSchool.Ice, 1, null),
        new(1, "10-ft line", "1d6", DamageType.Lightning, "A line of crackling lightning.", null,
            new Guid("e0000000-0000-0000-0000-000000000003"), false, false, 2, "Spark", 8,
            StatType.Dexterity, SpellSchool.Lightning, 1, null),
        new(1, null, "1d8", DamageType.Radiant, "A searing beam of holy light.", null,
            new Guid("e0000000-0000-0000-0000-000000000004"), false, false, 2, "Radiant Beam", 12,
            StatType.Will, SpellSchool.Radiant, 1, null),
    ];

    /// <summary>Weapons.</summary>
    internal static IReadOnlyList<Weapon> Weapons { get; } =
    [
        new("1d6+2", DamageType.Bludgeoning, "A simple bludgeoning weapon.",
            new Guid("b0000000-0000-0000-0000-000000000001"), false, false, "Mace", null, 1, null, StatType.Strength),
        new("1d6", DamageType.Piercing, "A light ranged bow.",
            new Guid("b0000000-0000-0000-0000-000000000002"), false, true, "Shortbow", 12, 0, "Ranged.", StatType.Dexterity),
        new("1d12", DamageType.Slashing, "A massive two-handed blade.",
            new Guid("b0000000-0000-0000-0000-000000000003"), false, true, "Greatsword", null, 1, null, StatType.Strength),
    ];
}
