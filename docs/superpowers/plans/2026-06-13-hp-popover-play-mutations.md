# HP Popover + Live-Play Mutations Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the live `/heroes/[id]` sheet interactive — wire all eight hero play-mutation endpoints (HP damage/heal/temp, wounds, hit dice, mana, recover-all) to the pinned vitals via a hero-actions Svelte context, an HP stepper popover, and interactive wound/hit-dice/mana tiles plus a Rest action.

**Architecture:** The `[id]` page provides a `HeroActions` context (heroId + reactive `busy`/`error` + the eight methods, each POSTing then calling `invalidateAll()` to re-fetch). Interactive tiles consume the context via `getContext` (optionally — read-only when absent). The server owns all rules; the client sends amounts and re-fetches. Spec: `docs/superpowers/specs/2026-06-13-hp-popover-play-mutations-design.md`.

**Tech Stack:** SvelteKit 2.x / Svelte 5 runes (incl. `.svelte.ts` rune module + snippets), TypeScript, Tailwind v4, Vitest.

**Project convention:** Tests after implementation (not TDD). Each feature task implements + verifies with `npm run check` (must end 0 errors / 0 warnings); unit tests for the new pure logic land in Task 11.

---

## File Structure

**Create:**
- `NS.Client/src/lib/sheet/runAction.ts` — pure mutation orchestration (busy/error/refresh).
- `NS.Client/src/lib/sheet/heroActions.svelte.ts` — `HeroActions` interface, `createHeroActions`, `HERO_ACTIONS` context key (runes).
- `NS.Client/src/lib/sheet/components/TilePopover.svelte` — reusable trigger+panel popover.
- `NS.Client/src/lib/sheet/components/HitDiceTile.svelte` — extracted, interactive hit-dice tile.
- `NS.Client/src/lib/sheet/components/ManaTile.svelte` — new, casters-only mana tile.
- `NS.Client/src/lib/sheet/components/RestButton.svelte` — recover-all action with confirm.
- `NS.Client/src/lib/sheet/runAction.test.ts` — unit tests (Task 11).

**Modify:**
- `NS.Client/src/lib/api/client.ts` — eight mutation wrappers.
- `NS.Client/src/lib/api/client.test.ts` — wrapper tests (Task 11).
- `NS.Client/src/lib/sheet/components/HpTile.svelte` — add popover.
- `NS.Client/src/lib/sheet/components/WoundTrack.svelte` — add popover.
- `NS.Client/src/lib/sheet/components/VitalsRow.svelte` — use `HitDiceTile`, add `ManaTile`.
- `NS.Client/src/lib/sheet/components/HeroSheet.svelte` — render `RestButton`.
- `NS.Client/src/routes/(app)/heroes/[id]/+page.ts` — return `heroId`.
- `NS.Client/src/routes/(app)/heroes/[id]/+page.svelte` — provide the context.

---

### Task 1: API client mutation wrappers

**Files:**
- Modify: `NS.Client/src/lib/api/client.ts`

- [ ] **Step 1: Append the eight wrappers**

Add at the end of `NS.Client/src/lib/api/client.ts` (after `getReferenceCollection`). They reuse the existing private `apiFetch`, which already returns `undefined` for 204 and throws `ApiError` on non-2xx:

```ts
/** POST /heroes/{id}/take-damage — apply damage (temp HP absorbs first, server-side). */
export function takeDamage(heroId: string, amount: number): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/take-damage`, {
		method: 'POST',
		body: JSON.stringify({ amount })
	});
}

/** POST /heroes/{id}/heal — restore hit points. */
export function heal(heroId: string, amount: number): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/heal`, {
		method: 'POST',
		body: JSON.stringify({ amount })
	});
}

/** POST /heroes/{id}/grant-temp-hp — set temporary hit points (non-stacking, server-side). */
export function grantTempHp(heroId: string, amount: number): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/grant-temp-hp`, {
		method: 'POST',
		body: JSON.stringify({ amount })
	});
}

/** POST /heroes/{id}/gain-wound — add a wound. */
export function gainWound(heroId: string): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/gain-wound`, { method: 'POST' });
}

/** POST /heroes/{id}/heal-wound — remove a wound. */
export function healWound(heroId: string): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/heal-wound`, { method: 'POST' });
}

