namespace NSSoloDB;

/// <summary>The starter set of reference data inserted by the seeder.</summary>
/// <remarks>
/// Content is sourced from the Nimble quickstart rules (see <c>docs/rules/nimble-basic-rules.md</c>).
/// Where the quickstart defines an entity it is transcribed faithfully — <see cref="Spells"/>,
/// <see cref="Features"/> (full level 1-4 sets for the four quickstart classes), <see cref="Conditions"/>,
/// <see cref="Actions"/>, and <see cref="Rules"/>. Entities the quickstart does <b>not</b> define
/// (<see cref="Ancestries"/>, <see cref="Backgrounds"/>, <see cref="Armor"/>, <see cref="Weapons"/>) are
/// kept as honest, clearly-labeled placeholders; their descriptions say so rather than presenting
/// invented data as canonical.
///
/// GUIDs are fixed and hand-written (never <see cref="System.Guid.CreateVersion7"/>) so heroes can
/// reference rows by a known id and seeding stays deterministic across restarts. Six GUIDs are reused
/// by the client <c>fixtures/caldra.ts</c> fixture and the seeding tests and must not change: Human
/// (<c>a…0001</c>), Mace (<c>b…0001</c>), Rusty Mail (<c>c…0001</c>), Wooden Buckler (<c>c…0002</c>),
/// Radiant Judgment (<c>d…0001</c>), and Lay on Hands (<c>d…0002</c>).
///
/// Seeding is seed-only-when-empty, so editing this data later requires a fresh database.
/// </remarks>
internal static class SeedData
{
    /// <summary>The core actions and reactions available in combat.</summary>
    internal static IReadOnlyList<ActionReference> Actions { get; } =
    [
        new(ActionType.Heroic, 1,
            "Roll the die on your weapon, spell, or ability and deal that much damage. A roll of 1 on the Primary Die misses; rolling the max on the Primary Die is a crit (roll it again and add, repeating).",
            null, new Guid("ac000000-0000-0000-0000-000000000001"), "Attack"),
        new(ActionType.Heroic, 1,
            "Move up to your speed (default 6 spaces). Movement can be split around other actions, and you may spend multiple actions to move multiple times.",
            null, new Guid("ac000000-0000-0000-0000-000000000002"), "Move"),
        new(ActionType.Reaction, 1, "Reduce the damage from a single attack by your Armor.",
            "Once per round", new Guid("ac000000-0000-0000-0000-000000000003"), "Defend"),
        new(ActionType.Reaction, 1,
            "When a creature within 2 spaces would be struck, push them aside and become the new target; you enter their space and move them to an adjacent space.",
            "Once per round", new Guid("ac000000-0000-0000-0000-000000000004"), "Interpose"),
        new(ActionType.Reaction, 1,
            "A melee attack made with disadvantage against an adjacent enemy as it willingly moves away. Only heroes make opportunity attacks.",
            null, new Guid("ac000000-0000-0000-0000-000000000005"), "Opportunity Attack"),
        new(ActionType.Reaction, 1,
            "Grant an ally advantage on a roll if you can reasonably explain how you help (limit one Help per roll).",
            null, new Guid("ac000000-0000-0000-0000-000000000006"), "Help"),
        new(ActionType.Heroic, 1,
            "Make a DC 12 skill check to Ask a Question, Create an Opening (+1 to your next Primary Die against a target), or Anticipate Danger (-1 to Primary Dice against you). You cannot reuse the same skill to Assess in one encounter.",
            null, new Guid("ac000000-0000-0000-0000-000000000007"), "Assess"),
        new(ActionType.Free, 0,
            "Simple tasks — open an unlocked door, drop an item, shout a short phrase, end concentration — cost no action or resource.",
            "Once per turn", new Guid("ac000000-0000-0000-0000-000000000008"), "Free Action"),
    ];

    /// <summary>Playable ancestries. Placeholder — the Nimble quickstart does not define ancestries.</summary>
    internal static IReadOnlyList<Ancestry> Ancestries { get; } =
    [
        new("Versatile and ambitious. (Placeholder — the quickstart rules do not define ancestries; the full game has 5 common and 19 exotic ancestries.)",
            new Guid("a0000000-0000-0000-0000-000000000001"), "Human", ["Adaptable"]),
        new("Graceful and long-lived. (Placeholder — not defined in the quickstart rules.)",
            new Guid("a0000000-0000-0000-0000-000000000002"), "Elf", ["Keen Senses"]),
        new("Stout and steadfast. (Placeholder — not defined in the quickstart rules.)",
            new Guid("a0000000-0000-0000-0000-000000000003"), "Dwarf", ["Stonecunning"]),
    ];

