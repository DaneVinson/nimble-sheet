# Nimble — Basic / Quickstart Rules (reference)

> **What this is.** A Markdown transcription of the *Nimble Quickstart Rules* PDF, converted for
> easy re-reading by Claude and as the authoritative source for NimbleSheets' domain model and
> reference-data seeding. **Source:** `NimbleBasicRules.pdf` (15 pages, retrieved 2026-06-18 from
> `C:\temp\NimbleBasicRules.pdf`). Full game at **NimbleRPG.com**.
>
> **Fidelity notes — read these before trusting this file:**
> - **Rules prose, class stat blocks, class features, subclasses, monster stat blocks, magic items,
>   and the Control/Chaos tables are transcribed faithfully** (wording preserved; only OCR/encoding
>   artifacts cleaned: `•` bullets, `–` en-dashes, `×` for multiplication, `—` em-dashes).
> - **⚠️ The spell lists are NOT in this file.** In the PDF the Fire / Ice / Lightning / Radiant
>   spell pages are **graphical spell cards** — text extraction recovered only the section headers,
>   not a single spell. The cantrips and tier-1 spells for each school must be transcribed by hand
>   from the PDF images or pulled from the full rules. This is the single biggest gap and the main
>   blocker for authentic spell seeding. See [Spell Lists](#spell-lists-not-extractable--gap).
> - **Save advantage/disadvantage** is written explicitly. The PDF marks these with ▲ (advantage) /
>   ▼ (disadvantage) symbols next to a stat; this maps to the app's `HeroSaves(AdvantageOn,
>   DisadvantageOn)` and the sheet's `SAVE▲ / SAVE▼` markers.
> - **The two starter adventures are summarized**, not transcribed verbatim (they are GM narrative,
>   not rules). Their mechanically-relevant content — monster stat blocks and magic items — **is**
>   preserved verbatim in [Bestiary](#bestiary-starter-adventures) and [Adventure Items](#adventure-items).

---

## Table of contents

1. [Core Rules](#core-rules) — stats, skills, checks & saves, advantage, HP/wounds/death, hit dice, movement, resting, leveling
2. [Combat](#combat) — initiative, reactions, actions, spells, assess
3. [Running Monsters (GM)](#running-monsters-gm)
4. [Classes](#classes) — The Cheat, Hunter, Mage, Oathsworn
5. [Reference Tables](#reference-tables) — Control Table, Chaos Table
6. [Spell Lists](#spell-lists-not-extractable--gap) — ⚠️ gap
7. [Bestiary](#bestiary-starter-adventures)
8. [Adventure Items](#adventure-items)
9. [Starter Adventures (summary)](#starter-adventures-summary)
10. [What the Full Game Adds](#what-the-full-game-adds)

---

## Core Rules

Nimble is a fast-paced heroic fantasy TTRPG built to maximize cool, memorable moments. It is easy to
teach and prep for the GM, and quick for players to learn; it rewards creativity, clever tactics, and
teamwork while cutting downtime between turns.

### Stats

Heroes have **4 stats**: **2 Key Stats** (crucial to their class) and **2 Secondary Stats**. When an
ability or spell references **"KEY,"** use one of your Key Stats.

- **Strength (STR).** Raw physical power, resilience, endurance, resistance to harm. Affects STR
  weapon damage, resistance to Wounds, HP recovery, Concentration, STR saves, carrying capacity,
  Grappling, and the **Might** skill.
- **Dexterity (DEX).** Agility, reflexes, precision with blades or bows. Affects DEX weapon damage,
  Initiative, DEX saves, Grappling, can contribute to Armor, and the **Stealth** and **Finesse** skills.
- **Intelligence (INT).** Knowledge and reasoning (arcane, tactics, street smarts). Affects languages,
  spellcasting, use of wands and spell scrolls, INT saves, and the **Arcana**, **Examination**, and
  **Lore** skills.
- **Will (WIL).** Force of personality, courage, and wisdom; shapes interactions with nature and
  society. Affects spellcasting, WIL saves, and the **Insight**, **Influence**, **Naturecraft**, and
  **Perception** skills.

### Skills

Skills gauge how well your hero interacts with the world. Each skill is keyed to a stat:

| Skill | Stat | Use |
|---|---|---|
| Arcana | INT | Understanding magical phenomena and enchantments. |
| Examination | INT | Uncovering clues, diagnosing injuries, unraveling traps or mechanical devices. |
| Finesse | DEX | Careful hand/foot work: picking locks, disarming traps, piloting vehicles, tinkering, card tricks. |
| Influence | WIL | Persuasiveness, charm, captivating performance. |
| Insight | WIL | Understanding people/situations: detect lies, make sense of clues, retroactively reason about a situation. |
| Might | STR | Lifting, breaking obstacles, climbing, swimming, jumping, feats of strength. |
| Lore | INT | History of civilizations, kingdoms, religions, cultural significance. |
| Naturecraft | WIL | Wilderness survival, navigation, tracking, handling animals, identifying flora. |
| Perception | WIL | Spotting hidden objects, secret passages, etc. |
| Stealth | DEX | Staying unseen and unheard. |

### Skill Checks & Saves

**Skill check:** roll **1d20 + skill**. Meet or exceed the **Difficulty Challenge (DC)** to succeed.
A natural **1 always fails**; a natural **20 always succeeds**, regardless of bonuses. DC examples:

- **Easy** — spotting a large Ogre crouched behind a small bush: DC 8 Perception.
- **Challenging** — calming an injured Owlbear stuck in a trap: DC 15 Naturecraft.
- **Extremely Difficult** — disarming an ancient legendary trap: DC 20+ Finesse.

**Saves:** when the world affects you, roll **1d20 + relevant stat**. Natural 1 always fails, 20
always saves. You may **choose to fail** any save instead of rolling.

- **STR Save** — resist forced movement, poison, extreme temperatures.
- **DEX Save** — dive for cover or stay on your feet.
- **INT Save** — see through tricks and illusions.
- **WIL Save** — resist charm or fear effects.

**Heroes and Saves:**
- Unless otherwise noted, the DC for effects a hero causes is **10 + KEY**.
- Each hero has **1 advantaged save (▲)**, **1 disadvantaged save (▼)**, and **2 neutral**.
- *Example:* a Berserker (STR ▲, INT ▼) rolls all STR saves with advantage and all INT saves with
  disadvantage.

### Advantage / Disadvantage

In a favorable situation the GM may grant **advantage**: roll **1 additional die of the same type and
remove the lowest**. In a grim situation or for a long-shot idea, roll with **disadvantage** (remove
the **highest** die instead).

- *Example 1.* Greataxe (2d6) with advantage → roll 1 extra die, remove the lowest.
- *Example 2.* Greataxe with **disadvantage 2** → roll 2 extra dice, remove the 2 highest. If tied,
  remove dice from left to right.

### Hit Points & Dying

**Hit Points (HP)** represent your ability to endure damage. Damage reduces HP (cannot go below 0).
At **0 HP** you gain **1 Wound** and are **Dying** until you regain HP. While Dying, you are limited
to **1 action**, and:

- Attacking / casting spells causes **1 Wound** unless you make a **DC 10 STR save**.
- Taking damage while Dying causes **2 Wounds**; a crit causes **3** instead.

### Wounds

Wounds are serious injuries — a long-term gauge of how close you are to death. HP is recovered
quickly, but Wounds may take many days of resting to recover (usually **1 per Safe Rest**).

### Death

You die at **6 Wounds**. Revival exists but is rare and often comes at a very steep cost.

### Hit Dice

**Hit Dice (HD)** represent quick recuperation and are spent to regain HP. Heroes start with a max of
**1 Hit Die at level 1**; the max increases by **1 per level**. Hit Dice are recovered on a **Safe
Rest**.

### Speed, Range, & Reach

- **Speed** — how far you move; default **6 spaces** unless noted. A grid square/hex ≈ 5 ft / 1 meter.
- **Range & Reach** — some abilities/weapons/spells specify a Range or Reach. If none is specified,
  default to **Reach 1**.
- **In Melee** — if any enemy is adjacent to you, your **ranged attacks** are made with disadvantage
  (Reach attacks are unaffected).
- **Falling & Forced Movement** — when forcibly moved but stopped by an obstacle, take **1d6 damage
  per space the movement was shortened**. If you hit another creature, both split the damage. Falling
  inflicts **1d6 bludgeoning per 10 ft (2 spaces)** fallen.

### Resting

**Field Rests** (while adventuring, to regain HP):
- **Catch Breath** — at least 10 minutes. Expend any number of Hit Dice one at a time (roll each, add
  STR), regaining that many HP.
- **Make Camp** — rest at least 8 hours with food and sleep: take the **max** value for each expended
  Hit Die instead of rolling (add STR as usual).

**Safe Rests** take place in a safe location (an inn, a secret oasis, a stocked cabin, near a sacred
shrine, etc.). Camping in open wilderness or a dungeon does **not** qualify. After a Safe Rest, heroes
recover **all HP, all Hit Dice, all mana (and other class resources), and heal 1 Wound**.

### Leveling Up

A GM may allow a level (LVL) after an appropriately challenging quest. On gaining a level:

- **HP Increase.** Roll your Hit Die **with advantage** and increase max HP by that much.
- **More Endurance.** Hit Die max increases by 1 (typically equal to your level).
- **More Skilled.** Gain **1 skill point**. You may also move 1 point from one skill to another (as
  long as it doesn't go negative). **Max bonus a skill can have is +12.**
- **Class Features.** Gain new class features for your level — may increase your mana pool, grant new
  spells, or let you select a subclass.
- **Other Adjustments.** If any base stats increase, adjust dependent sheet elements (skills, damage,
  Initiative, armor, mana, languages, etc.).

---

## Combat

### Starting Combat (Initiative)

Combat begins when the GM calls **"Roll Initiative!"** Each hero rolls **1d20 + Initiative bonus**.
The result sets how many actions you have on your **first turn**:

- **1-digit result** → 1 action.
- **2-digit result** → 2 actions.
- **20+ (or a natural 20)** → all 3 actions!

Regardless of the roll, at the **end of their first turn** every hero gains all **3 actions** back.
Heroes go first (whoever is ready first, or whoever fits the story); play proceeds clockwise.
Monsters typically act last, though some are fast enough to act sooner. The GM may let players
**switch turn order** within a round to enable teamwork.

### Heroic Reactions

Reactions cost **1 action**, are performed when it is **not your turn**, and each may be used **no more
than once per round** (you then start your turn with fewer actions).

- **Defend.** Reduce damage from a single attack by your **Armor**. At GM discretion some damage isn't
  avoidable (e.g., psychic damage, some areas of effect).
- **Interpose.** If a creature within 2 spaces would be struck, push them out of the way and become
  the new target: you enter their space and move them to an adjacent space of your choice.
- **Interpose AND Defend?** Yes — if you have the actions, you may do both at once. But neither can be
  used again until after your next turn (each is 1/round).
- **Opportunity Attack.** A melee attack with **disadvantage** against an adjacent enemy that
  willingly moves away. **Only heroes** make opportunity attacks; monsters do not.
- **Help.** Grant an ally advantage on a roll if you can reasonably explain how (limit one Help per
  roll). The GM may require a skill check or grant it automatically based on the idea.

### Heroic Actions

Heroes get **3 actions** per turn to attack, move, cast spells, etc. Generally any single thing costs
1 action; strong spells/abilities may cost more. **All 3 actions recharge at the end of your turn** —
spend freely.

- **Attack.** Roll the die listed on the spell/weapon/ability and deal that much damage. **A roll of 1
  misses** (no effect). For multi-die attacks, the **leftmost die is the Primary Die** — it determines
  hit/miss.
  - **Exploding Critical Hits.** Rolling the **max** on a Primary Die is a **crit**: roll the Primary
    Die again and add it; repeat each time you roll the max (no limit). **Crits ignore monster armor.**
  - **Rushed Attacks.** You may attack more than once per turn, but each additional attack is rushed,
    imposing **cumulative disadvantage** for each attack after the first.
  - **Saves.** For attacks that trigger a save, monsters roll with **increasing advantage** instead.
- **Move.** Move up to your speed (typically 6 spaces); may be broken up with other actions, and you
  may use multiple actions to Move multiple times.
- **Free Actions.** Cost no action or resource unless specified. Simple tasks (open an unlocked door,
  shout a short phrase, drop an item, end concentration, etc.) are free **1/turn**.
- **Spells.** Casting requires **1 free hand** (or a held spellcasting focus), the ability to speak,
  and possibly mana. A spell's **mana cost equals its tier**; **cantrips cost no mana**.
  - **Upcasting.** Spend additional mana on tiered spells (up to your unlocked tier) to strengthen
    them per extra mana. You may upcast only up to the tier you've unlocked.

### Assess

A way to include creativity/role-play in combat. Choose one and make a **DC 12 skill check** (the
skill must fit the circumstances):

- **Ask a Question** — about an enemy weakness/ability/plans, the environment, story, etc. The GM
  answers honestly.
- **Create an Opening** — increase the next Primary Die roll **against a target** by 1 this round.
- **Anticipate Danger** — reduce all Primary Dice rolled **against you** by 1 this round.

**Monsters Are Smart!** You cannot Assess using the **same skill** more than once in a single
encounter — foes adapt to your tactics.

### Magic (overview)

There are **6 main schools of magic**. Each school has **cantrips** and **9 tiers** of progressively
more powerful spells, unlocked as heroes level up. Heroes can cast any spell from schools they know
within tiers they've unlocked. **Secret Spells** are hidden/lost spells adventurers may stumble upon.

---

## Running Monsters (GM)

The GM controls monsters in combat. Monsters do **not** use Heroic Actions/Reactions; they move, use
the actions on their stat block, then end their turn. **Monsters die at 0 HP.**

### Default Monster Stats

Unless otherwise noted, monsters are **unarmored**, have **speed 6** (can replace an attack to move
again), attacks have **Reach 1**, and roll **1d20 for all saves** (some may have advantage/disadvantage
when appropriate).

A stat block reads: the creature's **HP**, its **Armor (M or H)**, and its **Speed** (fly, burrow,
etc.). A LVL of e.g. **1/3** means 3 such creatures are about as strong as a level-1 hero.

### Monster Armor

- **Medium Armor (M) — "Just the Dice."** Ignores all damage modifiers from stats/effects; takes
  damage from the **sum of the dice only**.
- **Heavy Armor (H) — "Half the Dice."** Ignores modifiers and takes **half the sum of the dice**
  (rounding up).
- **Tell your players** when a monster has armor — it shouldn't be a secret.

### Minions

Minions are low-threat monsters, easy to kill but dangerous in numbers:

- **No HP to track** — any damage kills a minion.
- **Easy Attacks** — each minion attacks with a single damage die, **cannot crit**, and misses on a 1.
- **Simplified Defense** — when multiple minions attack one target, their damage is **combined and
  counts as a single attack**, so a hero can Defend or Interpose against them all at once.
- **Act Together** — move and attack all minions targeting a hero at once (e.g., 5 minions → roll 5d6,
  ignoring 1s).

---

## Classes

> The four premade quickstart heroes map to these classes: **Kessa Quickstep** (Cheat), **Thorne
> Underbough** (Hunter), **Virel of the Ember Eye** (Mage), **Caldra Brightward** (Oathsworn). Stat
> blocks below are level-1 starting values; features are listed by level (1–4). Saves use ▲ advantage
> / ▼ disadvantage.

### The Cheat

A sneaky, backstabby scoundrel.

| | |
|---|---|
| **Key Stats** | DEX, INT |
| **Hit Die** | 1d6 |
| **Starting HP** | 10 |
| **Saves** | DEX ▲, WIL ▼ |
| **Armor** | Leather Armor |
| **Weapons** | DEX Weapons |
| **Starting Gear** | 2 Daggers, Sling, Cheap Hides, Chalk |

**Level 1**
- **Sneak Attack.** (1/turn) When you crit, deal **+1d6** damage.
- **Vicious Opportunist.** (1/turn) When you hit a **Distracted** target with a melee attack, you may
  change the Primary Die roll to whatever you like (changing it to the max value counts as a crit).
- *Distracted:* a target is Distracted if it is adjacent to or Taunted by an ally, or if it cannot see you.

**Level 2**
- **Cheat.** You're a well-rounded cheater. Gain:
  - (1/round) Move or Hide for free.
  - (1/day) Change any skill check to **10 + INT**.
  - If you roll less than 10 on Initiative, you may change it to **10**.
  - Advantage on skill checks while playing games, competitions, or placing wagers. (If you're caught…)

**Level 3**
- **Subclass.** Choose a Cheat subclass.
- **Sneak Attack (2).** Your Sneak Attack becomes **1d8**.
- **Thieves' Cant.** You learn the secret language of rogues and scoundrels.

**Level 4**
- **Key Stat Increase.** +1 DEX or INT.
- **Underhanded Ability.** Choose an Underhanded Ability.
- **Trade Secrets.** When you spend a night talking shop with other roguish types during a Safe Rest,
  you may choose different Cheat options available to you.

**Subclasses — Tools of the…**
- **Silent Blade.** *Amidst All This Commotion…* — if a creature dies while you Sneak Attack it, turn
  **Invisible** until you attack again or until the start of your next turn. *Leave No Trace* —
  advantage on Stealth checks while at full health.
- **Scoundrel.**
  - *Low Blow.* When you Sneak Attack, you may spend 2 additional actions to **Incapacitate** the
    target for their next turn on a failed STR save (DC 10 + INT). Save or fail, they are **Taunted**
    by you until you drop to 0 HP.
  - *Sweet Talk.* Advantage on all Influence checks with NPCs you've just met, until you fail an
    Influence check with them or meet a 2nd time (disadvantage afterward until you regain favor).

**Underhanded Abilities**
- **Feinting Attack.** If you miss for the 2nd time in a round, you may change the Primary Die roll to
  any result.
- **I'm Outta Here!** When an ally within 4 spaces is crit, turn invisible until the end of your next
  turn and move up to half your speed for free.
- **Misdirection.** Gain **INT armor**. Whenever you Defend, you may halve the damage instead.
- **Trickshot.** When you throw a dagger, it returns to your hand at end of turn. On a hit, it
  ricochets to another creature within 2 spaces, dealing half as much damage.

### Hunter

A determined tracker and survivalist.

| | |
|---|---|
| **Key Stats** | DEX, WIL |
| **Hit Die** | 1d8 |
| **Starting HP** | 13 |
| **Saves** | DEX ▲, INT ▼ |
| **Armor** | Leather Armor |
| **Weapons** | DEX Weapons |
| **Starting Gear** | Shortbow, Cheap Hides, Dagger, Hunting Trap |

**Level 1**
- **Hunter's Mark.** Action: a creature you can see is marked as your **quarry** for 1 day (or until
  you mark another). It can't be hidden from you, and your attacks against it gain your choice of
  **advantage OR +LVL damage** (choose before each attack).
- **Forager.** Advantage on skill checks to find food and water in the wild.

**Level 2**
- **Thrill of the Hunt (TotH).** Choose **2** TotH abilities. Gain a **charge** during an encounter
  whenever: your quarry dies; or you hit your quarry in melee or crit it at range.
- **Roll & Strike.** Action: if you have no TotH charges, move up to your speed toward your quarry; if
  you end adjacent, make a melee attack against it for free.
- **Remember the Wild.** When you spend a day in the wilderness during a Safe Rest, you may choose
  different Hunter options.

**Level 3**
- **Subclass.** Choose a Hunter subclass.
- **Tracker's Intuition.** Study tracks/clues to discern a past encounter: kind and number of
  creatures, direction, key actions, and passage of time.

**Level 4**
- **Thrill of the Hunt (2).** Choose a 3rd TotH ability.
- **Key Stat Increase.** +1 DEX or WIL.
- **Explorer of the Wilds.** +2 speed; gain a climbing speed.

**Subclasses — Keeper of the…**
- **Shadowpath.** *Ambusher* — when you roll Initiative you may use Hunter's Mark for free; advantage
  on the first attack each encounter. *Skilled Tracker* — advantage on checks to track creatures.
  *Skilled Navigator* — you cannot become lost by nonmagical means.
- **Wild Heart.** *Impressive Form* — +5 max HP; upgrade your Hit Dice to **d10s**. *I Have the High
  Ground* — when you roll Initiative or gain one or more TotH charges, move up to half your speed for
  free, ignoring difficult terrain.

**Thrill of the Hunt abilities** *(unless noted, each costs 1 charge and cannot miss; abilities that
spend charges can't generate new ones; unused charges are lost when combat ends):*
- **Addling Arrow.** Action: attack with a ranged weapon. The target's next attack must be against the
  closest other creature, chosen at random.
- **Decoy.** When you Defend: the attack misses instead, and you move up to half your speed away
  (where you really were all along!).
- **Grease Trap.** (1/encounter) Reaction (an enemy moves adjacent to you or an ally within 6 spaces):
  target falls **Prone**, is vulnerable to the next fire damage it takes, and is treated as
  **Smoldering**.
- **Hail of Arrows.** (Half range) 2 actions: shoot all creatures in a **3×3 area**; their speed is
  halved until the end of their next turn.
- **Sharpshooter.** Action: if you have not moved this turn and your quarry is 4+ spaces away, attack
  it for **double damage**.

### Mage

A brilliant spellcaster and scholar.

| | |
|---|---|
| **Key Stats** | INT, WIL |
| **Hit Die** | 1d6 |
| **Starting HP** | 10 |
| **Saves** | INT ▲, STR ▼ |
| **Armor** | Cloth |
| **Weapons** | Blades, Staves, Wands |
| **Starting Gear** | Adventurer's Garb, Staff, Soap |

**Level 1**
- **Elemental Spellcasting.** You know **Fire, Ice, and Lightning cantrips**.

**Level 2**
- **Mana and Unlock Tier 1 Spells.** Unlock **tier 1 Fire, Ice, and Lightning** spells and gain a mana
  pool. **Max mana = (INT × 3) + LVL**, recharging on a Safe Rest.
- **Talented Researcher.** Advantage on Arcana or Lore checks when you have access to many books and
  time to study.

**Level 3**
- **Subclass.** Choose a Mage subclass.
- **Elemental Mastery.** Learn the **Utility Spells** from 1 spell school you know (in the full rules).
- **Study!** When you study arcane books or are tutored by a higher-level Mage during a Safe Rest, you
  may choose different Mage options.

**Level 4**
- **Spellshaper.** Enhance spells with powerful effects by spending additional mana. Choose **2
  Spellshaper abilities**.
- **Tier 2 Spells.** You may now cast tier 2 spells and upcast at tier 2.
- **Key Stat Increase.** +1 INT or WIL.

**Subclasses — Invoker of…**
- **Chaos.** *Force of Chaos* — whenever you cast a spell you may spend **1 less mana**; when you do,
  and whenever you crit, **Invoke Chaos** (roll on the [Chaos Table](#chaos-table)).
- **Control.** *Force of Will* — (1/round) on your turn, **Demand Control**: choose 1 not-yet-chosen
  option from the [Control Table](#control-table); resets when you roll Initiative or once all options
  are used. *Deny Fate* — whenever you miss with a spell or an effect you cause is saved against, you
  **MUST** Demand Control.

**Spellshaper** *(Spellshaper subclass-line; use 1/turn):*
- **Echo Casting.** (2 × mana, min. 1 mana) When you cast a tiered single-target spell, cast a copy on
  a 2nd target for free.
- **Precise Casting.** (1+ mana) Choose 1 creature per mana spent to be unaffected by a spell you cast.
- **Extra-Dimensional Vision.** (2 mana) Ignore a spell's line-of-sight requirement; it phases through
  barriers to reach a target you know of within range.
- **Stretch Time.** (2 mana) Reduce a spell's action cost by 1 (min 1).

### Oathsworn

An honorable protector.

| | |
|---|---|
| **Key Stats** | STR, WIL |
| **Hit Die** | 1d10 |
| **Starting HP** | 17 |
| **Saves** | STR ▲, DEX ▼ |
| **Armor** | All Armor |
| **Weapons** | STR Weapons |
| **Starting Gear** | Mace, Rusty Mail, Wooden Buckler, Manacles |

**Level 1**
- **Radiant Judgment.** Whenever an enemy attacks you, if you have no **Judgment Dice**, roll your
  Judgment Dice (**2d6**). On your next melee attack this encounter, if you hit, deal that much
  additional **radiant** damage. The dice are expended whether you hit or miss.
- **Lay on Hands.** Gain a magical healing pool. **Max = 5 × LVL**, recharging on a Safe Rest. Action:
  touch a target and spend any amount of remaining power to restore that many HP.

**Level 2**
- **Mana and Radiant Spellcasting.** You know **Radiant cantrips and tier 1 Radiant spells** and gain
  a mana pool. **Max mana = WIL + LVL**, recharging on a Safe Rest.
- **Zealot.** When you attack with a melee weapon, you may spend mana (up to your highest unlocked
  spell tier), choosing one per mana:
  - **Condemning Strike.** Deal **+5 radiant** damage.
  - **Blessed Aim.** Decrease the target's armor by 1 step for this attack.
- **Paragon of Virtue.** Advantage on Influence checks when forthrightly telling the truth;
  disadvantage when misleading.

**Level 3**
- **Subclass.** Commit to an **Oath** and gain its benefits.
- **Radiant Judgment (2).** Your Judgment Dice become **d8s**.
- **Sacred Decree.** Learn 1 Sacred Decree.

**Level 4**
- **My Life, for My Friends.** You can **Interpose** for free.
- **Tier 2 Spells.** You may now cast tier 2 spells and upcast at tier 2.
- **Key Stat Increase.** +1 STR or WIL.

**Subclasses — Oath of…**
- **Vengeance.** *Aura of Zeal* — whenever you roll Judgment Dice, roll **1 more**. Gain an aura
  (Reach 4); your Radiant Judgment also triggers when an ally within your aura is attacked while you
  have no Judgment Dice.
- **Refuge.** *Aura of Refuge* — your shields gain **+WIL armor** and count as your spellcasting
  focus. Gain an aura (Reach 4); you can Interpose for an ally anywhere within your aura.

**Sacred Decrees**
- **Courage!** (1/encounter) When you or an ally in your aura would drop to 0 HP, set their HP to **1**
  instead.
- **Explosive Judgment.** (1/encounter) 2 actions: expend your Judgment Dice, deal that much radiant
  damage to all enemies in your aura.
- **Reliable Justice.** Whenever you roll Judgment Dice, roll with advantage (roll one extra, drop the
  lowest).
- **Shining Mandate.** The first time each round you're attacked while you already have Judgment Dice,
  select an ally in your aura to roll one and apply it to their next attack. Advantage on checks to
  see through illusions.
- **Stand Fast, Friends!** When you roll Initiative, grant allies temp HP equal to **STR + WIL**. You
  and allies in your aura have advantage against fear and effects that would move or knock you Prone.
- **Serve Selflessly.** When you perform a notable selfless act during a Safe Rest, you may choose
  different Oathsworn options.

---

## Reference Tables

### Control Table

*(Mage — Invoker of Control)*

- **I INSIST.** Cast a cantrip for free, ignoring all disadvantage; it cannot miss.
- **ELEMENTAL AFFLICTION.** A creature of your choice within 12 spaces gains **Charged**,
  **Smoldering**, or **Slowed** (half speed on next turn).
- **NO.** Choose a creature; it cannot harm a creature of your choice on its next turn.
- **LOSE CONTROL.** Do ALL of the above, but the GM chooses each time.

### Chaos Table

*(Mage — Invoker of Chaos. Unless noted, ongoing effects last up to 1 minute or until Chaos triggers
again. The PDF notes the "real" Chaos Table is a GM secret; this is the published version.)*

| d20 | Effect |
|---|---|
| 1 | **Elemental Eruption.** Creatures within 6 spaces make a DEX save or take **INT d10** fire damage (half on save). **You** fail the save. |
| 2 | **Backfire.** Suffer 1 Wound. The spell you just cast also targets you (or an enemy if it was beneficial). |
| 3 | **Aww, Nuts!** Polymorph into a cute squirrel until you take damage. Top priority: find acorns (squirrels can't cast spells). |
| 4 | **Summon Aetherlings.** At the end of each of your turns, summon **INT** hostile aetherling minions adjacent to you that act immediately after you (size: d6). |
| 5 | **Graviturgical Grace.** A random enemy is pulled adjacent to you at the end of each of your turns. |
| 6 | **Liquefy Legs.** You fall Prone, cannot stand, and your speed becomes 0 while out of water. |
| 7 | **Elemental Entanglement.** An enemy controls 1 Action for you at the start of each of your turns. |
| 8 | **Ethereal Cocoon.** Enveloped in a cocoon until end of your next turn: Prone, unable to move/speak, immune to damage; spend all Actions casting cantrips at the nearest creature. |
| 9 | **Manastorm.** The last spell you cast is cast again for free, against a random target. |
| 10 | **Reality Warp.** Everywhere within 6 spaces becomes difficult terrain. |
| 11 | **Displacement.** Teleport (1d4): 1 = the worst place (GM's choice); 2 = UP 6 spaces (3d6 falling damage); 3 = player's choice, 6 spaces; 4 = player's choice, 12 spaces. |
| 12 | **Chaos Step.** Swap places with any creature you can see. |
| 13 | **Mindfire.** The dumbest enemy within 16 spaces takes **INT d6** psychic damage (ignoring armor) and gains Smoldering. |
| 14 | **Emerge Beautiful.** Sprout butterfly wings; gain a flying speed. |
| 15 | **Unbiggen.** Your size is halved. Advantage on Stealth; attacks against you have disadvantage. |
| 16 | **Embiggen.** Your size is doubled. Gain **INT d10** Temp HP and advantage on STR saves (instead of disadvantage). |
| 17 | **Awakening.** A 3rd eye appears; advantage on the Assess action and all attacks. |
| 18 | **Diamond Skin.** Multiply your Armor by **INT**; you can Defend for free each round. |
| 19 | **Mighty Mana.** Your spells (including the triggering one) are cast as if you spent 2 additional mana (ignoring your natural max). |
| 20 | **Elemental Overload.** Enemies within 12 spaces take **INT d8** lightning damage. You regain **INT** mana at the end of your turns (expires at end of combat if unused). |

---

## Spell Lists (NOT extractable — gap)

⚠️ **The PDF's spell pages did not extract.** The quickstart includes spell pages headed
**FIRE SPELLS**, **ICE SPELLS**, **LIGHTNING SPELLS**, and **RADIANT SPELLS** (the Mage knows Fire/Ice/
Lightning; the Oathsworn knows Radiant). In the source these are laid out as **graphical spell cards**,
so text extraction recovered only the four headers — **no spell names, mana costs, damage, ranges, or
effects came through**.

**To fill this gap**, the cantrips and tier-1 spells for each school must be transcribed by hand from
the PDF page images (pages ~11 and ~13), or sourced from the full Nimble rules (NimbleRPG.com). Until
then, the app's seeded spells remain placeholder data rather than authentic Nimble spells.

What we *do* know from the surrounding rules:
- A spell's **mana cost = its tier**; **cantrips cost 0 mana**.
- The four schools relevant to the quickstart classes are **Fire, Ice, Lightning** (Mage) and
  **Radiant** (Oathsworn). The full game has **6 schools** total.
- Each school has cantrips plus **9 tiers**; the quickstart only unlocks **cantrips and tier 1** (and
  tier 2 at level 4).

---

## Bestiary (starter adventures)

Stat blocks transcribed verbatim. Format: **Name** — `LVL`, size, **HP**, armor (M/H), saves; then
abilities/attacks. Defaults (speed 6, Reach 1, 1d20 saves, unarmored) apply unless noted.

- **Goblin** — `LVL 1/3`, small, **HP 15**.
  - *Haha, Missed Me!* Whenever an attack misses you, deal **1 psychic** damage in return.
  - *Stab.* 1d6+2 (or *Shoot*, Range 8). Can't crit.
- **Goblin Minion** — follows minion rules (any damage kills it; can't crit; misses on 1).
  - *Stab.* 1d6.
- **Goblin Flunkie** — **HP 15**.
  - *Stab.* 1d6+2 (or *Shoot*, Range 8). Can't crit.
- **Goblin Ratrider** — **HP 30** (rider) / **10** (rat); *(Ratrider is at half HP with 3 or fewer
  heroes).*
  - *CHAAARGE!* If you move at least 4 spaces in a straight line, attack with advantage once.
  - *Bite & Stab (×2).* 1d6+2. On crit: **Prone**.
- **Giant Spider** — `LVL 2`, **Armor M**, **HP 27**.
  - *Web.* (Range 6) 1d8+2. On hit: **Restrained** (escape DC 12, or any slashing/fire damage). **OR:**
  - *Bite.* (Restrained target) 2d8+4, **Poisoned** (disadvantage on rolls; ends when healed).
- **Krogg, Goblin King** — `Level 2 Solo`, Angry Bugbear, **HP 75**, **Armor M**, **STR ▲, DEX ▲**.
  - *ACTIONS (after each hero's turn, choose one):*
    - **Manglemaul.** Move 6. 2d6+3 damage, **Grappled** (escape DC 10). **OR:**
    - **Crack Skulls.** Move 6. Swing a Grappled creature at another creature; both take 2d6+3 damage,
      ending the Grapple.
  - *BLOODIED:* at 35 HP, Krogg's damage increases to **2d8+3**.
  - *LAST STAND:* Krogg is dying! If he takes 20 more damage he dies; until then he has **Heavy armor**.

---

## Adventure Items

Magic items / notable loot from the starter adventures (verbatim):

- **Golden Acorn.** (1-time use) Reroll any 1 die. *(Gift from Moonblossom, the fairy.)*
- **Manglemaul.** (Rare, 2-handed Maul) 1d6+STR bludgeoning. On hit: you may **Grapple** a creature
  smaller than you (escape DC 10). Action: swing a creature Grappled this way at another within Reach,
  damaging both and ending the Grapple.
- **Cloak of Lesser Windform.** Lets the wearer come and go **invisibly**. *(Used by the goblin Pinky.)*
- **Golden Heart Locket.** Opens to small paintings of Marla Homebrew's children. *(Quest return item.)*
- **Sprig's Slop.** Eating one bowl heals **2 Hit Dice worth of HP**; eating more does not heal further
  and may cause vomiting.
- **Moonblossom's Healing Kiss.** Restores all HP and removes all Wounds for the most injured hero.

---

## Starter Adventures (summary)

*The PDF bundles a two-chapter intro adventure, "The Garden of Death." These are GM narrative;
summarized here. Their monster stat blocks and items are preserved above. See the source PDF for full
read-aloud text and room-by-room detail.*

### Chapter 1 — "A Tiny Rescue" (Level 1)

At the **Valley's Rest** inn in **Merivale**, goblins kidnap the town's beloved fairy **Moonblossom**.
The heroes give chase north into the **Elderwild** (an ancient magical forest), fight a pack of goblin
minions plus a **Goblin Ratrider**, and rescue Moonblossom. They're tasked to hunt the remaining
goblins (reward: 20 gp each) and find a curious map and a note implicating **"Krogg"** and
**"Greenthumb."** The party levels to **2** on resting back in town. *(Teaches: Initiative, basic
attacks/moves, Field Rests, Defend & Interpose, dropping to 0 HP.)*

### Chapter 2 — "Goblins of the Crystal Crag" (Level 2, for 2–8+ heroes)

The froglin wizard **Greenthumb** planted a sentient **Deathbriar** that animates the dead; his attempt
to control it failed, so he seeks to become a **Lich**. He hired goblins (led by chieftain **Krogg**, a
bugbear) to steal magical items for his ritual, but they're withholding the goods, so Greenthumb sent
**Rootbreakers** (animated plant creatures) to take them by force. The heroes explore an abandoned mine
(Rootbreaker Pit, Atrium, Spider Chamber, Drill Room, Goblin's Den, Slop Hall, Krogg's Quarters), can
ally with the trapped Rootbreakers, and confront **Krogg**. Treasure includes the **Manglemaul** and a
scrawled letter exposing the deal with "GREETOM." On returning to Merivale to Safe Rest (a 2–3 day
journey) the heroes reach **level 3**. The next chapter is in the full GM's Guide.

---

## What the Full Game Adds

*(From the PDF's "Get the Full Rules!" page — NimbleRPG.com. Useful as a roadmap of content beyond this
quickstart.)*

**Core Rules:**
- Complete rules including **Conditions, Cover, Grappling**, custom hero creation, and more.
- **Ancestries:** 5 Common + 19 Exotic.
- **24 Backgrounds** & 15 customizable Adventuring Motivations.
- **Items:** Weapons, Equipment, Armor, Magical Items, Spell Scrolls & Wands.
- **Spells:** 6 spell schools, **58 core spells**, and 18 Utility Spells.
- **14+ Variant Rules.**

**Heroes — 11 classes, all to level 20:**
- Berserker, **Cheat**, Commander, **Hunter**, **Mage**, **Oathsworn**, Shadowmancer, Shepherd,
  Songweaver, Stormshifter, Zephyr.
- Plus **22 subclasses** and 4 narrative subclasses: Oathbreaker, Spellblade, Reaver, Beastmaster.

**Game Master's Guide:** advanced GM tools, adventuring rewards (items, secret spells, boons), monster
running/building, a **69-monster Mini Bestiary**, legendary monsters (21 new) with solo-vs-party rules,
a 3-shot starter adventure, a sandbox setting (9 detailed locations), 7 one-shot adventures, and quick
5e-conversion rules.

---

*This file is the canonical local copy of the Nimble quickstart rules for the NimbleSheets project.
When seeding or modeling reference data, prefer this document over the placeholder demo data currently
in `NS.SoloDB/SeedData.cs`. The biggest outstanding fidelity gap is the **spell lists** (see above).*
