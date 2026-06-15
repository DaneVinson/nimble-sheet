<script lang="ts">
	import type { Ancestry, Background } from '$lib/api/types';
	import { ApiError } from '$lib/api/client';
	import type { HeroBuildModel } from './model';
	import { validateBuild, type BuildErrors } from './validate';
	import IdentitySection from './IdentitySection.svelte';
	import VitalsSection from './VitalsSection.svelte';
	import CombatSection from './CombatSection.svelte';
	import StatsSection from './StatsSection.svelte';
	import SavesSection from './SavesSection.svelte';
	import SkillsSection from './SkillsSection.svelte';
	import ClassResourcesSection from './ClassResourcesSection.svelte';

	let {
		initial,
		ancestries,
		backgrounds,
		submitLabel,
		onsubmit
	}: {
		initial: HeroBuildModel;
		ancestries: Ancestry[];
		backgrounds: Background[];
		submitLabel: string;
		onsubmit: (model: HeroBuildModel) => Promise<void>;
	} = $props();

	// Deep-copy the initial prop once at mount so edits don't mutate the caller's object.
	// svelte-ignore state_referenced_locally
	let model = $state<HeroBuildModel>(structuredClone(initial));
	let errors = $state<BuildErrors>({});
	let busy = $state(false);
	let formError = $state<string | null>(null);

	async function handleSubmit(event: SubmitEvent) {
		event.preventDefault();
		errors = validateBuild(model);
		if (Object.keys(errors).length > 0) {
			return;
		}
		busy = true;
		formError = null;
		try {
			await onsubmit($state.snapshot(model) as HeroBuildModel);
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
		{errors}
	/>
	<VitalsSection bind:maxHp={model.maxHp} bind:maxMana={model.maxMana} {errors} />
	<CombatSection bind:combatStats={model.combatStats} />
	<StatsSection bind:stats={model.stats} />
	<SavesSection bind:saves={model.saves} />
	<SkillsSection bind:skills={model.skills} />
	<ClassResourcesSection bind:resources={model.resources} />

	{#if formError}<p class="text-sm text-red-400">{formError}</p>{/if}
	<button
		type="submit"
		disabled={busy}
		class="rounded bg-blue-700 px-4 py-2 text-sm font-semibold text-white hover:bg-blue-600 disabled:opacity-50"
	>
		{submitLabel}
	</button>
</form>
