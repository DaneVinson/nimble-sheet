import { redirect } from '@sveltejs/kit';

/** Root sends users to the hero list; the (app) guard handles unauthenticated users. */
export function load() {
	throw redirect(302, '/heroes');
}