    /// <summary>Wearable armor and shields. Mostly placeholder/inferred — the quickstart has no armor table.</summary>
    internal static IReadOnlyList<Armor> Armor { get; } =
    [
        new(ArmorType.Mail, 6, "6 + DEX armor. Oathsworn starting armor.",
            new Guid("c0000000-0000-0000-0000-000000000001"), "Rusty Mail"),
        new(ArmorType.Shield, 2, "+2 armor.",
            new Guid("c0000000-0000-0000-0000-000000000002"), "Wooden Buckler"),
        new(ArmorType.Cloth, 3, "3 + DEX armor. (Inferred — the quickstart has no armor table.)",
            new Guid("c0000000-0000-0000-0000-000000000003"), "Cloth Garb"),
        new(ArmorType.Leather, 4, "4 + DEX armor. (Inferred — the quickstart has no armor table.)",
            new Guid("c0000000-0000-0000-0000-000000000004"), "Leather Armor"),
        new(ArmorType.Plate, 8, "8 armor. (Inferred placeholder — not defined in the quickstart rules.)",
            new Guid("c0000000-0000-0000-0000-000000000005"), "Plate Mail"),
    ];

    /// <summary>Character backgrounds. Placeholder — the quickstart does not define backgrounds.</summary>
    internal static IReadOnlyList<Background> Backgrounds { get; } =
    [
        new("(Placeholder — the quickstart rules do not define backgrounds; the full game has 24.)",
            "No mechanical grant defined.",
            new Guid("ba000000-0000-0000-0000-000000000001"), "Adventurer"),
        new("(Placeholder — not defined in the quickstart rules.)",
            "No mechanical grant defined.",
            new Guid("ba000000-0000-0000-0000-000000000002"), "Wanderer"),
    ];

    /// <summary>Status conditions referenced by the quickstart rules.</summary>
    internal static IReadOnlyList<Condition> Conditions { get; } =
    [
        new("You cannot see; attacks you make miss the wrong target and attacks against you are favored. (Referenced by Snowblind; see core rules for the full effect.)",
            new Guid("f0000000-0000-0000-0000-000000000001"), "Blinded"),
        new("Whenever you take lightning damage you are Charged for 1 minute. Some lightning spells require or consume the Charged condition.",
            new Guid("f0000000-0000-0000-0000-000000000002"), "Charged"),
        new("A target is Distracted if it is adjacent to or Taunted by an ally, or if it cannot see you.",
            new Guid("f0000000-0000-0000-0000-000000000003"), "Distracted"),
        new("At 0 HP you are Dying until you regain HP. While Dying you have only 1 action; attacking or casting causes 1 Wound unless you make a DC 10 STR save; taking damage causes 2 Wounds (3 on a crit).",
            new Guid("f0000000-0000-0000-0000-000000000004"), "Dying"),
        new("Overcome by fear. (Referenced — Rebuke deals double damage to Frightened targets; see core rules.)",
            new Guid("f0000000-0000-0000-0000-000000000005"), "Frightened"),
        new("Held in place and unable to move (escape DC varies). (See core rules.)",
            new Guid("f0000000-0000-0000-0000-000000000006"), "Grappled"),
        new("Off-balance or impeded. (Referenced by Shatter — a max die against a Hampered target counts as a crit; see core rules.)",
            new Guid("f0000000-0000-0000-0000-000000000007"), "Hampered"),
        new("You cannot take actions on your next turn. (Referenced by the Cheat's Low Blow.)",
            new Guid("f0000000-0000-0000-0000-000000000008"), "Incapacitated"),
        new("You cannot be seen. (Referenced by the Cheat's Silent Blade and the Cloak of Lesser Windform.)",
            new Guid("f0000000-0000-0000-0000-000000000009"), "Invisible"),
        new("Disadvantage on rolls; the condition ends when you are healed.",
            new Guid("f0000000-0000-0000-0000-00000000000a"), "Poisoned"),
        new("You are lying down; melee attacks against you have advantage.",
            new Guid("f0000000-0000-0000-0000-00000000000b"), "Prone"),
        new("Held fast; escape DC 12, or end it by taking any slashing or fire damage.",
            new Guid("f0000000-0000-0000-0000-00000000000c"), "Restrained"),
        new("Your speed is halved on your next turn.",
            new Guid("f0000000-0000-0000-0000-00000000000d"), "Slowed"),
        new("On fire. Referenced by Fire spells — Ignite deals heavy damage to a Smoldering target and ends the condition. (See core rules for ongoing damage.)",
            new Guid("f0000000-0000-0000-0000-00000000000e"), "Smoldering"),
        new("Compelled to focus on the taunter. (Referenced by the Cheat's Scoundrel; see core rules.)",
            new Guid("f0000000-0000-0000-0000-00000000000f"), "Taunted"),
    ];

