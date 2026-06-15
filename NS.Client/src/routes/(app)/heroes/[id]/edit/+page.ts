import { error } from '@sveltejs/kit';
import { getHero, ApiError } from '$lib/api/client';
import { getCollection } from '$lib/reference/cache';
import type { Ancestry, Background } from '$lib/api/types';

/** Load the hero to edit plus the reference collections for the selects. */
export async function load({ params }: { params: { id: string } }) {
	try {
		const [hero, ancestries, backgrounds] = await Promise.all([
			getHero(params.id),
			getCollection<Ancestry>('ancestries'),
			getCollection<Background>('backgrounds')
		]);
		return { hero, ancestries, backgrounds };
	} catch (e) {
		if (e instanceof ApiError && e.status === 404) {
			throw error(404, 'Hero not found');
		}
		throw e;
	}
}
