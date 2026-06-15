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
  weaponId: string;
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
  armorId: string;
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
  spellId: string;
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
  magicItemId: string;
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
