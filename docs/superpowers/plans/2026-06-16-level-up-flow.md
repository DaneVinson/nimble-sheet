# Level-Up & Pending-Choice Flow Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add inline affordances on the hero sheet to level a hero and resolve the resulting pending state (HP increase, stat increase, skill allocation, subclass at L3).

**Architecture:** Client-only (all six endpoints/domain methods exist). New client wrappers + `heroActions` methods (including a composite `levelUp(hp)` that applies HP then increments level in one refresh), four new view-model fields, a pure skill-allocation helper, and a `LevelUpControls.svelte` placed next to the Rest button that surfaces each pending consequence as its own `TilePopover`. Server-authoritative, POST → `invalidateAll()`, no optimistic updates.

**Tech Stack:** SvelteKit 2 / Svelte 5 runes, Vitest (NS.Client). No server/domain changes.

---

## Conventions (read before every task)

- **Svelte 5 runes**; dark Tailwind utilities directly. Follow existing components (`RestButton.svelte`, the `*Editor.svelte` components).
- **Testing order — NO TDD (project preference):** implement first, then write the test, then run it.
- **Commits:** one per task. Work on branch `feat/level-up-flow` (already created; spec committed at `f4ffc18`).
- **Shared button class:** `editorButton` from `src/lib/sheet/components/styles.ts`.
- Commands (from `NS.Client/`): `npm test`, `npm run check`, `npm run build`.

## Reference facts

- Server request records (exist, unchanged; JSON binds by name, `HeroId` from route): `LevelUpRequest(Guid HeroId, IReadOnlyList<string> PendingChoices)`, `ApplyHpIncreaseRequest(Guid HeroId, int Amount)`, `ApplyStatIncreaseRequest(Guid HeroId, StatType Stat)`, `FinalizeSkillAllocationRequest(Guid HeroId, HeroSkills UpdatedSkills)`, `SetSubclassRequest(Guid HeroId, string Subclass)`.
- `LevelUp` sets `PendingStatIncrease = true` and `UnspentSkillPoints++` on every level. `FinalizeSkillAllocation` replaces all skills and zeroes the pool (under-spending loses points). Enums serialize as names (`"Strength"`).
- TS types (`src/lib/api/types.ts`): `StatType = 'Strength'|'Dexterity'|'Intelligence'|'Will'`; `HeroSkills` = `{ arcana; examination; finesse; influence; insight; lore; might; naturecraft; perception; stealth }` (all `number`). `Hero` has `pendingStatIncrease: boolean`, `unspentSkillPoints: number`, `subclass: string|null`, `level: number`, `skills: HeroSkills`.
- `SheetViewModel` has `level`, `subclass`, `skills: SkillViewModel[]` (display), `className`. `HeroSheet.svelte` renders `<RestButton />` inside `<div class="flex justify-end">` in the pinned region.
- Composite `levelUp` ordering and the runes component cannot be unit-tested (the `heroActions.svelte.ts` runes module and `.svelte` components aren't compiled by the standalone Vitest config) — they're covered by browser verification (Task 5). The pure skill-allocation helper IS unit-tested (Task 3).

## File Structure

- Modify: `NS.Client/src/lib/api/client.ts` — 5 wrappers
- Modify: `NS.Client/src/lib/api/client.test.ts` — wrapper tests
- Modify: `NS.Client/src/lib/sheet/heroActions.svelte.ts` — interface + factory (composite levelUp + 3 methods)
- Modify: `NS.Client/src/lib/sheet/viewmodel.ts` — 4 fields
- Modify: `NS.Client/src/lib/sheet/resolve.ts` — populate the fields
- Modify: `NS.Client/src/lib/sheet/resolve.test.ts` — assert the fields
- Create: `NS.Client/src/lib/sheet/levelUp/skillAllocation.ts` — pure allocation helper
- Create: `NS.Client/src/lib/sheet/levelUp/skillAllocation.test.ts`
- Create: `NS.Client/src/lib/sheet/components/LevelUpControls.svelte`
- Modify: `NS.Client/src/lib/sheet/components/HeroSheet.svelte` — render `LevelUpControls`
- Modify: `CLAUDE.md` — document the flow

---

## Task 1: Client wrappers + actions

**Files:** Modify `client.ts`, `client.test.ts`, `heroActions.svelte.ts`

- [ ] **Step 1: Add wrappers** to `client.ts` (in the collection-mutations area). Note `applyHpIncrease` is used by the composite action but is still its own wrapper:

```ts
/** POST /heroes/{id}/level-up — advance one level (feature choices handled separately). */
export function levelUp(heroId: string): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/level-up`, {
		method: 'POST',
		body: JSON.stringify({ pendingChoices: [] })
	});
}

