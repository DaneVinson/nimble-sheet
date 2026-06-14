<script lang="ts">
	import { getContext } from 'svelte';
	import { HERO_ACTIONS, type HeroActions } from '../heroActions.svelte';
	import TilePopover from './TilePopover.svelte';

	let { current, max }: { current: number; max: number } = $props();

	const actions = getContext<HeroActions | undefined>(HERO_ACTIONS);
	let amount = $state(1);

	const btn =
		'rounded bg-slate-700 px-2 py-1 text-xs font-semibold text-white hover:bg-slate-600 disabled:opacity-50';
</script>

{#snippet face()}
	<div class="rounded-lg bg-indigo-900 p-2.5 text-center">
		<div class="text-[9px] uppercase tracking-[0.14em] text-indigo-200">Mana</div>
		<div class="mt-1 text-2xl font-extrabold text-white">{current}</div>
		<div class="text-[10px] text-indigo-200">{max} max</div>
	</div>
{/snippet}

{#if actions}
	<TilePopover label="Spend mana" onopen={() => (amount = 1)}>
		{#snippet trigger()}{@render face()}{/snippet}
		{#snippet content()}
			<div class="flex items-center gap-1">
				<input
					type="number"
					min="1"
					bind:value={amount}
					class="w-14 rounded bg-slate-900 px-1.5 py-1 text-xs text-white"
					aria-label="Mana to spend"
				/>
				<button type="button" class={btn} disabled={actions.busy} onclick={() => actions.spendMana(amount)}>
					Spend
				</button>
			</div>
			{#if actions.error}<p class="mt-1 text-[11px] text-red-400">{actions.error}</p>{/if}
		{/snippet}
	</TilePopover>
{:else}
	{@render face()}
{/if}