/** POST /heroes/{id}/spend-hit-dice — spend hit dice and apply the rolled healing. */
export function spendHitDice(heroId: string, count: number, healingAmount: number): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/spend-hit-dice`, {
		method: 'POST',
		body: JSON.stringify({ count, healingAmount })
	});
}

/** POST /heroes/{id}/spend-mana — spend mana. */
export function spendMana(heroId: string, amount: number): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/spend-mana`, {
		method: 'POST',
		body: JSON.stringify({ amount })
	});
}

/** POST /heroes/{id}/recover-all-resources — clear temp HP and restore resources (rest). */
export function recoverAll(heroId: string): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/recover-all-resources`, { method: 'POST' });
}
```

- [ ] **Step 2: Verify type-check**

Run `npm run check` from `C:\Development\repos\GitHub\nimble-sheet\NS.Client`.
Expected: `0 errors and 0 warnings`. (`apiFetch<void>` returning `undefined as void` is valid TS.)

- [ ] **Step 3: Commit**

```bash
git add NS.Client/src/lib/api/client.ts
git commit -m "feat(client): add hero play-mutation API wrappers"
```

---

### Task 2: runAction orchestration helper (pure)

**Files:**
- Create: `NS.Client/src/lib/sheet/runAction.ts`

- [ ] **Step 1: Write the helper**

```ts
import { ApiError } from '$lib/api/client';

/**
 * Run a hero mutation: flag busy, clear the previous error, perform the action, then refresh.
 * On failure the error message is surfaced and the refresh is skipped. Kept free of runes so it
 * is unit-testable; the reactive bindings live in heroActions.svelte.ts.
 */
export async function runAction(
	action: () => Promise<void>,
	refresh: () => Promise<void>,
	setBusy: (busy: boolean) => void,
	setError: (error: string | null) => void
): Promise<void> {
	setBusy(true);
	setError(null);
	try {
		await action();
		await refresh();
	} catch (e) {
		setError(e instanceof ApiError ? e.message : 'Action failed.');
	} finally {
		setBusy(false);
	}
}
```

- [ ] **Step 2: Verify type-check**

Run `npm run check`. Expected: `0 errors and 0 warnings`.

- [ ] **Step 3: Commit**

```bash
git add NS.Client/src/lib/sheet/runAction.ts
git commit -m "feat(client): add runAction mutation orchestration helper"
```

---

### Task 3: Hero-actions context (runes module)

**Files:**
- Create: `NS.Client/src/lib/sheet/heroActions.svelte.ts`

- [ ] **Step 1: Write the factory + context key**

The factory takes a **getter** for the hero id (not a bare string): SvelteKit reuses the `[id]` page component across param changes, so reading the id lazily keeps the actions bound to the currently-displayed hero.

```ts
import { invalidateAll } from '$app/navigation';
import {
	gainWound, grantTempHp, heal, healWound, recoverAll, spendHitDice, spendMana, takeDamage
} from '$lib/api/client';
import { runAction } from './runAction';

/** Context key for the per-hero mutation actions. */
export const HERO_ACTIONS = Symbol('heroActions');

/** Reactive mutation actions for the displayed hero. `busy`/`error` are shared across all actions. */
export interface HeroActions {
	readonly busy: boolean;
	readonly error: string | null;
	takeDamage(amount: number): Promise<void>;
	heal(amount: number): Promise<void>;
	grantTempHp(amount: number): Promise<void>;
	gainWound(): Promise<void>;
	healWound(): Promise<void>;
	spendHitDice(count: number, healingAmount: number): Promise<void>;
	spendMana(amount: number): Promise<void>;
	recoverAll(): Promise<void>;
}

/** Create the actions bound to a (lazily-read) hero id. Each action POSTs then re-fetches. */
export function createHeroActions(getHeroId: () => string): HeroActions {
	let busy = $state(false);
	let error = $state<string | null>(null);
	const setBusy = (value: boolean) => (busy = value);
	const setError = (value: string | null) => (error = value);
	const run = (action: () => Promise<void>) => runAction(action, invalidateAll, setBusy, setError);

	return {
		get busy() {
			return busy;
		},
		get error() {
			return error;
		},
		takeDamage: (amount) => run(() => takeDamage(getHeroId(), amount)),
		heal: (amount) => run(() => heal(getHeroId(), amount)),
		grantTempHp: (amount) => run(() => grantTempHp(getHeroId(), amount)),
		gainWound: () => run(() => gainWound(getHeroId())),
		healWound: () => run(() => healWound(getHeroId())),
		spendHitDice: (count, healingAmount) => run(() => spendHitDice(getHeroId(), count, healingAmount)),
		spendMana: (amount) => run(() => spendMana(getHeroId(), amount)),
		recoverAll: () => run(() => recoverAll(getHeroId()))
	};
}
```

