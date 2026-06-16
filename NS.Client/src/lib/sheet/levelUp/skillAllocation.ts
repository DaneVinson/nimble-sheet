import type { HeroSkills } from '$lib/api/types';

/** The ten skills with display labels, in display order. */
export const SKILLS: { key: keyof HeroSkills; label: string }[] = [
	{ key: 'arcana', label: 'Arcana' },
	{ key: 'examination', label: 'Examination' },
	{ key: 'finesse', label: 'Finesse' },
	{ key: 'influence', label: 'Influence' },
	{ key: 'insight', label: 'Insight' },
	{ key: 'lore', label: 'Lore' },
	{ key: 'might', label: 'Might' },
	{ key: 'naturecraft', label: 'Naturecraft' },
	{ key: 'perception', label: 'Perception' },
	{ key: 'stealth', label: 'Stealth' }
];

/** Maximum bonus any single skill can reach. */
export const SKILL_CAP = 12;

/** Total points allocated from `start` to `working` (working is never below start, so this is >= 0). */
export function spentPoints(start: HeroSkills, working: HeroSkills): number {
	return SKILLS.reduce((sum, { key }) => sum + (working[key] - start[key]), 0);
}

/** A skill can be incremented when it is under the cap and budget remains. */
export function canIncrement(start: HeroSkills, working: HeroSkills, key: keyof HeroSkills, budget: number): boolean {
	return working[key] < SKILL_CAP && spentPoints(start, working) < budget;
}

/** A skill can be decremented when it is above its starting value. */
export function canDecrement(start: HeroSkills, working: HeroSkills, key: keyof HeroSkills): boolean {
	return working[key] > start[key];
}

/** Allocation can be finalized only when exactly the full budget has been spent. */
export function canFinalize(start: HeroSkills, working: HeroSkills, budget: number): boolean {
	return spentPoints(start, working) === budget;
}
