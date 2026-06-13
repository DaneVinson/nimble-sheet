# Live API Integration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the character-sheet fixture with live API data behind a login/create-user flow, a guarded hero list, and a `/heroes/[id]` detail page — reusing the existing resolver and `HeroSheet` components unchanged.

**Architecture:** SvelteKit SPA (SSR off). Client-side `load` functions fetch through a shared API client (`$lib/api/client.ts`) that attaches a localStorage-backed JWT and centralises 401 handling. A guarded `(app)` route group holds the hero pages; reference data is fetched lazily per collection and cached for the session. Spec: `docs/superpowers/specs/2026-06-13-live-api-integration-design.md`.

**Tech Stack:** SvelteKit 2.x / Svelte 5 (runes), TypeScript, Vite 8, Flowbite Svelte, Vitest.

**Project convention:** This project writes **tests after implementation** (not TDD). Each feature task implements and is hand-verified with `npm run check`; automated unit tests for the new pure logic are added in Task 9.

---

## File Structure

**Create:**
- `NS.Client/src/lib/auth/session.ts` — JWT session store (localStorage-backed).
- `NS.Client/src/lib/api/client.ts` — `apiFetch`, `ApiError`, typed endpoint wrappers.
- `NS.Client/src/lib/reference/cache.ts` — `neededResources`, `getCollection`, `assembleReferenceData`.
- `NS.Client/src/routes/login/+page.svelte` — login / create-account page.
- `NS.Client/src/routes/(app)/+layout.ts` — auth guard.
- `NS.Client/src/routes/(app)/+layout.svelte` — top bar (user name + logout).
- `NS.Client/src/routes/(app)/heroes/+page.ts` — load hero list.
- `NS.Client/src/routes/(app)/heroes/+page.svelte` — hero list UI.
- `NS.Client/src/routes/(app)/heroes/[id]/+page.ts` — load hero + reference + resolve.
- `NS.Client/src/routes/(app)/heroes/[id]/+page.svelte` — render `HeroSheet`.
- `NS.Client/src/routes/(app)/heroes/[id]/+error.svelte` — not-found / error boundary.
- `NS.Client/src/test/app-stub.ts` — Vitest stub for `$app/*` virtual modules (Task 10).
- `NS.Client/src/lib/auth/session.test.ts`, `src/lib/api/client.test.ts`, `src/lib/reference/cache.test.ts` — unit tests (Task 10).

**Modify:**
- `NS.Client/vite.config.ts` — dev proxy.
- `NS.Client/vitest.config.ts` — `$lib` + `$app/*` aliases for tests (Task 10).
- `NS.Client/src/routes/+page.svelte` → replaced by `NS.Client/src/routes/+page.ts` (root redirect to `/heroes`).

**Delete:**
- `NS.Client/src/routes/sheet/+page.svelte` (and the now-empty `sheet/` folder).

---

### Task 1: Vite dev proxy

So `npm run dev` (Vite, port 5173) reaches the API on `http://localhost:5197`.

**Files:**
- Modify: `NS.Client/vite.config.ts`

- [ ] **Step 1: Add the proxy**

Replace the file contents with:

```ts
import { sveltekit } from '@sveltejs/kit/vite';
import tailwindcss from '@tailwindcss/vite';
import { defineConfig } from 'vite';

// API routes are unprefixed (/heroes, /users, /reference). In dev, Vite serves the
// SPA on its own port, so proxy the API routes to the NS.WebApp HTTP endpoint.
// Plain HTTP (5197) avoids the self-signed HTTPS cert. Production is same-origin
// (NS.WebApp serves the built SPA) and never hits this proxy.
const API_TARGET = 'http://localhost:5197';

export default defineConfig({
	plugins: [tailwindcss(), sveltekit()],
	server: {
		proxy: {
			'/heroes': API_TARGET,
			'/users': API_TARGET,
			'/reference': API_TARGET
		}
	}
});
```

