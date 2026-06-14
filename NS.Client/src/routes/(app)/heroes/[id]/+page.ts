import { error } from '@sveltejs/kit';
import { getHero, ApiError } from '$lib/api/client';
import { assembleReferenceData } from '$lib/reference/cache';
import { resolveSheet } from '$lib/sheet/resolve';

/** Load a hero, its reference data, and resolve the sheet view-model. */
export async function load({ params }: { params: { id: string } }) {
	try {
		const hero = await getHero(params.id);
		const reference = await assembleReferenceData(hero);
		return { vm: resolveSheet(hero, reference), heroId: hero.id };
	} catch (e) {
		if (e instanceof ApiError && e.status === 404) {
			throw error(404, 'Hero not found');
		}
		throw e;
	}
}
