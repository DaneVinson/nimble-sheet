import type { DieType, HeroClass, StatType } from '$lib/api/types';

/** All selectable hero classes, in domain order. */
export const heroClasses: HeroClass[] = [
	'Berserker', 'Cheat', 'Commander', 'Hunter', 'Mage', 'Oathsworn',
	'Shadowmancer', 'Shepherd', 'Songweaver', 'Stormshifter', 'Zephyr'
];

/** All hit die types. */
export const dieTypes: DieType[] = ['D4', 'D6', 'D8', 'D10', 'D12'];

/** All stat types (for saves). */
export const statTypes: StatType[] = ['Strength', 'Dexterity', 'Intelligence', 'Will'];
