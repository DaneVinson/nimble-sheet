import type { HeroBuildModel } from './model';

/** Field-keyed validation messages for the required build fields. */
export type BuildErrors = Partial<Record<'name' | 'ancestryId' | 'maxHp' | 'maxMana', string>>;

/** Validate the required build fields; everything else defers to the server. */
export function validateBuild(model: HeroBuildModel): BuildErrors {
	const errors: BuildErrors = {};
	if (model.name.trim() === '') {
		errors.name = 'Name is required.';
	}
	if (model.ancestryId === '') {
		errors.ancestryId = 'Select an ancestry.';
	}
	if (!(model.maxHp > 0)) {
		errors.maxHp = 'Max HP must be greater than 0.';
	}
	if (model.maxMana !== null && model.maxMana < 0) {
		errors.maxMana = 'Max mana cannot be negative.';
	}
	return errors;
}
