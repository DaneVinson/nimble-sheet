<script lang="ts">
	import type { HeroSaves } from '$lib/api/types';
	import { statTypes } from './options';

	let { saves = $bindable() }: { saves: HeroSaves } = $props();

	const field = 'mt-1 w-full rounded bg-slate-900 px-2 py-1 text-sm text-white';
	const lbl = 'block text-xs text-slate-400';
</script>

<section class="rounded-lg bg-slate-800 p-4">
	<h2 class="mb-3 text-sm font-semibold uppercase tracking-wide text-slate-300">Saves</h2>
	<div class="grid gap-3 sm:grid-cols-2">
		<label class={lbl}>Advantage on
			<select bind:value={saves.advantageOn} class={field}>
				{#each statTypes as s (s)}<option value={s}>{s}</option>{/each}
			</select>
		</label>
		<label class={lbl}>Disadvantage on
			<select bind:value={saves.disadvantageOn} class={field}>
				{#each statTypes as s (s)}<option value={s}>{s}</option>{/each}
			</select>
		</label>
	</div>
</section>
