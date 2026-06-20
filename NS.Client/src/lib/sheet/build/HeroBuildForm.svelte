<script lang="ts">
	import type { Ancestry, Background } from '$lib/api/types';
	import { ApiError } from '$lib/api/client';
	import { normalizeBuild, type HeroBuildModel } from './model';
	import { validateBuild, type BuildErrors } from './validate';
	import { finalScores } from './classDefs';
	import IdentitySection from './IdentitySection.svelte';
	import AbilityScoresSection from './AbilityScoresSection.svelte';
	import VitalsSection from './VitalsSection.svelte';

	let {
		initial,
		ancestries,
		backgrounds,
		submitLabel,
		mode,
		level = 1,
		onsubmit
	}: {
		initial: HeroBuildModel;
		ancestries: Ancestry[];
		backgrounds: Background[];
		submitLabel: string;
		mode: 'create' | 'edit';
		level?: number;
		onsubmit: (model: HeroBuildModel) => Promise<void>;
	} = $props();

	// svelte-ignore state_referenced_locally
	let model = $state<HeroBuildModel>(structuredClone(initial));
	let errors = $state<BuildErrors>({});
	let busy = $state(false);
	let formError = $state<string | null>(null);

	const zero = { dexterity: 0, intelligence: 0, strength: 0, will: 0 };
	const selectedAncestry = $derived(ancestries.find((a) => a.id === model.ancestryId));
	const previewFinal = $derived(finalScores(model.baseAbilityScores, selectedAncestry?.abilityBonuses ?? zero));

	async function handleSubmit(event: SubmitEvent) {
		event.preventDefault();
		errors = validateBuild(model, { mode, level });
		if (Object.keys(errors).length > 0) {
			return;
		}
		busy = true;
		formError = null;
		try {
			await onsubmit(normalizeBuild($state.snapshot(model) as HeroBuildModel));
		} catch (e) {
			formError = e instanceof ApiError ? e.message : 'Save failed.';
		} finally {
			busy = false;
		}
	}
</script>

<form onsubmit={handleSubmit} class="mx-auto max-w-3xl space-y-4 px-4 py-8">
	<IdentitySection
		bind:name={model.name}
		bind:ancestryId={model.ancestryId}
		bind:backgroundId={model.backgroundId}
		bind:heroClass={model.heroClass}
		{ancestries}
		{backgrounds}
		classLocked={mode === 'edit'}
		{errors}
	/>
	<AbilityScoresSection
		bind:baseAbilityScores={model.baseAbilityScores}
		ancestry={selectedAncestry}
		editable={mode === 'create'}
	/>
	<VitalsSection
		bind:maxHp={model.maxHp}
		heroClass={model.heroClass}
		finalScores={previewFinal}
		{mode}
		{level}
		{errors}
	/>

	{#if errors.baseAbilityScores}<p class="text-sm text-red-400">{errors.baseAbilityScores}</p>{/if}
	{#if formError}<p class="text-sm text-red-400">{formError}</p>{/if}
	<button
		type="submit"
		disabled={busy}
		class="rounded bg-blue-700 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-600 disabled:opacity-50"
	>
		{submitLabel}
	</button>
</form>
