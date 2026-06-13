import { writable } from 'svelte/store';

const STORAGE_KEY = 'ns.session';

/** The authenticated session: a JWT plus the owning user's id. */
export interface Session {
	token: string;
	userId: string;
}

function readStorage(): Session | null {
	if (typeof localStorage === 'undefined') return null;
	const raw = localStorage.getItem(STORAGE_KEY);
	if (!raw) return null;
	try {
		const parsed = JSON.parse(raw) as Partial<Session>;
		if (typeof parsed.token === 'string' && typeof parsed.userId === 'string') {
			return { token: parsed.token, userId: parsed.userId };
		}
	} catch {
		// Corrupt value — fall through and treat as no session.
	}
	return null;
}

/** Current session, hydrated from localStorage on load. `null` when logged out. */
export const session = writable<Session | null>(readStorage());

/** Persist a session and update the store (called after login). */
export function setSession(value: Session): void {
	if (typeof localStorage !== 'undefined') {
		localStorage.setItem(STORAGE_KEY, JSON.stringify(value));
	}
	session.set(value);
}

/** Clear the session from the store and localStorage (logout / 401). */
export function clearSession(): void {
	if (typeof localStorage !== 'undefined') {
		localStorage.removeItem(STORAGE_KEY);
	}
	session.set(null);
}
