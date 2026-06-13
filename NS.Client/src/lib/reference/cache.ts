import { getReferenceCollection } from '$lib/api/client';
import type {
	Ancestry, Armor, Background, Condition, Feature, Hero, MagicItem,
	ReferenceData, Spell, Weapon
} from '$lib/api/types';

/** Reference collection route segment. */
export type ReferenceResource =
	| 'ancestries' | 'armor' | 'backgrounds' | 'conditions'
	| 'features' | 'magic-items' | 'spells' | 'weapons';

const cache = new Map<ReferenceResource, Promise<unknown[]>>();

/** Fetch a reference collection, caching the in-flight/resolved promise for the session. */
export function getCollection<T>(resource: ReferenceResource): Promise<T[]> {
	let entry = cache.get(resource);
	if (!entry) {
		entry = getReferenceCollection<T>(resource);
		cache.set(resource, entry);
	}
	return entry as Promise<T[]>;
}

/** Reset the cache — used by tests. */
export function clearReferenceCache(): void {
	cache.clear();
}

/** The reference resources a hero actually references (ancestries always). */
export function neededResources(hero: Hero): ReferenceResource[] {
	const needed: ReferenceResource[] = ['ancestries'];
	if (hero.backgroundId) needed.push('backgrounds');
	if (hero.armor.length) needed.push('armor');
	if (hero.weapons.length) needed.push('weapons');
	if (hero.activeConditions.length) needed.push('conditions');
	if (hero.features.length) needed.push('features');
	if (hero.magicItems.length) needed.push('magic-items');
	if (hero.knownSpells.length) needed.push('spells');
	return needed;
}

/**
 * Build the ReferenceData bundle a hero needs: fetch (or reuse cached) only the
 * collections it references; unused collections come back as empty arrays.
 */
export async function assembleReferenceData(hero: Hero): Promise<ReferenceData> {
	const needed = new Set(neededResources(hero));
	const fetchIf = <T>(resource: ReferenceResource): Promise<T[]> =>
		needed.has(resource) ? getCollection<T>(resource) : Promise.resolve([]);

	const [
		ancestries, backgrounds, armor, weapons, conditions, features, magicItems, spells
	] = await Promise.all([
		fetchIf<Ancestry>('ancestries'),
		fetchIf<Background>('backgrounds'),
		fetchIf<Armor>('armor'),
		fetchIf<Weapon>('weapons'),
		fetchIf<Condition>('conditions'),
		fetchIf<Feature>('features'),
		fetchIf<MagicItem>('magic-items'),
		fetchIf<Spell>('spells')
	]);

	return { ancestries, backgrounds, armor, weapons, conditions, features, magicItems, spells };
}
