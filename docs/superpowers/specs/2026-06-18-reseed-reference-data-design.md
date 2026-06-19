# Reseed Reference Data from the Quickstart Rules — Design

**Date:** 2026-06-18
**Status:** Approved (brainstorming) — implement directly (brief spec, no separate plan)

## Problem

`NS.SoloDB/SeedData.cs` is illustrative demo data — a tiny hand-written set, some of it
D&D-flavored (Human/Elf/Dwarf with Darkvision/Stonecunning, Acolyte/Soldier backgrounds) — written
to exercise the app before any real content existed. Now that the Nimble quickstart rules are
transcribed in `docs/rules/nimble-basic-rules.md` (including the OCR-recovered spell lists), we can
replace the placeholder data with **authentic** content where the rules provide it.

## Goals

- Replace `SeedData.cs` collections with authentic quickstart content for the entities the rules
  actually define: **Spells, Features, Conditions, Actions, Rules**.
- Keep **honest, clearly-labeled placeholders** for entities the quickstart does **not** define
  (Ancestries, Backgrounds, Armor, Weapons), and a mix of authentic + placeholder for Magic Items.
- **Preserve every GUID** the client fixture (`NS.Client/src/lib/fixtures/caldra.ts`) and the seeding
  tests depend on, so neither needs changes.
- Keep `SeedingTests` green and add a few authenticity assertions.

## Non-Goals

- Seeding heroes or users (heroes are user-owned, created via the build API).
- Authoring full-game content not in the quickstart (5+19 ancestries, 24 backgrounds, full
  weapon/armor tables, spells beyond the 16 quickstart spells, the remaining 7 classes' features).
- Any domain, endpoint, validator, or client changes. This is data-only inside `NS.SoloDB`
  (plus test additions).
- Inventing plausible-but-unsourced content for the non-quickstart entities (explicitly rejected in
  brainstorming — placeholders are labeled as such, not dressed up as canonical).

## Decisions (from brainstorming)

| Question | Decision |
|---|---|
| Non-sourced entities (ancestries/backgrounds/armor/weapons) | **Honest, labeled placeholders** (descriptions note "not in the quickstart") |
| Feature breadth | **Full L1–4 features for all 4 quickstart classes**, incl. subclass features and selectable ability lists |
| GUID strategy | Keep the existing prefix scheme; **preserve the 6 fixture-referenced GUIDs**; fresh sequential GUIDs for the rest |
| Magic items | Authentic adventure items + one placeholder |

## Per-entity plan

Source = `docs/rules/nimble-basic-rules.md` unless noted "placeholder/inferred".

- **Spells** (`e…`) — the **16 authentic** quickstart spells (Fire/Ice/Lightning/Radiant, 4 each).
  Map each card to `Spell(ActionCost, AreaOfEffect, DamageExpression, DamageType, Description,
  Duration, Id, IsConcentration, IsSecret, ManaCost, Name, Range, SaveType, School, Tier,
  UpcastEffect)`. `ManaCost = Tier` (cantrips = tier 0, mana 0). `Range` vs Reach captured in
  description where the domain has only `Range`. Keep `e…0001` a Fire spell (the Wand magic item's
  `ContainedSpellId`).
  - *Note:* `SpellSchool` enum is `Fire, Ice, Lightning, Radiant` — exactly the 4 quickstart schools.
- **Features** (`d…`) — full quickstart features for **Cheat, Hunter, Mage, Oathsworn**: each class's
  L1–4 named features, subclass features (with `Subclass` set), and the selectable ability lists
  (Thrill of the Hunt, Underhanded Abilities, Spellshaper, Sacred Decrees) modeled as a single
  `Feature` each with `SelectableOptions = [ability names]`. **Preserve** `d…0001` = Radiant Judgment
  and `d…0002` = Lay on Hands (Oathsworn L1; referenced by the Caldra fixture).
- **Conditions** (`f…`) — only the conditions the quickstart actually references: Blinded, Charged,
  Distracted, Dying, Frightened, Grappled, Hampered, Incapacitated, Invisible, Poisoned, Prone,
  Restrained, Slowed, Smoldering, Taunted. (The current seed's *Bleeding* and *Dazed* are dropped —
  they are not quickstart conditions.) Authentic descriptions where the quickstart defines them
  (e.g. Poisoned = "disadvantage on rolls, healing ends"; Restrained = "escape DC 12 or any
  slashing/fire damage"; Distracted, Slowed, Prone, Dying); a short "(see core rules)" note where the
  quickstart only references the condition without defining it.
- **Actions** (`ac…`) — Attack, Move, Defend, Interpose, Opportunity Attack, Help, Assess, Free
  Action, mapped to `ActionReference(ActionType, Cost, Description, FrequencyLimit, Id, Name)`.
- **Rules** (`ce…`) — authentic `RuleReference` rows across categories: Combat (Exploding Crits,
  Dying, Initiative, Rushed Attacks, Monster Armor), Resting (Field Rest, Safe Rest), Conditions
  (Wounds, Death), Movement (Speed/Range/Reach, Falling), LevelUp, Spellcasting (Mana, Upcasting).
- **Ancestries** (`a…`) — **placeholder**. Keep **Human** at `a…0001` (fixture + test). Keep
  Elf/Dwarf rows but relabel descriptions as placeholders.
- **Backgrounds** (`ba…`) — **placeholder** (keep 1–2 rows, labeled).
- **Armor** (`c…`) — **placeholder/inferred**. Keep **Rusty Mail** `c…0001` and **Wooden Buckler**
  `c…0002` (fixture). Add Cloth/Leather/Plate rows with inferred values, labeled.
- **Weapons** (`b…`) — **placeholder/inferred**. Keep **Mace** `b…0001` (fixture). Add quickstart
  starting-gear weapons (Dagger, Shortbow, Staff, Sling) with inferred stats, labeled.
- **Magic Items** (`da…`) — authentic adventure items: Manglemaul, Cloak of Lesser Windform, Golden
  Acorn, Golden Heart Locket; plus one labeled placeholder. The Wand item keeps `ContainedSpellId`
  pointing at a Fire spell.

## Honesty marker

Placeholder/inferred rows include a short suffix in their `Description`, e.g.
`"… (placeholder — not defined in the Nimble quickstart rules)"` or `"(inferred)"`, so the app never
presents invented data as canonical. Authentic rows carry no such marker.

## Tests

- `SeedingTests` keep passing unchanged: every collection non-empty; `Human` at `a…0001`.
- **Add** assertions:
  - A known **spell** is seeded under its GUID with expected fields (e.g. Flame Dart — Fire cantrip,
    `1d10`).
  - A known **feature** is seeded (Radiant Judgment at `d…0001`, class Oathsworn, level 1).
  - Spells count == 16; the 4 schools are all represented.
- Update the `SeedData` class XML-doc comment to describe the new authentic-vs-placeholder split.

## Operational caveat (no code change)

Seeding is **seed-only-when-empty** (idempotent), so an existing `nimble-sheet.db` will **not** pick
up the new rows — delete the DB file to reseed. Note this in the spec and CLAUDE.md's seeding section.

## Verification

- `dotnet test NimbleSheets.slnx` green (existing + new seeding assertions).
- `dotnet build` clean (0 warnings).
- Client unaffected (fixture GUIDs preserved) — no `caldra.ts` / resolver changes.