- [ ] **Step 2: Verify type-check passes**

Run: `npm run check`
Expected: `0 errors and 0 warnings`.

- [ ] **Step 3: Commit**

```bash
git add NS.Client/vite.config.ts
git commit -m "feat(client): proxy API routes to NS.WebApp in dev"
```

---

### Task 2: Session store

localStorage-backed JWT session. Reads/writes through a guarded `localStorage` so it is safe in non-browser (test) environments.

**Files:**
- Create: `NS.Client/src/lib/auth/session.ts`

- [ ] **Step 1: Write the store**

```ts
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
```

- [ ] **Step 2: Verify type-check passes**

Run: `npm run check`
Expected: `0 errors and 0 warnings`.

- [ ] **Step 3: Commit**

```bash
git add NS.Client/src/lib/auth/session.ts
git commit -m "feat(client): add localStorage-backed session store"
```

---

### Task 3: API client

Typed fetch wrapper: attaches the bearer token, throws `ApiError` on non-2xx, clears session + redirects on 401.

**Files:**
- Create: `NS.Client/src/lib/api/client.ts`

- [ ] **Step 1: Write the client**

```ts
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
		const body = await response.json();
		// FastEndpoints validation errors carry { errors: { field: [msgs] } } or { message }.
		if (body && typeof body === 'object') {
			if (typeof body.message === 'string') return body.message;
			if (body.errors && typeof body.errors === 'object') {
				const first = Object.values(body.errors).flat()[0];
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
```

- [ ] **Step 2: Verify type-check passes**

Run: `npm run check`
Expected: `0 errors and 0 warnings`. (If `body.message`/`body.errors` raise implicit-any errors, the `body && typeof body === 'object'` guard plus the `as` casts already present should satisfy strict mode; if not, type `body` as `Record<string, unknown>`.)

- [ ] **Step 3: Commit**

```bash
git add NS.Client/src/lib/api/client.ts
git commit -m "feat(client): add API client with bearer auth and 401 handling"
```

---

### Task 4: Reference cache + assembly

Fetch only the reference collections a hero references, cache each for the session, and assemble a full `ReferenceData` bundle for the resolver. `neededResources` is a pure function (unit-tested in Task 9).

**Files:**
- Create: `NS.Client/src/lib/reference/cache.ts`

- [ ] **Step 1: Write the cache + assembly**

```ts
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
```

- [ ] **Step 2: Verify type-check passes**

Run: `npm run check`
Expected: `0 errors and 0 warnings`.

- [ ] **Step 3: Commit**

```bash
git add NS.Client/src/lib/reference/cache.ts
git commit -m "feat(client): add lazy per-collection reference cache and assembly"
```

---

### Task 5: Login / create-account page

**Files:**
- Create: `NS.Client/src/routes/login/+page.svelte`

- [ ] **Step 1: Write the page**

