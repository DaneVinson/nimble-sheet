import { afterEach, describe, expect, it, vi } from 'vitest';
import { caldra } from '../fixtures/caldra';
import type { Hero } from '../api/types';

// vi.mock is hoisted — the factory must not reference outer variables.
// We access the mock spy after import via vi.mocked().
vi.mock('$lib/api/client', () => ({
	getReferenceCollection: vi.fn()
}));

import { getReferenceCollection } from '$lib/api/client';
import {
	assembleReferenceData, clearReferenceCache, neededResources
} from './cache';

const mockGetReferenceCollection = vi.mocked(getReferenceCollection);

afterEach(() => {
	clearReferenceCache();
	mockGetReferenceCollection.mockReset();
});

describe('neededResources', () => {
	it('always includes ancestries', () => {
		const empty = { ...caldra, backgroundId: null, armor: [], weapons: [],
			activeConditions: [], features: [], magicItems: [], knownSpells: [] } as Hero;
		expect(neededResources(empty)).toEqual(['ancestries']);
	});

	it('includes a collection only when the hero references it', () => {
		const needed = neededResources(caldra);
		expect(needed).toContain('ancestries');
		expect(needed).toContain('weapons'); // Caldra has a mace
		expect(needed).not.toContain('spells'); // Oathsworn fixture has no spells
	});
});

describe('assembleReferenceData', () => {
	it('fetches only needed collections and fills the rest with []', async () => {
		mockGetReferenceCollection.mockImplementation((r: string) => Promise.resolve([{ id: `x-${r}` }]));
		const empty = { ...caldra, backgroundId: null, armor: [], weapons: [],
			activeConditions: [], features: [], magicItems: [], knownSpells: [] } as Hero;

		const refs = await assembleReferenceData(empty);

		expect(mockGetReferenceCollection).toHaveBeenCalledTimes(1);
		expect(mockGetReferenceCollection).toHaveBeenCalledWith('ancestries');
		expect(refs.ancestries).toHaveLength(1);
		expect(refs.spells).toEqual([]);
		expect(refs.weapons).toEqual([]);
	});

	it('caches collections across calls', async () => {
		mockGetReferenceCollection.mockResolvedValue([{ id: 'a' }]);
		await assembleReferenceData(caldra);
		await assembleReferenceData(caldra);
		const ancestryCalls = mockGetReferenceCollection.mock.calls.filter((c) => c[0] === 'ancestries');
		expect(ancestryCalls).toHaveLength(1);
	});
});
