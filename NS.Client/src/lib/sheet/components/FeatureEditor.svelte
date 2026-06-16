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