/** POST /heroes/{id}/apply-hp-increase — raise max + current HP by the rolled amount. */
export function applyHpIncrease(heroId: string, amount: number): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/apply-hp-increase`, {
		method: 'POST',
		body: JSON.stringify({ amount })
	});
}

/** POST /heroes/{id}/apply-stat-increase — apply the pending +1 stat increase. */
export function applyStatIncrease(heroId: string, stat: string): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/apply-stat-increase`, {
		method: 'POST',
		body: JSON.stringify({ stat })
	});
}

/** POST /heroes/{id}/finalize-skill-allocation — replace skills and clear unspent points. */
export function finalizeSkillAllocation(heroId: string, updatedSkills: HeroSkills): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/finalize-skill-allocation`, {
		method: 'POST',
		body: JSON.stringify({ updatedSkills })
	});
}

/** POST /heroes/{id}/set-subclass — set the hero's subclass. */
export function setSubclass(heroId: string, subclass: string): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/set-subclass`, {
		method: 'POST',
		body: JSON.stringify({ subclass })
	});
}
```

`client.ts` imports types from `./types`; add `HeroSkills` to that import if not already present (the existing import is `import type { Hero } from './types';` and `import type { HeroBuildModel } from '$lib/sheet/build/model';` — add `HeroSkills` to the `./types` import: `import type { Hero, HeroSkills } from './types';`).

- [ ] **Step 2: Add wrapper tests** to `client.test.ts` — add `applyStatIncrease, finalizeSkillAllocation, levelUp, setSubclass` to the import from `./client`, and add inside the existing `describe('collection wrappers', ...)`:

```ts
it('levelUp posts an empty pendingChoices list', async () => {
	const fetchMock = captureFetch(204);
	await levelUp('h1');
	expect(fetchMock).toHaveBeenCalledWith(
		'/api/heroes/h1/level-up',
		expect.objectContaining({ method: 'POST', body: JSON.stringify({ pendingChoices: [] }) })
	);
});

it('applyStatIncrease posts the stat name', async () => {
	const fetchMock = captureFetch(204);
	await applyStatIncrease('h1', 'Strength');
	expect(fetchMock).toHaveBeenCalledWith(
		'/api/heroes/h1/apply-stat-increase',
		expect.objectContaining({ method: 'POST', body: JSON.stringify({ stat: 'Strength' }) })
	);
});

it('finalizeSkillAllocation posts updatedSkills', async () => {
	const fetchMock = captureFetch(204);
	const skills = { arcana: 1, examination: 0, finesse: 0, influence: 0, insight: 0, lore: 0, might: 2, naturecraft: 0, perception: 0, stealth: 0 };
	await finalizeSkillAllocation('h1', skills);
	expect(fetchMock).toHaveBeenCalledWith(
		'/api/heroes/h1/finalize-skill-allocation',
		expect.objectContaining({ method: 'POST', body: JSON.stringify({ updatedSkills: skills }) })
	);
});

