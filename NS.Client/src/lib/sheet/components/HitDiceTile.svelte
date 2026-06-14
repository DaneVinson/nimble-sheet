<script lang="ts">
	import { getContext } from 'svelte';
	import { HERO_ACTIONS, type HeroActions } from '../heroActions.svelte';
	import TilePopover from './TilePopover.svelte';

	let { die, available, max }: { die: string; available: number; max: number } = $props();

	const actions = getContext<HeroActions | undefined>(HERO_ACTIONS);
	let count = $state(1);
	let healing = $state(0);

	const btn =
		'rounded bg-slate-700 px-2 py-1 text-xs font-semibold text-white hover:bg-slate-600 disabled:opacity-50';

	function reset() {
		count = 1;
		healing = 0;
	}
</script>

{#snippet face()}
	<div class="rounded-lg bg-slate-800 p-2.5 text-center">
		<div class="text-[9px] uppercase tracking-[0.14em] text-slate-400">Hit Dice</div>
		<div class="mt-1 text-2xl font-extrabold text-white">{die}</div>
		<div class="text-[10px] text-slate-400">{available} / {max}</div>
	</div>
{/snippet}

{#if actions}
	<TilePopover label="Spend hit dice" onopen={reset}>
		{#snippet trigger()}{@render face()}{/snippet}
		{#snippet content()}
			<label class="block text-[11px] text-slate-300">
				Dice
				<input
					type="number"
					min="1"
					max={available}
					bind:value={count}
					class="mt-0.5 w-full rounded bg-slate-900 px-1.5 py-1 text-xs text-white"
				/>
			</label>
			<label class="mt-1 block text-[11px] text-slate-300">
				Heal
				<input
					type="number"
					min="0"
					bind:value={healing}
					class="mt-0.5 w-full rounded bg-slate-900 px-1.5 py-1 text-xs text-white"
				/>
			</label>
			<button
				type="button"
				class="{btn} mt-2 w-full"
				disabled={actions.busy || available === 0 || !(count > 0) || count > available || !(healing >= 0)}
				onclick={() => actions.spendHitDice(count, healing)}
			>
				Spend
			</button>
			{#if actions.error}<p class="mt-1 text-[11px] text-red-400">{actions.error}</p>{/if}
		{/snippet}
	</TilePopover>
{:else}
	{@render face()}
{/if}
