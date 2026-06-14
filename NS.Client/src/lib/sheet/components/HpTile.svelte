<script lang="ts">
	import { getContext } from 'svelte';
	import { HERO_ACTIONS, type HeroActions } from '../heroActions.svelte';
	import TilePopover from './TilePopover.svelte';

	let { current, max, temp }: { current: number; max: number; temp: number } = $props();

	const actions = getContext<HeroActions | undefined>(HERO_ACTIONS);
	let tempInput = $state(0);

	const btn =
		'rounded bg-slate-700 px-2 py-1 text-xs font-semibold text-white hover:bg-slate-600 disabled:opacity-50';
</script>

{#snippet face()}
	<div class="rounded-lg bg-gradient-to-b from-red-900 to-red-950 p-2.5 text-center">
		<div class="text-[9px] uppercase tracking-[0.14em] text-red-200">Hit Points</div>
		<div class="text-3xl font-black leading-none text-white">{current}</div>
		<div class="mt-1 text-[10px] text-red-200">+{temp} temp · {max} max</div>
	</div>
{/snippet}

{#if actions}
	<TilePopover label="Adjust hit points" onopen={() => (tempInput = 0)}>
		{#snippet trigger()}{@render face()}{/snippet}
		{#snippet content()}
			<div class="flex items-center justify-between gap-1">
				<button type="button" class={btn} disabled={actions.busy} onclick={() => actions.takeDamage(5)}>−5</button>
				<button type="button" class={btn} disabled={actions.busy} onclick={() => actions.takeDamage(1)}>−1</button>
				<span class="min-w-8 text-center text-sm font-bold text-white">{current}</span>
				<button type="button" class={btn} disabled={actions.busy} onclick={() => actions.heal(1)}>+1</button>
				<button type="button" class={btn} disabled={actions.busy} onclick={() => actions.heal(5)}>+5</button>
			</div>
			<div class="mt-2 flex items-center gap-1">
				<input
					type="number"
					min="0"
					bind:value={tempInput}
					class="w-14 rounded bg-slate-900 px-1.5 py-1 text-xs text-white"
					aria-label="Temp HP amount"
				/>
				<button type="button" class={btn} disabled={actions.busy || !(tempInput >= 0)} onclick={() => actions.grantTempHp(tempInput)}>
					Temp
				</button>
			</div>
			{#if actions.error}<p class="mt-1 text-[11px] text-red-400">{actions.error}</p>{/if}
		{/snippet}
	</TilePopover>
{:else}
	{@render face()}
{/if}
