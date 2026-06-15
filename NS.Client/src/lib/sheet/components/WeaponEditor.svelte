<script lang="ts">
	import { getContext } from 'svelte';
	import type { Weapon } from '$lib/api/types';
	import type { WeaponViewModel } from '../viewmodel';
	import { getCollection } from '$lib/reference/cache';
	import { HERO_ACTIONS, type HeroActions } from '../heroActions.svelte';
	import Panel from './Panel.svelte';
	import TilePopover from './TilePopover.svelte';

	let { weapons }: { weapons: WeaponViewModel[] } = $props();

	const actions = getContext<HeroActions | undefined>(HERO_ACTIONS);

	let catalog = $state<Weapon[]>([]);
	let selectedId = $state('');
	let equipped = $state(false);

	const ownedIds = $derived(new Set(weapons.map((w) => w.weaponId)));
	const available = $derived(catalog.filter((w) => !ownedIds.has(w.id)));

	const btn = 'rounded bg-slate-700 px-2 py-1 text-xs font-semibold text-white hover:bg-slate-600 disabled:opacity-50';

	async function loadCatalog() {
		selectedId = '';
		equipped = false;
		if (catalog.length === 0) {
			catalog = await getCollection<Weapon>('weapons');
		}
	}

	async function add() {
		if (!actions || selectedId === '') return;
		await actions.addWeapon(selectedId, equipped, null);
		selectedId = '';
		equipped = false;
	}
</script>

<Panel title="Weapons" empty={weapons.length === 0 && !actions} emptyText="No weapons.">
	<ul class="space-y-2">
		{#each weapons as w (w.weaponId)}
			<li class="flex items-start justify-between gap-2 text-sm text-slate-200">
				<div>
					<span class="font-semibold text-white">{w.name}</span>
					<span class="text-slate-400">{w.damage} {w.damageType} · {w.statLabel}</span>
					{#if w.isTwoHanded}<span class="text-slate-500"> · two-handed</span>{/if}
					{#if w.isEquipped}<span class="text-green-400"> · equipped</span>{/if}
					{#if w.notes}<div class="text-xs text-slate-500">{w.notes}</div>{/if}
				</div>
				{#if actions}
					<div class="flex shrink-0 gap-1">
						<button type="button" class={btn} disabled={actions.busy} onclick={() => actions.setWeaponEquipped(w.weaponId, !w.isEquipped)}>
							{w.isEquipped ? 'Unequip' : 'Equip'}
						</button>
						<button type="button" class={btn} disabled={actions.busy} aria-label={`Remove ${w.name}`} onclick={() => actions.removeWeapon(w.weaponId)}>✕</button>
					</div>
				{/if}
			</li>
		{/each}
	</ul>

	{#if actions}
		<div class="mt-2">
			<TilePopover label="Add weapon" onopen={loadCatalog}>
				{#snippet trigger()}<span class={btn}>+ Add</span>{/snippet}
				{#snippet content()}
					<select bind:value={selectedId} class="w-full rounded bg-slate-900 px-1.5 py-1 text-xs text-white" aria-label="Weapon to add">
						<option value="">— select —</option>
						{#each available as w (w.id)}<option value={w.id}>{w.name}</option>{/each}
					</select>
					<label class="mt-2 flex items-center gap-1 text-xs text-slate-300">
						<input type="checkbox" bind:checked={equipped} /> Equipped
					</label>
					<button type="button" class={`${btn} mt-2 w-full`} disabled={actions.busy || selectedId === ''} onclick={add}>Add</button>
					{#if actions.error}<p class="mt-1 text-[11px] text-red-400">{actions.error}</p>{/if}
				{/snippet}
			</TilePopover>
		</div>
	{/if}
</Panel>
