import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { get } from 'svelte/store';

function memoryStorage(): Storage {
	const map = new Map<string, string>();
	return {
		getItem: (k) => map.get(k) ?? null,
		setItem: (k, v) => void map.set(k, v),
		removeItem: (k) => void map.delete(k),
		clear: () => map.clear(),
		key: () => null,
		get length() { return map.size; }
	} as Storage;
}

beforeEach(() => {
	vi.stubGlobal('localStorage', memoryStorage());
	vi.resetModules();
});
afterEach(() => vi.unstubAllGlobals());

describe('session store', () => {
	it('round-trips a session through localStorage', async () => {
		const { session, setSession, clearSession } = await import('./session');
		setSession({ name: 'Caldra', token: 't', userId: 'u' });
		expect(get(session)).toEqual({ name: 'Caldra', token: 't', userId: 'u' });
		expect(localStorage.getItem('ns.session')).toContain('"token":"t"');
		clearSession();
		expect(get(session)).toBeNull();
		expect(localStorage.getItem('ns.session')).toBeNull();
	});
});