```svelte
<script lang="ts">
	import { goto } from '$app/navigation';
	import { Button, Card, Input, Label } from 'flowbite-svelte';
	import { ApiError, createUser, login } from '$lib/api/client';
	import { setSession } from '$lib/auth/session';

	let mode = $state<'login' | 'create'>('login');
	let name = $state('');
	let email = $state('');
	let error = $state<string | null>(null);
	let busy = $state(false);

	async function submit() {
		error = null;
		busy = true;
		try {
			if (mode === 'create') {
				await createUser(name, email);
			}
			const result = await login(name);
			setSession({ token: result.token, userId: result.userId });
			await goto('/heroes');
		} catch (e) {
			error =
				e instanceof ApiError && e.status === 401
					? 'No user found with that name.'
					: e instanceof ApiError
						? e.message
						: 'Something went wrong. Please try again.';
		} finally {
			busy = false;
		}
	}
</script>

<svelte:head><title>Sign in — NimbleSheets</title></svelte:head>

<div class="dark flex min-h-screen items-center justify-center bg-slate-950 px-4">
	<Card class="w-full max-w-sm border-slate-800 bg-slate-900 p-6">
		<h1 class="mb-1 text-2xl font-bold text-white">NimbleSheets</h1>
		<p class="mb-4 text-sm text-slate-400">
			{mode === 'login' ? 'Sign in with your name.' : 'Create an account.'}
		</p>

		<form onsubmit={(e) => { e.preventDefault(); submit(); }} class="flex flex-col gap-4">
			<div>
				<Label for="name" class="mb-1 text-slate-300">Name</Label>
				<Input id="name" bind:value={name} required placeholder="Your name" />
			</div>

			{#if mode === 'create'}
				<div>
					<Label for="email" class="mb-1 text-slate-300">Email</Label>
					<Input id="email" type="email" bind:value={email} required placeholder="you@example.com" />
				</div>
			{/if}

			{#if error}
				<p class="text-sm text-red-400">{error}</p>
			{/if}

			<Button type="submit" color="primary" disabled={busy}>
				{mode === 'login' ? 'Log in' : 'Create account & log in'}
			</Button>
		</form>

		<button
			type="button"
			class="mt-4 text-sm text-slate-400 underline hover:text-slate-200"
			onclick={() => { mode = mode === 'login' ? 'create' : 'login'; error = null; }}
		>
			{mode === 'login' ? 'Need an account? Create one' : 'Already have an account? Log in'}
		</button>
	</Card>
</div>
```

- [ ] **Step 2: Verify type-check passes**

Run: `npm run check`
Expected: `0 errors and 0 warnings`.

- [ ] **Step 3: Commit**

```bash
git add NS.Client/src/routes/login/+page.svelte
git commit -m "feat(client): add login and create-account page"
```

---

### Task 6: Guarded (app) route group

A layout `load` that redirects unauthenticated users to `/login`, plus a top bar with the user name and a logout button.

**Files:**
- Create: `NS.Client/src/routes/(app)/+layout.ts`
- Create: `NS.Client/src/routes/(app)/+layout.svelte`

- [ ] **Step 1: Write the guard load**

`NS.Client/src/routes/(app)/+layout.ts`:

```ts
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
```

- [ ] **Step 2: Write the layout shell**

`NS.Client/src/routes/(app)/+layout.svelte`:

```svelte
<script lang="ts">
	import { goto } from '$app/navigation';
	import { navigating } from '$app/stores';
	import { clearSession } from '$lib/auth/session';

	let { children } = $props();

	function logout() {
		clearSession();
		goto('/login');
	}
</script>

<div class="dark min-h-screen bg-slate-950 text-slate-200">
	{#if $navigating}
		<div class="fixed inset-x-0 top-0 z-50 h-0.5 animate-pulse bg-blue-500"></div>
	{/if}
	<header class="flex items-center justify-between border-b border-slate-800 px-4 py-3">
		<a href="/heroes" class="text-lg font-bold text-white">NimbleSheets</a>
		<button
			type="button"
			class="text-sm text-slate-400 underline hover:text-slate-200"
			onclick={logout}
		>
			Log out
		</button>
	</header>
	<main>
		{@render children()}
	</main>
</div>
```

This satisfies the spec's `$navigating`-driven loading indicator (a thin top bar during route transitions under `(app)`).

- [ ] **Step 3: Verify type-check passes**

Run: `npm run check`
Expected: `0 errors and 0 warnings`.

- [ ] **Step 4: Commit**

```bash
git add "NS.Client/src/routes/(app)/+layout.ts" "NS.Client/src/routes/(app)/+layout.svelte"
git commit -m "feat(client): add guarded (app) layout with auth redirect and logout"
```

---

### Task 7: Root redirect

Replace the scaffold landing page with a redirect to `/heroes` (the guard sends unauthenticated users on to `/login`).

