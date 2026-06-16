# Features Editing Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add inline add/remove of class features on the hero sheet, filtered to the hero's class and level, with selectable-option choices captured at add time.

**Architecture:** Client-only (the `AddFeature`/`RemoveFeature` endpoints already exist). One new `FeatureEditor.svelte` mirrors the existing reference-backed collection editors (`ConditionEditor` is the closest analog), plus a two-axis catalog filter (class + level) and choices checkboxes. Adds client wrappers, `heroActions` methods, and a `featureId` on the view model.

**Tech Stack:** SvelteKit 2 / Svelte 5 runes, Vitest (NS.Client). No server/domain changes.

---

## Conventions (read before every task)

- **Svelte 5 runes**; dark Tailwind utilities directly (no `dark:`). Follow the existing editor components.
- **Testing order — NO TDD (project preference):** implement first, then write the test, then run it.
- **Commits:** one per task. Work on branch `feat/feature-editing` (already created; the spec is its first commit, `92896ba`).
- **Reference catalog:** use `getCollection<T>(resource)` from `src/lib/reference/cache.ts` (session-cached, evict-on-failure). Resource string for features is `'features'`.
- **Shared button class:** `editorButton` from `src/lib/sheet/components/styles.ts`.
- Commands (from `NS.Client/`): `npm test`, `npm run check`, `npm run build`.

## Reference facts

- Server DTOs (exist, unchanged): `AddFeatureRequest(Guid HeroId, IReadOnlyList<string> Choices, Guid FeatureId, int LevelGained)`, `RemoveFeatureRequest(Guid HeroId, Guid FeatureId)`. JSON binds by name; route supplies `HeroId`.
- TS types (`src/lib/api/types.ts`): `Feature { class: HeroClass; description: string; frequencyLimit: string | null; id: string; level: number; name: string; selectableOptions: string[] | null; subclass: string | null }`. `HeroFeature { choices: string[]; featureId: string; heroId: string; levelGained: number }`. `HeroClass` is a string-union.
- `FeatureViewModel` (`src/lib/sheet/viewmodel.ts`) currently: `{ name; description; level; subclass; frequencyLimit; choices: string[] }`. `FeatureLevelGroup { level: number; features: FeatureViewModel[] }`; `SheetViewModel.features: FeatureLevelGroup[]`, plus `className: HeroClass` and `level: number`.
- Resolver `buildFeatures(hero, features)` (`src/lib/sheet/resolve.ts`) builds ONE vm object per owned feature using `ref?.x ?? fallback` (a single object, not two branches), grouped by `owned.levelGained`.

## File Structure

- Modify: `NS.Client/src/lib/api/client.ts` — `addFeature`, `removeFeature`
- Modify: `NS.Client/src/lib/api/client.test.ts` — `addFeature` test
- Modify: `NS.Client/src/lib/sheet/heroActions.svelte.ts` — interface + factory
- Modify: `NS.Client/src/lib/sheet/viewmodel.ts` — `featureId` on `FeatureViewModel`
- Modify: `NS.Client/src/lib/sheet/resolve.ts` — populate `featureId`
- Modify: `NS.Client/src/lib/sheet/resolve.test.ts` — assert `featureId`
- Create: `NS.Client/src/lib/sheet/components/FeatureEditor.svelte`
- Modify: `NS.Client/src/lib/sheet/components/FeaturesPanel.svelte` — compose `FeatureEditor`
- Modify: `CLAUDE.md` — note features editing

---

## Task 1: Client wrappers + actions

**Files:** Modify `NS.Client/src/lib/api/client.ts`, `client.test.ts`, `heroActions.svelte.ts`

- [ ] **Step 1: Add wrappers** to `client.ts` (in the `// --- collection mutations ---` area):

```ts
/** POST /heroes/{id}/add-feature — grant a class feature with any selectable-option choices. */
export function addFeature(heroId: string, featureId: string, choices: string[], levelGained: number): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/add-feature`, {
		method: 'POST',
		body: JSON.stringify({ featureId, choices, levelGained })
	});
}

/** POST /heroes/{id}/remove-feature — remove a feature by its reference id. */
export function removeFeature(heroId: string, featureId: string): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/remove-feature`, {
		method: 'POST',
		body: JSON.stringify({ featureId })
	});
}
```

- [ ] **Step 2: Add a wrapper test** to `client.test.ts` — add `addFeature` to the existing import from `./client`, and add this test inside the existing `describe('collection wrappers', ...)` block (reuse the existing `captureFetch` helper):