it('setSubclass posts the subclass name', async () => {
	const fetchMock = captureFetch(204);
	await setSubclass('h1', 'Ravager');
	expect(fetchMock).toHaveBeenCalledWith(
		'/api/heroes/h1/set-subclass',
		expect.objectContaining({ method: 'POST', body: JSON.stringify({ subclass: 'Ravager' }) })
	);
});
```

- [ ] **Step 3: Extend `heroActions.svelte.ts`.** Add to the import from `$lib/api/client`: `applyHpIncrease, applyStatIncrease, finalizeSkillAllocation, setSubclass`, and `levelUp as levelUpRequest` (aliased to avoid colliding with the action method). Add to the `HeroActions` interface:

```ts
	levelUp(hpIncrease: number): Promise<void>;
	applyStatIncrease(stat: string): Promise<void>;
	finalizeSkillAllocation(skills: HeroSkills): Promise<void>;
	setSubclass(subclass: string): Promise<void>;
```

Add `HeroSkills` to the type import at the top of the file (it imports hero-action wrappers from `$lib/api/client`; add a `import type { HeroSkills } from '$lib/api/types';`). Add to the returned object in `createHeroActions`:

```ts
		levelUp: (hpIncrease) =>
			run(async () => {
				if (hpIncrease > 0) {
					await applyHpIncrease(getHeroId(), hpIncrease);
				}
				await levelUpRequest(getHeroId());
			}),
		applyStatIncrease: (stat) => run(() => applyStatIncrease(getHeroId(), stat)),
		finalizeSkillAllocation: (skills) => run(() => finalizeSkillAllocation(getHeroId(), skills)),
		setSubclass: (subclass) => run(() => setSubclass(getHeroId(), subclass)),
```

- [ ] **Step 4: Verify.** From `NS.Client/`: `npm test` (4 new tests pass) and `npm run check` (0 errors/0 warnings).

- [ ] **Step 5: Commit.**

```bash
git add NS.Client/src/lib/api/client.ts NS.Client/src/lib/api/client.test.ts NS.Client/src/lib/sheet/heroActions.svelte.ts
git commit -m "feat(client): level-up wrappers and actions

Co-Authored-By: Claude Opus 4.8 <noreply@anthropic.com>"
```

---

## Task 2: View-model fields + resolver

**Files:** Modify `viewmodel.ts`, `resolve.ts`, `resolve.test.ts`

- [ ] **Step 1: Extend `SheetViewModel`** in `viewmodel.ts`. Add `import type { ..., HeroSkills } from '../api/types';` (the file already imports types from `../api/types` — add `HeroSkills`). Add these fields to the `SheetViewModel` interface (group them after `subclass`):

```ts
  pendingStatIncrease: boolean;
  unspentSkillPoints: number;
  needsSubclass: boolean;
  skillValues: HeroSkills;
```

- [ ] **Step 2: Populate them in `resolve.ts`.** In the object returned by `resolveSheet`, add (near `subclass: hero.subclass,`):

```ts
    pendingStatIncrease: hero.pendingStatIncrease,
    unspentSkillPoints: hero.unspentSkillPoints,
    needsSubclass: hero.level >= 3 && hero.subclass === null,
    skillValues: { ...hero.skills },
```

- [ ] **Step 3: Add resolver tests** to `resolve.test.ts` (reuse the file's existing `caldra` fixture and reference bundle; substitute the bundle's actual variable name for `REF`):

```ts
it('carries level-up pending state and editable skill values', () => {
	const vm = resolveSheet(caldra, REF);
	expect(vm.pendingStatIncrease).toBe(caldra.pendingStatIncrease);
	expect(vm.unspentSkillPoints).toBe(caldra.unspentSkillPoints);
	expect(vm.needsSubclass).toBe(caldra.level >= 3 && caldra.subclass === null);
	expect(vm.skillValues).toEqual(caldra.skills);
});

it('skillValues is an independent copy of the hero skills', () => {
	const vm = resolveSheet(caldra, REF);
	vm.skillValues.might = 99;
	expect(caldra.skills.might).not.toBe(99);
});
```

- [ ] **Step 4: Verify.** From `NS.Client/`: `npm test && npm run check` → PASS, 0 errors.

- [ ] **Step 5: Commit.**

```bash
git add NS.Client/src/lib/sheet/viewmodel.ts NS.Client/src/lib/sheet/resolve.ts NS.Client/src/lib/sheet/resolve.test.ts
git commit -m "feat(client): expose level-up pending state on the view model