    /// <summary>Class features for the four quickstart classes (levels 1-4, with subclass features).</summary>
    internal static IReadOnlyList<Feature> Features { get; } =
    [
        // ---- Oathsworn (d…0001 and d…0002 are reused by the Caldra fixture; do not change) ----
        new(HeroClass.Oathsworn,
            "Whenever an enemy attacks you, if you have no Judgment Dice, roll your Judgment Dice (2d6). On your next melee attack this encounter, if you hit, deal that much additional radiant damage. The dice are expended whether you hit or miss.",
            null, new Guid("d0000000-0000-0000-0000-000000000001"), 1, "Radiant Judgment", null, null),
        new(HeroClass.Oathsworn,
            "Gain a magical pool of healing power equal to 5 × LVL, recharging on a Safe Rest. Action: touch a target and spend any amount of remaining power to restore that many HP.",
            null, new Guid("d0000000-0000-0000-0000-000000000002"), 1, "Lay on Hands", null, null),
        new(HeroClass.Oathsworn,
            "You know Radiant cantrips and tier 1 Radiant spells and gain a mana pool equal to WIL + LVL, recharging on a Safe Rest.",
            null, new Guid("d0000000-0000-0000-0000-000000000003"), 2, "Mana and Radiant Spellcasting", null, null),
        new(HeroClass.Oathsworn,
            "When you attack with a melee weapon you may spend mana (up to your highest unlocked spell tier), choosing one per mana: Condemning Strike (+5 radiant damage) or Blessed Aim (decrease the target's armor by 1 step for this attack).",
            null, new Guid("d0000000-0000-0000-0000-000000000004"), 2, "Zealot", null, null),
        new(HeroClass.Oathsworn,
            "Advantage on Influence checks when forthrightly telling the truth; disadvantage when misleading.",
            null, new Guid("d0000000-0000-0000-0000-000000000005"), 2, "Paragon of Virtue", null, null),
        new(HeroClass.Oathsworn, "Commit yourself to an Oath and gain its benefits.",
            null, new Guid("d0000000-0000-0000-0000-000000000006"), 3, "Subclass",
            ["Oath of Vengeance", "Oath of Refuge"], null),
        new(HeroClass.Oathsworn, "Your Judgment Dice become d8s.",
            null, new Guid("d0000000-0000-0000-0000-000000000007"), 3, "Radiant Judgment (2)", null, null),
        new(HeroClass.Oathsworn, "Learn one Sacred Decree.",
            null, new Guid("d0000000-0000-0000-0000-000000000008"), 3, "Sacred Decree",
            ["Courage!", "Explosive Judgment", "Reliable Justice", "Shining Mandate", "Stand Fast, Friends!"], null),
        new(HeroClass.Oathsworn, "You can Interpose for free.",
            null, new Guid("d0000000-0000-0000-0000-000000000009"), 4, "My Life, for My Friends", null, null),
        new(HeroClass.Oathsworn, "You may now cast tier 2 spells and upcast spells at tier 2.",
            null, new Guid("d0000000-0000-0000-0000-00000000000a"), 4, "Tier 2 Spells", null, null),
        new(HeroClass.Oathsworn, "+1 STR or WIL.",
            null, new Guid("d0000000-0000-0000-0000-00000000000b"), 4, "Key Stat Increase", null, null),
        new(HeroClass.Oathsworn,
            "Whenever you roll Judgment Dice, roll 1 more. Gain an aura with a Reach of 4; your Radiant Judgment also triggers when an ally within your aura is attacked while you have no Judgment Dice.",
            null, new Guid("d0000000-0000-0000-0000-00000000000c"), 3, "Aura of Zeal", null, "Oath of Vengeance"),
        new(HeroClass.Oathsworn,
            "Your shields gain +WIL armor and count as your spellcasting focus. Gain an aura with a Reach of 4; you can Interpose for an ally anywhere within your aura.",
            null, new Guid("d0000000-0000-0000-0000-00000000000d"), 3, "Aura of Refuge", null, "Oath of Refuge"),

        // ---- Cheat ----
        new(HeroClass.Cheat, "When you crit, deal +1d6 damage.",
            "Once per turn", new Guid("d0000000-0000-0000-0000-00000000000e"), 1, "Sneak Attack", null, null),
        new(HeroClass.Cheat,
            "When you hit a Distracted target with a melee attack, you may change the Primary Die roll to whatever you like (changing it to the max value counts as a crit).",
            "Once per turn", new Guid("d0000000-0000-0000-0000-00000000000f"), 1, "Vicious Opportunist", null, null),
        new(HeroClass.Cheat,
            "You're a well-rounded cheater: (1/round) Move or Hide for free; (1/day) change any skill check to 10 + INT; if you roll under 10 on Initiative you may change it to 10; advantage on skill checks while playing games, competitions, or placing wagers.",
            null, new Guid("d0000000-0000-0000-0000-000000000010"), 2, "Cheat", null, null),
        new(HeroClass.Cheat, "Choose a Cheat subclass.",
            null, new Guid("d0000000-0000-0000-0000-000000000011"), 3, "Subclass",
            ["Silent Blade", "Scoundrel"], null),
        new(HeroClass.Cheat, "Your Sneak Attack becomes 1d8.",
            null, new Guid("d0000000-0000-0000-0000-000000000012"), 3, "Sneak Attack (2)", null, null),
        new(HeroClass.Cheat, "You learn the secret language of rogues and scoundrels.",
            null, new Guid("d0000000-0000-0000-0000-000000000013"), 3, "Thieves' Cant", null, null),
        new(HeroClass.Cheat, "+1 DEX or INT.",
            null, new Guid("d0000000-0000-0000-0000-000000000014"), 4, "Key Stat Increase", null, null),
        new(HeroClass.Cheat, "Choose an Underhanded Ability.",
            null, new Guid("d0000000-0000-0000-0000-000000000015"), 4, "Underhanded Ability",
            ["Feinting Attack", "I'm Outta Here!", "Misdirection", "Trickshot"], null),
        new(HeroClass.Cheat,
            "When you spend a night talking shop with other roguish types during a Safe Rest, you may choose different Cheat options available to you.",
            null, new Guid("d0000000-0000-0000-0000-000000000016"), 4, "Trade Secrets", null, null),
        new(HeroClass.Cheat,
            "If a creature dies while you Sneak Attack it, you may turn Invisible until you attack again or until the beginning of your next turn.",
            null, new Guid("d0000000-0000-0000-0000-000000000017"), 3, "Amidst All This Commotion…", null, "Silent Blade"),
        new(HeroClass.Cheat, "Advantage on Stealth checks when you are at full health.",
            null, new Guid("d0000000-0000-0000-0000-000000000018"), 3, "Leave No Trace", null, "Silent Blade"),
        new(HeroClass.Cheat,
            "When you Sneak Attack you may spend 2 additional actions to Incapacitate the target on a failed STR save (DC 10 + INT). Save or fail, they are Taunted by you until you drop to 0 HP.",
            null, new Guid("d0000000-0000-0000-0000-000000000019"), 3, "Low Blow", null, "Scoundrel"),
        new(HeroClass.Cheat,
            "Advantage on Influence checks with NPCs you've just met, until you fail an Influence check with them or meet a second time (disadvantage thereafter until you regain favor).",
            null, new Guid("d0000000-0000-0000-0000-00000000001a"), 3, "Sweet Talk", null, "Scoundrel"),

        // ---- Hunter ----
        new(HeroClass.Hunter,
            "Action: a creature you can see is marked as your quarry for 1 day (or until you mark another). It can't be hidden from you, and your attacks against it gain your choice of advantage OR +LVL damage (choose before each attack).",
            null, new Guid("d0000000-0000-0000-0000-00000000001b"), 1, "Hunter's Mark", null, null),
        new(HeroClass.Hunter, "Advantage on skill checks to find food and water in the wild.",
            null, new Guid("d0000000-0000-0000-0000-00000000001c"), 1, "Forager", null, null),
        new(HeroClass.Hunter,
            "Choose 2 Thrill of the Hunt abilities. Gain a charge during an encounter when your quarry dies, or when you hit your quarry in melee or crit it at range.",
            null, new Guid("d0000000-0000-0000-0000-00000000001d"), 2, "Thrill of the Hunt",
            ["Addling Arrow", "Decoy", "Grease Trap", "Hail of Arrows", "Sharpshooter"], null),
        new(HeroClass.Hunter,
            "Action: if you have no Thrill of the Hunt charges, move up to your speed toward your quarry; if you end adjacent, make a melee attack against it for free.",
            null, new Guid("d0000000-0000-0000-0000-00000000001e"), 2, "Roll & Strike", null, null),
        new(HeroClass.Hunter,
            "When you spend a day in the wilderness during a Safe Rest, you may choose different Hunter options available to you.",
            null, new Guid("d0000000-0000-0000-0000-00000000001f"), 2, "Remember the Wild", null, null),
        new(HeroClass.Hunter, "Choose a Hunter subclass.",
            null, new Guid("d0000000-0000-0000-0000-000000000020"), 3, "Subclass",
            ["Shadowpath", "Wild Heart"], null),
        new(HeroClass.Hunter,
            "Study tracks and clues to discern a past encounter: the kind and number of creatures, their direction, key actions, and the passage of time.",
            null, new Guid("d0000000-0000-0000-0000-000000000021"), 3, "Tracker's Intuition", null, null),
        new(HeroClass.Hunter, "Choose a 3rd Thrill of the Hunt ability.",
            null, new Guid("d0000000-0000-0000-0000-000000000022"), 4, "Thrill of the Hunt (2)",
            ["Addling Arrow", "Decoy", "Grease Trap", "Hail of Arrows", "Sharpshooter"], null),
        new(HeroClass.Hunter, "+1 DEX or WIL.",
            null, new Guid("d0000000-0000-0000-0000-000000000023"), 4, "Key Stat Increase", null, null),
        new(HeroClass.Hunter, "+2 speed; gain a climbing speed.",
            null, new Guid("d0000000-0000-0000-0000-000000000024"), 4, "Explorer of the Wilds", null, null),
        new(HeroClass.Hunter,
            "When you roll Initiative you may use Hunter's Mark for free; gain advantage on the first attack you make each encounter.",
            null, new Guid("d0000000-0000-0000-0000-000000000025"), 3, "Ambusher", null, "Shadowpath"),
        new(HeroClass.Hunter, "You have advantage on skill checks to track creatures.",
            null, new Guid("d0000000-0000-0000-0000-000000000026"), 3, "Skilled Tracker", null, "Shadowpath"),
        new(HeroClass.Hunter, "You cannot become lost by nonmagical means.",
            null, new Guid("d0000000-0000-0000-0000-000000000027"), 3, "Skilled Navigator", null, "Shadowpath"),
        new(HeroClass.Hunter, "+5 max HP. Upgrade your Hit Dice to d10s.",
            null, new Guid("d0000000-0000-0000-0000-000000000028"), 3, "Impressive Form", null, "Wild Heart"),
        new(HeroClass.Hunter,
            "When you roll Initiative or gain one or more Thrill of the Hunt charges, move up to half your speed for free, ignoring difficult terrain.",
            null, new Guid("d0000000-0000-0000-0000-000000000029"), 3, "I Have the High Ground", null, "Wild Heart"),

        // ---- Mage ----
        new(HeroClass.Mage, "You know Fire, Ice, and Lightning cantrips.",
            null, new Guid("d0000000-0000-0000-0000-00000000002a"), 1, "Elemental Spellcasting", null, null),
        new(HeroClass.Mage,
            "You unlock tier 1 Fire, Ice, and Lightning spells and gain a mana pool whose maximum is always (INT × 3) + LVL, recharging on a Safe Rest.",
            null, new Guid("d0000000-0000-0000-0000-00000000002b"), 2, "Mana and Unlock Tier 1 Spells", null, null),
        new(HeroClass.Mage,
            "Advantage on Arcana or Lore checks when you have access to a large amount of books and time to study them.",
            null, new Guid("d0000000-0000-0000-0000-00000000002c"), 2, "Talented Researcher", null, null),
        new(HeroClass.Mage, "Choose a Mage subclass.",
            null, new Guid("d0000000-0000-0000-0000-00000000002d"), 3, "Subclass",
            ["Invoker of Chaos", "Invoker of Control"], null),
        new(HeroClass.Mage, "Learn the Utility Spells from one spell school you know (available in the full rules).",
            null, new Guid("d0000000-0000-0000-0000-00000000002e"), 3, "Elemental Mastery", null, null),
        new(HeroClass.Mage,
            "When you study arcane books or are tutored by a higher-level Mage during a Safe Rest, you may choose different Mage options available to you.",
            null, new Guid("d0000000-0000-0000-0000-00000000002f"), 3, "Study!", null, null),
        new(HeroClass.Mage, "Choose 2 Spellshaper abilities. Enhance your spells by spending additional mana.",
            null, new Guid("d0000000-0000-0000-0000-000000000030"), 4, "Spellshaper",
            ["Echo Casting", "Precise Casting", "Extra-Dimensional Vision", "Stretch Time"], null),
        new(HeroClass.Mage, "You may now cast tier 2 spells and upcast spells at tier 2.",
            null, new Guid("d0000000-0000-0000-0000-000000000031"), 4, "Tier 2 Spells", null, null),
        new(HeroClass.Mage, "+1 INT or WIL.",
            null, new Guid("d0000000-0000-0000-0000-000000000032"), 4, "Key Stat Increase", null, null),
        new(HeroClass.Mage,
            "Whenever you cast a spell you may spend 1 less mana; when you do, and whenever you crit, Invoke Chaos (roll on the Chaos Table).",
            null, new Guid("d0000000-0000-0000-0000-000000000033"), 3, "Force of Chaos", null, "Invoker of Chaos"),
        new(HeroClass.Mage,
            "(1/round) On your turn, Demand Control: choose one not-yet-chosen option from the Control Table; resets when you roll Initiative or once all options are used.",
            "Once per round", new Guid("d0000000-0000-0000-0000-000000000034"), 3, "Force of Will", null, "Invoker of Control"),
        new(HeroClass.Mage,
            "Whenever you miss with a spell, or an effect you cause is saved against, you MUST Demand Control.",
            null, new Guid("d0000000-0000-0000-0000-000000000035"), 3, "Deny Fate", null, "Invoker of Control"),
    ];

