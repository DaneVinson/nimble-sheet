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