- [ ] **Step 2: Verify type-check**

Run `npm run check`. Expected: `0 errors and 0 warnings`. (Runes are valid in a `.svelte.ts` module. If `$state` is flagged, confirm the file extension is exactly `heroActions.svelte.ts`.)

- [ ] **Step 3: Commit**

```bash
git add NS.Client/src/lib/sheet/heroActions.svelte.ts
git commit -m "feat(client): add hero-actions context factory"
```

---

### Task 4: Provide the context from the [id] page

**Files:**
- Modify: `NS.Client/src/routes/(app)/heroes/[id]/+page.ts`
- Modify: `NS.Client/src/routes/(app)/heroes/[id]/+page.svelte`

- [ ] **Step 1: Return `heroId` from the load**

In `+page.ts`, change the success return to include the id:

```ts
		return { vm: resolveSheet(hero, reference), heroId: hero.id };
```

(Leave the rest of the file — imports, the 404 mapping — unchanged.)

- [ ] **Step 2: Set the context in the page**

Replace the contents of `+page.svelte` with:

```svelte
<script lang="ts">
	import { setContext } from 'svelte';
	import HeroSheet from '$lib/sheet/components/HeroSheet.svelte';
	import { HERO_ACTIONS, createHeroActions } from '$lib/sheet/heroActions.svelte';

	let { data } = $props();

	setContext(HERO_ACTIONS, createHeroActions(() => data.heroId));
</script>

<svelte:head><title>{data.vm.name} — NimbleSheets</title></svelte:head>

<div class="px-4 py-8">
	<HeroSheet vm={data.vm} />
</div>
```

- [ ] **Step 3: Verify type-check**

Run `npm run check`. Expected: `0 errors and 0 warnings`. (If `'$lib/sheet/heroActions.svelte'` fails to resolve, try the explicit `'$lib/sheet/heroActions.svelte.js'` form — TS `bundler` resolution maps `.js`→`.ts`; use whichever yields 0/0. Record which you used so Tasks 6–10 match.)

- [ ] **Step 4: Commit**

```bash
git add "NS.Client/src/routes/(app)/heroes/[id]/+page.ts" "NS.Client/src/routes/(app)/heroes/[id]/+page.svelte"
git commit -m "feat(client): provide hero-actions context on the detail page"
```

---

### Task 5: Reusable TilePopover

**Files:**
- Create: `NS.Client/src/lib/sheet/components/TilePopover.svelte`

- [ ] **Step 1: Write the popover**

A root-wrapped trigger + floating panel; window-level outside-click and Esc close it. Because the trigger is inside the same root the outside-click check uses, clicking the trigger never self-closes.

```svelte
<script lang="ts">
	import type { Snippet } from 'svelte';

	let {
		label,
		trigger,
		content,
		onopen
	}: {
		label: string;
		trigger: Snippet;
		content: Snippet;
		onopen?: () => void;
	} = $props();

	let open = $state(false);
	let root = $state<HTMLElement | null>(null);

	function toggle() {
		open = !open;
		if (open) {
			onopen?.();
		}
	}

	function handleWindowClick(event: MouseEvent) {
		if (open && root && !root.contains(event.target as Node)) {
			open = false;
		}
	}

	function handleKeydown(event: KeyboardEvent) {
		if (event.key === 'Escape') {
			open = false;
		}
	}
</script>

<svelte:window onclick={handleWindowClick} onkeydown={handleKeydown} />

<div bind:this={root} class="relative">
	<button type="button" aria-label={label} class="block w-full text-left" onclick={toggle}>
		{@render trigger()}
	</button>
	{#if open}
		<div
			role="dialog"
			class="absolute left-1/2 z-30 mt-1 w-44 -translate-x-1/2 rounded-lg border border-slate-700 bg-slate-800 p-2 shadow-xl"
		>
			{@render content()}
		</div>
	{/if}
</div>
```

- [ ] **Step 2: Verify type-check**

Run `npm run check`. Expected: `0 errors and 0 warnings`.

- [ ] **Step 3: Commit**

```bash
git add "NS.Client/src/lib/sheet/components/TilePopover.svelte"
git commit -m "feat(client): add reusable TilePopover"
```

---

### Task 6: Interactive HP tile

