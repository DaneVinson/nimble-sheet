<script lang="ts">
	import { goto } from '$app/navigation';
	import HeroBuildForm from '$lib/sheet/build/HeroBuildForm.svelte';
	import { blankBuildModel, type HeroBuildModel } from '$lib/sheet/build/model';
	import { createHero } from '$lib/api/client';

	let { data } = $props();

	async function submit(model: HeroBuildModel) {
		const { id } = await createHero(model);
		await goto(`/heroes/${id}`);
	}
</script>

<svelte:head><title>New hero — NimbleSheets</title></svelte:head>

<HeroBuildForm
	initial={blankBuildModel()}
	ancestries={data.ancestries}
	backgrounds={data.backgrounds}
	submitLabel="Create hero"
	mode="create"
	onsubmit={submit}
/>
