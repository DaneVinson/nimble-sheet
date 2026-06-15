<script lang="ts">
	import type { HeroStats } from '$lib/api/types';

	let { stats = $bindable() }: { stats: HeroStats } = $props();

	const field = 'mt-1 w-full rounded bg-slate-900 px-2 py-1 text-sm text-white';
	const lbl = 'block text-xs text-slate-400';
</script>

<section class="rounded-lg bg-slate-800 p-4">
	<h2 class="mb-3 text-sm font-semibold uppercase tracking-wide text-slate-300">Stats</h2>
	<div class="grid grid-cols-2 gap-3 sm:grid-cols-4">
		<label class={lbl}>STR
			<input type="number" bind:value={stats.strength} class={field} />
		</label>
		<label class={lbl}>DEX
			<input type="number" bind:value={stats.dexterity} class={field} />
		</label>
		<label class={lbl}>INT
			<input type="number" bind:value={stats.intelligence} class={field} />
		</label>
		<label class={lbl}>WIL
			<input type="number" bind:value={stats.will} class={field} />
		</label>
	</div>
</section>
