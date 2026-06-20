import type { AbilityScores, DieType, HeroClass, StatType } from '$lib/api/types';

export interface ClassDef {
  casterFromLevel: number | null;
  hitDie: DieType;
  manaFormula: 'mageInt' | 'oathswornWil' | null;
  saveAdvantage: StatType;
  saveDisadvantage: StatType;
  speed: number;
  startingHp: number;
}

export const classDefs: Record<string, ClassDef> = {
  Cheat: { casterFromLevel: null, hitDie: 'D6', manaFormula: null, saveAdvantage: 'Dexterity', saveDisadvantage: 'Will', speed: 6, startingHp: 10 },
  Hunter: { casterFromLevel: null, hitDie: 'D8', manaFormula: null, saveAdvantage: 'Dexterity', saveDisadvantage: 'Intelligence', speed: 6, startingHp: 13 },
  Mage: { casterFromLevel: 1, hitDie: 'D6', manaFormula: 'mageInt', saveAdvantage: 'Intelligence', saveDisadvantage: 'Strength', speed: 6, startingHp: 10 },
  Oathsworn: { casterFromLevel: 2, hitDie: 'D10', manaFormula: 'oathswornWil', saveAdvantage: 'Strength', saveDisadvantage: 'Dexterity', speed: 6, startingHp: 17 }
};

export const playableClasses = Object.keys(classDefs) as HeroClass[];

const dieFace = (die: DieType): number => Number(die.slice(1));

export function abilityModifier(finalScore: number): number {
  return Math.floor((finalScore - 10) / 2);
}

export function finalScores(base: AbilityScores, bonuses: AbilityScores): AbilityScores {
  return {
    dexterity: base.dexterity + bonuses.dexterity,
    intelligence: base.intelligence + bonuses.intelligence,
    strength: base.strength + bonuses.strength,
    will: base.will + bonuses.will
  };
}

export function startingHp(heroClass: HeroClass): number {
  return classDefs[heroClass]?.startingHp ?? 0;
}

export function maxHpBounds(heroClass: HeroClass, level: number): { min: number; max: number } {
  const def = classDefs[heroClass];
  if (!def) return { min: 1, max: Number.MAX_SAFE_INTEGER };
  return { min: def.startingHp, max: def.startingHp + dieFace(def.hitDie) * (level - 1) };
}

export function previewMaxMana(heroClass: HeroClass, final: AbilityScores, level: number): number | null {
  const def = classDefs[heroClass];
  if (!def || def.casterFromLevel === null || level < def.casterFromLevel) return null;
  if (def.manaFormula === 'mageInt') return abilityModifier(final.intelligence) * 3 + level;
  if (def.manaFormula === 'oathswornWil') return abilityModifier(final.will) + level;
  return null;
}
