import { describe, expect, it } from 'vitest';
import { blankBuildModel, heroToBuildModel } from './model';
import { caldra } from '$lib/fixtures/caldra';

describe('blankBuildModel', () => {
	it('returns level-1 defaults with empty ancestry and no mana', () => {
		const m = blankBuildModel();
		expect(m.ancestryId).toBe('');
		expect(m.maxHp).toBe(1);
		expect(m.maxMana).toBeNull();
		expect(m.heroClass).toBe('Berserker');
		expect(m.combatStats.hitDieType).toBe('D8');
		expect(m.stats).toEqual({ dexterity: 0, intelligence: 0, strength: 0, will: 0 });
	});
});

describe('heroToBuildModel', () => {
	it('maps every build field from a hero', () => {
		const m = heroToBuildModel(caldra);
		expect(m.name).toBe(caldra.name);
		expect(m.ancestryId).toBe(caldra.ancestryId);
		expect(m.backgroundId).toBe(caldra.backgroundId);
		expect(m.heroClass).toBe(caldra.class);
		expect(m.maxHp).toBe(caldra.maxHp);
		expect(m.maxMana).toBe(caldra.maxMana);
		expect(m.combatStats).toEqual(caldra.combatStats);
		expect(m.resources).toEqual(caldra.resources);
		expect(m.saves).toEqual(caldra.saves);
		expect(m.skills).toEqual(caldra.skills);
		expect(m.stats).toEqual(caldra.stats);
	});

	it('produces independent nested copies', () => {
		const m = heroToBuildModel(caldra);
		m.stats.strength = 99;
		expect(caldra.stats.strength).not.toBe(99);
	});
});
