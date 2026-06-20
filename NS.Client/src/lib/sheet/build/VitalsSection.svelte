<script lang="ts">
	import type { AbilityScores, HeroClass } from '$lib/api/types';
	import { maxHpBounds, previewMaxMana, startingHp } from './classDefs';

	let {
		maxHp = $bindable(),
		heroClass,
		finalScores: final,
		mode,
		level,
		errors
	}: {
		maxHp: number;
		heroClass: HeroClass | '';
		finalScores: AbilityScores;
		mode: 'create' | 'edit';
		level: number;
		errors: { maxHp?: string };
	} = $props();

	const hasClass = $derived(heroClass !== '');
	const createHp = $derived(hasClass ? startingHp(heroClass as HeroClass) : 0);
	const mana = $derived(hasClass ? previewMaxMana(heroClass as HeroClass, final, level) : null);
	const bounds = $derived(hasClass ? maxHpBounds(heroClass as HeroClass, level) : { min: 1, max: 1 });

	const field = 'mt-1 w-full rounded bg-slate-900 px-2 py-1 text-sm text-white';
	const lbl = 'block text-xs text-slate-400';
</script>

<section class="rounded-lg bg-slate-800 p-4">
	<h2 class="mb-3 text-sm font-semibold uppercase tracking-wide text-slate-300">Vitals</h2>
	<div class="grid gap-3 sm:grid-cols-2">
		<div class={lbl}>
			Max HP
			{#if mode === 'create'}
				<div class="mt-1 rounded bg-slate-900 px-2 py-1 text-sm text-white">{createHp || '—'}</div>
				<span class="mt-1 block text-[11px] text-slate-500">Set by class.</span>
			{:else}
				<input type="number" min={bounds.min} max={bounds.max} bind:value={maxHp} class={field} />
				<span class="mt-1 block text-[11px] text-slate-500">Allowed {bounds.min}–{bounds.max} at level {level}.</span>
				{#if errors.maxHp}<span class="mt-1 block text-[11px] text-red-400">{errors.maxHp}</span>{/if}
			{/if}
		</div>
		{#if mana !== null}
			<div class={lbl}>
				Max mana
				<div class="mt-1 rounded bg-slate-900 px-2 py-1 text-sm text-white">{mana}</div>
				<span class="mt-1 block text-[11px] text-slate-500">Set by class.</span>
			</div>
		{/if}
	</div>
</section>
