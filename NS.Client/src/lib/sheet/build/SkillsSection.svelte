<script lang="ts">
	import type { HeroSkills } from '$lib/api/types';

	let { skills = $bindable() }: { skills: HeroSkills } = $props();

	const field = 'mt-1 w-full rounded bg-slate-900 px-2 py-1 text-sm text-white';
	const lbl = 'block text-xs text-slate-400';
</script>

<section class="rounded-lg bg-slate-800 p-4">
	<h2 class="mb-3 text-sm font-semibold uppercase tracking-wide text-slate-300">Skills</h2>
	<div class="grid grid-cols-2 gap-3 sm:grid-cols-5">
		<label class={lbl}>Arcana<input type="number" bind:value={skills.arcana} class={field} /></label>
		<label class={lbl}>Examination<input type="number" bind:value={skills.examination} class={field} /></label>
		<label class={lbl}>Finesse<input type="number" bind:value={skills.finesse} class={field} /></label>
		<label class={lbl}>Influence<input type="number" bind:value={skills.influence} class={field} /></label>
		<label class={lbl}>Insight<input type="number" bind:value={skills.insight} class={field} /></label>
		<label class={lbl}>Lore<input type="number" bind:value={skills.lore} class={field} /></label>
		<label class={lbl}>Might<input type="number" bind:value={skills.might} class={field} /></label>
		<label class={lbl}>Naturecraft<input type="number" bind:value={skills.naturecraft} class={field} /></label>
		<label class={lbl}>Perception<input type="number" bind:value={skills.perception} class={field} /></label>
		<label class={lbl}>Stealth<input type="number" bind:value={skills.stealth} class={field} /></label>
	</div>
</section>