**Files:**
- Modify: `NS.Client/src/lib/sheet/components/HpTile.svelte`

- [ ] **Step 1: Rewrite HpTile with the popover**

The tile's visual becomes a local `face` snippet, reused as the trigger or rendered standalone when there's no actions context. Steppers apply immediately (`−`=damage, `+`=heal); the center shows live `current` (updates after the re-fetch). Temp HP has its own field + Set.

```svelte
<script lang="ts">
	import { getContext } from 'svelte';
	import { HERO_ACTIONS, type HeroActions } from '../heroActions.svelte';
	import TilePopover from './TilePopover.svelte';

	let { current, max, temp }: { current: number; max: number; temp: number } = $props();

	const actions = getContext<HeroActions | undefined>(HERO_ACTIONS);
	let tempInput = $state(0);

	const btn =
		'rounded bg-slate-700 px-2 py-1 text-xs font-semibold text-white hover:bg-slate-600 disabled:opacity-50';
</script>

{#snippet face()}
	<div class="rounded-lg bg-gradient-to-b from-red-900 to-red-950 p-2.5 text-center">
		<div class="text-[9px] uppercase tracking-[0.14em] text-red-200">Hit Points</div>
		<div class="text-3xl font-black leading-none text-white">{current}</div>
		<div class="mt-1 text-[10px] text-red-200">+{temp} temp · {max} max</div>
	</div>
{/snippet}

{#if actions}
	<TilePopover label="Adjust hit points" onopen={() => (tempInput = 0)}>
		{#snippet trigger()}{@render face()}{/snippet}
		{#snippet content()}
			<div class="flex items-center justify-between gap-1">
				<button type="button" class={btn} disabled={actions.busy} onclick={() => actions.takeDamage(5)}>−5</button>
				<button type="button" class={btn} disabled={actions.busy} onclick={() => actions.takeDamage(1)}>−1</button>
				<span class="min-w-8 text-center text-sm font-bold text-white">{current}</span>
				<button type="button" class={btn} disabled={actions.busy} onclick={() => actions.heal(1)}>+1</button>
				<button type="button" class={btn} disabled={actions.busy} onclick={() => actions.heal(5)}>+5</button>
			</div>
			<div class="mt-2 flex items-center gap-1">
				<input
					type="number"
					min="0"
					bind:value={tempInput}
					class="w-14 rounded bg-slate-900 px-1.5 py-1 text-xs text-white"
					aria-label="Temp HP amount"
				/>
				<button type="button" class={btn} disabled={actions.busy} onclick={() => actions.grantTempHp(tempInput)}>
					Temp
				</button>
			</div>
			{#if actions.error}<p class="mt-1 text-[11px] text-red-400">{actions.error}</p>{/if}
		{/snippet}
	</TilePopover>
{:else}
	{@render face()}
{/if}
```

- [ ] **Step 2: Verify type-check**

Run `npm run check`. Expected: `0 errors and 0 warnings`. (Match the `heroActions.svelte` import form chosen in Task 4 Step 3.)

- [ ] **Step 3: Commit**

```bash
git add "NS.Client/src/lib/sheet/components/HpTile.svelte"
git commit -m "feat(client): add HP damage/heal/temp popover"
```

---

### Task 7: Interactive wound track

**Files:**
- Modify: `NS.Client/src/lib/sheet/components/WoundTrack.svelte`

- [ ] **Step 1: Rewrite WoundTrack with a popover**

Keep the existing pip face; wrap it when actions are present. Heal is disabled at 0 wounds; the server enforces the rest.

