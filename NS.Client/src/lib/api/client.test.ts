import { afterEach, describe, expect, it, vi } from 'vitest';
// `$app/navigation` (goto) resolves to src/test/app-stub.ts via the Vitest alias (Step 0).
import { addArmor, addCondition, addFeature, addGearItem, addMagicItem, addSpell, addWeapon, ApiError, applyHpIncrease, applyStatIncrease, createHero, finalizeSkillAllocation, gainWound, getHeroes, levelUp, login, removeWeapon, setSubclass, setWeaponEquipped, spendHitDice, takeDamage, updateHero } from './client';
import { clearSession } from '$lib/auth/session';

afterEach(() => {
	vi.restoreAllMocks();
	clearSession();
});

function mockFetch(status: number, body: unknown) {
	vi.stubGlobal('fetch', vi.fn(() =>
		Promise.resolve(new Response(body === undefined ? null : JSON.stringify(body), { status }))
	));
}

describe('apiFetch via login', () => {
	it('returns parsed JSON on 200', async () => {
		mockFetch(200, { token: 't', userId: 'u' });
		await expect(login('Caldra')).resolves.toEqual({ token: 't', userId: 'u' });
	});

	it('throws ApiError with the validation message on 400', async () => {
		mockFetch(400, { errors: { name: ['Name is required'] } });
		await expect(login('')).rejects.toMatchObject({ status: 400, message: 'Name is required' });
	});
});

describe('getHeroes error mapping', () => {
	it('throws ApiError on 500', async () => {
		mockFetch(500, { message: 'boom' });
		await expect(getHeroes()).rejects.toBeInstanceOf(ApiError);
	});
});

function captureFetch(status = 204) {
	const fetchMock = vi.fn((_input: RequestInfo | URL, _init?: RequestInit) =>
		Promise.resolve(new Response(null, { status }))
	);
	vi.stubGlobal('fetch', fetchMock);
	return fetchMock;
}

describe('play-mutation wrappers', () => {
	it('takeDamage posts the amount to the take-damage route', async () => {
		const fetchMock = captureFetch(204);
		await takeDamage('h1', 5);
		expect(fetchMock).toHaveBeenCalledWith(
			'/api/heroes/h1/take-damage',
			expect.objectContaining({ method: 'POST', body: JSON.stringify({ amount: 5 }) })
		);
	});

	it('spendHitDice posts count and healingAmount', async () => {
		const fetchMock = captureFetch(204);
		await spendHitDice('h1', 2, 7);
		expect(fetchMock).toHaveBeenCalledWith(
			'/api/heroes/h1/spend-hit-dice',
			expect.objectContaining({ method: 'POST', body: JSON.stringify({ count: 2, healingAmount: 7 }) })
		);
	});

	it('gainWound posts an empty JSON body with a JSON content type and resolves on 204', async () => {
		const fetchMock = captureFetch(204);
		await expect(gainWound('h1')).resolves.toBeUndefined();
		const [path, init] = fetchMock.mock.calls[0] as [string, RequestInit];
		expect(path).toBe('/api/heroes/h1/gain-wound');
		expect(init.method).toBe('POST');
		// FastEndpoints rejects DTO-bound POSTs lacking a JSON content type with 415, even when
		// the request carries no payload (gain-wound binds HeroId from the route), so a no-body
		// mutation must still send application/json with an empty object body.
		expect(init.body).toBe('{}');
		expect(new Headers(init.headers).get('Content-Type')).toBe('application/json');
	});

	it('surfaces an ApiError on a 400', async () => {
		captureFetch(400);
		await expect(takeDamage('h1', 5)).rejects.toBeInstanceOf(ApiError);
	});
});

