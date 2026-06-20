import type { HeroBuildModel } from './model';
import { POINT_BUY_BUDGET, POINT_BUY_MAX, POINT_BUY_MIN, totalCost } from './pointBuy';
import { maxHpBounds, playableClasses } from './classDefs';

/** Field-keyed validation messages for the build form. */
export type BuildErrors = Partial<Record<'name' | 'ancestryId' | 'heroClass' | 'baseAbilityScores' | 'maxHp', string>>;

/** Validate the build model. The server remains authoritative. */
export function validateBuild(model: HeroBuildModel, opts: { mode: 'create' | 'edit'; level: number }): BuildErrors {
  const errors: BuildErrors = {};

  if (model.name.trim() === '') {
    errors.name = 'Name is required.';
  }
  if (model.ancestryId === '') {
    errors.ancestryId = 'Select an ancestry.';
  }

  if (opts.mode === 'create') {
    if (model.heroClass === '' || !playableClasses.includes(model.heroClass)) {
      errors.heroClass = 'Select a class.';
    }
    const scores = model.baseAbilityScores;
    const inRange = [scores.dexterity, scores.intelligence, scores.strength, scores.will]
      .every((s) => s >= POINT_BUY_MIN && s <= POINT_BUY_MAX);
    if (!inRange) {
      errors.baseAbilityScores = `Each ability must be between ${POINT_BUY_MIN} and ${POINT_BUY_MAX}.`;
    } else if (totalCost(scores) > POINT_BUY_BUDGET) {
      errors.baseAbilityScores = `Ability scores cost more than ${POINT_BUY_BUDGET} points.`;
    }
  } else {
    if (model.heroClass !== '') {
      const { min, max } = maxHpBounds(model.heroClass, opts.level);
      if (model.maxHp < min || model.maxHp > max) {
        errors.maxHp = `Max HP must be between ${min} and ${max}.`;
      }
    }
  }

  return errors;
}
