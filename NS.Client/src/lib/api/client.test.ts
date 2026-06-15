import { afterEach, describe, expect, it, vi } from 'vitest';
// `$app/navigation` (goto) resolves to src/test/app-stub.ts via the Vitest alias (Step 0).
import { addArmor, addWeapon, ApiError, createHero, gainWound, getHeroes, login, removeWeapon, setWeaponEquipped, spendHitDice, takeDamage, updateHero } from './client';
import { clearSession } from '$lib/auth/session';
import { blankBuildModel } from '$lib/sheet/build/model';

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

	it('gainWound posts with no body and resolves on 204', async () => {
		const fetchMock = captureFetch(204);
		await expect(gainWound('h1')).resolves.toBeUndefined();
		const [path, init] = fetchMock.mock.calls[0] as [string, RequestInit];
		expect(path).toBe('/api/heroes/h1/gain-wound');
		expect(init.method).toBe('POST');
		expect(init.body).toBeUndefined();
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
});

describe('hero build wrappers', () => {
	it('createHero posts the build and returns the new id', async () => {
		const fetchMock = vi.fn(() =>
			Promise.resolve(new Response(JSON.stringify({ id: 'h9' }), { status: 201 }))
		);
		vi.stubGlobal('fetch', fetchMock);
		const model = blankBuildModel();
		await expect(createHero(model)).resolves.toEqual({ id: 'h9' });
		expect(fetchMock).toHaveBeenCalledWith(
			'/api/heroes',
			expect.objectContaining({ method: 'POST', body: JSON.stringify(model) })
		);
	});

	it('updateHero PUTs to the hero route and resolves on 204', async () => {
		const fetchMock = captureFetch(204);
		const model = blankBuildModel();
		await expect(updateHero('h9', model)).resolves.toBeUndefined();
		expect(fetchMock).toHaveBeenCalledWith(
			'/api/heroes/h9',
			expect.objectContaining({ method: 'PUT', body: JSON.stringify(model) })
		);
	});
});