```ts
it('addFeature posts featureId/choices/levelGained', async () => {
	const fetchMock = captureFetch(204);
	await addFeature('h1', 'f1', ['Option A'], 3);
	expect(fetchMock).toHaveBeenCalledWith(
		'/api/heroes/h1/add-feature',
		expect.objectContaining({ method: 'POST', body: JSON.stringify({ featureId: 'f1', choices: ['Option A'], levelGained: 3 }) })
	);
});
```

- [ ] **Step 3: Extend `heroActions.svelte.ts`** — add `addFeature, removeFeature` to the import from `$lib/api/client`. Add to the `HeroActions` interface:

```ts
	addFeature(featureId: string, choices: string[], levelGained: number): Promise<void>;
	removeFeature(featureId: string): Promise<void>;
```

Add to the returned object in `createHeroActions`:

```ts
		addFeature: (featureId, choices, levelGained) => run(() => addFeature(getHeroId(), featureId, choices, levelGained)),
		removeFeature: (featureId) => run(() => removeFeature(getHeroId(), featureId)),
```

- [ ] **Step 4: Verify.** From `NS.Client/`: `npm test` (new test passes, count +1) and `npm run check` (0 errors/0 warnings).

- [ ] **Step 5: Commit.**

```bash
git add NS.Client/src/lib/api/client.ts NS.Client/src/lib/api/client.test.ts NS.Client/src/lib/sheet/heroActions.svelte.ts
git commit -m "feat(client): add feature add/remove wrappers and actions

Co-Authored-By: Claude Opus 4.8 <noreply@anthropic.com>"
```

---

## Task 2: `featureId` on the view model + resolver

**Files:** Modify `NS.Client/src/lib/sheet/viewmodel.ts`, `resolve.ts`, `resolve.test.ts`

- [ ] **Step 1: Add `featureId`** as the first field of `FeatureViewModel` in `viewmodel.ts`:

```ts
export interface FeatureViewModel {
  featureId: string;
  name: string;
  description: string;
  level: number;
  subclass: string | null;
  frequencyLimit: string | null;
  choices: string[];
}
```

- [ ] **Step 2: Populate it in `resolve.ts`.** In `buildFeatures`, the per-feature object is built as `const vm = { name: ..., description: ..., ... }`. Add `featureId: owned.featureId,` as the first property of that object literal.

