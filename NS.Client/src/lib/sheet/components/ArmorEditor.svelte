<script lang="ts">
	import { getContext } from 'svelte';
	import type { Armor } from '$lib/api/types';
	import type { ArmorViewModel } from '../viewmodel';
	import { getCollection } from '$lib/reference/cache';
	import { HERO_ACTIONS, type HeroActions } from '../heroActions.svelte';
	import { editorButton } from './styles';
	import Panel from './Panel.svelte';
	import TilePopover from './TilePopover.svelte';

	let { armorItems }: { armorItems: ArmorViewModel[] } = $props();

	const actions = getContext<HeroActions | undefined>(HERO_ACTIONS);

	let catalog = $state<Armor[]>([]);
	let selectedId = $state('');
	let equipped = $state(false);
	let catalogError = $state<string | null>(null);

	const ownedIds = $derived(new Set(armorItems.map((a) => a.armorId)));
	const available = $derived(catalog.filter((a) => !ownedIds.has(a.id)));

	async function loadCatalog() {
		selectedId = '';
		equipped = false;
		catalogError = null;
		if (catalog.length === 0) {
			try {
				catalog = await getCollection<Armor>('armor');
			} catch {
				catalogError = 'Failed to load armor catalog.';
			}
		}
	}

	async function add() {
		if (!actions || selectedId === '') return;
		await actions.addArmor(selectedId, equipped);
		selectedId = '';
		equipped = false;
	}
</script>

<Panel title="Armor" empty={armorItems.length === 0 && !actions} emptyText="No armor.">
	<ul class="space-y-2">
		{#each armorItems as a (a.armorId)}
			<li class="flex items-start justify-between gap-2 text-sm text-slate-200">
				<div>
					<span class="font-semibold text-white">{a.name}</span>
					<span class="text-slate-400">{a.type} · +{a.armorValue}</span>
					{#if a.isEquipped}<span class="text-green-400"> · equipped</span>{/if}
				</div>
				{#if actions}
					<div class="flex shrink-0 gap-1">
						<button type="button" class={editorButton} disabled={actions.busy} onclick={() => actions.setArmorEquipped(a.armorId, !a.isEquipped)}>
							{a.isEquipped ? 'Unequip' : 'Equip'}
						</button>
						<button type="button" class={editorButton} disabled={actions.busy} aria-label={`Remove ${a.name}`} onclick={() => actions.removeArmor(a.armorId)}>✕</button>
					</div>
				{/if}
			</li>
		{/each}
	</ul>

	{#if actions}
		<div class="mt-2">
			<TilePopover label="Add armor" onopen={loadCatalog}>
				{#snippet trigger()}<span class={editorButton}>+ Add</span>{/snippet}
				{#snippet content()}
					<select bind:value={selectedId} class="w-full rounded bg-slate-900 px-1.5 py-1 text-xs text-white" aria-label="Armor to add">
						<option value="">— select —</option>
						{#each available as a (a.id)}<option value={a.id}>{a.name}</option>{/each}
					</select>
					<label class="mt-2 flex items-center gap-1 text-xs text-slate-300">
						<input type="checkbox" bind:checked={equipped} /> Equipped
					</label>
					<button type="button" class={`${editorButton} mt-2 w-full`} disabled={actions.busy || selectedId === ''} onclick={add}>Add</button>
					{#if catalogError}<p class="mt-1 text-[11px] text-red-400">{catalogError}</p>{/if}
					{#if actions.error}<p class="mt-1 text-[11px] text-red-400">{actions.error}</p>{/if}
				{/snippet}
			</TilePopover>
		</div>
	{/if}
</Panel>