    /// <summary>Magic items. Adventure items from the quickstart, plus one placeholder.</summary>
    internal static IReadOnlyList<MagicItem> MagicItems { get; } =
    [
        new(null, "A warhammer with a bear trap on the end, wielded by Krogg the Goblin King.",
            "Rare 2-handed Maul: 1d6 + STR bludgeoning. On hit you may Grapple a creature smaller than you (escape DC 10); action: swing a creature Grappled this way at another within Reach, damaging both and ending the Grapple.",
            new Guid("da000000-0000-0000-0000-000000000001"), null, "Manglemaul", "Rare"),
        new(null, "A traveler's cloak humming with wind magic, used by the goblin Pinky.",
            "Lets the wearer come and go invisibly.",
            new Guid("da000000-0000-0000-0000-000000000002"), null, "Cloak of Lesser Windform", "Uncommon"),
        new(null, "An acorn from the Fairy Tree, gifted by Moonblossom.",
            "One-time use: reroll any one die.",
            new Guid("da000000-0000-0000-0000-000000000003"), 1, "Golden Acorn", "Uncommon"),
        new(null, "Opens to reveal small paintings of Marla Homebrew's children.",
            "A quest keepsake with no mechanical effect.",
            new Guid("da000000-0000-0000-0000-000000000004"), null, "Golden Heart Locket", "Common"),
        new(new Guid("e0000000-0000-0000-0000-000000000001"),
            "A slender wand humming with heat. (Placeholder example item — not from the quickstart.)",
            "Cast Flame Dart without spending mana.",
            new Guid("da000000-0000-0000-0000-000000000005"), 3, "Wand of Flame Dart", "Uncommon"),
    ];