```svelte
<script lang="ts">
	import { getContext } from 'svelte';
	import { HERO_ACTIONS, type HeroActions } from '../heroActions.svelte';
	import TilePopover from './TilePopover.svelte';

	let {
		current,
		max,
		isDead,
		isDying
	}: {
		current: number;
		max: number;
		isDead: boolean;
		isDying: boolean;
	} = $props();

	const actions = getContext<HeroActions | undefined>(HERO_ACTIONS);
	const pips = $derived(Array.from({ length: max }, (_, i) => i < current));

	const btn =
		'rounded bg-slate-700 px-2 py-1 text-xs font-semibold text-white hover:bg-slate-600 disabled:opacity-50';
</script>

{#snippet face()}
	<div class="rounded-lg bg-slate-800 p-2.5 text-center">
		<div class="text-[9px] uppercase tracking-[0.14em] text-slate-400">
			Wounds
			{#if isDead}<span class="ml-1 text-red-400">· Dead</span>
			{:else if isDying}<span class="ml-1 text-amber-400">· Dying</span>{/if}
		</div>
		<div class="mt-2 flex items-center justify-center gap-1">
			{#each pips as filled, i (i)}
				<span class="h-3 w-3 rounded-full border-2 {filled ? 'border-red-500 bg-red-500' : 'border-slate-500'}"></span>
			{/each}
			<span class="ml-0.5 text-sm text-slate-400">☠</span>
		</div>
	</div>
{/snippet}

{#if actions}
	<TilePopover label="Adjust wounds">
		{#snippet trigger()}{@render face()}{/snippet}
		{#snippet content()}
			<div class="mb-2 text-center text-xs text-slate-300">{current} / {max} wounds</div>
			<div class="flex justify-center gap-1">
				<button type="button" class={btn} disabled={actions.busy || current === 0} onclick={() => actions.healWound()}>
					Heal
				</button>
				<button type="button" class={btn} disabled={actions.busy} onclick={() => actions.gainWound()}>
					Gain
				</button>
			</div>
			{#if actions.error}<p class="mt-1 text-[11px] text-red-400">{actions.error}</p>{/if}
		{/snippet}
	</TilePopover>
{:else}
	{@render face()}
{/if}
```

- [ ] **Step 2: Verify type-check**

Run `npm run check`. Expected: `0 errors and 0 warnings`.

- [ ] **Step 3: Commit**

```bash
git add "NS.Client/src/lib/sheet/components/WoundTrack.svelte"
git commit -m "feat(client): add gain/heal wound controls"
```

---

### Task 8: Extract interactive HitDiceTile

**Files:**
- Create: `NS.Client/src/lib/sheet/components/HitDiceTile.svelte`
- Modify: `NS.Client/src/lib/sheet/components/VitalsRow.svelte`

- [ ] **Step 1: Create HitDiceTile**

Mirrors the inline hit-dice markup currently in `VitalsRow`, plus a popover to spend dice with a rolled healing amount.

```svelte
<script lang="ts">
	import { getContext } from 'svelte';
	import { HERO_ACTIONS, type HeroActions } from '../heroActions.svelte';
	import TilePopover from './TilePopover.svelte';

	let { die, available, max }: { die: string; available: number; max: number } = $props();

	const actions = getContext<HeroActions | undefined>(HERO_ACTIONS);
	let count = $state(1);
	let healing = $state(0);

	const btn =
		'rounded bg-slate-700 px-2 py-1 text-xs font-semibold text-white hover:bg-slate-600 disabled:opacity-50';

	function reset() {
		count = 1;
		healing = 0;
	}
</script>

{#snippet face()}
	<div class="rounded-lg bg-slate-800 p-2.5 text-center">
		<div class="text-[9px] uppercase tracking-[0.14em] text-slate-400">Hit Dice</div>
		<div class="mt-1 text-2xl font-extrabold text-white">{die}</div>
		<div class="text-[10px] text-slate-400">{available} / {max}</div>
	</div>
{/snippet}

{#if actions}
	<TilePopover label="Spend hit dice" onopen={reset}>
		{#snippet trigger()}{@render face()}{/snippet}
		{#snippet content()}
			<label class="block text-[11px] text-slate-300">
				Dice
				<input
					type="number"
					min="1"
					max={available}
					bind:value={count}
					class="mt-0.5 w-full rounded bg-slate-900 px-1.5 py-1 text-xs text-white"
				/>
			</label>
			<label class="mt-1 block text-[11px] text-slate-300">
				Heal
				<input
					type="number"
					min="0"
					bind:value={healing}
					class="mt-0.5 w-full rounded bg-slate-900 px-1.5 py-1 text-xs text-white"
				/>
			</label>
			<button
				type="button"
				class="{btn} mt-2 w-full"
				disabled={actions.busy || available === 0}
				onclick={() => actions.spendHitDice(count, healing)}
			>
				Spend
			</button>
			{#if actions.error}<p class="mt-1 text-[11px] text-red-400">{actions.error}</p>{/if}
		{/snippet}
	</TilePopover>
{:else}
	{@render face()}
{/if}
```

- [ ] **Step 2: Use HitDiceTile in VitalsRow**

In `VitalsRow.svelte`, add the import and replace the inline hit-dice `<div>…</div>` block with the component. The script becomes:

```svelte
<script lang="ts">
	import type { SheetViewModel } from '../viewmodel';
	import HpTile from './HpTile.svelte';
	import WoundTrack from './WoundTrack.svelte';
	import HitDiceTile from './HitDiceTile.svelte';

	let { vm }: { vm: SheetViewModel } = $props();
</script>
```

And replace this block:

```svelte
	<div class="rounded-lg bg-slate-800 p-2.5 text-center">
		<div class="text-[9px] uppercase tracking-[0.14em] text-slate-400">Hit Dice</div>
		<div class="mt-1 text-2xl font-extrabold text-white">{vm.hitDice.die}</div>
		<div class="text-[10px] text-slate-400">{vm.hitDice.available} / {vm.hitDice.max}</div>
	</div>
```

with:

```svelte
	<HitDiceTile die={vm.hitDice.die} available={vm.hitDice.available} max={vm.hitDice.max} />
```

(Leave the HP, Wounds, Armor, and Init tiles as they are.)

- [ ] **Step 3: Verify type-check**

Run `npm run check`. Expected: `0 errors and 0 warnings`.

- [ ] **Step 4: Commit**

```bash
git add "NS.Client/src/lib/sheet/components/HitDiceTile.svelte" "NS.Client/src/lib/sheet/components/VitalsRow.svelte"
git commit -m "feat(client): add spend-hit-dice tile"
```

---

### Task 9: Mana tile (casters)

**Files:**
- Create: `NS.Client/src/lib/sheet/components/ManaTile.svelte`
- Modify: `NS.Client/src/lib/sheet/components/VitalsRow.svelte`

- [ ] **Step 1: Create ManaTile**

```svelte
<script lang="ts">
	import { getContext } from 'svelte';
	import { HERO_ACTIONS, type HeroActions } from '../heroActions.svelte';
	import TilePopover from './TilePopover.svelte';

	let { current, max }: { current: number; max: number } = $props();

	const actions = getContext<HeroActions | undefined>(HERO_ACTIONS);
	let amount = $state(1);

	const btn =
		'rounded bg-slate-700 px-2 py-1 text-xs font-semibold text-white hover:bg-slate-600 disabled:opacity-50';
</script>

{#snippet face()}
	<div class="rounded-lg bg-indigo-900 p-2.5 text-center">
		<div class="text-[9px] uppercase tracking-[0.14em] text-indigo-200">Mana</div>
		<div class="mt-1 text-2xl font-extrabold text-white">{current}</div>
		<div class="text-[10px] text-indigo-200">{max} max</div>
	</div>
{/snippet}

{#if actions}
	<TilePopover label="Spend mana" onopen={() => (amount = 1)}>
		{#snippet trigger()}{@render face()}{/snippet}
		{#snippet content()}
			<div class="flex items-center gap-1">
				<input
					type="number"
					min="1"
					bind:value={amount}
					class="w-14 rounded bg-slate-900 px-1.5 py-1 text-xs text-white"
					aria-label="Mana to spend"
				/>
				<button type="button" class={btn} disabled={actions.busy} onclick={() => actions.spendMana(amount)}>
					Spend
				</button>
			</div>
			{#if actions.error}<p class="mt-1 text-[11px] text-red-400">{actions.error}</p>{/if}
		{/snippet}
	</TilePopover>
{:else}
	{@render face()}
{/if}
```

- [ ] **Step 2: Render it in VitalsRow for casters**

Add the import `import ManaTile from './ManaTile.svelte';` to the `VitalsRow.svelte` script, and insert the mana tile after the `HitDiceTile` (still inside the grid):

```svelte
	{#if vm.mana}
		<ManaTile current={vm.mana.current} max={vm.mana.max} />
	{/if}
```

(`vm.mana` is `null` for non-casters, so the tile only appears for casters. The existing `grid-cols-2 sm:grid-cols-5` grid simply wraps the 6th tile — acceptable.)

- [ ] **Step 3: Verify type-check**

Run `npm run check`. Expected: `0 errors and 0 warnings`.

- [ ] **Step 4: Commit**

```bash
git add "NS.Client/src/lib/sheet/components/ManaTile.svelte" "NS.Client/src/lib/sheet/components/VitalsRow.svelte"
git commit -m "feat(client): add spend-mana tile for casters"
```

---

### Task 10: Rest action

**Files:**
- Create: `NS.Client/src/lib/sheet/components/RestButton.svelte`
- Modify: `NS.Client/src/lib/sheet/components/HeroSheet.svelte`

