import type { AbilityScores, Hero, HeroClass } from '$lib/api/types';
import { POINT_BUY_MIN } from './pointBuy';
import { startingHp } from './classDefs';

/** The client-side editable shape of a hero's player-set build inputs. */
export interface HeroBuildModel {
  name: string;
  ancestryId: string;
  backgroundId: string | null;
  heroClass: HeroClass | '';
  baseAbilityScores: AbilityScores;
  maxHp: number;
}

/** A level-1 default build for the create form (class unset, all scores at the point-buy minimum). */
export function blankBuildModel(): HeroBuildModel {
  return {
    name: '',
    ancestryId: '',
    backgroundId: null,
    heroClass: '',
    baseAbilityScores: {
      dexterity: POINT_BUY_MIN,
      intelligence: POINT_BUY_MIN,
      strength: POINT_BUY_MIN,
      will: POINT_BUY_MIN
    },
    maxHp: 0
  };
}

/** Map a loaded hero onto an editable build model for the edit form. */
export function heroToBuildModel(hero: Hero): HeroBuildModel {
  return {
    name: hero.name,
    ancestryId: hero.ancestryId,
    backgroundId: hero.backgroundId,
    heroClass: hero.class,
    baseAbilityScores: { ...hero.baseAbilityScores },
    maxHp: hero.maxHp
  };
}

function coerceNumber(value: number): number {
  return Number.isFinite(value) ? value : 0;
}

/** Coerce cleared numeric inputs back to numbers before submit. */
export function normalizeBuild(model: HeroBuildModel): HeroBuildModel {
  return {
    ...model,
    maxHp: coerceNumber(model.maxHp),
    baseAbilityScores: {
      dexterity: coerceNumber(model.baseAbilityScores.dexterity),
      intelligence: coerceNumber(model.baseAbilityScores.intelligence),
      strength: coerceNumber(model.baseAbilityScores.strength),
      will: coerceNumber(model.baseAbilityScores.will)
    }
  };
}

/** The default Max HP shown for a chosen class at create (the class's starting HP). */
export function defaultMaxHpForClass(heroClass: HeroClass | ''): number {
  return heroClass === '' ? 0 : startingHp(heroClass);
}
