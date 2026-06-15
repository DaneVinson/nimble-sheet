<script lang="ts">
	let {
		maxHp = $bindable(),
		maxMana = $bindable(),
		errors
	}: {
		maxHp: number;
		maxMana: number | null;
		errors: { maxHp?: string; maxMana?: string };
	} = $props();

	const field = 'mt-1 w-full rounded bg-slate-900 px-2 py-1 text-sm text-white';
	const lbl = 'block text-xs text-slate-400';
</script>

<section class="rounded-lg bg-slate-800 p-4">
	<h2 class="mb-3 text-sm font-semibold uppercase tracking-wide text-slate-300">Vitals</h2>
	<div class="grid gap-3 sm:grid-cols-2">
		<label class={lbl}>
			Max HP
			<input type="number" min="1" bind:value={maxHp} class={field} />
			{#if errors.maxHp}<span class="mt-1 block text-[11px] text-red-400">{errors.maxHp}</span>{/if}
		</label>
		<label class={lbl}>
			Max mana (casters — leave blank if none)
			<input type="number" min="0" bind:value={maxMana} class={field} />
			{#if errors.maxMana}<span class="mt-1 block text-[11px] text-red-400">{errors.maxMana}</span>{/if}
		</label>
	</div>
</section>
