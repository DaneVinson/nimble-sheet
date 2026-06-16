<script lang="ts">
	import { getContext } from 'svelte';
	import type { MagicItem } from '$lib/api/types';
	import type { MagicItemViewModel } from '../viewmodel';
	import { getCollection } from '$lib/reference/cache';
	import { HERO_ACTIONS, type HeroActions } from '../heroActions.svelte';
	import { editorButton } from './styles';
	import Panel from './Panel.svelte';
	import TilePopover from './TilePopover.svelte';

	let { magicItems }: { magicItems: MagicItemViewModel[] } = $props();

	const actions = getContext<HeroActions | undefined>(HERO_ACTIONS);

	let catalog = $state<MagicItem[]>([]);
	let selectedId = $state('');
	let equipped = $state(false);
	let charges = $state<number | null>(null);
	let catalogError = $state<string | null>(null);

	const ownedIds = $derived(new Set(magicItems.map((m) => m.magicItemId)));
	const available = $derived(catalog.filter((m) => !ownedIds.has(m.id)));

	async function loadCatalog() {
		selectedId = '';
		equipped = false;
		charges = null;
		catalogError = null;
		if (catalog.length === 0) {
			try {
				catalog = await getCollection<MagicItem>('magic-items');
			} catch {
				catalogError = 'Failed to load magic item catalog.';
			}
		}
	}

	function onSelect() {
		const ref = catalog.find((m) => m.id === selectedId);
		charges = ref?.maxCharges ?? null;
	}

	async function add() {
		if (!actions || selectedId === '') return;
		await actions.addMagicItem(selectedId, equipped, charges === null || Number.isNaN(charges) ? null : charges);
		selectedId = '';
		equipped = false;
		charges = null;
	}
</script>

<Panel title="Magic Items" empty={magicItems.length === 0 && !actions} emptyText="No magic items.">
	<ul class="space-y-2">
		{#each magicItems as m (m.magicItemId)}
			<li class="flex items-start justify-between gap-2 text-sm text-slate-200">
				<div>
					<span class="font-semibold text-white">{m.name}</span>
					<span class="text-slate-400">{m.rarity}</span>
					{#if m.charges}<span class="text-slate-400"> · {m.charges.remaining}/{m.charges.max} charges</span>{/if}
					{#if m.isEquipped}<span class="text-green-400"> · equipped</span>{/if}
					<div class="text-xs text-slate-500">{m.effect}</div>
				</div>
				{#if actions}
					<div class="flex shrink-0 gap-1">
						<button type="button" class={editorButton} disabled={actions.busy} onclick={() => actions.setMagicItemEquipped(m.magicItemId, !m.isEquipped)}>
							{m.isEquipped ? 'Unequip' : 'Equip'}
						</button>
						<button type="button" class={editorButton} disabled={actions.busy} aria-label={`Remove ${m.name}`} onclick={() => actions.removeMagicItem(m.magicItemId)}>✕</button>
					</div>
				{/if}
			</li>
		{/each}
	</ul>

	{#if actions}
		<div class="mt-2">
			<TilePopover label="Add magic item" onopen={loadCatalog}>
				{#snippet trigger()}<span class={editorButton}>+ Add</span>{/snippet}
				{#snippet content()}
					<select bind:value={selectedId} onchange={onSelect} class="w-full rounded bg-slate-900 px-1.5 py-1 text-xs text-white" aria-label="Magic item to add">
						<option value="">— select —</option>
						{#each available as m (m.id)}<option value={m.id}>{m.name}</option>{/each}
					</select>
					<label class="mt-2 flex items-center gap-1 text-xs text-slate-300">
						<input type="checkbox" bind:checked={equipped} /> Equipped
					</label>
					<label class="mt-2 block text-xs text-slate-300">Charges (blank = none)
						<input type="number" min="0" bind:value={charges} class="mt-1 w-full rounded bg-slate-900 px-1.5 py-1 text-xs text-white" />
					</label>
					<button type="button" class={`${editorButton} mt-2 w-full`} disabled={actions.busy || selectedId === ''} onclick={add}>Add</button>
					{#if catalogError}<p class="mt-1 text-[11px] text-red-400">{catalogError}</p>{/if}
					{#if actions.error}<p class="mt-1 text-[11px] text-red-400">{actions.error}</p>{/if}
				{/snippet}
			</TilePopover>
		</div>
	{/if}
</Panel>