Co-Authored-By: Claude Opus 4.8 <noreply@anthropic.com>"
```

---

## Task 3: Pure skill-allocation helper + tests

**Files:** Create `src/lib/sheet/levelUp/skillAllocation.ts` and `skillAllocation.test.ts`

- [ ] **Step 1: Create `skillAllocation.ts`:**

```ts
import type { HeroSkills } from '$lib/api/types';

/** The ten skills with display labels, in display order. */
export const SKILLS: { key: keyof HeroSkills; label: string }[] = [
	{ key: 'arcana', label: 'Arcana' },
	{ key: 'examination', label: 'Examination' },
	{ key: 'finesse', label: 'Finesse' },
	{ key: 'influence', label: 'Influence' },
	{ key: 'insight', label: 'Insight' },
	{ key: 'lore', label: 'Lore' },
	{ key: 'might', label: 'Might' },
	{ key: 'naturecraft', label: 'Naturecraft' },
	{ key: 'perception', label: 'Perception' },
	{ key: 'stealth', label: 'Stealth' }
];

/** Maximum bonus any single skill can reach. */
export const SKILL_CAP = 12;

/** Total points allocated from `start` to `working` (working is never below start, so this is >= 0). */
export function spentPoints(start: HeroSkills, working: HeroSkills): number {
	return SKILLS.reduce((sum, { key }) => sum + (working[key] - start[key]), 0);
}

/** A skill can be incremented when it is under the cap and budget remains. */
export function canIncrement(start: HeroSkills, working: HeroSkills, key: keyof HeroSkills, budget: number): boolean {
	return working[key] < SKILL_CAP && spentPoints(start, working) < budget;
}

/** A skill can be decremented when it is above its starting value. */
export function canDecrement(start: HeroSkills, working: HeroSkills, key: keyof HeroSkills): boolean {
	return working[key] > start[key];
}

/** Allocation can be finalized only when exactly the full budget has been spent. */
export function canFinalize(start: HeroSkills, working: HeroSkills, budget: number): boolean {
	return spentPoints(start, working) === budget;
}
```

- [ ] **Step 2: Create `skillAllocation.test.ts`:**

```ts
import { describe, expect, it } from 'vitest';
import type { HeroSkills } from '$lib/api/types';
import { SKILLS, SKILL_CAP, spentPoints, canIncrement, canDecrement, canFinalize } from './skillAllocation';

const base: HeroSkills = {
	arcana: 0, examination: 0, finesse: 0, influence: 0, insight: 0,
	lore: 0, might: 0, naturecraft: 0, perception: 0, stealth: 0
};

describe('skillAllocation', () => {
	it('lists all ten skills', () => {
		expect(SKILLS.length).toBe(10);
	});

	it('spentPoints sums the deltas from start to working', () => {
		const working = { ...base, might: 2, arcana: 1 };
		expect(spentPoints(base, working)).toBe(3);
	});

	it('canIncrement is false when the budget is exhausted', () => {
		const working = { ...base, might: 1 };
		expect(canIncrement(base, working, 'arcana', 1)).toBe(false);
		expect(canIncrement(base, base, 'arcana', 1)).toBe(true);
	});

	it('canIncrement is false at the skill cap', () => {
		const working = { ...base, might: SKILL_CAP };
		expect(canIncrement(base, working, 'might', 99)).toBe(false);
	});

	it('canDecrement is false at the starting value', () => {
		const start = { ...base, might: 3 };
		expect(canDecrement(start, { ...start }, 'might')).toBe(false);
		expect(canDecrement(start, { ...start, might: 4 }, 'might')).toBe(true);
	});

	it('canFinalize requires the full budget spent', () => {
		expect(canFinalize(base, { ...base, might: 1 }, 2)).toBe(false);
		expect(canFinalize(base, { ...base, might: 2 }, 2)).toBe(true);
	});
});
```

- [ ] **Step 3: Run the tests.**

Run (from `NS.Client/`): `npm test`
Expected: PASS (6 new tests).

- [ ] **Step 4: Commit.**

```bash
git add NS.Client/src/lib/sheet/levelUp/skillAllocation.ts NS.Client/src/lib/sheet/levelUp/skillAllocation.test.ts
git commit -m "feat(client): pure skill-allocation helper for level-up

