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
  baseAbilityScores: { dexterity: 10, intelligence: 8, strength: 14, will: 14 },
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
  userId,
  weapons: [{ heroId, isEquipped: true, notes: null, weaponId: maceId }]
};

export const referenceData: ReferenceData = {
  ancestries: [
    { abilityBonuses: { dexterity: 0, intelligence: 0, strength: 0, will: 0 }, id: ancestryHumanId, name: 'Human', description: 'Versatile and ambitious.', traits: ['Adaptable'] }
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
