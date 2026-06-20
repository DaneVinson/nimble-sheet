<script lang="ts">
	import type { AbilityScores, Ancestry } from '$lib/api/types';
	import { abilityModifier, finalScores } from './classDefs';
	import { canDecrement, canIncrement, remaining, type AbilityKey } from './pointBuy';

	let {
		baseAbilityScores = $bindable(),
		ancestry,
		editable
	}: {
		baseAbilityScores: AbilityScores;
		ancestry: Ancestry | undefined;
		editable: boolean;
	} = $props();

	const rows: { key: AbilityKey; label: string }[] = [
		{ key: 'strength', label: 'STR' },
		{ key: 'dexterity', label: 'DEX' },
		{ key: 'intelligence', label: 'INT' },
		{ key: 'will', label: 'WIL' }
	];

	const zero: AbilityScores = { dexterity: 0, intelligence: 0, strength: 0, will: 0 };
	const bonuses = $derived(ancestry?.abilityBonuses ?? zero);
	const final = $derived(finalScores(baseAbilityScores, bonuses));
	const left = $derived(remaining(baseAbilityScores));

	function inc(key: AbilityKey) {
		if (editable && canIncrement(baseAbilityScores, key)) {
			baseAbilityScores = { ...baseAbilityScores, [key]: baseAbilityScores[key] + 1 };
		}
	}
	function dec(key: AbilityKey) {
		if (editable && canDecrement(baseAbilityScores, key)) {
			baseAbilityScores = { ...baseAbilityScores, [key]: baseAbilityScores[key] - 1 };
		}
	}
	const sign = (n: number) => (n >= 0 ? `+${n}` : `${n}`);
	const btn = 'h-6 w-6 rounded bg-slate-700 text-sm font-bold text-white hover:bg-slate-600 disabled:opacity-40';
</script>

<section class="rounded-lg bg-slate-800 p-4">
	<div class="mb-3 flex items-center justify-between">
		<h2 class="text-sm font-semibold uppercase tracking-wide text-slate-300">Ability Scores</h2>
		{#if editable}
			<span class="text-xs {left < 0 ? 'text-red-400' : 'text-slate-400'}">Points left: {left}</span>
		{/if}
	</div>
	<div class="grid grid-cols-1 gap-2 sm:grid-cols-2">
		{#each rows as row (row.key)}
			<div class="flex items-center justify-between rounded bg-slate-900 px-3 py-2">
				<span class="text-xs font-semibold text-slate-300">{row.label}</span>
				<div class="flex items-center gap-2">
					{#if editable}
						<button type="button" class={btn} aria-label={`Decrease ${row.label}`} disabled={!canDecrement(baseAbilityScores, row.key)} onclick={() => dec(row.key)}>−</button>
					{/if}
					<span class="w-6 text-center text-sm font-bold text-white">{baseAbilityScores[row.key]}</span>
					{#if editable}
						<button type="button" class={btn} aria-label={`Increase ${row.label}`} disabled={!canIncrement(baseAbilityScores, row.key)} onclick={() => inc(row.key)}>+</button>
					{/if}
					<span class="ml-2 w-20 text-right text-[11px] text-slate-400">
						final {final[row.key]} ({sign(abilityModifier(final[row.key]))})
					</span>
				</div>
			</div>
		{/each}
	</div>
	{#if !editable}
		<p class="mt-2 text-[11px] text-slate-500">Ability scores are set at creation.</p>
	{/if}
</section>