**Files:**
- Delete: `NS.Client/src/routes/+page.svelte`
- Create: `NS.Client/src/routes/+page.ts`

- [ ] **Step 1: Remove the scaffold page and add the redirect**

Delete `NS.Client/src/routes/+page.svelte`, then create `NS.Client/src/routes/+page.ts`:

```ts
import { redirect } from '@sveltejs/kit';

/** Root sends users to the hero list; the (app) guard handles unauthenticated users. */
export function load() {
	throw redirect(302, '/heroes');
}
```

- [ ] **Step 2: Verify type-check passes**

Run: `npm run check`
Expected: `0 errors and 0 warnings`.

- [ ] **Step 3: Commit**

```bash
git add NS.Client/src/routes/+page.ts
git rm NS.Client/src/routes/+page.svelte
git commit -m "feat(client): redirect root to /heroes"
```

---

### Task 8: Hero list page

**Files:**
- Create: `NS.Client/src/routes/(app)/heroes/+page.ts`
- Create: `NS.Client/src/routes/(app)/heroes/+page.svelte`

- [ ] **Step 1: Write the load**

`NS.Client/src/routes/(app)/heroes/+page.ts`:

```ts
import { getHeroes } from '$lib/api/client';

/** Load the authenticated user's heroes. */
export async function load() {
	const heroes = await getHeroes();
	return { heroes };
}
```

- [ ] **Step 2: Write the list UI**

`NS.Client/src/routes/(app)/heroes/+page.svelte`:

```svelte
<script lang="ts">
	import { Card } from 'flowbite-svelte';

	let { data } = $props();
</script>

<svelte:head><title>Heroes — NimbleSheets</title></svelte:head>

<div class="mx-auto max-w-3xl px-4 py-8">
	<h1 class="mb-6 text-2xl font-bold text-white">Your Heroes</h1>

	{#if data.heroes.length === 0}
		<p class="text-slate-400">You don't have any heroes yet.</p>
	{:else}
		<ul class="flex flex-col gap-3">
			{#each data.heroes as hero (hero.id)}
				<li>
					<a href={`/heroes/${hero.id}`} class="block">
						<Card class="border-slate-800 bg-slate-900 p-4 hover:border-slate-600">
							<div class="flex items-baseline justify-between">
								<span class="text-lg font-semibold text-white">{hero.name}</span>
								<span class="text-sm text-slate-400">Level {hero.level}</span>
							</div>
							<div class="mt-1 text-sm text-slate-400">
								{hero.class}{hero.subclass ? ` · ${hero.subclass}` : ''} · HP {hero.currentHp}/{hero.maxHp}
							</div>
						</Card>
					</a>
				</li>
			{/each}
		</ul>
	{/if}
</div>
```

- [ ] **Step 3: Verify type-check passes**

Run: `npm run check`
Expected: `0 errors and 0 warnings`.

- [ ] **Step 4: Commit**

```bash
git add "NS.Client/src/routes/(app)/heroes/+page.ts" "NS.Client/src/routes/(app)/heroes/+page.svelte"
git commit -m "feat(client): add hero list page"
```

---

### Task 9: Hero detail page + remove /sheet

Fetch the hero, assemble its reference data, resolve, and render the existing `HeroSheet`. Then delete the superseded `/sheet` fixture route.

**Files:**
- Create: `NS.Client/src/routes/(app)/heroes/[id]/+page.ts`
- Create: `NS.Client/src/routes/(app)/heroes/[id]/+page.svelte`
- Create: `NS.Client/src/routes/(app)/heroes/[id]/+error.svelte`
- Delete: `NS.Client/src/routes/sheet/+page.svelte`

- [ ] **Step 1: Write the load**

`NS.Client/src/routes/(app)/heroes/[id]/+page.ts`:

```ts
import { error } from '@sveltejs/kit';
import { getHero } from '$lib/api/client';
import { ApiError } from '$lib/api/client';
import { assembleReferenceData } from '$lib/reference/cache';
import { resolveSheet } from '$lib/sheet/resolve';

/** Load a hero, its reference data, and resolve the sheet view-model. */
export async function load({ params }: { params: { id: string } }) {
	try {
		const hero = await getHero(params.id);
		const reference = await assembleReferenceData(hero);
		return { vm: resolveSheet(hero, reference) };
	} catch (e) {
		if (e instanceof ApiError && e.status === 404) {
			throw error(404, 'Hero not found');
		}
		throw e;
	}
}
```

- [ ] **Step 2: Write the detail page**

`NS.Client/src/routes/(app)/heroes/[id]/+page.svelte`:

```svelte
<script lang="ts">
	import HeroSheet from '$lib/sheet/components/HeroSheet.svelte';

	let { data } = $props();
</script>

<svelte:head><title>{data.vm.name} — NimbleSheets</title></svelte:head>

<div class="px-4 py-8">
	<HeroSheet vm={data.vm} />
</div>
```

- [ ] **Step 3: Write the error boundary**

`NS.Client/src/routes/(app)/heroes/[id]/+error.svelte`:

```svelte
<script lang="ts">
	import { page } from '$app/state';
</script>

<div class="mx-auto max-w-3xl px-4 py-16 text-center">
	<h1 class="mb-2 text-2xl font-bold text-white">{page.status}</h1>
	<p class="mb-6 text-slate-400">{page.error?.message ?? 'Something went wrong.'}</p>
	<a href="/heroes" class="text-sm text-blue-400 underline hover:text-blue-300">Back to heroes</a>
</div>
```

- [ ] **Step 4: Delete the /sheet route**

```bash
git rm NS.Client/src/routes/sheet/+page.svelte
```

- [ ] **Step 5: Verify type-check passes**

