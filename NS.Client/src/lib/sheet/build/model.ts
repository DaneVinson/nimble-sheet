import type {
	ClassResources, Hero, HeroClass, HeroCombatStats, HeroSaves, HeroSkills, HeroStats
} from '$lib/api/types';

/** The client-side editable shape of a hero's build attributes (mirrors the API's HeroBuildRequest). */
export interface HeroBuildModel {
	name: string;
	ancestryId: string;
	backgroundId: string | null;
	heroClass: HeroClass;
	maxHp: number;
	maxMana: number | null;
	combatStats: HeroCombatStats;
	resources: ClassResources;
	saves: HeroSaves;
	skills: HeroSkills;
	stats: HeroStats;
}

/** A level-1 default build for the create form. */
export function blankBuildModel(): HeroBuildModel {
	return {
		name: '',
		ancestryId: '',
		backgroundId: null,
		heroClass: 'Berserker',
		maxHp: 1,
		maxMana: null,
		combatStats: { armor: 0, hitDieType: 'D8', initiativeBonus: 0, speed: 6 },
		resources: {
			judgmentDiceCount: null,
			judgmentDiceType: null,
			layOnHandsPool: null,
			thrillCharges: null
		},
		saves: { advantageOn: 'Strength', disadvantageOn: 'Dexterity' },
		skills: {
			arcana: 0, examination: 0, finesse: 0, influence: 0, insight: 0,
			lore: 0, might: 0, naturecraft: 0, perception: 0, stealth: 0
		},
		stats: { dexterity: 0, intelligence: 0, strength: 0, will: 0 }
	};
}

// Coerce an empty/NaN numeric input (Svelte binds a cleared number field to null) back to 0.
function coerceNumber(value: number): number {
	return Number.isFinite(value) ? value : 0;
}

/**
 * Coerce the required (non-nullable) numeric build fields — combat stats, stats, and skills — from
 * a cleared input's null/NaN back to 0 before submit. The server's int properties reject JSON null,
 * so this turns a blanked field into its neutral default instead of an opaque 400. Nullable fields
 * (maxMana, the class-resource pools) are intentionally left as-is.
 */
export function normalizeBuild(model: HeroBuildModel): HeroBuildModel {
	return {
		...model,
		combatStats: {
			...model.combatStats,
			armor: coerceNumber(model.combatStats.armor),
			initiativeBonus: coerceNumber(model.combatStats.initiativeBonus),
			speed: coerceNumber(model.combatStats.speed)
		},
		stats: {
			dexterity: coerceNumber(model.stats.dexterity),
			intelligence: coerceNumber(model.stats.intelligence),
			strength: coerceNumber(model.stats.strength),
			will: coerceNumber(model.stats.will)
		},
		skills: {
			arcana: coerceNumber(model.skills.arcana),
			examination: coerceNumber(model.skills.examination),
			finesse: coerceNumber(model.skills.finesse),
			influence: coerceNumber(model.skills.influence),
			insight: coerceNumber(model.skills.insight),
			lore: coerceNumber(model.skills.lore),
			might: coerceNumber(model.skills.might),
			naturecraft: coerceNumber(model.skills.naturecraft),
			perception: coerceNumber(model.skills.perception),
			stealth: coerceNumber(model.skills.stealth)
		}
	};
}

/** Map a loaded hero's build fields onto an editable model (independent nested copies) for the edit form. */
export function heroToBuildModel(hero: Hero): HeroBuildModel {
	return {
		name: hero.name,
		ancestryId: hero.ancestryId,
		backgroundId: hero.backgroundId,
		heroClass: hero.class,
		maxHp: hero.maxHp,
		maxMana: hero.maxMana,
		combatStats: { ...hero.combatStats },
		resources: { ...hero.resources },
		saves: { ...hero.saves },
		skills: { ...hero.skills },
		stats: { ...hero.stats }
	};
}
