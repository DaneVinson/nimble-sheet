import { getCollection } from '$lib/reference/cache';
import type { Ancestry, Background } from '$lib/api/types';

/** Load the reference collections the build form needs for its selects. */
export async function load() {
	const [ancestries, backgrounds] = await Promise.all([
		getCollection<Ancestry>('ancestries'),
		getCollection<Background>('backgrounds')
	]);
	return { ancestries, backgrounds };
}
