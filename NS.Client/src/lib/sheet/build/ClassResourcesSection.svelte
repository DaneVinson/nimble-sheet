<script lang="ts">
	import type { ClassResources } from '$lib/api/types';
	import { dieTypes } from './options';

	let { resources = $bindable() }: { resources: ClassResources } = $props();

	const field = 'mt-1 w-full rounded bg-slate-900 px-2 py-1 text-sm text-white';
	const lbl = 'block text-xs text-slate-400';
</script>

<section class="rounded-lg bg-slate-800 p-4">
	<h2 class="mb-1 text-sm font-semibold uppercase tracking-wide text-slate-300">Class Resources</h2>
	<p class="mb-3 text-[11px] text-slate-500">Leave blank any field not used by your class.</p>
	<div class="grid grid-cols-2 gap-3 sm:grid-cols-4">
		<label class={lbl}>Judgment dice count
			<input type="number" min="0" bind:value={resources.judgmentDiceCount} class={field} />
		</label>
		<label class={lbl}>Judgment die type
			<select bind:value={resources.judgmentDiceType} class={field}>
				<option value={null}>— none —</option>
				{#each dieTypes as d (d)}<option value={d}>{d}</option>{/each}
			</select>
		</label>
		<label class={lbl}>Lay on hands pool
			<input type="number" min="0" bind:value={resources.layOnHandsPool} class={field} />
		</label>
		<label class={lbl}>Thrill charges
			<input type="number" min="0" bind:value={resources.thrillCharges} class={field} />
		</label>
	</div>
</section>