    /// <summary>Rules references across categories, drawn from the quickstart core rules.</summary>
    internal static IReadOnlyList<RuleReference> Rules { get; } =
    [
        new(RuleCategory.Combat, "At 0 HP you are Dying; you have 1 action and risk Wounds until you regain HP.",
            new Guid("ce000000-0000-0000-0000-000000000001"), "Dying"),
        new(RuleCategory.Resting, "Catch Breath (10 min) or Make Camp (8 hours): expend Hit Dice to regain HP, adding STR to each.",
            new Guid("ce000000-0000-0000-0000-000000000002"), "Field Rest"),
        new(RuleCategory.Conditions, "Each Wound is a lasting injury (usually healed 1 per Safe Rest); 6 Wounds means death.",
            new Guid("ce000000-0000-0000-0000-000000000003"), "Wounds"),
        new(RuleCategory.Combat, "Rolling the max on a Primary Die crits: roll it again and add to the total, repeating each time you roll the max. Crits ignore monster armor.",
            new Guid("ce000000-0000-0000-0000-000000000004"), "Exploding Critical Hits"),
        new(RuleCategory.Resting, "In a safe location, recover all HP, Hit Dice, mana and class resources, and heal 1 Wound.",
            new Guid("ce000000-0000-0000-0000-000000000005"), "Safe Rest"),
        new(RuleCategory.Conditions, "You die at 6 Wounds. Revival is rare and costly.",
            new Guid("ce000000-0000-0000-0000-000000000006"), "Death"),
        new(RuleCategory.Combat, "Roll 1d20 + Initiative. A 1-digit result starts your first turn with 1 action, 2 digits with 2, and 20+ with all 3; you regain all 3 at the end of your first turn.",
            new Guid("ce000000-0000-0000-0000-000000000007"), "Initiative"),
        new(RuleCategory.Combat, "You may attack more than once per turn, but each attack after the first is rushed, imposing cumulative disadvantage.",
            new Guid("ce000000-0000-0000-0000-000000000008"), "Rushed Attacks"),
        new(RuleCategory.Combat, "Medium Armor (M) takes damage from the sum of the dice only; Heavy Armor (H) takes half the sum (rounding up). Crits ignore both.",
            new Guid("ce000000-0000-0000-0000-000000000009"), "Monster Armor"),
        new(RuleCategory.Combat, "Advantage: roll one extra die of the same type and drop the lowest. Disadvantage: drop the highest instead.",
            new Guid("ce000000-0000-0000-0000-00000000000a"), "Advantage & Disadvantage"),
        new(RuleCategory.Movement, "Speed defaults to 6 spaces. If no Range or Reach is specified, default to Reach 1. Ranged attacks while an enemy is adjacent are made with disadvantage.",
            new Guid("ce000000-0000-0000-0000-00000000000b"), "Speed, Range & Reach"),
        new(RuleCategory.Movement, "Forced movement stopped by an obstacle deals 1d6 per shortened space. Falling deals 1d6 bludgeoning per 10 ft (2 spaces).",
            new Guid("ce000000-0000-0000-0000-00000000000c"), "Falling & Forced Movement"),
        new(RuleCategory.LevelUp, "On a level: roll your Hit Die with advantage to raise max HP, +1 Hit Die max, +1 skill point (cap +12 per skill), new class features.",
            new Guid("ce000000-0000-0000-0000-00000000000d"), "Leveling Up"),
        new(RuleCategory.Spellcasting, "A spell's mana cost equals its tier; cantrips cost no mana. You can cast spells from schools you know within tiers you have unlocked.",
            new Guid("ce000000-0000-0000-0000-00000000000e"), "Mana & Spellcasting"),
        new(RuleCategory.Spellcasting, "Spend additional mana on a tiered spell (up to your unlocked tier) to strengthen it per extra mana.",
            new Guid("ce000000-0000-0000-0000-00000000000f"), "Upcasting"),
    ];

