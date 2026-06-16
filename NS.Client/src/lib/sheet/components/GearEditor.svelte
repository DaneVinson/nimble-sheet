<script lang="ts">
	import { getContext } from 'svelte';
	import type { GearViewModel } from '../viewmodel';
	import { HERO_ACTIONS, type HeroActions } from '../heroActions.svelte';
	import { editorButton } from './styles';
	import Panel from './Panel.svelte';
	import TilePopover from './TilePopover.svelte';

	let { gear }: { gear: GearViewModel[] } = $props();

	const actions = getContext<HeroActions | undefined>(HERO_ACTIONS);

	let name = $state('');
	let quantity = $state(1);

	const canAdd = $derived(name.trim() !== '' && quantity > 0);

	function reset() {
		name = '';
		quantity = 1;
	}

	async function add() {
		if (!actions || !canAdd) return;
		await actions.addGearItem(name.trim(), quantity);
		reset();
	}
</script>

<Panel title="Gear" empty={gear.length === 0 && !actions} emptyText="No gear.">
	<ul class="space-y-1">
		{#each gear as g (g.name)}
			<li class="flex items-center justify-between gap-2 text-sm text-slate-200">
				<span>
					<span class="font-semibold text-white">{g.name}</span>
					{#if g.quantity > 1}<span class="text-slate-400"> ×{g.quantity}</span>{/if}
				</span>
				{#if actions}
					<button type="button" class={editorButton} disabled={actions.busy} aria-label={`Remove ${g.name}`} onclick={() => actions.removeGearItem(g.name)}>✕</button>
				{/if}
			</li>
		{/each}
	</ul>

	{#if actions}
		<div class="mt-2">
			<TilePopover label="Add gear" onopen={reset}>
				{#snippet trigger()}<span class={editorButton}>+ Add</span>{/snippet}
				{#snippet content()}
					<input type="text" bind:value={name} placeholder="Item name" class="w-full rounded bg-slate-900 px-1.5 py-1 text-xs text-white" aria-label="Gear name" />
					<label class="mt-2 block text-xs text-slate-300">Quantity
						<input type="number" min="1" bind:value={quantity} class="mt-1 w-full rounded bg-slate-900 px-1.5 py-1 text-xs text-white" />
					</label>
					<button type="button" class={`${editorButton} mt-2 w-full`} disabled={actions.busy || !canAdd} onclick={add}>Add</button>
					{#if actions.error}<p class="mt-1 text-[11px] text-red-400">{actions.error}</p>{/if}
				{/snippet}
			</TilePopover>
		</div>
	{/if}
</Panel>
