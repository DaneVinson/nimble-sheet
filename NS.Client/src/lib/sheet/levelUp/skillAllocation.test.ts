import { describe, expect, it } from 'vitest';
import type { HeroSkills } from '$lib/api/types';
import { SKILLS, SKILL_CAP, spentPoints, canIncrement, canDecrement, canFinalize } from './skillAllocation';

const base: HeroSkills = {
	arcana: 0, examination: 0, finesse: 0, influence: 0, insight: 0,
	lore: 0, might: 0, naturecraft: 0, perception: 0, stealth: 0
};

describe('skillAllocation', () => {
	it('lists all ten skills', () => {
		expect(SKILLS.length).toBe(10);
	});

	it('spentPoints sums the deltas from start to working', () => {
		const working = { ...base, might: 2, arcana: 1 };
		expect(spentPoints(base, working)).toBe(3);
	});

	it('canIncrement is false when the budget is exhausted', () => {
		const working = { ...base, might: 1 };
		expect(canIncrement(base, working, 'arcana', 1)).toBe(false);
		expect(canIncrement(base, base, 'arcana', 1)).toBe(true);
	});

	it('canIncrement is false at the skill cap', () => {
		const working = { ...base, might: SKILL_CAP };
		expect(canIncrement(base, working, 'might', 99)).toBe(false);
	});

	it('canDecrement is false at the starting value', () => {
		const start = { ...base, might: 3 };
		expect(canDecrement(start, { ...start }, 'might')).toBe(false);
		expect(canDecrement(start, { ...start, might: 4 }, 'might')).toBe(true);
	});

	it('canFinalize requires the full budget spent', () => {
		expect(canFinalize(base, { ...base, might: 1 }, 2)).toBe(false);
		expect(canFinalize(base, { ...base, might: 2 }, 2)).toBe(true);
	});
});
