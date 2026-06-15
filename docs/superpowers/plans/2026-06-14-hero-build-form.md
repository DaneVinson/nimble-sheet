# Hero Build Form Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a shared client form to create (`/heroes/new`) and edit (`/heroes/[id]/edit`) a hero's build attributes, mapping to the existing `HeroBuildRequest` API.

**Architecture:** NS.Client is a pure SPA, so submission is client-side (`apiFetch` → `goto`). A single `HeroBuildForm.svelte` (single sectioned page, Layout A) serves both routes; thin route pages supply the initial model (`blankBuildModel()` or `heroToBuildModel(hero)`) and reference data, and wire submit/navigate. Build fields only — collections are managed elsewhere and preserved server-side.

**Tech Stack:** SvelteKit 2 / Svelte 5 runes (`$state`, `$props`, `$bindable`), TypeScript, Tailwind v4, Vitest. Spec: `docs/superpowers/specs/2026-06-14-hero-build-form-design.md`.

**Project conventions:** Standard SvelteKit/TypeScript idioms (NOT the C# conventions). Tabs for indentation. Each feature task ends with `npm run check` (must be `0 errors and 0 warnings`) run from `C:\Development\repos\GitHub\nimble-sheet\NS.Client`. Tests come AFTER implementation (Task 11), not TDD. Components are not unit-tested (no component harness); only the pure model/validate logic and API wrappers are.

---

## File Structure

**Create:**
- `NS.Client/src/lib/sheet/build/model.ts` — `HeroBuildModel`, `blankBuildModel()`, `heroToBuildModel()`.
- `NS.Client/src/lib/sheet/build/validate.ts` — `BuildErrors`, `validateBuild()`.
- `NS.Client/src/lib/sheet/build/options.ts` — enum option arrays.
- `NS.Client/src/lib/sheet/build/IdentitySection.svelte`, `VitalsSection.svelte`, `CombatSection.svelte`, `StatsSection.svelte`, `SavesSection.svelte`, `SkillsSection.svelte`, `ClassResourcesSection.svelte`.
- `NS.Client/src/lib/sheet/build/HeroBuildForm.svelte`.
- `NS.Client/src/routes/(app)/heroes/new/+page.ts`, `+page.svelte`.
- `NS.Client/src/routes/(app)/heroes/[id]/edit/+page.ts`, `+page.svelte`, `+error.svelte`.
- `NS.Client/src/lib/sheet/build/model.test.ts`, `validate.test.ts`.

**Modify:**
- `NS.Client/src/lib/api/client.ts` — `createHero`, `updateHero`.
- `NS.Client/src/lib/api/client.test.ts` — wrapper tests.
- `NS.Client/src/routes/(app)/heroes/+page.svelte` — "New hero" link.
- `NS.Client/src/routes/(app)/heroes/[id]/+page.svelte` — "Edit" link.

---

### Task 1: Build model

**Files:**
- Create: `NS.Client/src/lib/sheet/build/model.ts`

- [ ] **Step 1: Write the model module**

```ts
import type {
	ClassResources, Hero, HeroClass, HeroCombatStats, HeroSaves, HeroSkills, HeroStats
} from '$lib/api/types';

/** The client-side editable shape of a hero's build attributes (mirrors the API's HeroBuildRequest). */
export interface HeroBuildModel {
	name: string;
	ancestryId: string;
	backgroundId: string | null;
	heroClass: HeroClass;
	maxHp: number;
	maxMana: number | null;
	combatStats: HeroCombatStats;
	resources: ClassResources;
	saves: HeroSaves;
	skills: HeroSkills;
	stats: HeroStats;
}

/** A level-1 default build for the create form. */
export function blankBuildModel(): HeroBuildModel {
	return {
		name: '',
		ancestryId: '',
		backgroundId: null,
		heroClass: 'Berserker',
		maxHp: 1,
		maxMana: null,
		combatStats: { armor: 0, hitDieType: 'D8', initiativeBonus: 0, speed: 6 },
		resources: {
			judgmentDiceCount: null,
			judgmentDiceType: null,
			layOnHandsPool: null,
			thrillCharges: null
		},
		saves: { advantageOn: 'Strength', disadvantageOn: 'Dexterity' },
		skills: {
			arcana: 0, examination: 0, finesse: 0, influence: 0, insight: 0,
			lore: 0, might: 0, naturecraft: 0, perception: 0, stealth: 0
		},
		stats: { dexterity: 0, intelligence: 0, strength: 0, will: 0 }
	};
}

/** Map a loaded hero's build fields onto an editable model (independent nested copies) for the edit form. */
export function heroToBuildModel(hero: Hero): HeroBuildModel {
	return {
		name: hero.name,
		ancestryId: hero.ancestryId,
		backgroundId: hero.backgroundId,
		heroClass: hero.class,
		maxHp: hero.maxHp,
		maxMana: hero.maxMana,
		combatStats: { ...hero.combatStats },
		resources: { ...hero.resources },
		saves: { ...hero.saves },
		skills: { ...hero.skills },
		stats: { ...hero.stats }
	};
}
```

- [ ] **Step 2: Verify type-check**

Run `npm run check` from `C:\Development\repos\GitHub\nimble-sheet\NS.Client`. Expected: `0 errors and 0 warnings`.

- [ ] **Step 3: Commit**

```bash
git add NS.Client/src/lib/sheet/build/model.ts
git commit -m "feat(client): add hero build model + mappers"
```

---

### Task 2: Validation

**Files:**
- Create: `NS.Client/src/lib/sheet/build/validate.ts`

- [ ] **Step 1: Write the validator**

```ts
import type { HeroBuildModel } from './model';

/** Field-keyed validation messages for the required build fields. */
export type BuildErrors = Partial<Record<'name' | 'ancestryId' | 'maxHp' | 'maxMana', string>>;

/** Validate the required build fields; everything else defers to the server. */
export function validateBuild(model: HeroBuildModel): BuildErrors {
	const errors: BuildErrors = {};
	if (model.name.trim() === '') {
		errors.name = 'Name is required.';
	}
	if (model.ancestryId === '') {
		errors.ancestryId = 'Select an ancestry.';
	}
	if (!(model.maxHp > 0)) {
		errors.maxHp = 'Max HP must be greater than 0.';
	}
	if (model.maxMana !== null && model.maxMana < 0) {
		errors.maxMana = 'Max mana cannot be negative.';
	}
	return errors;
}
```

- [ ] **Step 2: Verify type-check** — `npm run check`. Expected: `0 errors and 0 warnings`.

- [ ] **Step 3: Commit**

```bash
git add NS.Client/src/lib/sheet/build/validate.ts
git commit -m "feat(client): add hero build validation"
```

---

### Task 3: API wrappers

**Files:**
- Modify: `NS.Client/src/lib/api/client.ts`

- [ ] **Step 1: Append the wrappers**

Add at the END of `NS.Client/src/lib/api/client.ts` (after `recoverAll`). Add the type import at the top of the file alongside the existing `import type { Hero } from './types';` line — add a new line `import type { HeroBuildModel } from '$lib/sheet/build/model';` (type-only, no runtime cycle).

```ts
/** POST /heroes — create a hero from build attributes; returns the new id. */
export function createHero(build: HeroBuildModel): Promise<{ id: string }> {
	return apiFetch<{ id: string }>('/heroes', {
		method: 'POST',
		body: JSON.stringify(build)
	});
}

/** PUT /heroes/{id} — update a hero's build attributes. */
export function updateHero(id: string, build: HeroBuildModel): Promise<void> {
	return apiFetch<void>(`/heroes/${id}`, {
		method: 'PUT',
		body: JSON.stringify(build)
	});
}
```

- [ ] **Step 2: Verify type-check** — `npm run check`. Expected: `0 errors and 0 warnings`.

- [ ] **Step 3: Commit**

```bash
git add NS.Client/src/lib/api/client.ts
git commit -m "feat(client): add createHero/updateHero API wrappers"
```

---

### Task 4: Options + Identity & Vitals sections

**Files:**
- Create: `NS.Client/src/lib/sheet/build/options.ts`
- Create: `NS.Client/src/lib/sheet/build/IdentitySection.svelte`
- Create: `NS.Client/src/lib/sheet/build/VitalsSection.svelte`

- [ ] **Step 1: Create the option arrays**

`NS.Client/src/lib/sheet/build/options.ts`:

```ts
import type { DieType, HeroClass, StatType } from '$lib/api/types';

/** All selectable hero classes, in domain order. */
export const heroClasses: HeroClass[] = [
	'Berserker', 'Cheat', 'Commander', 'Hunter', 'Mage', 'Oathsworn',
	'Shadowmancer', 'Shepherd', 'Songweaver', 'Stormshifter', 'Zephyr'
];

/** All hit die types. */
export const dieTypes: DieType[] = ['D4', 'D6', 'D8', 'D10', 'D12'];

/** All stat types (for saves). */
export const statTypes: StatType[] = ['Strength', 'Dexterity', 'Intelligence', 'Will'];
```

- [ ] **Step 2: Create IdentitySection**

`NS.Client/src/lib/sheet/build/IdentitySection.svelte`:

```svelte
<script lang="ts">
	import type { Ancestry, Background, HeroClass } from '$lib/api/types';
	import { heroClasses } from './options';

	let {
		name = $bindable(),
		ancestryId = $bindable(),
		backgroundId = $bindable(),
		heroClass = $bindable(),
		ancestries,
		backgrounds,
		errors
	}: {
		name: string;
		ancestryId: string;
		backgroundId: string | null;
		heroClass: HeroClass;
		ancestries: Ancestry[];
		backgrounds: Background[];
		errors: { name?: string; ancestryId?: string };
	} = $props();

	const field = 'mt-1 w-full rounded bg-slate-900 px-2 py-1 text-sm text-white';
	const lbl = 'block text-xs text-slate-400';
</script>

<section class="rounded-lg bg-slate-800 p-4">
	<h2 class="mb-3 text-sm font-semibold uppercase tracking-wide text-slate-300">Identity</h2>
	<div class="grid gap-3 sm:grid-cols-2">
		<label class={lbl}>
			Name
			<input type="text" bind:value={name} class={field} />
			{#if errors.name}<span class="mt-1 block text-[11px] text-red-400">{errors.name}</span>{/if}
		</label>
		<label class={lbl}>
			Class
			<select bind:value={heroClass} class={field}>
				{#each heroClasses as c (c)}<option value={c}>{c}</option>{/each}
			</select>
		</label>
		<label class={lbl}>
			Ancestry
			<select bind:value={ancestryId} class={field}>
				<option value="">— select —</option>
				{#each ancestries as a (a.id)}<option value={a.id}>{a.name}</option>{/each}
			</select>
			{#if errors.ancestryId}<span class="mt-1 block text-[11px] text-red-400">{errors.ancestryId}</span>{/if}
		</label>
		<label class={lbl}>
			Background
			<select bind:value={backgroundId} class={field}>
				<option value={null}>— none —</option>
				{#each backgrounds as b (b.id)}<option value={b.id}>{b.name}</option>{/each}
			</select>
		</label>
	</div>
</section>
```

- [ ] **Step 3: Create VitalsSection**

`NS.Client/src/lib/sheet/build/VitalsSection.svelte`:

```svelte
<script lang="ts">
	let {
		maxHp = $bindable(),
		maxMana = $bindable(),
		errors
	}: {
		maxHp: number;
		maxMana: number | null;
		errors: { maxHp?: string; maxMana?: string };
	} = $props();

	const field = 'mt-1 w-full rounded bg-slate-900 px-2 py-1 text-sm text-white';
	const lbl = 'block text-xs text-slate-400';
</script>

<section class="rounded-lg bg-slate-800 p-4">
	<h2 class="mb-3 text-sm font-semibold uppercase tracking-wide text-slate-300">Vitals</h2>
	<div class="grid gap-3 sm:grid-cols-2">
		<label class={lbl}>
			Max HP
			<input type="number" min="1" bind:value={maxHp} class={field} />
			{#if errors.maxHp}<span class="mt-1 block text-[11px] text-red-400">{errors.maxHp}</span>{/if}
		</label>
		<label class={lbl}>
			Max mana (casters — leave blank if none)
			<input type="number" min="0" bind:value={maxMana} class={field} />
			{#if errors.maxMana}<span class="mt-1 block text-[11px] text-red-400">{errors.maxMana}</span>{/if}
		</label>
	</div>
</section>
```

- [ ] **Step 4: Verify type-check** — `npm run check`. Expected: `0 errors and 0 warnings`.

- [ ] **Step 5: Commit**

```bash
git add NS.Client/src/lib/sheet/build/options.ts NS.Client/src/lib/sheet/build/IdentitySection.svelte NS.Client/src/lib/sheet/build/VitalsSection.svelte
git commit -m "feat(client): add build options + identity/vitals sections"
```

---

### Task 5: Combat, Stats & Saves sections

**Files:**
- Create: `NS.Client/src/lib/sheet/build/CombatSection.svelte`
- Create: `NS.Client/src/lib/sheet/build/StatsSection.svelte`
- Create: `NS.Client/src/lib/sheet/build/SavesSection.svelte`

- [ ] **Step 1: CombatSection**

`NS.Client/src/lib/sheet/build/CombatSection.svelte`:

```svelte
<script lang="ts">
	import type { HeroCombatStats } from '$lib/api/types';
	import { dieTypes } from './options';

	let { combatStats = $bindable() }: { combatStats: HeroCombatStats } = $props();

	const field = 'mt-1 w-full rounded bg-slate-900 px-2 py-1 text-sm text-white';
	const lbl = 'block text-xs text-slate-400';
</script>

<section class="rounded-lg bg-slate-800 p-4">
	<h2 class="mb-3 text-sm font-semibold uppercase tracking-wide text-slate-300">Combat</h2>
	<div class="grid grid-cols-2 gap-3 sm:grid-cols-4">
		<label class={lbl}>Armor
			<input type="number" bind:value={combatStats.armor} class={field} />
		</label>
		<label class={lbl}>Hit die
			<select bind:value={combatStats.hitDieType} class={field}>
				{#each dieTypes as d (d)}<option value={d}>{d}</option>{/each}
			</select>
		</label>
		<label class={lbl}>Initiative
			<input type="number" bind:value={combatStats.initiativeBonus} class={field} />
		</label>
		<label class={lbl}>Speed
			<input type="number" bind:value={combatStats.speed} class={field} />
		</label>
	</div>
</section>
```

- [ ] **Step 2: StatsSection**

`NS.Client/src/lib/sheet/build/StatsSection.svelte`:

```svelte
<script lang="ts">
	import type { HeroStats } from '$lib/api/types';

	let { stats = $bindable() }: { stats: HeroStats } = $props();

	const field = 'mt-1 w-full rounded bg-slate-900 px-2 py-1 text-sm text-white';
	const lbl = 'block text-xs text-slate-400';
</script>

<section class="rounded-lg bg-slate-800 p-4">
	<h2 class="mb-3 text-sm font-semibold uppercase tracking-wide text-slate-300">Stats</h2>
	<div class="grid grid-cols-2 gap-3 sm:grid-cols-4">
		<label class={lbl}>STR
			<input type="number" bind:value={stats.strength} class={field} />
		</label>
		<label class={lbl}>DEX
			<input type="number" bind:value={stats.dexterity} class={field} />
		</label>
		<label class={lbl}>INT
			<input type="number" bind:value={stats.intelligence} class={field} />
		</label>
		<label class={lbl}>WIL
			<input type="number" bind:value={stats.will} class={field} />
		</label>
	</div>
</section>
```

- [ ] **Step 3: SavesSection**

`NS.Client/src/lib/sheet/build/SavesSection.svelte`:

```svelte
<script lang="ts">
	import type { HeroSaves } from '$lib/api/types';
	import { statTypes } from './options';

	let { saves = $bindable() }: { saves: HeroSaves } = $props();

	const field = 'mt-1 w-full rounded bg-slate-900 px-2 py-1 text-sm text-white';
	const lbl = 'block text-xs text-slate-400';
</script>

<section class="rounded-lg bg-slate-800 p-4">
	<h2 class="mb-3 text-sm font-semibold uppercase tracking-wide text-slate-300">Saves</h2>
	<div class="grid gap-3 sm:grid-cols-2">
		<label class={lbl}>Advantage on
			<select bind:value={saves.advantageOn} class={field}>
				{#each statTypes as s (s)}<option value={s}>{s}</option>{/each}
			</select>
		</label>
		<label class={lbl}>Disadvantage on
			<select bind:value={saves.disadvantageOn} class={field}>
				{#each statTypes as s (s)}<option value={s}>{s}</option>{/each}
			</select>
		</label>
	</div>
</section>
```

- [ ] **Step 4: Verify type-check** — `npm run check`. Expected: `0 errors and 0 warnings`.

- [ ] **Step 5: Commit**

```bash
git add NS.Client/src/lib/sheet/build/CombatSection.svelte NS.Client/src/lib/sheet/build/StatsSection.svelte NS.Client/src/lib/sheet/build/SavesSection.svelte
git commit -m "feat(client): add combat/stats/saves sections"
```

---

### Task 6: Skills & Class Resources sections

**Files:**
- Create: `NS.Client/src/lib/sheet/build/SkillsSection.svelte`
- Create: `NS.Client/src/lib/sheet/build/ClassResourcesSection.svelte`

- [ ] **Step 1: SkillsSection** (the ten skills are enumerated explicitly — do not use a computed `bind:value={skills[key]}`)

`NS.Client/src/lib/sheet/build/SkillsSection.svelte`:

```svelte
<script lang="ts">
	import type { HeroSkills } from '$lib/api/types';

	let { skills = $bindable() }: { skills: HeroSkills } = $props();

	const field = 'mt-1 w-full rounded bg-slate-900 px-2 py-1 text-sm text-white';
	const lbl = 'block text-xs text-slate-400';
</script>

<section class="rounded-lg bg-slate-800 p-4">
	<h2 class="mb-3 text-sm font-semibold uppercase tracking-wide text-slate-300">Skills</h2>
	<div class="grid grid-cols-2 gap-3 sm:grid-cols-5">
		<label class={lbl}>Arcana<input type="number" bind:value={skills.arcana} class={field} /></label>
		<label class={lbl}>Examination<input type="number" bind:value={skills.examination} class={field} /></label>
		<label class={lbl}>Finesse<input type="number" bind:value={skills.finesse} class={field} /></label>
		<label class={lbl}>Influence<input type="number" bind:value={skills.influence} class={field} /></label>
		<label class={lbl}>Insight<input type="number" bind:value={skills.insight} class={field} /></label>
		<label class={lbl}>Lore<input type="number" bind:value={skills.lore} class={field} /></label>
		<label class={lbl}>Might<input type="number" bind:value={skills.might} class={field} /></label>
		<label class={lbl}>Naturecraft<input type="number" bind:value={skills.naturecraft} class={field} /></label>
		<label class={lbl}>Perception<input type="number" bind:value={skills.perception} class={field} /></label>
		<label class={lbl}>Stealth<input type="number" bind:value={skills.stealth} class={field} /></label>
	</div>
</section>
```

- [ ] **Step 2: ClassResourcesSection** (all fields optional; empty number inputs bind to `null`)

`NS.Client/src/lib/sheet/build/ClassResourcesSection.svelte`:

```svelte
<script lang="ts">
	import type { ClassResources } from '$lib/api/types';
	import { dieTypes } from './options';

	let { resources = $bindable() }: { resources: ClassResources } = $props();

	const field = 'mt-1 w-full rounded bg-slate-900 px-2 py-1 text-sm text-white';
	const lbl = 'block text-xs text-slate-400';
</script>

<section class="rounded-lg bg-slate-800 p-4">
	<h2 class="mb-1 text-sm font-semibold uppercase tracking-wide text-slate-300">Class Resources</h2>
	<p class="mb-3 text-[11px] text-slate-500">Leave blank any field not used by your class.</p>
	<div class="grid grid-cols-2 gap-3 sm:grid-cols-4">
		<label class={lbl}>Judgment dice count
			<input type="number" min="0" bind:value={resources.judgmentDiceCount} class={field} />
		</label>
		<label class={lbl}>Judgment die type
			<select bind:value={resources.judgmentDiceType} class={field}>
				<option value={null}>— none —</option>
				{#each dieTypes as d (d)}<option value={d}>{d}</option>{/each}
			</select>
		</label>
		<label class={lbl}>Lay on hands pool
			<input type="number" min="0" bind:value={resources.layOnHandsPool} class={field} />
		</label>
		<label class={lbl}>Thrill charges
			<input type="number" min="0" bind:value={resources.thrillCharges} class={field} />
		</label>
	</div>
</section>
```

- [ ] **Step 3: Verify type-check** — `npm run check`. Expected: `0 errors and 0 warnings`. (If `bind:value` to a `number | null` resource field is flagged, confirm the field input has no `required`; empty maps to `null`, which matches the type.)

- [ ] **Step 4: Commit**

```bash
git add NS.Client/src/lib/sheet/build/SkillsSection.svelte NS.Client/src/lib/sheet/build/ClassResourcesSection.svelte
git commit -m "feat(client): add skills/class-resources sections"
```

---

### Task 7: HeroBuildForm

**Files:**
- Create: `NS.Client/src/lib/sheet/build/HeroBuildForm.svelte`

- [ ] **Step 1: Compose the form**

`NS.Client/src/lib/sheet/build/HeroBuildForm.svelte`:

```svelte
<script lang="ts">
	import type { Ancestry, Background } from '$lib/api/types';
	import { ApiError } from '$lib/api/client';
	import type { HeroBuildModel } from './model';
	import { validateBuild, type BuildErrors } from './validate';
	import IdentitySection from './IdentitySection.svelte';
	import VitalsSection from './VitalsSection.svelte';
	import CombatSection from './CombatSection.svelte';
	import StatsSection from './StatsSection.svelte';
	import SavesSection from './SavesSection.svelte';
	import SkillsSection from './SkillsSection.svelte';
	import ClassResourcesSection from './ClassResourcesSection.svelte';

	let {
		initial,
		ancestries,
		backgrounds,
		submitLabel,
		onsubmit
	}: {
		initial: HeroBuildModel;
		ancestries: Ancestry[];
		backgrounds: Background[];
		submitLabel: string;
		onsubmit: (model: HeroBuildModel) => Promise<void>;
	} = $props();

	let model = $state<HeroBuildModel>(structuredClone(initial));
	let errors = $state<BuildErrors>({});
	let busy = $state(false);
	let formError = $state<string | null>(null);

	async function handleSubmit(event: SubmitEvent) {
		event.preventDefault();
		errors = validateBuild(model);
		if (Object.keys(errors).length > 0) {
			return;
		}
		busy = true;
		formError = null;
		try {
			await onsubmit($state.snapshot(model) as HeroBuildModel);
		} catch (e) {
			formError = e instanceof ApiError ? e.message : 'Save failed.';
		} finally {
			busy = false;
		}
	}
</script>

<form onsubmit={handleSubmit} class="mx-auto max-w-3xl space-y-4 px-4 py-8">
	<IdentitySection
		bind:name={model.name}
		bind:ancestryId={model.ancestryId}
		bind:backgroundId={model.backgroundId}
		bind:heroClass={model.heroClass}
		{ancestries}
		{backgrounds}
		{errors}
	/>
	<VitalsSection bind:maxHp={model.maxHp} bind:maxMana={model.maxMana} {errors} />
	<CombatSection bind:combatStats={model.combatStats} />
	<StatsSection bind:stats={model.stats} />
	<SavesSection bind:saves={model.saves} />
	<SkillsSection bind:skills={model.skills} />
	<ClassResourcesSection bind:resources={model.resources} />

	{#if formError}<p class="text-sm text-red-400">{formError}</p>{/if}
	<button
		type="submit"
		disabled={busy}
		class="rounded bg-blue-700 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-600 disabled:opacity-50"
	>
		{submitLabel}
	</button>
</form>
```

- [ ] **Step 2: Verify type-check** — `npm run check`. Expected: `0 errors and 0 warnings`. (`structuredClone(initial)` deep-copies the plain load data so editing the form never mutates `data`. `$state.snapshot` unwraps the proxy before the API call.)

- [ ] **Step 3: Commit**

```bash
git add NS.Client/src/lib/sheet/build/HeroBuildForm.svelte
git commit -m "feat(client): add HeroBuildForm composition"
```

---

### Task 8: Create route (/heroes/new)

**Files:**
- Create: `NS.Client/src/routes/(app)/heroes/new/+page.ts`
- Create: `NS.Client/src/routes/(app)/heroes/new/+page.svelte`

- [ ] **Step 1: Load ancestries + backgrounds**

`NS.Client/src/routes/(app)/heroes/new/+page.ts`:

```ts
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
```

- [ ] **Step 2: Render the create form**

`NS.Client/src/routes/(app)/heroes/new/+page.svelte`:

```svelte
<script lang="ts">
	import { goto } from '$app/navigation';
	import HeroBuildForm from '$lib/sheet/build/HeroBuildForm.svelte';
	import { blankBuildModel, type HeroBuildModel } from '$lib/sheet/build/model';
	import { createHero } from '$lib/api/client';

	let { data } = $props();

	async function submit(model: HeroBuildModel) {
		const { id } = await createHero(model);
		await goto(`/heroes/${id}`);
	}
</script>

<svelte:head><title>New hero — NimbleSheets</title></svelte:head>

<HeroBuildForm
	initial={blankBuildModel()}
	ancestries={data.ancestries}
	backgrounds={data.backgrounds}
	submitLabel="Create hero"
	onsubmit={submit}
/>
```

- [ ] **Step 3: Verify type-check** — `npm run check`. Expected: `0 errors and 0 warnings`.

- [ ] **Step 4: Commit**

```bash
git add "NS.Client/src/routes/(app)/heroes/new/+page.ts" "NS.Client/src/routes/(app)/heroes/new/+page.svelte"
git commit -m "feat(client): add /heroes/new create page"
```

---

### Task 9: Edit route (/heroes/[id]/edit)

**Files:**
- Create: `NS.Client/src/routes/(app)/heroes/[id]/edit/+page.ts`
- Create: `NS.Client/src/routes/(app)/heroes/[id]/edit/+page.svelte`
- Create: `NS.Client/src/routes/(app)/heroes/[id]/edit/+error.svelte`

- [ ] **Step 1: Load the hero + reference, mapping 404**

`NS.Client/src/routes/(app)/heroes/[id]/edit/+page.ts`:

```ts
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
```

- [ ] **Step 2: Render the edit form**

`NS.Client/src/routes/(app)/heroes/[id]/edit/+page.svelte`:

```svelte
<script lang="ts">
	import { goto } from '$app/navigation';
	import HeroBuildForm from '$lib/sheet/build/HeroBuildForm.svelte';
	import { heroToBuildModel, type HeroBuildModel } from '$lib/sheet/build/model';
	import { updateHero } from '$lib/api/client';

	let { data } = $props();

	async function submit(model: HeroBuildModel) {
		await updateHero(data.hero.id, model);
		await goto(`/heroes/${data.hero.id}`);
	}
</script>

<svelte:head><title>Edit {data.hero.name} — NimbleSheets</title></svelte:head>

<HeroBuildForm
	initial={heroToBuildModel(data.hero)}
	ancestries={data.ancestries}
	backgrounds={data.backgrounds}
	submitLabel="Save changes"
	onsubmit={submit}
/>
```

- [ ] **Step 3: Add the 404 boundary** (identical to the existing `[id]/+error.svelte`)

`NS.Client/src/routes/(app)/heroes/[id]/edit/+error.svelte`:

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

- [ ] **Step 4: Verify type-check** — `npm run check`. Expected: `0 errors and 0 warnings`.

- [ ] **Step 5: Commit**

```bash
git add "NS.Client/src/routes/(app)/heroes/[id]/edit/+page.ts" "NS.Client/src/routes/(app)/heroes/[id]/edit/+page.svelte" "NS.Client/src/routes/(app)/heroes/[id]/edit/+error.svelte"
git commit -m "feat(client): add /heroes/[id]/edit page"
```

---

### Task 10: Navigation entry points

**Files:**
- Modify: `NS.Client/src/routes/(app)/heroes/+page.svelte`
- Modify: `NS.Client/src/routes/(app)/heroes/[id]/+page.svelte`

- [ ] **Step 1: "New hero" on the heroes list**

In `NS.Client/src/routes/(app)/heroes/+page.svelte`, replace this line:

```svelte
	<h1 class="mb-6 text-2xl font-bold text-white">Your Heroes</h1>
```

with:

```svelte
	<div class="mb-6 flex items-center justify-between">
		<h1 class="text-2xl font-bold text-white">Your Heroes</h1>
		<a href="/heroes/new" class="rounded bg-blue-700 px-3 py-1.5 text-sm font-semibold text-white hover:bg-blue-600">New hero</a>
	</div>
```

- [ ] **Step 2: "Edit" on the hero sheet page**

In `NS.Client/src/routes/(app)/heroes/[id]/+page.svelte`, replace the body `<div>` block:

```svelte
<div class="px-4 py-8">
	{#key data.heroId}
		<HeroActionsScope vm={data.vm} heroId={data.heroId} />
	{/key}
</div>
```

with:

```svelte
<div class="px-4 py-8">
	<div class="mx-auto mb-3 flex max-w-3xl justify-end">
		<a
			href={`/heroes/${data.heroId}/edit`}
			class="rounded border border-slate-700 bg-slate-800 px-3 py-1 text-xs font-semibold text-slate-200 hover:border-slate-600"
		>
			Edit
		</a>
	</div>
	{#key data.heroId}
		<HeroActionsScope vm={data.vm} heroId={data.heroId} />
	{/key}
</div>
```

- [ ] **Step 3: Verify type-check** — `npm run check`. Expected: `0 errors and 0 warnings`.

- [ ] **Step 4: Commit**

```bash
git add "NS.Client/src/routes/(app)/heroes/+page.svelte" "NS.Client/src/routes/(app)/heroes/[id]/+page.svelte"
git commit -m "feat(client): add new-hero and edit entry points"
```

---

### Task 11: Unit tests (tests-after)

**Files:**
- Create: `NS.Client/src/lib/sheet/build/model.test.ts`
- Create: `NS.Client/src/lib/sheet/build/validate.test.ts`
- Modify: `NS.Client/src/lib/api/client.test.ts`

- [ ] **Step 1: model tests**

`NS.Client/src/lib/sheet/build/model.test.ts`:

```ts
import { describe, expect, it } from 'vitest';
import { blankBuildModel, heroToBuildModel } from './model';
import { caldra } from '$lib/fixtures/caldra';

describe('blankBuildModel', () => {
	it('returns level-1 defaults with empty ancestry and no mana', () => {
		const m = blankBuildModel();
		expect(m.ancestryId).toBe('');
		expect(m.maxHp).toBe(1);
		expect(m.maxMana).toBeNull();
		expect(m.heroClass).toBe('Berserker');
		expect(m.combatStats.hitDieType).toBe('D8');
		expect(m.stats).toEqual({ dexterity: 0, intelligence: 0, strength: 0, will: 0 });
	});
});

describe('heroToBuildModel', () => {
	it('maps every build field from a hero', () => {
		const m = heroToBuildModel(caldra);
		expect(m.name).toBe(caldra.name);
		expect(m.ancestryId).toBe(caldra.ancestryId);
		expect(m.backgroundId).toBe(caldra.backgroundId);
		expect(m.heroClass).toBe(caldra.class);
		expect(m.maxHp).toBe(caldra.maxHp);
		expect(m.maxMana).toBe(caldra.maxMana);
		expect(m.combatStats).toEqual(caldra.combatStats);
		expect(m.resources).toEqual(caldra.resources);
		expect(m.saves).toEqual(caldra.saves);
		expect(m.skills).toEqual(caldra.skills);
		expect(m.stats).toEqual(caldra.stats);
	});

	it('produces independent nested copies', () => {
		const m = heroToBuildModel(caldra);
		m.stats.strength = 99;
		expect(caldra.stats.strength).not.toBe(99);
	});
});
```

- [ ] **Step 2: validate tests**

`NS.Client/src/lib/sheet/build/validate.test.ts`:

```ts
import { describe, expect, it } from 'vitest';
import { validateBuild } from './validate';
import { blankBuildModel } from './model';

function valid() {
	return { ...blankBuildModel(), name: 'Caldra', ancestryId: 'a1', maxHp: 10 };
}

describe('validateBuild', () => {
	it('returns no errors for a complete model', () => {
		expect(validateBuild(valid())).toEqual({});
	});

	it('flags an empty/whitespace name', () => {
		expect(validateBuild({ ...valid(), name: '  ' }).name).toBeDefined();
	});

	it('flags a missing ancestry', () => {
		expect(validateBuild({ ...valid(), ancestryId: '' }).ancestryId).toBeDefined();
	});

	it('flags non-positive maxHp', () => {
		expect(validateBuild({ ...valid(), maxHp: 0 }).maxHp).toBeDefined();
	});

	it('flags negative maxMana but allows null', () => {
		expect(validateBuild({ ...valid(), maxMana: -1 }).maxMana).toBeDefined();
		expect(validateBuild({ ...valid(), maxMana: null }).maxMana).toBeUndefined();
	});
});
```

- [ ] **Step 3: API wrapper tests**

In `NS.Client/src/lib/api/client.test.ts`, extend the import on line 3 to add `createHero` and `updateHero`, add a `blankBuildModel` import after it, and append the describe block.

Change line 3 to:

```ts
import { ApiError, createHero, gainWound, getHeroes, login, spendHitDice, takeDamage, updateHero } from './client';
```

Add after line 4 (`import { clearSession } from '$lib/auth/session';`):

```ts
import { blankBuildModel } from '$lib/sheet/build/model';
```

Append at the end of the file:

```ts
describe('hero build wrappers', () => {
	it('createHero posts the build and returns the new id', async () => {
		const fetchMock = vi.fn(() =>
			Promise.resolve(new Response(JSON.stringify({ id: 'h9' }), { status: 201 }))
		);
		vi.stubGlobal('fetch', fetchMock);
		const model = blankBuildModel();
		await expect(createHero(model)).resolves.toEqual({ id: 'h9' });
		expect(fetchMock).toHaveBeenCalledWith(
			'/heroes',
			expect.objectContaining({ method: 'POST', body: JSON.stringify(model) })
		);
	});

	it('updateHero PUTs to the hero route and resolves on 204', async () => {
		const fetchMock = captureFetch(204);
		const model = blankBuildModel();
		await expect(updateHero('h9', model)).resolves.toBeUndefined();
		expect(fetchMock).toHaveBeenCalledWith(
			'/heroes/h9',
			expect.objectContaining({ method: 'PUT', body: JSON.stringify(model) })
		);
	});
});
```

- [ ] **Step 4: Run the tests**

Run `npm test` from `NS.Client`. Expected: all suites pass — the prior 25 tests plus the new model (3), validate (5), and wrapper (2) tests.

- [ ] **Step 5: Commit**

```bash
git add NS.Client/src/lib/sheet/build/model.test.ts NS.Client/src/lib/sheet/build/validate.test.ts NS.Client/src/lib/api/client.test.ts
git commit -m "test(client): cover build model, validation, and hero wrappers"
```

---

### Task 12: Full verification + browser smoke

**Files:** none.

- [ ] **Step 1: Type-check, build, test**

Run from `NS.Client/`:
```bash
npm run check && npm run build && npm test
```
Expected: check `0 errors and 0 warnings`; build succeeds; all tests pass.

- [ ] **Step 2: Browser smoke (recommended — now fully unblocked by seeding)**

1. Refresh the SPA into the host: delete `NS.WebApp/wwwroot` (so the next build re-copies the SPA), then from `NS.WebApp/`: `dotnet run --launch-profile http` (serves API + SPA on `http://localhost:5197`).
2. Open `http://localhost:5197/login`, create a user, and log in.
3. From `/heroes`, click **New hero**. Fill the form: a name, pick **Human** ancestry (seeded), class **Oathsworn**, Max HP 17, leave mana blank, set a few stats/skills. Click **Create hero**.
4. Expect to land on `/heroes/{id}` showing the new hero; the banner shows the ancestry/class names resolved from seeded data.
5. Click **Edit**, change the name and Max HP, **Save changes**; expect to return to the sheet with the updated values.
6. Stop the server.

Record any deviation.

- [ ] **Step 3: Final commit (only if verification fixups were needed)**

```bash
git add -A
git commit -m "chore(client): verification fixups for hero build form"
```

---

## Notes for the implementer

- **`$bindable` sections:** each section receives a slice of the model via `bind:` and declares those props with `$bindable()`. Mutating a nested field (e.g. `combatStats.armor`) updates the same object the parent's `model` holds — that's intended.
- **Number inputs:** binding a `number` field is fine (precedent: `HpTile`'s `tempInput`). For optional fields typed `number | null` (`maxMana`, the resource fields), an empty input binds to `null`, matching the type — do not add `required`.
- **Don't mutate load data:** `HeroBuildForm` deep-clones `initial` with `structuredClone`, and snapshots with `$state.snapshot` before the API call. Keep both.
- **No collections:** the form never touches weapons/armor/spells/etc.; `UpdateBuild` preserves them server-side.
- **Enum selects** are driven by the arrays in `options.ts`; keep them in sync with the `api/types.ts` unions if the domain enums ever change.
```
