<script lang="ts">
	import { getContext } from 'svelte';
	import type { Condition } from '$lib/api/types';
	import type { ConditionViewModel } from '../viewmodel';
	import { getCollection } from '$lib/reference/cache';
	import { HERO_ACTIONS, type HeroActions } from '../heroActions.svelte';
	import { editorButton } from './styles';
	import Panel from './Panel.svelte';
	import TilePopover from './TilePopover.svelte';

	let { conditions }: { conditions: ConditionViewModel[] } = $props();

	const actions = getContext<HeroActions | undefined>(HERO_ACTIONS);

	let catalog = $state<Condition[]>([]);
	let selectedId = $state('');
	let expires = $state('');
	let catalogError = $state<string | null>(null);

	const ownedIds = $derived(new Set(conditions.map((c) => c.conditionId)));
	const available = $derived(catalog.filter((c) => !ownedIds.has(c.id)));

	async function loadCatalog() {
		selectedId = '';
		expires = '';
		catalogError = null;
		if (catalog.length === 0) {
			try {
				catalog = await getCollection<Condition>('conditions');
			} catch {
				catalogError = 'Failed to load condition catalog.';
			}
		}
	}

	async function add() {
		if (!actions || selectedId === '') return;
		await actions.addCondition(selectedId, expires.trim() === '' ? null : expires.trim());
		selectedId = '';
		expires = '';
	}
</script>

<Panel title="Conditions" empty={conditions.length === 0 && !actions} emptyText="No active conditions.">
	<ul class="space-y-2">
		{#each conditions as c (c.conditionId)}
			<li class="flex items-start justify-between gap-2 text-sm text-slate-200">
				<div>
					<span class="font-semibold text-white">{c.name}</span>
					{#if c.expiresAtEndOf}<span class="text-slate-400"> · expires {c.expiresAtEndOf}</span>{/if}
					<div class="text-xs text-slate-500">{c.description}</div>
				</div>
				{#if actions}
					<button type="button" class={editorButton} disabled={actions.busy} aria-label={`Remove ${c.name}`} onclick={() => actions.removeCondition(c.conditionId)}>✕</button>
				{/if}
			</li>
		{/each}
	</ul>

	{#if actions}
		<div class="mt-2">
			<TilePopover label="Add condition" onopen={loadCatalog}>
				{#snippet trigger()}<span class={editorButton}>+ Add</span>{/snippet}
				{#snippet content()}
					<select bind:value={selectedId} class="w-full rounded bg-slate-900 px-1.5 py-1 text-xs text-white" aria-label="Condition to add">
						<option value="">— select —</option>
						{#each available as c (c.id)}<option value={c.id}>{c.name}</option>{/each}
					</select>
					<input type="text" bind:value={expires} placeholder="Expires at end of (optional)" class="mt-2 w-full rounded bg-slate-900 px-1.5 py-1 text-xs text-white" aria-label="Expires at end of" />
					<button type="button" class={`${editorButton} mt-2 w-full`} disabled={actions.busy || selectedId === ''} onclick={add}>Add</button>
					{#if catalogError}<p class="mt-1 text-[11px] text-red-400">{catalogError}</p>{/if}
					{#if actions.error}<p class="mt-1 text-[11px] text-red-400">{actions.error}</p>{/if}
				{/snippet}
			</TilePopover>
		</div>
	{/if}
</Panel>