    /// <summary>The 16 quickstart spells: Fire, Ice, Lightning (Mage), and Radiant (Oathsworn).</summary>
    /// <remarks>Cantrips are tier 0 with mana cost 0; tier 1/2 cost mana equal to their tier. The
    /// <c>Range</c> value carries the card's Range or Reach (clarified in the description).</remarks>
    internal static IReadOnlyList<Spell> Spells { get; } =
    [
        // Fire
        new(1, null, "1d10", DamageType.Fire, "Hurl a dart of fire at one target. On Crit: Smoldering.", null,
            new Guid("e0000000-0000-0000-0000-000000000001"), false, false, 0, "Flame Dart", 8, null, SpellSchool.Fire, 0, null),
        new(1, null, null, null, "Give an ally within Range an extra action. Spend 1 mana to cast this as a reaction.", null,
            new Guid("e0000000-0000-0000-0000-000000000002"), false, false, 0, "Heart's Fire", 4, null, SpellSchool.Fire, 0, null),
        new(2, null, "4d10", DamageType.Fire, "Deal 4d10 to a Smoldering target, ending the condition on hit.", null,
            new Guid("e0000000-0000-0000-0000-000000000003"), false, false, 1, "Ignite", 8, null, SpellSchool.Fire, 1, "+10 damage."),
        new(1, null, null, DamageType.Fire, "A weapon you touch is enchanted with magical flame: it deals +KEY damage and inflicts Smoldering on crit.", null,
            new Guid("e0000000-0000-0000-0000-000000000004"), false, false, 2, "Enchant Weapon", null, null, SpellSchool.Fire, 2, null),

        // Ice
        new(1, null, "1d6", DamageType.Cold, "A lance of ice (cold/piercing). On Hit: Slowed.", null,
            new Guid("e0000000-0000-0000-0000-000000000005"), false, false, 0, "Ice Lance", 12, null, SpellSchool.Ice, 0, null),
        new(1, null, "1d6", DamageType.Cold, "Reach 1. On Hit: Blinded until the end of their next turn.", null,
            new Guid("e0000000-0000-0000-0000-000000000006"), false, false, 0, "Snowblind", 1, null, SpellSchool.Ice, 0, null),
        new(2, null, null, null, "Reaction (when attacked): gain 2 × KEY temp HP and Defend for free. The temp HP is lost at the start of your next turn.", null,
            new Guid("e0000000-0000-0000-0000-000000000007"), false, false, 1, "Frost Shield", null, null, SpellSchool.Ice, 1, "+2 × KEY temp HP."),
        new(1, null, "3d6", DamageType.Cold, "If any die rolls the max against a Hampered target, this counts as a crit. On Crit: +20 damage.", null,
            new Guid("e0000000-0000-0000-0000-000000000008"), false, false, 2, "Shatter", 12, null, SpellSchool.Ice, 2, null),

        // Lightning
        new(1, null, "2d8", DamageType.Lightning, "On a Miss: the lightning fails to find ground and strikes you instead.", null,
            new Guid("e0000000-0000-0000-0000-000000000009"), false, false, 0, "Zap", 12, null, SpellSchool.Lightning, 0, null),
        new(1, "Others within Reach", "2d8", DamageType.Lightning, "Requires the Charged condition (casting ends it). Reach 2. Deal 2d8 to others within Reach.", null,
            new Guid("e0000000-0000-0000-0000-00000000000a"), false, false, 0, "Overload", 2, null, SpellSchool.Lightning, 0, null),
        new(2, null, "3d8", DamageType.Lightning, "Also strikes the next closest creature to your target. On a Miss: strikes you instead.", null,
            new Guid("e0000000-0000-0000-0000-00000000000b"), false, false, 1, "Arc Lightning", 12, null, SpellSchool.Lightning, 1, "+4 damage."),
        new(1, null, null, null, "Reaction (when attacked): Defend for free. After damage is dealt, gain the Charged condition, then teleport anywhere within Range 4.", null,
            new Guid("e0000000-0000-0000-0000-00000000000c"), false, false, 2, "Alacrity", 4, null, SpellSchool.Lightning, 2, null),

        // Radiant
        new(1, null, "1d6", DamageType.Radiant, "Reach 4. Ignores armor and does not miss. Double damage against undead or cowardly targets (Frightened or behind cover).", null,
            new Guid("e0000000-0000-0000-0000-00000000000d"), false, false, 0, "Rebuke", 4, null, SpellSchool.Radiant, 0, null),
        new(1, null, null, null, "Reach 2. Give a creature advantage on the next attack they make (until the end of their next turn).", null,
            new Guid("e0000000-0000-0000-0000-00000000000e"), false, false, 0, "True Strike", 2, null, SpellSchool.Radiant, 0, null),
        new(1, null, null, null, "Reach 1. Heal a creature 1d6 + KEY HP.", null,
            new Guid("e0000000-0000-0000-0000-00000000000f"), false, false, 1, "Heal", 1, null, SpellSchool.Radiant, 1, "Choose one: +4 Reach, +1d6 healing, or +1 target."),
        new(1, null, null, null, "Designate a willing creature as your ward: they take half damage from all attacks; you are attacked for the other half.", "1 minute",
            new Guid("e0000000-0000-0000-0000-000000000010"), false, false, 2, "Warding Bond", null, null, SpellSchool.Radiant, 2, null),
    ];