describe('collection wrappers', () => {
	it('addWeapon posts weaponId/isEquipped/notes', async () => {
		const fetchMock = captureFetch(204);
		await addWeapon('h1', 'w1', true, null);
		expect(fetchMock).toHaveBeenCalledWith(
			'/api/heroes/h1/add-weapon',
			expect.objectContaining({ method: 'POST', body: JSON.stringify({ weaponId: 'w1', isEquipped: true, notes: null }) })
		);
	});

	it('removeWeapon posts the weaponId', async () => {
		const fetchMock = captureFetch(204);
		await removeWeapon('h1', 'w1');
		expect(fetchMock).toHaveBeenCalledWith(
			'/api/heroes/h1/remove-weapon',
			expect.objectContaining({ method: 'POST', body: JSON.stringify({ weaponId: 'w1' }) })
		);
	});

	it('setWeaponEquipped posts weaponId/isEquipped', async () => {
		const fetchMock = captureFetch(204);
		await setWeaponEquipped('h1', 'w1', false);
		expect(fetchMock).toHaveBeenCalledWith(
			'/api/heroes/h1/set-weapon-equipped',
			expect.objectContaining({ method: 'POST', body: JSON.stringify({ weaponId: 'w1', isEquipped: false }) })
		);
	});

	it('addArmor posts armorId/isEquipped', async () => {
		const fetchMock = captureFetch(204);
		await addArmor('h1', 'a1', true);
		expect(fetchMock).toHaveBeenCalledWith(
			'/api/heroes/h1/add-armor',
			expect.objectContaining({ method: 'POST', body: JSON.stringify({ armorId: 'a1', isEquipped: true }) })
		);
	});

	it('addMagicItem posts magicItemId/isEquipped/chargesRemaining', async () => {
		const fetchMock = captureFetch(204);
		await addMagicItem('h1', 'm1', false, 3);
		expect(fetchMock).toHaveBeenCalledWith(
			'/api/heroes/h1/add-magic-item',
			expect.objectContaining({ method: 'POST', body: JSON.stringify({ magicItemId: 'm1', isEquipped: false, chargesRemaining: 3 }) })
		);
	});

	it('addSpell posts spellId/tierUnlocked/notes', async () => {
		const fetchMock = captureFetch(204);
		await addSpell('h1', 's1', 2, null);
		expect(fetchMock).toHaveBeenCalledWith(
			'/api/heroes/h1/add-spell',
			expect.objectContaining({ method: 'POST', body: JSON.stringify({ spellId: 's1', tierUnlocked: 2, notes: null }) })
		);
	});

	it('addGearItem posts name/quantity', async () => {
		const fetchMock = captureFetch(204);
		await addGearItem('h1', 'Torch', 5);
		expect(fetchMock).toHaveBeenCalledWith(
			'/api/heroes/h1/add-gear-item',
			expect.objectContaining({ method: 'POST', body: JSON.stringify({ name: 'Torch', quantity: 5 }) })
		);
	});

	it('addCondition posts conditionId/expiresAtEndOf', async () => {
		const fetchMock = captureFetch(204);
		await addCondition('h1', 'c1', null);
		expect(fetchMock).toHaveBeenCalledWith(
			'/api/heroes/h1/add-condition',
			expect.objectContaining({ method: 'POST', body: JSON.stringify({ conditionId: 'c1', expiresAtEndOf: null }) })
		);
	});

	it('addFeature posts featureId/choices/levelGained', async () => {
		const fetchMock = captureFetch(204);
		await addFeature('h1', 'f1', ['Option A'], 3);
		expect(fetchMock).toHaveBeenCalledWith(
			'/api/heroes/h1/add-feature',
			expect.objectContaining({ method: 'POST', body: JSON.stringify({ featureId: 'f1', choices: ['Option A'], levelGained: 3 }) })
		);
	});

	it('applyHpIncrease posts the amount', async () => {
		const fetchMock = captureFetch(204);
		await applyHpIncrease('h1', 5);
		expect(fetchMock).toHaveBeenCalledWith(
			'/api/heroes/h1/apply-hp-increase',
			expect.objectContaining({ method: 'POST', body: JSON.stringify({ amount: 5 }) })
		);
	});

	it('levelUp posts an empty pendingChoices list', async () => {
		const fetchMock = captureFetch(204);
		await levelUp('h1');
		expect(fetchMock).toHaveBeenCalledWith(
			'/api/heroes/h1/level-up',
			expect.objectContaining({ method: 'POST', body: JSON.stringify({ pendingChoices: [] }) })
		);
	});

	it('applyStatIncrease posts the stat name', async () => {
		const fetchMock = captureFetch(204);
		await applyStatIncrease('h1', 'Strength');
		expect(fetchMock).toHaveBeenCalledWith(
			'/api/heroes/h1/apply-stat-increase',
			expect.objectContaining({ method: 'POST', body: JSON.stringify({ stat: 'Strength' }) })
		);
	});

	it('finalizeSkillAllocation posts updatedSkills', async () => {
		const fetchMock = captureFetch(204);
		const skills = { arcana: 1, examination: 0, finesse: 0, influence: 0, insight: 0, lore: 0, might: 2, naturecraft: 0, perception: 0, stealth: 0 };
		await finalizeSkillAllocation('h1', skills);
		expect(fetchMock).toHaveBeenCalledWith(
			'/api/heroes/h1/finalize-skill-allocation',
			expect.objectContaining({ method: 'POST', body: JSON.stringify({ updatedSkills: skills }) })
		);
	});

	it('setSubclass posts the subclass name', async () => {
		const fetchMock = captureFetch(204);
		await setSubclass('h1', 'Ravager');
		expect(fetchMock).toHaveBeenCalledWith(
			'/api/heroes/h1/set-subclass',
			expect.objectContaining({ method: 'POST', body: JSON.stringify({ subclass: 'Ravager' }) })
		);
	});
});

describe('hero build wrappers', () => {
	it('createHero posts the create DTO (class + base scores, no maxHp)', async () => {
		const fetchMock = captureFetch(201);
		// Response body for 201 create:
		vi.stubGlobal('fetch', vi.fn(() =>
			Promise.resolve(new Response(JSON.stringify({ id: 'h1' }), { status: 201 }))));
		const model = {
			name: 'Caldra', ancestryId: 'a1', backgroundId: null,
			heroClass: 'Oathsworn' as const,
			baseAbilityScores: { dexterity: 10, intelligence: 10, strength: 14, will: 12 },
			maxHp: 0
		};
		await createHero(model);
		const [path, init] = (globalThis.fetch as unknown as { mock: { calls: [string, RequestInit][] } }).mock.calls[0];
		expect(path).toBe('/api/heroes');
		expect(init.method).toBe('POST');
		expect(JSON.parse(init.body as string)).toEqual({
			name: 'Caldra', ancestryId: 'a1', backgroundId: null,
			heroClass: 'Oathsworn',
			baseAbilityScores: { dexterity: 10, intelligence: 10, strength: 14, will: 12 }
		});
	});

	it('updateHero puts the update DTO (name/ancestry/background/maxHp only)', async () => {
		const fetchMock = captureFetch(204);
		const model = {
			name: 'Caldra', ancestryId: 'a2', backgroundId: 'b1',
			heroClass: 'Oathsworn' as const,
			baseAbilityScores: { dexterity: 10, intelligence: 10, strength: 14, will: 12 },
			maxHp: 25
		};
		await updateHero('h1', model);
		expect(fetchMock).toHaveBeenCalledWith(
			'/api/heroes/h1',
			expect.objectContaining({ method: 'PUT', body: JSON.stringify({ name: 'Caldra', ancestryId: 'a2', backgroundId: 'b1', maxHp: 25 }) })
		);
	});
});
