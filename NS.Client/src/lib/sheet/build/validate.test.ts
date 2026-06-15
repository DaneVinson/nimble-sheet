import { describe, expect, it } from 'vitest';
import { validateBuild } from './validate';
import { blankBuildModel } from './model';

function valid() {
	return { ...blankBuildModel(), name: 'Caldra', ancestryId: 'a1', maxHp: 10 };
}

describe('validateBuild', () => {
	it('returns no errors for a complete model', () => {
		expect(validateBuild(valid())).toEqual({});
	});

	it('flags an empty/whitespace name', () => {
		expect(validateBuild({ ...valid(), name: '  ' }).name).toBeDefined();
	});

	it('flags a missing ancestry', () => {
		expect(validateBuild({ ...valid(), ancestryId: '' }).ancestryId).toBeDefined();
	});

	it('flags non-positive maxHp', () => {
		expect(validateBuild({ ...valid(), maxHp: 0 }).maxHp).toBeDefined();
	});

	it('flags negative maxMana but allows null', () => {
		expect(validateBuild({ ...valid(), maxMana: -1 }).maxMana).toBeDefined();
		expect(validateBuild({ ...valid(), maxMana: null }).maxMana).toBeUndefined();
	});
});