Co-Authored-By: Claude Opus 4.8 <noreply@anthropic.com>"
```

---

## Task 4: `LevelUpControls` component + wire into `HeroSheet`

**Files:** Create `src/lib/sheet/components/LevelUpControls.svelte`; Modify `HeroSheet.svelte`

- [ ] **Step 1: Create `LevelUpControls.svelte`:**

```svelte
<script lang="ts">
	import { getContext } from 'svelte';
	import type { HeroSkills, StatType } from '$lib/api/types';
	import type { SheetViewModel } from '../viewmodel';
	import { HERO_ACTIONS, type HeroActions } from '../heroActions.svelte';
	import { editorButton } from './styles';
	import TilePopover from './TilePopover.svelte';
	import { SKILLS, spentPoints, canIncrement, canDecrement, canFinalize } from '../levelUp/skillAllocation';

	let { vm }: { vm: SheetViewModel } = $props();

	const actions = getContext<HeroActions | undefined>(HERO_ACTIONS);

	const stats: StatType[] = ['Strength', 'Dexterity', 'Intelligence', 'Will'];

	let hpGained = $state(0);
	let subclass = $state('');
	// svelte-ignore state_referenced_locally
	let working = $state<HeroSkills>({ ...vm.skillValues });

	async function confirmLevelUp() {
		if (!actions) return;
		await actions.levelUp(hpGained);
		hpGained = 0;
	}

	function resetSkills() {
		working = { ...vm.skillValues };
	}

	async function finalizeSkills() {
		if (!actions) return;
		await actions.finalizeSkillAllocation({ ...working });
	}

	async function confirmSubclass() {
		if (!actions || subclass.trim() === '') return;
		await actions.setSubclass(subclass.trim());
		subclass = '';
	}

	const pending = 'bg-amber-700 hover:bg-amber-600';
</script>