Run: `npm run check`
Expected: `0 errors and 0 warnings`. (If `$app/state`'s `page` is unavailable in this Kit version, fall back to `import { page } from '$app/stores'` and use `$page.status` / `$page.error`.)

- [ ] **Step 6: Commit**

```bash
git add "NS.Client/src/routes/(app)/heroes/[id]/+page.ts" "NS.Client/src/routes/(app)/heroes/[id]/+page.svelte" "NS.Client/src/routes/(app)/heroes/[id]/+error.svelte"
git commit -m "feat(client): add live hero detail page; remove /sheet fixture route"
```

---

### Task 10: Unit tests (tests-after)

Cover the new pure/mockable logic. Vitest runs in the default node environment (matching the existing resolver test), so `localStorage` and `fetch` are stubbed per test.

**Files:**
- Modify: `NS.Client/vitest.config.ts`
- Create: `NS.Client/src/test/app-stub.ts`
- Create: `NS.Client/src/lib/reference/cache.test.ts`
- Create: `NS.Client/src/lib/api/client.test.ts`
- Create: `NS.Client/src/lib/auth/session.test.ts`

- [ ] **Step 0: Make `$lib` and `$app/*` resolvable under Vitest**

The standalone `vitest.config.ts` has no path aliases (that's why the existing resolver test imports relatively). The new source files import `$lib/...` and `$app/navigation`, which Vitest cannot resolve on its own. Add a `$lib` alias and a tiny stub for the SvelteKit `$app/*` virtual modules.

Create `NS.Client/src/test/app-stub.ts`:

```ts
// Minimal stand-ins for SvelteKit's $app/* virtual modules under Vitest.
import { writable } from 'svelte/store';

export const goto = async (_url?: string): Promise<void> => {};
export const navigating = writable(null);
export const page = writable({ status: 200, error: null });
```

Replace `NS.Client/vitest.config.ts` with:

```ts
import { defineConfig } from 'vitest/config';
import { fileURLToPath } from 'node:url';

export default defineConfig({
	test: {
		include: ['src/**/*.test.ts'],
		environment: 'node'
	},
	resolve: {
		alias: [
			{ find: '$lib', replacement: fileURLToPath(new URL('./src/lib', import.meta.url)) },
			{
				find: /^\$app\/(navigation|stores|state)$/,
				replacement: fileURLToPath(new URL('./src/test/app-stub.ts', import.meta.url))
			}
		]
	}
});
```

Run `npm test` to confirm the existing 9 resolver tests still pass with the new config before adding more.

- [ ] **Step 1: Test `neededResources` + `assembleReferenceData`**

`NS.Client/src/lib/reference/cache.test.ts`:

```ts
import { afterEach, describe, expect, it, vi } from 'vitest';
import { caldra } from '../fixtures/caldra';
import type { Hero } from '../api/types';

// Mock the API client so no real fetch happens.
const getReferenceCollection = vi.fn();
vi.mock('$lib/api/client', () => ({ getReferenceCollection }));

import {
	assembleReferenceData, clearReferenceCache, neededResources
} from './cache';

afterEach(() => {
	clearReferenceCache();
	getReferenceCollection.mockReset();
});

describe('neededResources', () => {
	it('always includes ancestries', () => {
		const empty = { ...caldra, backgroundId: null, armor: [], weapons: [],
			activeConditions: [], features: [], magicItems: [], knownSpells: [] } as Hero;
		expect(neededResources(empty)).toEqual(['ancestries']);
	});

	it('includes a collection only when the hero references it', () => {
		const needed = neededResources(caldra);
		expect(needed).toContain('ancestries');
		expect(needed).toContain('weapons'); // Caldra has a mace
		expect(needed).not.toContain('spells'); // Oathsworn fixture has no spells
	});
});

describe('assembleReferenceData', () => {
	it('fetches only needed collections and fills the rest with []', async () => {
		getReferenceCollection.mockImplementation((r: string) => Promise.resolve([{ id: `x-${r}` }]));
		const empty = { ...caldra, backgroundId: null, armor: [], weapons: [],
			activeConditions: [], features: [], magicItems: [], knownSpells: [] } as Hero;

		const refs = await assembleReferenceData(empty);

		expect(getReferenceCollection).toHaveBeenCalledTimes(1);
		expect(getReferenceCollection).toHaveBeenCalledWith('ancestries');
		expect(refs.ancestries).toHaveLength(1);
		expect(refs.spells).toEqual([]);
		expect(refs.weapons).toEqual([]);
	});

	it('caches collections across calls', async () => {
		getReferenceCollection.mockResolvedValue([{ id: 'a' }]);
		await assembleReferenceData(caldra);
		await assembleReferenceData(caldra);
		const ancestryCalls = getReferenceCollection.mock.calls.filter((c) => c[0] === 'ancestries');
		expect(ancestryCalls).toHaveLength(1);
	});
});
```

- [ ] **Step 2: Test `ApiError` mapping**

`NS.Client/src/lib/api/client.test.ts`:

```ts
import { afterEach, describe, expect, it, vi } from 'vitest';
// `$app/navigation` (goto) resolves to src/test/app-stub.ts via the Vitest alias (Step 0).
import { ApiError, getHeroes, login } from './client';
import { clearSession } from '$lib/auth/session';

afterEach(() => {
	vi.restoreAllMocks();
	clearSession();
});

function mockFetch(status: number, body: unknown) {
	vi.stubGlobal('fetch', vi.fn(() =>
		Promise.resolve(new Response(body === undefined ? null : JSON.stringify(body), { status }))
	));
}

describe('apiFetch via login', () => {
	it('returns parsed JSON on 200', async () => {
		mockFetch(200, { token: 't', userId: 'u' });
		await expect(login('Caldra')).resolves.toEqual({ token: 't', userId: 'u' });
	});

	it('throws ApiError with the validation message on 400', async () => {
		mockFetch(400, { errors: { name: ['Name is required'] } });
		await expect(login('')).rejects.toMatchObject({ status: 400, message: 'Name is required' });
	});
});

describe('getHeroes error mapping', () => {
	it('throws ApiError on 500', async () => {
		mockFetch(500, { message: 'boom' });
		await expect(getHeroes()).rejects.toBeInstanceOf(ApiError);
	});
});
```

- [ ] **Step 3: Test the session store**

`NS.Client/src/lib/auth/session.test.ts`:

```ts
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
		setSession({ token: 't', userId: 'u' });
		expect(get(session)).toEqual({ token: 't', userId: 'u' });
		expect(localStorage.getItem('ns.session')).toContain('"token":"t"');
		clearSession();
		expect(get(session)).toBeNull();
		expect(localStorage.getItem('ns.session')).toBeNull();
	});
});
```

- [ ] **Step 4: Run all tests**

Run: `npm test`
Expected: all suites pass — the existing 9 resolver tests plus the new cache/client/session tests. The `$lib` and `$app/*` resolution was set up in Step 0.

- [ ] **Step 5: Commit**

```bash
git add NS.Client/src/lib/reference/cache.test.ts NS.Client/src/lib/api/client.test.ts NS.Client/src/lib/auth/session.test.ts
git commit -m "test(client): cover reference cache, API client, and session store"
```

---

### Task 11: Full verification + manual smoke

**Files:** none (verification only).

- [ ] **Step 1: Type-check, build, test**

Run from `NS.Client/`:
```bash
npm run check && npm run build && npm test
```
Expected: check `0 errors and 0 warnings`; build succeeds to `build/`; all tests pass.

- [ ] **Step 2: Manual smoke against the live API**

The SoloDB database starts empty and reference data has **no write endpoint**, so a fully-populated sheet (resolved weapon/spell names) can't be produced through the API alone. Verify the integration path that *is* reachable:

1. Start the backend: from `NS.WebApp/`, `dotnet run` (serves API on `http://localhost:5197`).
2. Start the client: from `NS.Client/`, `npm run dev` (proxies to 5197).
3. Open the dev URL → root redirects to `/heroes` → guard redirects to `/login`.
4. **Create account** (name + email) → auto-login → lands on `/heroes` showing the empty-state.
5. Reload the page → still authenticated (localStorage session). **Log out** → back to `/login`.
6. Create a hero for the user (no create-hero UI this slice) with a direct API call to confirm list + detail render, e.g. from `NS.WebApp` Swagger-less curl/PowerShell using the token from step 4:
   `POST /heroes` with a minimal `HeroBuildRequest` body (see `NS.FastEndpoints` `HeroBuildRequest` for the exact fields; `ancestryId` may reference a non-existent id — the sheet will show a blank ancestry name, which is expected with no seeded reference data).
7. Reload `/heroes` → the hero card appears → click it → `/heroes/[id]` renders the sheet (identity, stats, skills, vitals). Visiting `/heroes/<random-guid>` → 404 error page with a "Back to heroes" link.

Record any deviations. Resolving full reference names end-to-end is a known gap pending a seeding mechanism (out of scope for this slice).

- [ ] **Step 3: Final commit (if any verification fixups were made)**

```bash
git add -A
git commit -m "chore(client): verification fixups for live API integration"
```

---

## Notes for the implementer

- **Resolver/components are frozen.** Tasks 8–9 must not edit `$lib/sheet/resolve.ts` or any `$lib/sheet/components/*`. If live data won't render, the fix is in the fetch/assembly layer or a fixture/API reconciliation — not the components.
- **Route groups** (`(app)`) don't appear in URLs; `/heroes` and `/heroes/[id]` are the real paths.
- **Flowbite Svelte** components (`Button`, `Card`, `Input`, `Label`) are already dependencies; import from `flowbite-svelte`.
- **Svelte 5 runes**: use `$state`, `$props`, `$derived`; event handlers are `onclick`/`onsubmit` (no colon).