- [ ] **Step 1: Create RestButton**

Renders nothing without an actions context. Uses the popover as a confirm step.

```svelte
<script lang="ts">
	import { getContext } from 'svelte';
	import { HERO_ACTIONS, type HeroActions } from '../heroActions.svelte';
	import TilePopover from './TilePopover.svelte';

	const actions = getContext<HeroActions | undefined>(HERO_ACTIONS);

	const btn =
		'rounded bg-slate-700 px-2 py-1 text-xs font-semibold text-white hover:bg-slate-600 disabled:opacity-50';
</script>

{#if actions}
	<TilePopover label="Rest">
		{#snippet trigger()}
			<span class="inline-block rounded border border-slate-700 bg-slate-800 px-3 py-1 text-xs font-semibold text-slate-200 hover:border-slate-600">
				Rest
			</span>
		{/snippet}
		{#snippet content()}
			<p class="mb-2 text-[11px] text-slate-300">Rest and recover all resources?</p>
			<button type="button" class="{btn} w-full" disabled={actions.busy} onclick={() => actions.recoverAll()}>
				Confirm rest
			</button>
			{#if actions.error}<p class="mt-1 text-[11px] text-red-400">{actions.error}</p>{/if}
		{/snippet}
	</TilePopover>
{/if}
```

- [ ] **Step 2: Render RestButton in HeroSheet's pinned region**

In `HeroSheet.svelte`, add `import RestButton from './RestButton.svelte';` to the script, and put a right-aligned Rest button just above `VitalsRow` inside the pinned `div.space-y-4`:

```svelte
	<div class="space-y-4 bg-slate-900 px-5 py-4">
		<div class="flex justify-end">
			<RestButton />
		</div>
		<VitalsRow {vm} />
		<StatRow stats={vm.stats} />
		<SkillsRow skills={vm.skills} />
	</div>
```

- [ ] **Step 3: Verify type-check**

Run `npm run check`. Expected: `0 errors and 0 warnings`.

- [ ] **Step 4: Commit**

```bash
git add "NS.Client/src/lib/sheet/components/RestButton.svelte" "NS.Client/src/lib/sheet/components/HeroSheet.svelte"
git commit -m "feat(client): add rest (recover-all-resources) action"
```

---

### Task 11: Unit tests (tests-after)

**Files:**
- Modify: `NS.Client/src/lib/api/client.test.ts`
- Create: `NS.Client/src/lib/sheet/runAction.test.ts`

- [ ] **Step 1: Add mutation-wrapper tests**

Append to `NS.Client/src/lib/api/client.test.ts` (extend the imports on line 3 to include the wrappers, then add the block):

Change line 3 to:

```ts
import { ApiError, gainWound, getHeroes, login, spendHitDice, takeDamage } from './client';
```

Add at the end of the file:

```ts
function captureFetch(status = 204) {
	const fetchMock = vi.fn(() => Promise.resolve(new Response(null, { status })));
	vi.stubGlobal('fetch', fetchMock);
	return fetchMock;
}

describe('play-mutation wrappers', () => {
	it('takeDamage posts the amount to the take-damage route', async () => {
		const fetchMock = captureFetch(204);
		await takeDamage('h1', 5);
		expect(fetchMock).toHaveBeenCalledWith(
			'/heroes/h1/take-damage',
			expect.objectContaining({ method: 'POST', body: JSON.stringify({ amount: 5 }) })
		);
	});

	it('spendHitDice posts count and healingAmount', async () => {
		const fetchMock = captureFetch(204);
		await spendHitDice('h1', 2, 7);
		expect(fetchMock).toHaveBeenCalledWith(
			'/heroes/h1/spend-hit-dice',
			expect.objectContaining({ method: 'POST', body: JSON.stringify({ count: 2, healingAmount: 7 }) })
		);
	});

	it('gainWound posts with no body and resolves on 204', async () => {
		const fetchMock = captureFetch(204);
		await expect(gainWound('h1')).resolves.toBeUndefined();
		const [path, init] = fetchMock.mock.calls[0] as [string, RequestInit];
		expect(path).toBe('/heroes/h1/gain-wound');
		expect(init.method).toBe('POST');
		expect(init.body).toBeUndefined();
	});

	it('surfaces an ApiError on a 400', async () => {
		captureFetch(400);
		await expect(takeDamage('h1', 5)).rejects.toBeInstanceOf(ApiError);
	});
});
```