    /// <summary>Weapons. Mostly placeholder/inferred — the quickstart has no weapon table.</summary>
    internal static IReadOnlyList<Weapon> Weapons { get; } =
    [
        new("1d6+2", DamageType.Bludgeoning, "A simple bludgeoning weapon. Oathsworn starting gear.",
            new Guid("b0000000-0000-0000-0000-000000000001"), false, false, "Mace", null, 1, null, StatType.Strength),
        new("1d4", DamageType.Piercing, "A light blade. Cheat starting gear. (Stats inferred — no quickstart weapon table.)",
            new Guid("b0000000-0000-0000-0000-000000000002"), false, false, "Dagger", null, 1, null, StatType.Dexterity),
        new("1d6", DamageType.Piercing, "A light ranged bow. Hunter starting gear. (Stats inferred.)",
            new Guid("b0000000-0000-0000-0000-000000000003"), false, true, "Shortbow", 12, 0, "Ranged.", StatType.Dexterity),
        new("1d6", DamageType.Bludgeoning, "A wooden staff. Mage starting gear. (Stats inferred.)",
            new Guid("b0000000-0000-0000-0000-000000000004"), false, true, "Staff", null, 1, null, StatType.Strength),
        new("1d4", DamageType.Bludgeoning, "A simple ranged sling. Cheat starting gear. (Stats inferred.)",
            new Guid("b0000000-0000-0000-0000-000000000005"), false, false, "Sling", 8, 0, "Ranged.", StatType.Dexterity),
    ];
}
