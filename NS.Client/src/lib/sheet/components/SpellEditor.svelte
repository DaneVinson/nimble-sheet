<script lang="ts">
	import { getContext } from 'svelte';
	import type { Spell } from '$lib/api/types';
	import type { SpellTierGroup } from '../viewmodel';
	import { getCollection } from '$lib/reference/cache';
	import { HERO_ACTIONS, type HeroActions } from '../heroActions.svelte';
	import { editorButton } from './styles';
	import Panel from './Panel.svelte';
	import TilePopover from './TilePopover.svelte';

	let { spellsByTier }: { spellsByTier: SpellTierGroup[] } = $props();

	const actions = getContext<HeroActions | undefined>(HERO_ACTIONS);

	let catalog = $state<Spell[]>([]);
	let selectedId = $state('');
	let tier = $state(1);
	let catalogError = $state<string | null>(null);

	const ownedIds = $derived(new Set(spellsByTier.flatMap((g) => g.spells.map((s) => s.spellId))));
	const available = $derived(catalog.filter((s) => !ownedIds.has(s.id)));
	const isEmpty = $derived(spellsByTier.length === 0);

	async function loadCatalog() {
		selectedId = '';
		catalogError = null;
		if (catalog.length === 0) {
			try {
				catalog = await getCollection<Spell>('spells');
			} catch {
				catalogError = 'Failed to load spell catalog.';
			}
		}
	}

	function onSelect() {
		const ref = catalog.find((s) => s.id === selectedId);
		if (ref) tier = ref.tier;
	}

	async function add() {
		if (!actions || selectedId === '') return;
		await actions.addSpell(selectedId, tier, null);
		selectedId = '';
	}
</script>

<Panel title="Spells" empty={isEmpty && !actions} emptyText="No spells known.">
	<div class="space-y-3">
		{#each spellsByTier as group (group.tier)}
			<div>
				<div class="text-xs font-semibold uppercase tracking-wide text-slate-400">Tier {group.tier}</div>
				<ul class="mt-1 space-y-1">
					{#each group.spells as s (s.spellId)}
						<li class="flex items-start justify-between gap-2 text-sm text-slate-200">
							<div>
								<span class="font-semibold text-white">{s.name}</span>
								<span class="text-slate-400">{s.manaCost} mana · {s.actionCost} action</span>
								{#if s.damage}<span class="text-slate-400"> · {s.damage} {s.damageType}</span>{/if}
							</div>
							{#if actions}
								<button type="button" class={editorButton} disabled={actions.busy} aria-label={`Remove ${s.name}`} onclick={() => actions.removeSpell(s.spellId)}>✕</button>
							{/if}
						</li>
					{/each}
				</ul>
			</div>
		{/each}
	</div>

	{#if actions}
		<div class="mt-2">
			<TilePopover label="Add spell" onopen={loadCatalog}>
				{#snippet trigger()}<span class={editorButton}>+ Add</span>{/snippet}
				{#snippet content()}
					<select bind:value={selectedId} onchange={onSelect} class="w-full rounded bg-slate-900 px-1.5 py-1 text-xs text-white" aria-label="Spell to add">
						<option value="">— select —</option>
						{#each available as s (s.id)}<option value={s.id}>{s.name} (T{s.tier})</option>{/each}
					</select>
					<label class="mt-2 block text-xs text-slate-300">Tier unlocked
						<input type="number" min="1" bind:value={tier} class="mt-1 w-full rounded bg-slate-900 px-1.5 py-1 text-xs text-white" />
					</label>
					<button type="button" class={`${editorButton} mt-2 w-full`} disabled={actions.busy || selectedId === ''} onclick={add}>Add</button>
					{#if catalogError}<p class="mt-1 text-[11px] text-red-400">{catalogError}</p>{/if}
					{#if actions.error}<p class="mt-1 text-[11px] text-red-400">{actions.error}</p>{/if}
				{/snippet}
			</TilePopover>
		</div>
	{/if}
</Panel>
