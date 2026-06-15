import { describe, expect, it } from 'vitest';
import { blankBuildModel, heroToBuildModel, hitDieFaceValue, normalizeBuild } from './model';
import { caldra } from '$lib/fixtures/caldra';

describe('hitDieFaceValue', () => {
	it('returns the numeric face value of a hit die', () => {
		expect(hitDieFaceValue('D4')).toBe(4);
		expect(hitDieFaceValue('D8')).toBe(8);
		expect(hitDieFaceValue('D12')).toBe(12);
	});
});

describe('blankBuildModel', () => {
	it('returns level-1 defaults with empty ancestry and no mana', () => {
		const m = blankBuildModel();
		expect(m.ancestryId).toBe('');
		expect(m.maxMana).toBeNull();
		expect(m.heroClass).toBe('Berserker');
		expect(m.combatStats.hitDieType).toBe('D8');
		expect(m.stats).toEqual({ dexterity: 0, intelligence: 0, strength: 0, will: 0 });
	});

	it('defaults maxHp to the hit die face value (D8 → 8), not 1', () => {
		const m = blankBuildModel();
		expect(m.maxHp).toBe(8);
		expect(m.maxHp).toBe(hitDieFaceValue(m.combatStats.hitDieType));
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

describe('normalizeBuild', () => {
	it('coerces cleared (null) required numerics to 0', () => {
		const model = blankBuildModel();
		// A cleared number input binds to null at runtime despite the number type.
		(model.stats as { strength: number | null }).strength = null;
		(model.skills as { arcana: number | null }).arcana = null;
		(model.combatStats as { speed: number | null }).speed = null;

		const normalized = normalizeBuild(model);

		expect(normalized.stats.strength).toBe(0);
		expect(normalized.skills.arcana).toBe(0);
		expect(normalized.combatStats.speed).toBe(0);
	});

	it('preserves valid numbers and leaves nullable fields untouched', () => {
		const model = { ...blankBuildModel(), maxMana: null };
		model.stats.will = 3;
		model.combatStats.armor = 5;
		model.resources.layOnHandsPool = null;

		const normalized = normalizeBuild(model);

		expect(normalized.stats.will).toBe(3);
		expect(normalized.combatStats.armor).toBe(5);
		expect(normalized.maxMana).toBeNull();
		expect(normalized.resources.layOnHandsPool).toBeNull();
	});
});
