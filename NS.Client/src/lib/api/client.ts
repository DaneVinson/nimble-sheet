import { get } from 'svelte/store';
import { goto } from '$app/navigation';
import { session, clearSession } from '$lib/auth/session';
import type { Hero } from './types';

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

async function apiFetch<T>(path: string, init: RequestInit = {}): Promise<T> {
	const current = get(session);
	const headers = new Headers(init.headers);
	if (current) {
		headers.set('Authorization', `Bearer ${current.token}`);
	}
	if (init.body !== undefined) {
		headers.set('Content-Type', 'application/json');
	}

	const response = await fetch(path, { ...init, headers });

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