- [ ] **Step 3: Add a resolver test** to `resolve.test.ts`. First read the top of the file to find the variable name it uses for the assembled `ReferenceData` bundle (the same one the existing tests pass to `resolveSheet`). Then add (substituting that bundle's name for `REF`):

```ts
it('carries the feature reference id for editing', () => {
	const vm = resolveSheet(caldra, REF);
	const ids = vm.features.flatMap((g) => g.features.map((f) => f.featureId)).sort();
	expect(ids).toEqual(caldra.features.map((f) => f.featureId).sort());
});
```

This compares the full set of ids regardless of grouping/sort order, so it holds even if `caldra` has zero features (both sides empty).

- [ ] **Step 4: Verify.** From `NS.Client/`: `npm test && npm run check` → PASS, 0 errors.

- [ ] **Step 5: Commit.**

```bash
git add NS.Client/src/lib/sheet/viewmodel.ts NS.Client/src/lib/sheet/resolve.ts NS.Client/src/lib/sheet/resolve.test.ts
git commit -m "feat(client): carry feature reference id on the view model

Co-Authored-By: Claude Opus 4.8 <noreply@anthropic.com>"
```

---

## Task 3: `FeatureEditor` component + wire into `FeaturesPanel`

**Files:** Create `NS.Client/src/lib/sheet/components/FeatureEditor.svelte`; Modify `FeaturesPanel.svelte`

- [ ] **Step 1: Create `FeatureEditor.svelte`:**

```svelte
<script lang="ts">
	import { getContext } from 'svelte';
	import type { Feature, HeroClass } from '$lib/api/types';
	import type { FeatureLevelGroup } from '../viewmodel';
	import { getCollection } from '$lib/reference/cache';
	import { HERO_ACTIONS, type HeroActions } from '../heroActions.svelte';
	import { editorButton } from './styles';
	import Panel from './Panel.svelte';
	import TilePopover from './TilePopover.svelte';

	let {
		features,
		heroClass,
		heroLevel
	}: { features: FeatureLevelGroup[]; heroClass: HeroClass; heroLevel: number } = $props();

	const actions = getContext<HeroActions | undefined>(HERO_ACTIONS);

	let catalog = $state<Feature[]>([]);
	let selectedId = $state('');
	let levelGained = $state(1);
	let choices = $state<string[]>([]);
	let catalogError = $state<string | null>(null);

	const ownedIds = $derived(new Set(features.flatMap((g) => g.features.map((f) => f.featureId))));
	const available = $derived(
		catalog.filter((f) => f.class === heroClass && f.level <= heroLevel && !ownedIds.has(f.id))
	);
	const selected = $derived(catalog.find((f) => f.id === selectedId));
	const isEmpty = $derived(features.length === 0);

	async function loadCatalog() {
		selectedId = '';
		levelGained = 1;
		choices = [];
		catalogError = null;
		if (catalog.length === 0) {
			try {
				catalog = await getCollection<Feature>('features');
			} catch {
				catalogError = 'Failed to load feature catalog.';
			}
		}
	}

	function onSelect() {
		choices = [];
		levelGained = selected?.level ?? heroLevel;
	}

	async function add() {
		if (!actions || selectedId === '') return;
		await actions.addFeature(selectedId, choices, levelGained);
		selectedId = '';
		levelGained = 1;
		choices = [];
	}
</script>

<Panel title="Features" empty={isEmpty && !actions} emptyText="No features.">
	<div class="space-y-4">
		{#each features as group (group.level)}
			<div>
				<div class="mb-1 text-xs font-semibold text-sky-300">Level {group.level}</div>
				<ul class="space-y-2">
					{#each group.features as f (f.featureId)}
						<li class="flex items-start justify-between gap-2 text-sm text-slate-200">
							<div>
								<span class="font-semibold text-white">{f.name}</span>
								{#if f.subclass}<span class="text-slate-400"> · {f.subclass}</span>{/if}
								{#if f.frequencyLimit}<span class="text-slate-500"> · {f.frequencyLimit}</span>{/if}
								<div class="text-xs text-slate-500">{f.description}</div>
								{#if f.choices.length > 0}<div class="text-xs text-sky-400">Chosen: {f.choices.join(', ')}</div>{/if}
							</div>
							{#if actions}
								<button type="button" class={editorButton} disabled={actions.busy} aria-label={`Remove ${f.name}`} onclick={() => actions.removeFeature(f.featureId)}>✕</button>
							{/if}
						</li>
					{/each}
				</ul>
			</div>
		{/each}
	</div>

	{#if actions}
		<div class="mt-2">
			<TilePopover label="Add feature" onopen={loadCatalog}>
				{#snippet trigger()}<span class={editorButton}>+ Add</span>{/snippet}
				{#snippet content()}
					<select bind:value={selectedId} onchange={onSelect} class="w-full rounded bg-slate-900 px-1.5 py-1 text-xs text-white" aria-label="Feature to add">
						<option value="">— select —</option>
						{#each available as f (f.id)}<option value={f.id}>{f.name} (L{f.level})</option>{/each}
					</select>
					{#if selected?.selectableOptions && selected.selectableOptions.length > 0}
						<div class="mt-2 space-y-1">
							<div class="text-[11px] uppercase tracking-wide text-slate-400">Choices</div>
							{#each selected.selectableOptions as opt (opt)}
								<label class="flex items-center gap-1 text-xs text-slate-300">
									<input type="checkbox" bind:group={choices} value={opt} /> {opt}
								</label>
							{/each}
						</div>
					{/if}
					<label class="mt-2 block text-xs text-slate-300">Level gained
						<input type="number" min="1" bind:value={levelGained} class="mt-1 w-full rounded bg-slate-900 px-1.5 py-1 text-xs text-white" />
					</label>
					<button type="button" class={`${editorButton} mt-2 w-full`} disabled={actions.busy || selectedId === ''} onclick={add}>Add</button>
					{#if catalogError}<p class="mt-1 text-[11px] text-red-400">{catalogError}</p>{/if}
					{#if actions.error}<p class="mt-1 text-[11px] text-red-400">{actions.error}</p>{/if}
				{/snippet}
			</TilePopover>
		</div>
	{/if}
</Panel>
```

- [ ] **Step 2: Wire into `FeaturesPanel.svelte`.** Replace its entire body with a delegation to the editor:

```svelte
<script lang="ts">
  import type { SheetViewModel } from '../viewmodel';
  import FeatureEditor from './FeatureEditor.svelte';

  let { vm }: { vm: SheetViewModel } = $props();
</script>

<FeatureEditor features={vm.features} heroClass={vm.className} heroLevel={vm.level} />
```

(The `Panel` import is dropped — `FeatureEditor` renders its own `Panel`.)

- [ ] **Step 3: Verify.** From `NS.Client/`: `npm run check` (0 errors/0 warnings), `npm test` (all pass), `npm run build` (success).

- [ ] **Step 4: Commit.**

```bash
git add NS.Client/src/lib/sheet/components/FeatureEditor.svelte NS.Client/src/lib/sheet/components/FeaturesPanel.svelte
git commit -m "feat: inline feature add/remove on the sheet

Co-Authored-By: Claude Opus 4.8 <noreply@anthropic.com>"
```

---

## Task 4: Browser verification + docs

**Files:** Modify `CLAUDE.md`

- [ ] **Step 1: Build the SPA into wwwroot and run the server** (plain `dotnet build` skips the SPA rebuild when `wwwroot/index.html` exists — rebuild explicitly):

```bash
cd NS.Client && npm run build && cd ..
rm -rf NS.WebApp/wwwroot && mkdir -p NS.WebApp/wwwroot && cp -r NS.Client/build/* NS.WebApp/wwwroot/
rm -f NS.WebApp/nimble-sheet.db
ASPNETCORE_ENVIRONMENT=Development dotnet run --project NS.WebApp/NS.WebApp.csproj --no-launch-profile
```
Server listens on `http://localhost:5000`.

- [ ] **Step 2: Drive the flow** (Playwright headless Chromium; reuse the scratch dir with `playwright` installed from prior verifications, and `npx playwright install chromium` if needed). Steps: login → create user → New hero (pick a class with seeded features; check what `GET /api/reference/features` returns and choose that class) → open the sheet → Features tab → "+ Add" → select a feature → if it shows Choices checkboxes, check one → Add → confirm the feature appears under its level group (with "Chosen: …" if a choice was picked) → click ✕ → confirm it disappears. Capture console/network errors and a screenshot of the populated Features panel.

  Note: the seed set (`SeedData.cs`) determines which classes have features. Query `GET /api/reference/features` (with a bearer token) first to pick a hero class that actually has at least one seeded feature; create the hero with that class so the filtered picker is non-empty.

- [ ] **Step 3: Record results.** PASS only if add (with a choice) and remove both round-trip on the re-fetched sheet with no JS/page errors. Stop the server when done.

- [ ] **Step 4: Update `CLAUDE.md`.** In the NS.Client "Collection editing" bullet, add features to the list of editable collections and note `FeatureEditor` filters the picker by the hero's class and level and captures selectable-option choices on add. (Features were previously called out as display-only / excluded — update that.)

- [ ] **Step 5: Commit.**

```bash
git add CLAUDE.md
git commit -m "docs: document inline features editing

Co-Authored-By: Claude Opus 4.8 <noreply@anthropic.com>"
```

- [ ] **Step 6: Finish the branch.** Use the `superpowers:finishing-a-development-branch` skill (verify tests, then merge `feat/feature-editing` to `main` with `--no-ff`, delete the branch; the user pushes).

---

## Self-Review

**Spec coverage:** add/remove features (Task 1 wrappers + Task 3 editor) ✓; picker filtered to class + `level ≤ hero.level`, excluding owned (Task 3 `available` derived) ✓; selectable-options as non-required multi-select checkboxes → `choices` (Task 3 `bind:group`) ✓; `levelGained` defaults to feature's level, editable (Task 3 `onSelect` + number input) ✓; `featureId` on view model + resolver, removable even with missing ref (Task 2 — the single-object resolver always sets `featureId: owned.featureId`) ✓; tests (wrapper + resolver) ✓; browser verification ✓; docs ✓. No server/domain changes ✓.

**Placeholder scan:** No TBD/TODO. Two "read the file first" notes (`resolve.test.ts` reference-bundle variable name; choosing a hero class with seeded features in verification) — each states the concrete action to take. All code steps show full code.

**Type consistency:** `addFeature(featureId, choices, levelGained)` signature matches across `client.ts`, the `HeroActions` interface, the factory, and the `FeatureEditor` call site. `removeFeature(featureId)` likewise. `FeatureViewModel.featureId` matches `owned.featureId` (`HeroFeature.featureId`) and the editor's `f.featureId` usage. Filter reads `f.class`/`f.level`/`f.id`/`f.selectableOptions` matching the `Feature` TS type. `FeatureEditor` props (`features`/`heroClass`/`heroLevel`) match the `FeaturesPanel` call site (`vm.features`/`vm.className`/`vm.level`).
