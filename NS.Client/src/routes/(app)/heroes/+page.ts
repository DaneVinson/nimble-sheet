import { getHeroes } from '$lib/api/client';

/** Load the authenticated user's heroes. */
export async function load() {
	const heroes = await getHeroes();
	return { heroes };
}
