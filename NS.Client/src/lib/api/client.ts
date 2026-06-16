import { get } from 'svelte/store';
import { goto } from '$app/navigation';
import { session, clearSession } from '$lib/auth/session';
import type { Hero } from './types';
import type { HeroBuildModel } from '$lib/sheet/build/model';

/** Error thrown for any non-2xx API response. */
export class ApiError extends Error {
	status: number;
	constructor(status: number, message: string) {
		super(message);
		this.name = 'ApiError';
		this.status = status;
	}
}

/** Login / create-user response shapes. */
export interface LoginResult {
	token: string;
	userId: string;
}
export interface CreateUserResult {
	id: string;
}

// All API endpoints are served under "/api" so they never collide with SPA client
// routes (e.g. /heroes, /heroes/{id}). Wrappers below pass logical paths ("/heroes");
// apiFetch prepends the prefix.
const API_BASE = '/api';

async function apiFetch<T>(path: string, init: RequestInit = {}): Promise<T> {
	const current = get(session);
	const headers = new Headers(init.headers);
	if (current) {
		headers.set('Authorization', `Bearer ${current.token}`);
	}
	if (init.body !== undefined) {
		headers.set('Content-Type', 'application/json');
	}

	const response = await fetch(`${API_BASE}${path}`, { ...init, headers });

	if (response.status === 401) {
		clearSession();
		await goto('/login');
		throw new ApiError(401, 'Unauthorized');
	}

	if (!response.ok) {
		throw new ApiError(response.status, await readErrorMessage(response));
	}

	if (response.status === 204) {
		return undefined as T;
	}
	return (await response.json()) as T;
}

async function readErrorMessage(response: Response): Promise<string> {
	try {
		const body: unknown = await response.json();
		// FastEndpoints validation errors carry { errors: { field: [msgs] } } or { message }.
		if (body && typeof body === 'object') {
			const record = body as Record<string, unknown>;
			if (typeof record.message === 'string') return record.message;
			if (record.errors && typeof record.errors === 'object') {
				const first = Object.values(record.errors as Record<string, unknown[]>).flat()[0];
				if (typeof first === 'string') return first;
			}
		}
	} catch {
		// Non-JSON body.
	}
	return `Request failed with status ${response.status}`;
}

/** POST /users/login — name-only login. 401 if the name is unknown. */
export function login(name: string): Promise<LoginResult> {
	return apiFetch<LoginResult>('/users/login', {
		method: 'POST',
		body: JSON.stringify({ name })
	});
}

/** POST /users — create a user. */
export function createUser(name: string, email: string): Promise<CreateUserResult> {
	return apiFetch<CreateUserResult>('/users', {
		method: 'POST',
		body: JSON.stringify({ name, email })
	});
}

/** GET /heroes — heroes owned by the authenticated user. */
export function getHeroes(): Promise<Hero[]> {
	return apiFetch<Hero[]>('/heroes');
}

/** GET /heroes/{id} — a single owned hero; 404 if missing or not owned. */
export function getHero(id: string): Promise<Hero> {
	return apiFetch<Hero>(`/heroes/${id}`);
}

/** GET /reference/{resource} — a full reference collection. */
export function getReferenceCollection<T>(resource: string): Promise<T[]> {
	return apiFetch<T[]>(`/reference/${resource}`);
}

/** POST /heroes/{id}/take-damage — apply damage (temp HP absorbs first, server-side). */
export function takeDamage(heroId: string, amount: number): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/take-damage`, {
		method: 'POST',
		body: JSON.stringify({ amount })
	});
}

/** POST /heroes/{id}/heal — restore hit points. */
export function heal(heroId: string, amount: number): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/heal`, {
		method: 'POST',
		body: JSON.stringify({ amount })
	});
}

/** POST /heroes/{id}/grant-temp-hp — set temporary hit points (non-stacking, server-side). */
export function grantTempHp(heroId: string, amount: number): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/grant-temp-hp`, {
		method: 'POST',
		body: JSON.stringify({ amount })
	});
}

/** POST /heroes/{id}/gain-wound — add a wound. */
export function gainWound(heroId: string): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/gain-wound`, { method: 'POST' });
}

/** POST /heroes/{id}/heal-wound — remove a wound. */
export function healWound(heroId: string): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/heal-wound`, { method: 'POST' });
}

/** POST /heroes/{id}/spend-hit-dice — spend hit dice and apply the rolled healing. */
export function spendHitDice(heroId: string, count: number, healingAmount: number): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/spend-hit-dice`, {
		method: 'POST',
		body: JSON.stringify({ count, healingAmount })
	});
}

/** POST /heroes/{id}/spend-mana — spend mana. */
export function spendMana(heroId: string, amount: number): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/spend-mana`, {
		method: 'POST',
		body: JSON.stringify({ amount })
	});
}

/** POST /heroes/{id}/recover-all-resources — clear temp HP and restore resources (rest). */
export function recoverAll(heroId: string): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/recover-all-resources`, { method: 'POST' });
}

// --- collection mutations ---

