<script lang="ts">
	import type { HeroCombatStats } from '$lib/api/types';
	import { dieTypes } from './options';

	let { combatStats = $bindable() }: { combatStats: HeroCombatStats } = $props();

	const field = 'mt-1 w-full rounded bg-slate-900 px-2 py-1 text-sm text-white';
	const lbl = 'block text-xs text-slate-400';
</script>

<section class="rounded-lg bg-slate-800 p-4">
	<h2 class="mb-3 text-sm font-semibold uppercase tracking-wide text-slate-300">Combat</h2>
	<div class="grid grid-cols-2 gap-3 sm:grid-cols-4">
		<label class={lbl}>Armor
			<input type="number" bind:value={combatStats.armor} class={field} />
		</label>
		<label class={lbl}>Hit die
			<select bind:value={combatStats.hitDieType} class={field}>
				{#each dieTypes as d (d)}<option value={d}>{d}</option>{/each}
			</select>
		</label>
		<label class={lbl}>Initiative
			<input type="number" bind:value={combatStats.initiativeBonus} class={field} />
		</label>
		<label class={lbl}>Speed
			<input type="number" bind:value={combatStats.speed} class={field} />
		</label>
	</div>
</section>
