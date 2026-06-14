import { redirect } from '@sveltejs/kit';
import { get } from 'svelte/store';
import { session } from '$lib/auth/session';

/** Guard: every page under (app) requires a session. */
export function load() {
	const current = get(session);
	if (!current) {
		throw redirect(302, '/login');
	}
	return { userId: current.userId };
}
