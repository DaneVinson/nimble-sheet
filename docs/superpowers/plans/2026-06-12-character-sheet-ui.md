# Character Sheet UI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a display-only, dark-mode Nimble character sheet in `NS.Client` that renders every section of a full hero from a typed fixture mirroring real API responses.

**Architecture:** Three layers keep a later API swap nearly free — (1) TypeScript DTO types mirroring the C# API DTOs, (2) a fixture shaped exactly like API payloads, (3) a pure resolver that joins the hero's ID-referenced collections to reference data and produces view models. Svelte 5 (runes) components render the view model. No backend changes.

**Tech Stack:** SvelteKit 2 / Svelte 5 (runes), TypeScript, Tailwind CSS v4, Vitest (resolver tests). All work is inside `NS.Client/`.

**Spec:** `docs/superpowers/specs/2026-06-12-character-sheet-ui-design.md`

---

## Conventions for this plan

- This is a **front-end** project. The C# conventions in `CLAUDE.md` do **not** apply here — follow standard SvelteKit/TypeScript idioms.
- **No TDD** (per project preference): implement first; the resolver unit tests are the final task. Do not write tests before implementation.
- Run all commands from `NS.Client/` unless stated otherwise.
- The sheet is **always dark** — style components directly with dark-tone Tailwind utilities (e.g. `bg-slate-800`, `text-slate-200`); do not rely on `dark:` variants.
- JSON property names are **camelCase** (FastEndpoints' System.Text.Json default). Note the non-obvious hero collection names: `activeConditions`, `knownSpells` (not `conditions`/`spells`).
- Enums serialize **by name** (`JsonStringEnumConverter`), so TS models them as string-union types (`'Oathsworn'`, `'D10'`, `'Strength'`).
- Inside `src/lib`, use **relative imports** (e.g. `../api/types`) — not the `$lib` alias — so the resolver and its tests run under a standalone Vitest config without the SvelteKit plugin. Components (`.svelte`) may use `$lib`.

---

## File Structure

| File | Responsibility |
|---|---|
| `src/lib/api/types.ts` | (create) TS interfaces mirroring the API DTOs + enum string-union types |
| `src/lib/fixtures/caldra.ts` | (create) `Hero` + `ReferenceData` fixture (Caldra Brightward, Oathsworn 1) |
| `src/lib/sheet/viewmodel.ts` | (create) view-model types the components render |
| `src/lib/sheet/resolve.ts` | (create) pure resolver: `Hero + ReferenceData` → `SheetViewModel` |
| `src/lib/sheet/format.ts` | (create) small display helpers (`formatModifier`, `formatDie`) |
| `src/lib/sheet/components/Panel.svelte` | (create) titled card wrapper + empty-state helper |
| `src/lib/sheet/components/HeroBanner.svelte` | (create) name + Ancestry·Class·Level |
| `src/lib/sheet/components/StatBlock.svelte` | (create) one stat tile with SAVE caret |
| `src/lib/sheet/components/StatRow.svelte` | (create) the four stat tiles |
| `src/lib/sheet/components/SkillsRow.svelte` | (create) the ten skills |
| `src/lib/sheet/components/HpTile.svelte` | (create) current/temp/max HP tile (popover-ready) |
| `src/lib/sheet/components/WoundTrack.svelte` | (create) 6 pips + skull, dead/dying styling |
| `src/lib/sheet/components/VitalsRow.svelte` | (create) HP/Wounds/Armor/Init/HitDice row |
| `src/lib/sheet/components/CombatPanel.svelte` | (create) weapons, armor, conditions |
| `src/lib/sheet/components/MagicPanel.svelte` | (create) spells grouped by tier |
| `src/lib/sheet/components/ClassResourcesPanel.svelte` | (create) class resource list |
| `src/lib/sheet/components/InventoryPanel.svelte` | (create) magic items, gear |
| `src/lib/sheet/components/FeaturesPanel.svelte` | (create) features grouped by level |
| `src/lib/sheet/components/SheetTabs.svelte` | (create) lightweight tab switcher |
| `src/lib/sheet/components/HeroSheet.svelte` | (create) top-level composition |
| `src/routes/sheet/+page.svelte` | (create) `/sheet` route: fixture → resolver → HeroSheet |
| `src/lib/sheet/resolve.test.ts` | (create, final task) Vitest resolver tests |
| `vitest.config.ts` | (create, final task) standalone Vitest config |
| `package.json` | (modify, final task) add `vitest` devDependency + `test` script |

---

## Task 1: API DTO types

**Files:**
- Create: `src/lib/api/types.ts`

- [ ] **Step 1: Write the types file**

Create `src/lib/api/types.ts` with the exact content below. Field names are camelCase to match API JSON; enums are string-union types matching the serialized enum names.

```ts
// Enum string-union types (serialized by name via JsonStringEnumConverter).
export type StatType = 'Strength' | 'Dexterity' | 'Intelligence' | 'Will';
export type DieType = 'D4' | 'D6' | 'D8' | 'D10' | 'D12';
export type ArmorType = 'Cloth' | 'Leather' | 'Mail' | 'Plate' | 'Shield';
export type DamageType =
  | 'Bludgeoning' | 'Cold' | 'Fire' | 'Lightning'
  | 'Piercing' | 'Psychic' | 'Radiant' | 'Slashing';
export type SpellSchool = 'Fire' | 'Ice' | 'Lightning' | 'Radiant';
export type ActionType = 'Free' | 'Heroic' | 'Reaction';
export type RuleCategory =
  | 'Combat' | 'Conditions' | 'LevelUp' | 'Movement' | 'Resting' | 'Spellcasting';
export type HeroClass =
  | 'Berserker' | 'Cheat' | 'Commander' | 'Hunter' | 'Mage' | 'Oathsworn'
  | 'Shadowmancer' | 'Shepherd' | 'Songweaver' | 'Stormshifter' | 'Zephyr';

// Hero value objects.
export interface HeroStats {
  dexterity: number;
  intelligence: number;
  strength: number;
  will: number;
}
export interface HeroSkills {
  arcana: number;
  examination: number;
  finesse: number;
  influence: number;
  insight: number;
  lore: number;
  might: number;
  naturecraft: number;
  perception: number;
  stealth: number;
}
export interface HeroCombatStats {
  armor: number;
  hitDieType: DieType;
  initiativeBonus: number;
  speed: number;
}
export interface HeroSaves {
  advantageOn: StatType;
  disadvantageOn: StatType;
}
export interface ClassResources {
  judgmentDiceCount: number | null;
  judgmentDiceType: DieType | null;
  layOnHandsPool: number | null;
  thrillCharges: number | null;
}

// Hero ID-referenced collections.
export interface HeroArmor { armorId: string; heroId: string; isEquipped: boolean; }
export interface HeroCondition { conditionId: string; expiresAtEndOf: string | null; heroId: string; }
export interface HeroFeature { choices: string[]; featureId: string; heroId: string; levelGained: number; }
export interface HeroGearItem { heroId: string; name: string; quantity: number; }
export interface HeroMagicItem { chargesRemaining: number | null; heroId: string; isEquipped: boolean; magicItemId: string; }
export interface HeroSpell { heroId: string; notes: string | null; spellId: string; tierUnlocked: number; }
export interface HeroWeapon { heroId: string; isEquipped: boolean; notes: string | null; weaponId: string; }

// Hero aggregate (as returned by GET /heroes/{id}).
export interface Hero {
  activeConditions: HeroCondition[];
  ancestryId: string;
  armor: HeroArmor[];
  backgroundId: string | null;
  class: HeroClass;
  combatStats: HeroCombatStats;
  currentHp: number;
  currentMana: number | null;
  currentWounds: number;
  features: HeroFeature[];
  gear: HeroGearItem[];
  hitDiceAvailable: number;
  id: string;
  isDead: boolean;
  isDying: boolean;
  knownSpells: HeroSpell[];
  level: number;
  magicItems: HeroMagicItem[];
  maxHitDice: number;
  maxHp: number;
  maxMana: number | null;
  name: string;
  pendingFeatureChoices: string[];
  pendingStatIncrease: boolean;
  resources: ClassResources;
  saves: HeroSaves;
  skills: HeroSkills;
  stats: HeroStats;
  subclass: string | null;
  tempHp: number;
  unspentSkillPoints: number;
  userId: string;
  weapons: HeroWeapon[];
}

// Reference entities.
export interface Ancestry { description: string; id: string; name: string; traits: string[]; }
export interface Background { description: string; grants: string; id: string; name: string; }
export interface Armor { armorType: ArmorType; armorValue: number; description: string; id: string; name: string; }
export interface Weapon {
  damageExpression: string;
  damageType: DamageType;
  description: string;
  id: string;
  isRare: boolean;
  isTwoHanded: boolean;
  name: string;
  range: number | null;
  reach: number;
  specialEffect: string | null;
  statUsed: StatType;
}
export interface Condition { description: string; id: string; name: string; }
export interface Feature {
  class: HeroClass;
  description: string;
  frequencyLimit: string | null;
  id: string;
  level: number;
  name: string;
  selectableOptions: string[] | null;
  subclass: string | null;
}
export interface MagicItem {
  containedSpellId: string | null;
  description: string;
  effect: string;
  id: string;
  maxCharges: number | null;
  name: string;
  rarity: string;
}
export interface Spell {
  actionCost: number;
  areaOfEffect: string | null;
  damageExpression: string | null;
  damageType: DamageType | null;
  description: string;
  duration: string | null;
  id: string;
  isConcentration: boolean;
  isSecret: boolean;
  manaCost: number;
  name: string;
  range: number | null;
  saveType: StatType | null;
  school: SpellSchool;
  tier: number;
  upcastEffect: string | null;
}

// Bundle of reference data needed to resolve a hero (mirrors fetching each /reference/* endpoint).
export interface ReferenceData {
  ancestries: Ancestry[];
  armor: Armor[];
  backgrounds: Background[];
  conditions: Condition[];
  features: Feature[];
  magicItems: MagicItem[];
  spells: Spell[];
  weapons: Weapon[];
}
```

- [ ] **Step 2: Type-check**

Run: `npm run check`
Expected: 0 errors, 0 warnings.

- [ ] **Step 3: Commit**

```bash
git add src/lib/api/types.ts
git commit -m "feat(client): add API DTO types for character sheet"
```

---

## Task 2: Fixture (Caldra Brightward)

**Files:**
- Create: `src/lib/fixtures/caldra.ts`

- [ ] **Step 1: Write the fixture**

Create `src/lib/fixtures/caldra.ts`. GUID strings are fixed and arbitrary (a fixture needs valid-looking GUIDs, not real v7 ones). The hero references a Mace, Rusty Mail, Wooden Buckler, two L1 features, and carries Manacles as gear. Spells, magic items, and conditions are intentionally empty.

```ts
import type { Hero, ReferenceData } from '../api/types';

const userId = '20000000-0000-0000-0000-000000000001';
const heroId = '10000000-0000-0000-0000-000000000001';
const ancestryHumanId = 'a0000000-0000-0000-0000-000000000001';
const maceId = 'b0000000-0000-0000-0000-000000000001';
const rustyMailId = 'c0000000-0000-0000-0000-000000000001';
const woodenBucklerId = 'c0000000-0000-0000-0000-000000000002';
const radiantJudgmentId = 'd0000000-0000-0000-0000-000000000001';
const layOnHandsId = 'd0000000-0000-0000-0000-000000000002';

export const caldra: Hero = {
  activeConditions: [],
  ancestryId: ancestryHumanId,
  armor: [
    { armorId: rustyMailId, heroId, isEquipped: true },
    { armorId: woodenBucklerId, heroId, isEquipped: true }
  ],
  backgroundId: null,
  class: 'Oathsworn',
  combatStats: { armor: 8, hitDieType: 'D10', initiativeBonus: 0, speed: 6 },
  currentHp: 17,
  currentMana: null,
  currentWounds: 0,
  features: [
    { choices: [], featureId: radiantJudgmentId, heroId, levelGained: 1 },
    { choices: [], featureId: layOnHandsId, heroId, levelGained: 1 }
  ],
  gear: [{ heroId, name: 'Manacles', quantity: 1 }],
  hitDiceAvailable: 1,
  id: heroId,
  isDead: false,
  isDying: false,
  knownSpells: [],
  level: 1,
  magicItems: [],
  maxHitDice: 1,
  maxHp: 17,
  maxMana: null,
  name: 'Caldra Brightward',
  pendingFeatureChoices: [],
  pendingStatIncrease: false,
  resources: {
    judgmentDiceCount: 2,
    judgmentDiceType: 'D6',
    layOnHandsPool: 5,
    thrillCharges: null
  },
  saves: { advantageOn: 'Strength', disadvantageOn: 'Dexterity' },
  skills: {
    arcana: -1,
    examination: -1,
    finesse: 0,
    influence: 4,
    insight: 4,
    lore: -1,
    might: 2,
    naturecraft: 2,
    perception: 2,
    stealth: 0
  },
  stats: { dexterity: 0, intelligence: -1, strength: 2, will: 2 },
  subclass: null,
  tempHp: 0,
  unspentSkillPoints: 0,
  userId
};

export const referenceData: ReferenceData = {
  ancestries: [
    { id: ancestryHumanId, name: 'Human', description: 'Versatile and ambitious.', traits: ['Adaptable'] }
  ],
  armor: [
    { id: rustyMailId, name: 'Rusty Mail', armorType: 'Mail', armorValue: 6, description: '6 + DEX armor.' },
    { id: woodenBucklerId, name: 'Wooden Buckler', armorType: 'Shield', armorValue: 2, description: '+2 armor.' }
  ],
  backgrounds: [],
  conditions: [],
  features: [
    {
      id: radiantJudgmentId, class: 'Oathsworn', level: 1, name: 'Radiant Judgment',
      description: 'When an enemy attacks you, if you have no Judgment Dice, roll your Judgment Dice (2d6). On your next melee hit this encounter, deal that much additional radiant damage.',
      frequencyLimit: null, selectableOptions: null, subclass: null
    },
    {
      id: layOnHandsId, class: 'Oathsworn', level: 1, name: 'Lay on Hands',
      description: 'A magical pool of healing power equal to 5 x LVL. Action: touch a target and spend any amount to restore that many HP.',
      frequencyLimit: null, selectableOptions: null, subclass: null
    }
  ],
  magicItems: [],
  spells: [],
  weapons: [
    {
      id: maceId, name: 'Mace', damageExpression: '1d6+2', damageType: 'Bludgeoning',
      statUsed: 'Strength', reach: 1, range: null, isRare: false, isTwoHanded: false,
      specialEffect: null, description: 'A simple bludgeoning weapon.'
    }
  ]
};
```

> Note: the Mace's `weaponId` is `maceId`, but the hero has no `HeroWeapon` entry referencing it unless added. Add the weapon link in the next step — it was omitted above intentionally so this step stays focused.

- [ ] **Step 2: Add the hero's weapon link**

In `src/lib/fixtures/caldra.ts`, the `caldra` object currently has no `weapons` property. Add it (TypeScript will already be flagging the missing required `weapons` field). Insert this property in alphabetical position, immediately after the `unspentSkillPoints` line and before `userId`:

```ts
  weapons: [{ heroId, isEquipped: true, notes: null, weaponId: maceId }],
```

- [ ] **Step 3: Type-check**

Run: `npm run check`
Expected: 0 errors, 0 warnings. (If `weapons` was still missing, this fails with "Property 'weapons' is missing" — fix by completing Step 2.)

- [ ] **Step 4: Commit**

```bash
git add src/lib/fixtures/caldra.ts
git commit -m "feat(client): add Caldra Brightward fixture"
```

---

## Task 3: Format helpers

**Files:**
- Create: `src/lib/sheet/format.ts`

- [ ] **Step 1: Write the helpers**

Create `src/lib/sheet/format.ts`:

```ts
import type { DieType } from '../api/types';

/** Formats a skill/bonus modifier: positive values get a leading '+', zero and negatives are shown as-is. */
export function formatModifier(value: number): string {
  return value > 0 ? `+${value}` : `${value}`;
}

/** Formats a die type for display, e.g. 'D10' -> 'd10'. */
export function formatDie(die: DieType): string {
  return die.toLowerCase();
}
```

- [ ] **Step 2: Commit**

```bash
git add src/lib/sheet/format.ts
git commit -m "feat(client): add sheet display format helpers"
```

---

## Task 4: View-model types

**Files:**
- Create: `src/lib/sheet/viewmodel.ts`

- [ ] **Step 1: Write the view-model types**

Create `src/lib/sheet/viewmodel.ts`:

```ts
import type { ArmorType, DamageType, HeroClass, SpellSchool, StatType } from '../api/types';

export type SaveMarker = 'advantage' | 'disadvantage' | null;

export interface StatViewModel {
  type: StatType;
  label: string;   // 'STR'
  value: number;   // raw stat value, e.g. 2 or -1
  save: SaveMarker;
}

export interface SkillViewModel {
  name: string;       // 'Arcana'
  statLabel: string;  // 'INT'
  bonus: number;
  display: string;    // '+4' / '-1' / '0'
}

export interface WeaponViewModel {
  name: string;
  damage: string;        // '1d6+2'
  damageType: DamageType;
  statLabel: string;     // 'STR'
  reach: number;
  range: number | null;
  isTwoHanded: boolean;
  isEquipped: boolean;
  notes: string | null;
}

export interface ArmorViewModel {
  name: string;
  type: ArmorType;
  armorValue: number;
  isEquipped: boolean;
}

export interface ConditionViewModel {
  name: string;
  description: string;
  expiresAtEndOf: string | null;
}

export interface SpellViewModel {
  name: string;
  tier: number;
  school: SpellSchool;
  manaCost: number;
  actionCost: number;
  damage: string | null;
  damageType: DamageType | null;
  description: string;
  notes: string | null;
}

export interface SpellTierGroup {
  tier: number;
  spells: SpellViewModel[];
}

export interface ClassResourceViewModel {
  label: string;  // 'Judgment Dice'
  value: string;  // '2d6'
}

export interface MagicItemViewModel {
  name: string;
  rarity: string;
  effect: string;
  description: string;
  isEquipped: boolean;
  charges: { remaining: number; max: number } | null;
}

export interface GearViewModel {
  name: string;
  quantity: number;
}

export interface FeatureViewModel {
  name: string;
  description: string;
  level: number;
  subclass: string | null;
  frequencyLimit: string | null;
  choices: string[];
}

export interface FeatureLevelGroup {
  level: number;
  features: FeatureViewModel[];
}

export interface SheetViewModel {
  name: string;
  level: number;
  className: HeroClass;
  ancestryName: string;
  backgroundName: string | null;
  subclass: string | null;

  hp: { current: number; max: number; temp: number };
  wounds: { current: number; max: number; isDead: boolean; isDying: boolean };
  armor: number;
  initiative: number;
  speed: number;
  hitDice: { die: string; available: number; max: number };
  mana: { current: number; max: number } | null;

  stats: StatViewModel[];
  skills: SkillViewModel[];

  weapons: WeaponViewModel[];
  armorItems: ArmorViewModel[];
  conditions: ConditionViewModel[];
  spellsByTier: SpellTierGroup[];
  classResources: ClassResourceViewModel[];
  magicItems: MagicItemViewModel[];
  gear: GearViewModel[];
  features: FeatureLevelGroup[];
}
```

- [ ] **Step 2: Type-check**

Run: `npm run check`
Expected: 0 errors, 0 warnings.

- [ ] **Step 3: Commit**

```bash
git add src/lib/sheet/viewmodel.ts
git commit -m "feat(client): add character sheet view-model types"
```

---

## Task 5: Resolver

**Files:**
- Create: `src/lib/sheet/resolve.ts`

- [ ] **Step 1: Write the resolver**

Create `src/lib/sheet/resolve.ts`. It joins the hero's ID-referenced collections to reference data, tolerating missing references with fallback labels, and computes display values.

```ts
import type {
  Feature, Hero, ReferenceData, Spell, StatType
} from '../api/types';
import { formatDie, formatModifier } from './format';
import type {
  ClassResourceViewModel, FeatureLevelGroup, SheetViewModel, SkillViewModel,
  SpellTierGroup, SpellViewModel, StatViewModel
} from './viewmodel';

const STAT_LABEL: Record<StatType, string> = {
  Strength: 'STR',
  Dexterity: 'DEX',
  Intelligence: 'INT',
  Will: 'WIL'
};

function byId<T extends { id: string }>(items: T[]): Map<string, T> {
  return new Map(items.map((item) => [item.id, item]));
}

function buildStats(hero: Hero): StatViewModel[] {
  const { advantageOn, disadvantageOn } = hero.saves;
  const make = (type: StatType, value: number): StatViewModel => ({
    type,
    label: STAT_LABEL[type],
    value,
    save: type === advantageOn ? 'advantage' : type === disadvantageOn ? 'disadvantage' : null
  });
  return [
    make('Strength', hero.stats.strength),
    make('Dexterity', hero.stats.dexterity),
    make('Intelligence', hero.stats.intelligence),
    make('Will', hero.stats.will)
  ];
}

function buildSkills(hero: Hero): SkillViewModel[] {
  const s = hero.skills;
  const rows: Array<[string, string, number]> = [
    ['Arcana', 'INT', s.arcana],
    ['Examination', 'INT', s.examination],
    ['Finesse', 'DEX', s.finesse],
    ['Influence', 'WIL', s.influence],
    ['Insight', 'WIL', s.insight],
    ['Lore', 'INT', s.lore],
    ['Might', 'STR', s.might],
    ['Naturecraft', 'WIL', s.naturecraft],
    ['Perception', 'WIL', s.perception],
    ['Stealth', 'DEX', s.stealth]
  ];
  return rows.map(([name, statLabel, bonus]) => ({
    name,
    statLabel,
    bonus,
    display: formatModifier(bonus)
  }));
}

function buildClassResources(hero: Hero): ClassResourceViewModel[] {
  const r = hero.resources;
  const out: ClassResourceViewModel[] = [];
  if (hero.maxMana !== null) {
    out.push({ label: 'Mana', value: `${hero.currentMana ?? 0} / ${hero.maxMana}` });
  }
  if (r.judgmentDiceCount !== null && r.judgmentDiceType !== null) {
    out.push({ label: 'Judgment Dice', value: `${r.judgmentDiceCount}${formatDie(r.judgmentDiceType)}` });
  }
  if (r.layOnHandsPool !== null) {
    out.push({ label: 'Lay on Hands', value: `${r.layOnHandsPool}` });
  }
  if (r.thrillCharges !== null) {
    out.push({ label: 'Thrill Charges', value: `${r.thrillCharges}` });
  }
  return out;
}

function buildSpellsByTier(hero: Hero, spells: Map<string, Spell>): SpellTierGroup[] {
  const resolved: SpellViewModel[] = hero.knownSpells.map((known) => {
    const spell = spells.get(known.spellId);
    if (!spell) {
      return {
        name: 'Unknown spell', tier: known.tierUnlocked, school: 'Fire', manaCost: 0,
        actionCost: 1, damage: null, damageType: null, description: '', notes: known.notes
      };
    }
    return {
      name: spell.name, tier: spell.tier, school: spell.school, manaCost: spell.manaCost,
      actionCost: spell.actionCost, damage: spell.damageExpression, damageType: spell.damageType,
      description: spell.description, notes: known.notes
    };
  });
  const tiers = new Map<number, SpellViewModel[]>();
  for (const s of resolved) {
    const group = tiers.get(s.tier) ?? [];
    group.push(s);
    tiers.set(s.tier, group);
  }
  return [...tiers.entries()]
    .sort((a, b) => a[0] - b[0])
    .map(([tier, group]) => ({ tier, spells: group.sort((a, b) => a.name.localeCompare(b.name)) }));
}

function buildFeatures(hero: Hero, features: Map<string, Feature>): FeatureLevelGroup[] {
  const groups = new Map<number, FeatureLevelGroup['features']>();
  for (const owned of hero.features) {
    const ref = features.get(owned.featureId);
    const vm = {
      name: ref?.name ?? 'Unknown feature',
      description: ref?.description ?? '',
      level: owned.levelGained,
      subclass: ref?.subclass ?? null,
      frequencyLimit: ref?.frequencyLimit ?? null,
      choices: owned.choices
    };
    const group = groups.get(owned.levelGained) ?? [];
    group.push(vm);
    groups.set(owned.levelGained, group);
  }
  return [...groups.entries()]
    .sort((a, b) => a[0] - b[0])
    .map(([level, group]) => ({ level, features: group.sort((a, b) => a.name.localeCompare(b.name)) }));
}

/** Joins a hero with reference data into the view model the sheet renders. */
export function resolveSheet(hero: Hero, reference: ReferenceData): SheetViewModel {
  const ancestries = byId(reference.ancestries);
  const backgrounds = byId(reference.backgrounds);
  const weapons = byId(reference.weapons);
  const armor = byId(reference.armor);
  const conditions = byId(reference.conditions);
  const magicItems = byId(reference.magicItems);
  const spells = byId(reference.spells);
  const features = byId(reference.features);

  return {
    name: hero.name,
    level: hero.level,
    className: hero.class,
    ancestryName: ancestries.get(hero.ancestryId)?.name ?? 'Unknown ancestry',
    backgroundName: hero.backgroundId ? backgrounds.get(hero.backgroundId)?.name ?? 'Unknown background' : null,
    subclass: hero.subclass,

    hp: { current: hero.currentHp, max: hero.maxHp, temp: hero.tempHp },
    wounds: { current: hero.currentWounds, max: 6, isDead: hero.isDead, isDying: hero.isDying },
    armor: hero.combatStats.armor,
    initiative: hero.combatStats.initiativeBonus,
    speed: hero.combatStats.speed,
    hitDice: { die: formatDie(hero.combatStats.hitDieType), available: hero.hitDiceAvailable, max: hero.maxHitDice },
    mana: hero.maxMana !== null ? { current: hero.currentMana ?? 0, max: hero.maxMana } : null,

    stats: buildStats(hero),
    skills: buildSkills(hero),

    weapons: hero.weapons.map((w) => {
      const ref = weapons.get(w.weaponId);
      return {
        name: ref?.name ?? 'Unknown weapon',
        damage: ref?.damageExpression ?? '—',
        damageType: ref?.damageType ?? 'Bludgeoning',
        statLabel: ref ? STAT_LABEL[ref.statUsed] : '—',
        reach: ref?.reach ?? 1,
        range: ref?.range ?? null,
        isTwoHanded: ref?.isTwoHanded ?? false,
        isEquipped: w.isEquipped,
        notes: w.notes
      };
    }),
    armorItems: hero.armor.map((a) => {
      const ref = armor.get(a.armorId);
      return {
        name: ref?.name ?? 'Unknown armor',
        type: ref?.armorType ?? 'Cloth',
        armorValue: ref?.armorValue ?? 0,
        isEquipped: a.isEquipped
      };
    }),
    conditions: hero.activeConditions.map((c) => {
      const ref = conditions.get(c.conditionId);
      return {
        name: ref?.name ?? 'Unknown condition',
        description: ref?.description ?? '',
        expiresAtEndOf: c.expiresAtEndOf
      };
    }),
    spellsByTier: buildSpellsByTier(hero, spells),
    classResources: buildClassResources(hero),
    magicItems: hero.magicItems.map((m) => {
      const ref = magicItems.get(m.magicItemId);
      return {
        name: ref?.name ?? 'Unknown item',
        rarity: ref?.rarity ?? '',
        effect: ref?.effect ?? '',
        description: ref?.description ?? '',
        isEquipped: m.isEquipped,
        charges: m.chargesRemaining !== null && ref?.maxCharges != null
          ? { remaining: m.chargesRemaining, max: ref.maxCharges }
          : null
      };
    }),
    gear: hero.gear.map((g) => ({ name: g.name, quantity: g.quantity })),
    features: buildFeatures(hero, features)
  };
}
```

- [ ] **Step 2: Type-check**

Run: `npm run check`
Expected: 0 errors, 0 warnings.

- [ ] **Step 3: Commit**

```bash
git add src/lib/sheet/resolve.ts
git commit -m "feat(client): add character sheet resolver"
```

---

## Task 6: Shared Panel component

**Files:**
- Create: `src/lib/sheet/components/Panel.svelte`

- [ ] **Step 1: Write Panel.svelte**

Create `src/lib/sheet/components/Panel.svelte`. A titled card wrapper; renders an empty-state message when `empty` is true.

```svelte
<script lang="ts">
  import type { Snippet } from 'svelte';

  let {
    title,
    empty = false,
    emptyText = 'Nothing here yet.',
    children
  }: {
    title: string;
    empty?: boolean;
    emptyText?: string;
    children: Snippet;
  } = $props();
</script>

<section class="rounded-lg bg-slate-800 p-4">
  <h3 class="mb-3 text-[10px] font-semibold uppercase tracking-[0.1em] text-sky-300">{title}</h3>
  {#if empty}
    <p class="text-sm italic text-slate-500">{emptyText}</p>
  {:else}
    {@render children()}
  {/if}
</section>
```

- [ ] **Step 2: Type-check**

Run: `npm run check`
Expected: 0 errors, 0 warnings.

- [ ] **Step 3: Commit**

```bash
git add src/lib/sheet/components/Panel.svelte
git commit -m "feat(client): add Panel wrapper component"
```

---

## Task 7: Header + stats + skills components

**Files:**
- Create: `src/lib/sheet/components/HeroBanner.svelte`
- Create: `src/lib/sheet/components/StatBlock.svelte`
- Create: `src/lib/sheet/components/StatRow.svelte`
- Create: `src/lib/sheet/components/SkillsRow.svelte`

- [ ] **Step 1: HeroBanner.svelte**

```svelte
<script lang="ts">
  let {
    name,
    ancestryName,
    className,
    level,
    subclass
  }: {
    name: string;
    ancestryName: string;
    className: string;
    level: number;
    subclass: string | null;
  } = $props();
</script>

<div class="bg-gradient-to-br from-blue-900 to-slate-900 px-5 py-5">
  <h1 class="text-2xl font-extrabold leading-tight text-white sm:text-3xl">{name}</h1>
  <p class="mt-1 text-sm text-blue-200">
    {ancestryName} · {className}{subclass ? ` (${subclass})` : ''} · Level {level}
  </p>
</div>
```

- [ ] **Step 2: StatBlock.svelte**

```svelte
<script lang="ts">
  import type { StatViewModel } from '../viewmodel';

  let { stat }: { stat: StatViewModel } = $props();
</script>

<div class="relative rounded-lg bg-slate-800 py-2 text-center">
  {#if stat.save === 'advantage'}
    <span class="absolute right-1.5 top-1 text-[8px] font-bold tracking-wide text-green-400" title="Advantage on {stat.label} saves">SAVE▲</span>
  {:else if stat.save === 'disadvantage'}
    <span class="absolute right-1.5 top-1 text-[8px] font-bold tracking-wide text-red-400" title="Disadvantage on {stat.label} saves">SAVE▼</span>
  {/if}
  <div class="text-2xl font-extrabold text-white">{stat.value}</div>
  <div class="mt-0.5 text-[10px] tracking-[0.12em] text-sky-300">{stat.label}</div>
</div>
```

- [ ] **Step 3: StatRow.svelte**

```svelte
<script lang="ts">
  import type { StatViewModel } from '../viewmodel';
  import StatBlock from './StatBlock.svelte';

  let { stats }: { stats: StatViewModel[] } = $props();
</script>

<div class="grid grid-cols-4 gap-2">
  {#each stats as stat (stat.type)}
    <StatBlock {stat} />
  {/each}
</div>
```

- [ ] **Step 4: SkillsRow.svelte**

```svelte
<script lang="ts">
  import type { SkillViewModel } from '../viewmodel';

  let { skills }: { skills: SkillViewModel[] } = $props();
</script>

<div class="grid grid-cols-2 gap-1.5 sm:grid-cols-5">
  {#each skills as skill (skill.name)}
    <div class="rounded-md bg-slate-800/70 py-1.5 text-center">
      <div class="text-[9px] uppercase tracking-wide text-slate-400">{skill.name}</div>
      <div class="text-sm font-bold text-white">{skill.display}</div>
      <div class="text-[8px] text-slate-500">{skill.statLabel}</div>
    </div>
  {/each}
</div>
```

- [ ] **Step 5: Type-check**

Run: `npm run check`
Expected: 0 errors, 0 warnings.

- [ ] **Step 6: Commit**

```bash
git add src/lib/sheet/components/HeroBanner.svelte src/lib/sheet/components/StatBlock.svelte src/lib/sheet/components/StatRow.svelte src/lib/sheet/components/SkillsRow.svelte
git commit -m "feat(client): add banner, stat, and skills components"
```

---

## Task 8: Vitals components

**Files:**
- Create: `src/lib/sheet/components/HpTile.svelte`
- Create: `src/lib/sheet/components/WoundTrack.svelte`
- Create: `src/lib/sheet/components/VitalsRow.svelte`

- [ ] **Step 1: HpTile.svelte**

The component is structured so a damage/heal popover can be added later without changing its shape. No interaction is wired now.

```svelte
<script lang="ts">
  let {
    current,
    max,
    temp
  }: {
    current: number;
    max: number;
    temp: number;
  } = $props();
</script>

<div class="rounded-lg bg-gradient-to-b from-red-900 to-red-950 p-2.5 text-center">
  <div class="text-[9px] uppercase tracking-[0.14em] text-red-200">Hit Points</div>
  <div class="text-3xl font-black leading-none text-white">{current}</div>
  <div class="mt-1 text-[10px] text-red-200">+{temp} temp · {max} max</div>
</div>
```

- [ ] **Step 2: WoundTrack.svelte**

```svelte
<script lang="ts">
  let {
    current,
    max,
    isDead,
    isDying
  }: {
    current: number;
    max: number;
    isDead: boolean;
    isDying: boolean;
  } = $props();

  const pips = $derived(Array.from({ length: max }, (_, i) => i < current));
</script>

<div class="rounded-lg bg-slate-800 p-2.5 text-center">
  <div class="text-[9px] uppercase tracking-[0.14em] text-slate-400">
    Wounds
    {#if isDead}<span class="ml-1 text-red-400">· Dead</span>
    {:else if isDying}<span class="ml-1 text-amber-400">· Dying</span>{/if}
  </div>
  <div class="mt-2 flex items-center justify-center gap-1">
    {#each pips as filled, i (i)}
      <span class="h-3 w-3 rounded-full border-2 {filled ? 'border-red-500 bg-red-500' : 'border-slate-500'}"></span>
    {/each}
    <span class="ml-0.5 text-sm text-slate-400">☠</span>
  </div>
</div>
```

- [ ] **Step 3: VitalsRow.svelte**

```svelte
<script lang="ts">
  import type { SheetViewModel } from '../viewmodel';
  import HpTile from './HpTile.svelte';
  import WoundTrack from './WoundTrack.svelte';

  let { vm }: { vm: SheetViewModel } = $props();
</script>

<div class="grid grid-cols-2 gap-2.5 sm:grid-cols-5">
  <HpTile current={vm.hp.current} max={vm.hp.max} temp={vm.hp.temp} />
  <WoundTrack current={vm.wounds.current} max={vm.wounds.max} isDead={vm.wounds.isDead} isDying={vm.wounds.isDying} />
  <div class="rounded-lg bg-slate-800 p-2.5 text-center">
    <div class="text-[9px] uppercase tracking-[0.14em] text-slate-400">Armor</div>
    <div class="mt-1 text-2xl font-extrabold text-white">{vm.armor}</div>
  </div>
  <div class="rounded-lg bg-slate-800 p-2.5 text-center">
    <div class="text-[9px] uppercase tracking-[0.14em] text-slate-400">Init</div>
    <div class="mt-1 text-2xl font-extrabold text-white">{vm.initiative}</div>
  </div>
  <div class="rounded-lg bg-slate-800 p-2.5 text-center">
    <div class="text-[9px] uppercase tracking-[0.14em] text-slate-400">Hit Dice</div>
    <div class="mt-1 text-2xl font-extrabold text-white">{vm.hitDice.die}</div>
    <div class="text-[10px] text-slate-400">{vm.hitDice.available} / {vm.hitDice.max}</div>
  </div>
</div>
```

- [ ] **Step 4: Type-check**

Run: `npm run check`
Expected: 0 errors, 0 warnings.

- [ ] **Step 5: Commit**

```bash
git add src/lib/sheet/components/HpTile.svelte src/lib/sheet/components/WoundTrack.svelte src/lib/sheet/components/VitalsRow.svelte
git commit -m "feat(client): add vitals row components"
```

---

## Task 9: Tab panel components

**Files:**
- Create: `src/lib/sheet/components/CombatPanel.svelte`
- Create: `src/lib/sheet/components/MagicPanel.svelte`
- Create: `src/lib/sheet/components/ClassResourcesPanel.svelte`
- Create: `src/lib/sheet/components/InventoryPanel.svelte`
- Create: `src/lib/sheet/components/FeaturesPanel.svelte`

- [ ] **Step 1: CombatPanel.svelte**

```svelte
<script lang="ts">
  import type { SheetViewModel } from '../viewmodel';
  import Panel from './Panel.svelte';

  let { vm }: { vm: SheetViewModel } = $props();
</script>

<div class="grid gap-3 sm:grid-cols-2">
  <Panel title="Weapons" empty={vm.weapons.length === 0} emptyText="No weapons.">
    <ul class="space-y-2">
      {#each vm.weapons as w (w.name)}
        <li class="text-sm text-slate-200">
          <span class="font-semibold text-white">{w.name}</span>
          <span class="text-slate-400">{w.damage} {w.damageType} · {w.statLabel}</span>
          {#if w.isTwoHanded}<span class="text-slate-500"> · two-handed</span>{/if}
          {#if w.notes}<div class="text-xs text-slate-500">{w.notes}</div>{/if}
        </li>
      {/each}
    </ul>
  </Panel>

  <Panel title="Armor" empty={vm.armorItems.length === 0} emptyText="No armor.">
    <ul class="space-y-2">
      {#each vm.armorItems as a (a.name)}
        <li class="text-sm text-slate-200">
          <span class="font-semibold text-white">{a.name}</span>
          <span class="text-slate-400">{a.type} · +{a.armorValue}</span>
          {#if a.isEquipped}<span class="text-green-400"> · equipped</span>{/if}
        </li>
      {/each}
    </ul>
  </Panel>

  <Panel title="Conditions" empty={vm.conditions.length === 0} emptyText="No active conditions.">
    <ul class="space-y-2">
      {#each vm.conditions as c (c.name)}
        <li class="text-sm text-slate-200">
          <span class="font-semibold text-white">{c.name}</span>
          {#if c.expiresAtEndOf}<span class="text-slate-400"> · expires {c.expiresAtEndOf}</span>{/if}
          <div class="text-xs text-slate-500">{c.description}</div>
        </li>
      {/each}
    </ul>
  </Panel>
</div>
```

- [ ] **Step 2: MagicPanel.svelte**

```svelte
<script lang="ts">
  import type { SheetViewModel } from '../viewmodel';
  import Panel from './Panel.svelte';

  let { vm }: { vm: SheetViewModel } = $props();
</script>

<Panel title="Spells" empty={vm.spellsByTier.length === 0} emptyText="No spells known.">
  <div class="space-y-4">
    {#each vm.spellsByTier as group (group.tier)}
      <div>
        <div class="mb-1 text-xs font-semibold text-sky-300">Tier {group.tier}</div>
        <ul class="space-y-2">
          {#each group.spells as s (s.name)}
            <li class="text-sm text-slate-200">
              <span class="font-semibold text-white">{s.name}</span>
              <span class="text-slate-400">{s.school} · {s.manaCost} mana · {s.actionCost} action{s.actionCost === 1 ? '' : 's'}</span>
              {#if s.damage}<span class="text-slate-400"> · {s.damage} {s.damageType}</span>{/if}
              <div class="text-xs text-slate-500">{s.description}</div>
            </li>
          {/each}
        </ul>
      </div>
    {/each}
  </div>
</Panel>
```

- [ ] **Step 3: ClassResourcesPanel.svelte**

```svelte
<script lang="ts">
  import type { SheetViewModel } from '../viewmodel';
  import Panel from './Panel.svelte';

  let { vm }: { vm: SheetViewModel } = $props();
</script>

<Panel title="Class Resources" empty={vm.classResources.length === 0} emptyText="No class resources.">
  <dl class="grid grid-cols-2 gap-3 sm:grid-cols-3">
    {#each vm.classResources as r (r.label)}
      <div class="rounded-md bg-slate-900/60 p-3 text-center">
        <dt class="text-[10px] uppercase tracking-wide text-slate-400">{r.label}</dt>
        <dd class="mt-1 text-lg font-bold text-white">{r.value}</dd>
      </div>
    {/each}
  </dl>
</Panel>
```

- [ ] **Step 4: InventoryPanel.svelte**

```svelte
<script lang="ts">
  import type { SheetViewModel } from '../viewmodel';
  import Panel from './Panel.svelte';

  let { vm }: { vm: SheetViewModel } = $props();
</script>

<div class="grid gap-3 sm:grid-cols-2">
  <Panel title="Magic Items" empty={vm.magicItems.length === 0} emptyText="No magic items.">
    <ul class="space-y-2">
      {#each vm.magicItems as m (m.name)}
        <li class="text-sm text-slate-200">
          <span class="font-semibold text-white">{m.name}</span>
          <span class="text-slate-400">{m.rarity}</span>
          {#if m.charges}<span class="text-slate-400"> · {m.charges.remaining}/{m.charges.max} charges</span>{/if}
          {#if m.isEquipped}<span class="text-green-400"> · equipped</span>{/if}
          <div class="text-xs text-slate-500">{m.effect}</div>
        </li>
      {/each}
    </ul>
  </Panel>

  <Panel title="Gear" empty={vm.gear.length === 0} emptyText="No gear.">
    <ul class="space-y-1">
      {#each vm.gear as g (g.name)}
        <li class="text-sm text-slate-200">
          <span class="font-semibold text-white">{g.name}</span>
          {#if g.quantity > 1}<span class="text-slate-400"> ×{g.quantity}</span>{/if}
        </li>
      {/each}
    </ul>
  </Panel>
</div>
```

- [ ] **Step 5: FeaturesPanel.svelte**

```svelte
<script lang="ts">
  import type { SheetViewModel } from '../viewmodel';
  import Panel from './Panel.svelte';

  let { vm }: { vm: SheetViewModel } = $props();
</script>

<Panel title="Features" empty={vm.features.length === 0} emptyText="No features.">
  <div class="space-y-4">
    {#each vm.features as group (group.level)}
      <div>
        <div class="mb-1 text-xs font-semibold text-sky-300">Level {group.level}</div>
        <ul class="space-y-2">
          {#each group.features as f (f.name)}
            <li class="text-sm text-slate-200">
              <span class="font-semibold text-white">{f.name}</span>
              {#if f.subclass}<span class="text-slate-400"> · {f.subclass}</span>{/if}
              {#if f.frequencyLimit}<span class="text-slate-500"> · {f.frequencyLimit}</span>{/if}
              <div class="text-xs text-slate-500">{f.description}</div>
              {#if f.choices.length > 0}<div class="text-xs text-sky-400">Chosen: {f.choices.join(', ')}</div>{/if}
            </li>
          {/each}
        </ul>
      </div>
    {/each}
  </div>
</Panel>
```

- [ ] **Step 6: Type-check**

Run: `npm run check`
Expected: 0 errors, 0 warnings.

- [ ] **Step 7: Commit**

```bash
git add src/lib/sheet/components/CombatPanel.svelte src/lib/sheet/components/MagicPanel.svelte src/lib/sheet/components/ClassResourcesPanel.svelte src/lib/sheet/components/InventoryPanel.svelte src/lib/sheet/components/FeaturesPanel.svelte
git commit -m "feat(client): add tab panel components"
```

---

## Task 10: Tabs + top-level HeroSheet

**Files:**
- Create: `src/lib/sheet/components/SheetTabs.svelte`
- Create: `src/lib/sheet/components/HeroSheet.svelte`

- [ ] **Step 1: SheetTabs.svelte**

A lightweight tab switcher using runes state (no external dependency). The active panel is selected by index with an `{#if}` chain — no snippets, so there is no hoisting concern.

```svelte
<script lang="ts">
  import type { SheetViewModel } from '../viewmodel';
  import CombatPanel from './CombatPanel.svelte';
  import MagicPanel from './MagicPanel.svelte';
  import ClassResourcesPanel from './ClassResourcesPanel.svelte';
  import InventoryPanel from './InventoryPanel.svelte';
  import FeaturesPanel from './FeaturesPanel.svelte';

  let { vm }: { vm: SheetViewModel } = $props();

  const tabs = ['Combat', 'Magic', 'Class Resources', 'Inventory', 'Features'];
  let active = $state(0);
</script>

<div>
  <div class="flex flex-wrap gap-1 border-b border-slate-700">
    {#each tabs as label, i (label)}
      <button
        type="button"
        class="border-b-2 px-3 py-2 text-xs sm:text-sm {active === i ? 'border-blue-500 font-semibold text-white' : 'border-transparent text-slate-400 hover:text-slate-200'}"
        onclick={() => (active = i)}
      >
        {label}
      </button>
    {/each}
  </div>
  <div class="pt-4">
    {#if active === 0}
      <CombatPanel {vm} />
    {:else if active === 1}
      <MagicPanel {vm} />
    {:else if active === 2}
      <ClassResourcesPanel {vm} />
    {:else if active === 3}
      <InventoryPanel {vm} />
    {:else}
      <FeaturesPanel {vm} />
    {/if}
  </div>
</div>
```

- [ ] **Step 2: HeroSheet.svelte**

```svelte
<script lang="ts">
  import type { SheetViewModel } from '../viewmodel';
  import HeroBanner from './HeroBanner.svelte';
  import VitalsRow from './VitalsRow.svelte';
  import StatRow from './StatRow.svelte';
  import SkillsRow from './SkillsRow.svelte';
  import SheetTabs from './SheetTabs.svelte';

  let { vm }: { vm: SheetViewModel } = $props();
</script>

<article class="mx-auto max-w-3xl overflow-hidden rounded-xl border border-slate-800 bg-slate-900 shadow-xl">
  <HeroBanner
    name={vm.name}
    ancestryName={vm.ancestryName}
    className={vm.className}
    level={vm.level}
    subclass={vm.subclass}
  />

  <div class="space-y-4 bg-slate-900 px-5 py-4">
    <VitalsRow {vm} />
    <StatRow stats={vm.stats} />
    <SkillsRow skills={vm.skills} />
  </div>

  <div class="bg-slate-900 px-5 pb-6">
    <SheetTabs {vm} />
  </div>
</article>
```

- [ ] **Step 3: Type-check**

Run: `npm run check`
Expected: 0 errors, 0 warnings.

- [ ] **Step 4: Commit**

```bash
git add src/lib/sheet/components/SheetTabs.svelte src/lib/sheet/components/HeroSheet.svelte
git commit -m "feat(client): add tabs and top-level HeroSheet"
```

---

## Task 11: `/sheet` route

**Files:**
- Create: `src/routes/sheet/+page.svelte`

- [ ] **Step 1: Write the route**

Create `src/routes/sheet/+page.svelte`. Loads the fixture, runs the resolver, renders the sheet on a dark page. The `dark` class is applied so any future Flowbite components render in dark mode.

```svelte
<script lang="ts">
  import { caldra, referenceData } from '$lib/fixtures/caldra';
  import { resolveSheet } from '$lib/sheet/resolve';
  import HeroSheet from '$lib/sheet/components/HeroSheet.svelte';

  const vm = resolveSheet(caldra, referenceData);
</script>

<svelte:head>
  <title>{vm.name} — NimbleSheets</title>
</svelte:head>

<div class="dark min-h-screen bg-slate-950 px-4 py-8">
  <HeroSheet {vm} />
</div>
```

- [ ] **Step 2: Type-check**

Run: `npm run check`
Expected: 0 errors, 0 warnings.

- [ ] **Step 3: Visual check in the dev server**

Run: `npm run dev`
Open the printed URL and navigate to `/sheet`. Verify against the spec:
- Banner shows "Caldra Brightward" and "Human · Oathsworn · Level 1".
- HP tile shows 17 (with "+0 temp · 17 max"); wound track shows 6 empty pips + skull; Armor 8; Init 0; Hit Dice d10 (1/1).
- Stats show 2 / 0 / -1 / 2 with `SAVE▲` (green) on STR and `SAVE▼` (red) on DEX.
- Skills row shows the ten values (Influence +4, Insight +4, Might/Naturecraft/Perception +2, Arcana/Examination/Lore -1, Finesse/Stealth 0).
- Combat tab: Mace + Rusty Mail + Wooden Buckler; Conditions panel shows empty state.
- Magic tab: "No spells known." Class Resources tab: Judgment Dice 2d6, Lay on Hands 5. Inventory tab: Magic Items empty, Gear shows Manacles. Features tab: Level 1 → Radiant Judgment, Lay on Hands.

Stop the dev server (Ctrl+C) when done.

- [ ] **Step 4: Commit**

```bash
git add src/routes/sheet/+page.svelte
git commit -m "feat(client): add /sheet route rendering the fixture hero"
```

---

## Task 12: Resolver unit tests (Vitest)

**Files:**
- Modify: `package.json`
- Create: `vitest.config.ts`
- Create: `src/lib/sheet/resolve.test.ts`

- [ ] **Step 1: Install Vitest**

Run: `npm install -D vitest`
Expected: `vitest` added to `devDependencies`.

- [ ] **Step 2: Add the `test` script**

In `package.json`, add a `test` script to the `scripts` object:

```json
		"test": "vitest run",
```

- [ ] **Step 3: Create the Vitest config**

Create `vitest.config.ts` at the `NS.Client/` root. It deliberately does **not** load the SvelteKit plugin (the resolver and fixture use relative imports, so no `$lib` alias or Svelte compilation is needed).

```ts
import { defineConfig } from 'vitest/config';

export default defineConfig({
  test: {
    include: ['src/**/*.test.ts'],
    environment: 'node'
  }
});
```

- [ ] **Step 4: Write the resolver tests**

Create `src/lib/sheet/resolve.test.ts`:

```ts
import { describe, expect, it } from 'vitest';
import { caldra, referenceData } from '../fixtures/caldra';
import type { Hero } from '../api/types';
import { resolveSheet } from './resolve';

describe('resolveSheet', () => {
  const vm = resolveSheet(caldra, referenceData);

  it('resolves identity fields', () => {
    expect(vm.name).toBe('Caldra Brightward');
    expect(vm.className).toBe('Oathsworn');
    expect(vm.ancestryName).toBe('Human');
    expect(vm.level).toBe(1);
  });

  it('derives save markers from HeroSaves', () => {
    const str = vm.stats.find((s) => s.type === 'Strength');
    const dex = vm.stats.find((s) => s.type === 'Dexterity');
    const int = vm.stats.find((s) => s.type === 'Intelligence');
    expect(str?.save).toBe('advantage');
    expect(dex?.save).toBe('disadvantage');
    expect(int?.save).toBe(null);
  });

  it('formats skill modifiers with sign rules', () => {
    const influence = vm.skills.find((s) => s.name === 'Influence');
    const lore = vm.skills.find((s) => s.name === 'Lore');
    const finesse = vm.skills.find((s) => s.name === 'Finesse');
    expect(influence?.display).toBe('+4');
    expect(lore?.display).toBe('-1');
    expect(finesse?.display).toBe('0');
  });

  it('formats hit die in lowercase', () => {
    expect(vm.hitDice.die).toBe('d10');
  });

  it('joins weapons to reference data', () => {
    expect(vm.weapons).toHaveLength(1);
    expect(vm.weapons[0].name).toBe('Mace');
    expect(vm.weapons[0].damage).toBe('1d6+2');
    expect(vm.weapons[0].statLabel).toBe('STR');
  });

  it('builds class resources, skipping mana for non-casters', () => {
    const labels = vm.classResources.map((r) => r.label);
    expect(labels).not.toContain('Mana');
    expect(vm.classResources).toContainEqual({ label: 'Judgment Dice', value: '2d6' });
    expect(vm.classResources).toContainEqual({ label: 'Lay on Hands', value: '5' });
  });

  it('groups features by level', () => {
    expect(vm.features).toHaveLength(1);
    expect(vm.features[0].level).toBe(1);
    expect(vm.features[0].features.map((f) => f.name)).toEqual(['Lay on Hands', 'Radiant Judgment']);
  });

  it('produces empty collections for absent sections', () => {
    expect(vm.spellsByTier).toEqual([]);
    expect(vm.magicItems).toEqual([]);
    expect(vm.conditions).toEqual([]);
  });

  it('falls back gracefully when a referenced entity is missing', () => {
    const heroWithBadWeapon: Hero = {
      ...caldra,
      weapons: [{ heroId: caldra.id, isEquipped: true, notes: null, weaponId: 'missing-id' }]
    };
    const result = resolveSheet(heroWithBadWeapon, referenceData);
    expect(result.weapons[0].name).toBe('Unknown weapon');
  });
});
```

- [ ] **Step 5: Run the tests**

Run: `npm test`
Expected: PASS — 1 file, 9 tests passing.

- [ ] **Step 6: Final gates**

Run: `npm run check`
Expected: 0 errors, 0 warnings.

Run: `npm run build`
Expected: build succeeds.

- [ ] **Step 7: Commit**

```bash
git add package.json package-lock.json vitest.config.ts src/lib/sheet/resolve.test.ts
git commit -m "test(client): add resolver unit tests with Vitest"
```

---

## Task 13: Update documentation

**Files:**
- Modify: `CLAUDE.md` (the `NS.Client` section)

- [ ] **Step 1: Document the sheet feature**

In `CLAUDE.md`, under the `## NS.Client` section, replace the line "Application features are TBD — only the scaffold exists so far." with a short description of the new structure:

```markdown
### Character Sheet (display-only)

The first feature: a read-only, dark-mode character sheet at route `/sheet`.

- **Data layer** (`src/lib/`): `api/types.ts` mirrors the API DTOs (camelCase; enums as string-union types matching the `JsonStringEnumConverter` names). `fixtures/caldra.ts` is a `Hero` + `ReferenceData` fixture shaped exactly like API responses. `sheet/resolve.ts` is a pure resolver joining the hero's ID-referenced collections to reference data into a `SheetViewModel` (`sheet/viewmodel.ts`); `sheet/format.ts` holds display helpers.
- **Components** (`src/lib/sheet/components/`): `HeroSheet` composes a pinned region (banner, vitals, stats with `SAVE▲/▼` save markers, skills) and a tab switcher (`SheetTabs`) over Combat / Magic / Class Resources / Inventory / Features panels. Always dark — styled with dark-tone Tailwind utilities directly, no `dark:` variants.
- **Tests**: `sheet/resolve.test.ts` (Vitest) covers the resolver; run with `npm test`.
- **Not yet wired**: live API calls (swap the fixture for `fetch()`), the HP damage/heal popover, and other play mutations are deferred to later slices. The sheet's eventual home is `/heroes/[id]` once auth/list exist.
```

- [ ] **Step 2: Commit**

```bash
git add CLAUDE.md
git commit -m "docs: document the character sheet UI in CLAUDE.md"
```

---

## Self-Review notes (addressed)

- **Spec coverage:** every spec section maps to a task — types (T1), fixture (T2/T5 data), resolver + helpers (T3–T5), all components incl. empty states (T6–T10), `/sheet` route + dark mode (T11), Vitest resolver tests (T12), docs (T13). The deferred HP popover is represented by the popover-ready `HpTile` (T8) and documented, not built.
- **Type consistency:** `resolveSheet(hero, reference)` signature, `SheetViewModel` field names, and the `vm` prop are identical across resolver, components, route, and tests. Hero collection JSON names (`activeConditions`, `knownSpells`, `weapons`, `armor`, `features`, `gear`, `magicItems`) match `Hero.cs`.
- **Fixture coverage:** populated panels = weapons, armor, features, class resources, gear; empty panels = spells, magic items, conditions — exercising both render paths.
- **Placeholders:** none — every code step contains complete content.
