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