- [ ] **Step 2: Add runAction tests**

`NS.Client/src/lib/sheet/runAction.test.ts`:

```ts
import { describe, expect, it, vi } from 'vitest';
import { runAction } from './runAction';
import { ApiError } from '$lib/api/client';

describe('runAction', () => {
	it('toggles busy true→false and refreshes on success', async () => {
		const busy: boolean[] = [];
		const refresh = vi.fn(() => Promise.resolve());
		await runAction(() => Promise.resolve(), refresh, (b) => busy.push(b), () => {});
		expect(busy).toEqual([true, false]);
		expect(refresh).toHaveBeenCalledOnce();
	});

	it('surfaces an ApiError message and skips refresh on failure', async () => {
		let error: string | null = 'stale';
		const refresh = vi.fn(() => Promise.resolve());
		await runAction(
			() => Promise.reject(new ApiError(400, 'Not enough mana')),
			refresh,
			() => {},
			(e) => (error = e)
		);
		expect(error).toBe('Not enough mana');
		expect(refresh).not.toHaveBeenCalled();
	});

	it('uses a generic message for a non-ApiError failure', async () => {
		let error: string | null = null;
		await runAction(() => Promise.reject(new Error('boom')), () => Promise.resolve(), () => {}, (e) => (error = e));
		expect(error).toBe('Action failed.');
	});
});
```

- [ ] **Step 3: Run the tests**

Run `npm test` from `NS.Client`.
Expected: all suites pass — the prior 18 tests plus the 4 new wrapper tests and 3 runAction tests (25 total). (`$lib/api/client` resolves `$app/navigation` via the test stub already configured in `vitest.config.ts`.)

- [ ] **Step 4: Commit**

```bash
git add NS.Client/src/lib/api/client.test.ts NS.Client/src/lib/sheet/runAction.test.ts
git commit -m "test(client): cover play-mutation wrappers and runAction"
```

---

### Task 12: Full verification + mutation smoke

**Files:** none.

- [ ] **Step 1: Type-check, build, test**

Run from `NS.Client/`:
```bash
npm run check && npm run build && npm test
```
Expected: check `0 errors and 0 warnings`; build succeeds; all tests pass.

- [ ] **Step 2: HTTP smoke of the mutation path (recommended)**

The endpoints existed before this slice, so the client wiring is the new surface. Confirm a mutation round-trips against the running API (no seeded reference data needed — a hero with empty collections is fine):

1. From `NS.WebApp/`: `dotnet run --launch-profile http` (API on `http://localhost:5197`).
2. Create a user + log in (see the live-API plan's smoke for the curl/PowerShell), capture `TOKEN` and the user id.
3. Create a hero: `POST /heroes` with a minimal `HeroBuildRequest` body (see `NS.FastEndpoints/HeroBuildRequest.cs` for exact fields; `ancestryId` may be any GUID). Capture the returned hero id `H`.
4. `GET /heroes/H` → note `currentHp`.
5. `POST /heroes/H/take-damage` `{"amount":5}` → expect 204; `GET /heroes/H` → `currentHp` dropped by 5 (or temp HP absorbed).
6. `POST /heroes/H/heal` `{"amount":3}` → 204; verify `currentHp` rose (clamped to `maxHp`).
7. Stop the server.

Record any deviation. (Browser-level visual verification of the popovers remains gated on reference-data seeding — deferred.)

- [ ] **Step 3: Final commit (only if verification fixups were needed)**

```bash
git add -A
git commit -m "chore(client): verification fixups for play mutations"
```

---

## Notes for the implementer

- **Context is consumed optionally.** Every interactive tile does `getContext<HeroActions | undefined>(HERO_ACTIONS)` and renders read-only when it's absent, so the components stay usable without a provider and `svelte-check` sees a defined type.
- **`.svelte.ts` import form:** import the context module as `'../heroActions.svelte'` (or `'$lib/sheet/heroActions.svelte'`). If resolution fails under `npm run check`, use the `.svelte.js` suffix form; keep it consistent across Tasks 4, 6–10.
- **Re-fetch, not optimistic.** Actions call `invalidateAll()`; never mutate `vm` locally. The HP popover's center reflects `current` after the load re-runs.
- **Svelte 5 idioms:** `$state`, `$derived`, `$props`, named snippets (`{#snippet trigger()}` / `{#snippet content()}`), `onclick`/`onkeydown` (no `on:`). The resolver and `SheetViewModel` are not modified.