/** POST /heroes/{id}/add-weapon — add a weapon from the reference catalog. */
export function addWeapon(heroId: string, weaponId: string, isEquipped: boolean, notes: string | null): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/add-weapon`, {
		method: 'POST',
		body: JSON.stringify({ weaponId, isEquipped, notes })
	});
}

/** POST /heroes/{id}/remove-weapon — remove a weapon by its reference id. */
export function removeWeapon(heroId: string, weaponId: string): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/remove-weapon`, {
		method: 'POST',
		body: JSON.stringify({ weaponId })
	});
}

/** POST /heroes/{id}/set-weapon-equipped — equip or unequip a weapon. */
export function setWeaponEquipped(heroId: string, weaponId: string, isEquipped: boolean): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/set-weapon-equipped`, {
		method: 'POST',
		body: JSON.stringify({ weaponId, isEquipped })
	});
}

/** POST /heroes/{id}/add-armor — add armor from the reference catalog. */
export function addArmor(heroId: string, armorId: string, isEquipped: boolean): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/add-armor`, {
		method: 'POST',
		body: JSON.stringify({ armorId, isEquipped })
	});
}

/** POST /heroes/{id}/remove-armor — remove armor by its reference id. */
export function removeArmor(heroId: string, armorId: string): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/remove-armor`, {
		method: 'POST',
		body: JSON.stringify({ armorId })
	});
}

/** POST /heroes/{id}/set-armor-equipped — equip or unequip armor. */
export function setArmorEquipped(heroId: string, armorId: string, isEquipped: boolean): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/set-armor-equipped`, {
		method: 'POST',
		body: JSON.stringify({ armorId, isEquipped })
	});
}

/** POST /heroes/{id}/add-magic-item — add a magic item from the reference catalog. */
export function addMagicItem(heroId: string, magicItemId: string, isEquipped: boolean, chargesRemaining: number | null): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/add-magic-item`, {
		method: 'POST',
		body: JSON.stringify({ magicItemId, isEquipped, chargesRemaining })
	});
}

/** POST /heroes/{id}/remove-magic-item — remove a magic item by its reference id. */
export function removeMagicItem(heroId: string, magicItemId: string): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/remove-magic-item`, {
		method: 'POST',
		body: JSON.stringify({ magicItemId })
	});
}

/** POST /heroes/{id}/set-magic-item-equipped — equip or unequip a magic item. */
export function setMagicItemEquipped(heroId: string, magicItemId: string, isEquipped: boolean): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/set-magic-item-equipped`, {
		method: 'POST',
		body: JSON.stringify({ magicItemId, isEquipped })
	});
}

/** POST /heroes/{id}/add-spell — learn a spell from the reference catalog. */
export function addSpell(heroId: string, spellId: string, tierUnlocked: number, notes: string | null): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/add-spell`, {
		method: 'POST',
		body: JSON.stringify({ spellId, tierUnlocked, notes })
	});
}

/** POST /heroes/{id}/remove-spell — forget a spell by its reference id. */
export function removeSpell(heroId: string, spellId: string): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/remove-spell`, {
		method: 'POST',
		body: JSON.stringify({ spellId })
	});
}

/** POST /heroes/{id}/add-condition — apply a condition from the reference catalog. */
export function addCondition(heroId: string, conditionId: string, expiresAtEndOf: string | null): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/add-condition`, {
		method: 'POST',
		body: JSON.stringify({ conditionId, expiresAtEndOf })
	});
}

/** POST /heroes/{id}/remove-condition — clear a condition by its reference id. */
export function removeCondition(heroId: string, conditionId: string): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/remove-condition`, {
		method: 'POST',
		body: JSON.stringify({ conditionId })
	});
}

/** POST /heroes/{id}/add-gear-item — add a free-text gear item. */
export function addGearItem(heroId: string, name: string, quantity: number): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/add-gear-item`, {
		method: 'POST',
		body: JSON.stringify({ name, quantity })
	});
}

/** POST /heroes/{id}/remove-gear-item — remove a gear item by name. */
export function removeGearItem(heroId: string, name: string): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/remove-gear-item`, {
		method: 'POST',
		body: JSON.stringify({ name })
	});
}

/** POST /heroes — create a hero from build attributes; returns the new id. */
export function createHero(build: HeroBuildModel): Promise<{ id: string }> {
	return apiFetch<{ id: string }>('/heroes', {
		method: 'POST',
		body: JSON.stringify(build)
	});
}

/** PUT /heroes/{id} — update a hero's build attributes. */
export function updateHero(id: string, build: HeroBuildModel): Promise<void> {
	return apiFetch<void>(`/heroes/${id}`, {
		method: 'PUT',
		body: JSON.stringify(build)
	});
}

/** POST /heroes/{id}/add-feature — grant a class feature with any selectable-option choices. */
export function addFeature(heroId: string, featureId: string, choices: string[], levelGained: number): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/add-feature`, {
		method: 'POST',
		body: JSON.stringify({ featureId, choices, levelGained })
	});
}

/** POST /heroes/{id}/remove-feature — remove a feature by its reference id. */
export function removeFeature(heroId: string, featureId: string): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/remove-feature`, {
		method: 'POST',
		body: JSON.stringify({ featureId })
	});
}
