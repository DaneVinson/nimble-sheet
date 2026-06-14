<script lang="ts">
	import { getContext } from 'svelte';
	import { HERO_ACTIONS, type HeroActions } from '../heroActions.svelte';
	import TilePopover from './TilePopover.svelte';

	const actions = getContext<HeroActions | undefined>(HERO_ACTIONS);

	const btn =
		'rounded bg-slate-700 px-2 py-1 text-xs font-semibold text-white hover:bg-slate-600 disabled:opacity-50';
</script>

{#if actions}
	<TilePopover label="Rest">
		{#snippet trigger()}
			<span class="inline-block rounded border border-slate-700 bg-slate-800 px-3 py-1 text-xs font-semibold text-slate-200 hover:border-slate-600">
				Rest
			</span>
		{/snippet}
		{#snippet content()}
			<p class="mb-2 text-[11px] text-slate-300">Rest and recover all resources?</p>
			<button type="button" class="{btn} w-full" disabled={actions.busy} onclick={() => actions.recoverAll()}>
				Confirm rest
			</button>
			{#if actions.error}<p class="mt-1 text-[11px] text-red-400">{actions.error}</p>{/if}
		{/snippet}
	</TilePopover>
{/if}
