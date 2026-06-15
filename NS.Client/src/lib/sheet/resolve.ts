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
        weaponId: w.weaponId,
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