{#if actions}
	<TilePopover label="Level up" onopen={() => (hpGained = 0)}>
		{#snippet trigger()}<span class={editorButton}>Level Up</span>{/snippet}
		{#snippet content()}
			<p class="mb-1 text-[11px] text-slate-300">Level up to {vm.level + 1}</p>
			<label class="block text-xs text-slate-300">HP gained
				<input type="number" min="0" bind:value={hpGained} class="mt-1 w-full rounded bg-slate-900 px-1.5 py-1 text-xs text-white" aria-label="HP gained" />
			</label>
			<button type="button" class={`${editorButton} mt-2 w-full`} disabled={actions.busy} onclick={confirmLevelUp}>Confirm level up</button>
			{#if actions.error}<p class="mt-1 text-[11px] text-red-400">{actions.error}</p>{/if}
		{/snippet}
	</TilePopover>

	{#if vm.pendingStatIncrease}
		<TilePopover label="Choose stat increase">
			{#snippet trigger()}<span class={`${editorButton} ${pending}`}>Stat +1</span>{/snippet}
			{#snippet content()}
				<p class="mb-1 text-[11px] text-slate-300">Choose a stat to increase</p>
				<div class="grid grid-cols-2 gap-1">
					{#each stats as s (s)}
						<button type="button" class={editorButton} disabled={actions.busy} onclick={() => actions.applyStatIncrease(s)}>{s.slice(0, 3).toUpperCase()}</button>
					{/each}
				</div>
				{#if actions.error}<p class="mt-1 text-[11px] text-red-400">{actions.error}</p>{/if}
			{/snippet}
		</TilePopover>
	{/if}

	{#if vm.unspentSkillPoints > 0}
		<TilePopover label="Allocate skill points" onopen={resetSkills}>
			{#snippet trigger()}<span class={`${editorButton} ${pending}`}>Skills +{vm.unspentSkillPoints}</span>{/snippet}
			{#snippet content()}
				<p class="mb-1 text-[11px] text-slate-300">Spent {spentPoints(vm.skillValues, working)} of {vm.unspentSkillPoints}</p>
				<div class="max-h-48 space-y-1 overflow-y-auto pr-1">
					{#each SKILLS as { key, label } (key)}
						<div class="flex items-center justify-between gap-1 text-xs text-slate-200">
							<span>{label}</span>
							<div class="flex items-center gap-1">
								<button type="button" class={editorButton} disabled={!canDecrement(vm.skillValues, working, key)} onclick={() => (working[key] -= 1)}>−</button>
								<span class="min-w-5 text-center">{working[key]}</span>
								<button type="button" class={editorButton} disabled={!canIncrement(vm.skillValues, working, key, vm.unspentSkillPoints)} onclick={() => (working[key] += 1)}>+</button>
							</div>
						</div>
					{/each}
				</div>
				<button type="button" class={`${editorButton} mt-2 w-full`} disabled={actions.busy || !canFinalize(vm.skillValues, working, vm.unspentSkillPoints)} onclick={finalizeSkills}>Finalize</button>
				{#if actions.error}<p class="mt-1 text-[11px] text-red-400">{actions.error}</p>{/if}
			{/snippet}
		</TilePopover>
	{/if}

	{#if vm.needsSubclass}
		<TilePopover label="Choose subclass" onopen={() => (subclass = '')}>
			{#snippet trigger()}<span class={`${editorButton} ${pending}`}>Subclass</span>{/snippet}
			{#snippet content()}
				<input type="text" bind:value={subclass} placeholder="Subclass name" class="w-full rounded bg-slate-900 px-1.5 py-1 text-xs text-white" aria-label="Subclass name" />
				<button type="button" class={`${editorButton} mt-2 w-full`} disabled={actions.busy || subclass.trim() === ''} onclick={confirmSubclass}>Set subclass</button>
				{#if actions.error}<p class="mt-1 text-[11px] text-red-400">{actions.error}</p>{/if}
			{/snippet}
		</TilePopover>
	{/if}
{/if}
```

(Each `TilePopover` renders a `relative` div, so they lay out as siblings in the flex row created in `HeroSheet`. The component renders nothing when there is no actions context.)

- [ ] **Step 2: Wire into `HeroSheet.svelte`.** Add the import and place `<LevelUpControls {vm} />` before `<RestButton />` in the existing justify-end row, widening it to wrap:

```svelte
  import RestButton from './RestButton.svelte';
  import LevelUpControls from './LevelUpControls.svelte';
```

```svelte
    <div class="flex flex-wrap items-center justify-end gap-2">
      <LevelUpControls {vm} />
      <RestButton />
    </div>
```

(Replaces the existing `<div class="flex justify-end"><RestButton /></div>`.)

- [ ] **Step 3: Verify.** From `NS.Client/`: `npm run check` (0 errors/0 warnings), `npm test` (all pass), `npm run build` (success).

- [ ] **Step 4: Commit.**

```bash
git add NS.Client/src/lib/sheet/components/LevelUpControls.svelte NS.Client/src/lib/sheet/components/HeroSheet.svelte
git commit -m "feat: inline level-up controls on the sheet

Co-Authored-By: Claude Opus 4.8 <noreply@anthropic.com>"
```

---

## Task 5: Browser verification + docs

**Files:** Modify `CLAUDE.md`

- [ ] **Step 1: Build the SPA into wwwroot and run the server** (plain `dotnet build` skips the SPA rebuild when `wwwroot/index.html` exists — rebuild explicitly):

```bash
cd NS.Client && npm run build && cd ..
rm -rf NS.WebApp/wwwroot && mkdir -p NS.WebApp/wwwroot && cp -r NS.Client/build/* NS.WebApp/wwwroot/
rm -f NS.WebApp/nimble-sheet.db
ASPNETCORE_ENVIRONMENT=Development dotnet run --project NS.WebApp/NS.WebApp.csproj --no-launch-profile
```
Server listens on `http://localhost:5000`.

- [ ] **Step 2: Drive the flow** (Playwright headless Chromium; reuse the scratch dir with `playwright` installed, `npx playwright install chromium` if needed). Steps: login → create user → create a hero → open the sheet. Then:
  1. Click **Level Up** → enter an HP gained value (e.g. 5) → Confirm. Re-fetched sheet should show level 2, HIT DICE max `2`, and HP raised by 5 (max and current).
  2. The **Stat +1** affordance should now be visible → open → click a stat (e.g. STR) → that stat increases by 1 and the affordance disappears.
  3. The **Skills +1** affordance should be visible → open → `+` a skill once (Finalize stays disabled until the 1 point is spent) → Finalize → that skill's bonus increases by 1 and the affordance disappears.
  4. Level up twice more to reach level 3 (entering HP each time), resolving stat/skill each time → the **Subclass** affordance should appear → open → type a subclass name → Set → the banner shows the subclass and the affordance disappears.
  Capture console/network errors and screenshots of: the pending affordances visible, and the post-resolution sheet.

- [ ] **Step 3: Record results.** PASS only if level/HP/hit-dice update, stat/skill/subclass resolve and their affordances clear, with no JS/page errors. Stop the server when done.

- [ ] **Step 4: Update `CLAUDE.md`.** In the NS.Client section, add a "Level-up flow" bullet (near the Live-play mutations / Collection editing bullets): `LevelUpControls.svelte` (next to Rest) exposes a Level Up popover (manual HP entry → applies HP then increments level in one action) and surfaces the resulting pending state as separate popovers — choose stat increase, allocate skill points (pure helper `levelUp/skillAllocation.ts`, enforces the +12 cap and requires all points spent), and choose subclass at L3. Note pending *feature* choices are out of scope (features added via `FeatureEditor`) and HP is entered manually. Also update the "Shipped" line for the current date.

- [ ] **Step 5: Commit.**

```bash
git add CLAUDE.md
git commit -m "docs: document the level-up flow

Co-Authored-By: Claude Opus 4.8 <noreply@anthropic.com>"
```

- [ ] **Step 6: Finish the branch.** Use the `superpowers:finishing-a-development-branch` skill (verify tests, then merge `feat/level-up-flow` to `main` with `--no-ff`, delete the branch; the user pushes).

---

## Self-Review

**Spec coverage:** Level Up control with manual HP, applying HP then incrementing level in one action (Task 1 composite + Task 4 popover) ✓; choose stat increase (Task 4 + `applyStatIncrease`) ✓; allocate skills with +12 cap and all-points-required, replacing skills (Task 3 helper + Task 4 + `finalizeSkillAllocation`) ✓; subclass free-text at L3 (`needsSubclass` + Task 4 + `setSubclass`) ✓; inline server-authoritative affordances, optional context (Task 4 `{#if actions}`) ✓; pending feature choices out of scope (empty `pendingChoices` in `levelUp` wrapper) ✓; view-model fields (Task 2) ✓; tests — wrappers + resolver + pure helper (Tasks 1-3); composite/component covered by browser verification (Task 5, noted in Reference facts) ✓; no server/domain changes ✓.

**Placeholder scan:** No TBD/TODO. The resolver test uses `REF` with an explicit instruction to substitute the file's real bundle variable name. All code steps show full code.

**Type consistency:** `levelUp(hpIncrease: number)`, `applyStatIncrease(stat: string)`, `finalizeSkillAllocation(skills: HeroSkills)`, `setSubclass(subclass: string)` match across the `HeroActions` interface, the factory, and the `LevelUpControls` call sites. Wrapper signatures (`levelUp(heroId)`, `applyHpIncrease(heroId, amount)`, etc.) match the factory call sites; `levelUp` is aliased to `levelUpRequest` in `heroActions` to avoid the name clash with the action method. View-model field names (`pendingStatIncrease`/`unspentSkillPoints`/`needsSubclass`/`skillValues`) match between `viewmodel.ts`, `resolve.ts`, and `LevelUpControls`. Helper signatures (`spentPoints`/`canIncrement`/`canDecrement`/`canFinalize` taking `(start, working, …)`) match the component usage and the tests. `SKILLS` keys are valid `keyof HeroSkills`.
