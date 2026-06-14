<script lang="ts">
	import { getContext } from 'svelte';
	import { HERO_ACTIONS, type HeroActions } from '../heroActions.svelte';
	import TilePopover from './TilePopover.svelte';

	let {
		current,
		max,
		isDead,
		isDying
	}: {
		current: number;
		max: number;
		isDead: boolean;
		isDying: boolean;
	} = $props();

	const actions = getContext<HeroActions | undefined>(HERO_ACTIONS);
	const pips = $derived(Array.from({ length: max }, (_, i) => i < current));

	const btn =
		'rounded bg-slate-700 px-2 py-1 text-xs font-semibold text-white hover:bg-slate-600 disabled:opacity-50';
</script>

{#snippet face()}
	<div class="rounded-lg bg-slate-800 p-2.5 text-center">
		<div class="text-[9px] uppercase tracking-[0.14em] text-slate-400">
			Wounds
			{#if isDead}<span class="ml-1 text-red-400">· Dead</span>
			{:else if isDying}<span class="ml-1 text-amber-400">· Dying</span>{/if}
		</div>
		<div class="mt-2 flex items-center justify-center gap-1">
			{#each pips as filled, i (i)}
				<span class="h-3 w-3 rounded-full border-2 {filled ? 'border-red-500 bg-red-500' : 'border-slate-500'}"></span>
			{/each}
			<span class="ml-0.5 text-sm text-slate-400">☠</span>
		</div>
	</div>
{/snippet}

{#if actions}
	<TilePopover label="Adjust wounds">
		{#snippet trigger()}{@render face()}{/snippet}
		{#snippet content()}
			<div class="mb-2 text-center text-xs text-slate-300">{current} / {max} wounds</div>
			<div class="flex justify-center gap-1">
				<button type="button" class={btn} disabled={actions.busy || current === 0} onclick={() => actions.healWound()}>
					Heal
				</button>
				<button type="button" class={btn} disabled={actions.busy} onclick={() => actions.gainWound()}>
					Gain
				</button>
			</div>
			{#if actions.error}<p class="mt-1 text-[11px] text-red-400">{actions.error}</p>{/if}
		{/snippet}
	</TilePopover>
{:else}
	{@render face()}
{/if}
