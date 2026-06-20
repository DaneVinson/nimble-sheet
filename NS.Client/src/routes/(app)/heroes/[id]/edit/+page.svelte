<script lang="ts">
	import { goto } from '$app/navigation';
	import HeroBuildForm from '$lib/sheet/build/HeroBuildForm.svelte';
	import { heroToBuildModel, type HeroBuildModel } from '$lib/sheet/build/model';
	import { updateHero } from '$lib/api/client';

	let { data } = $props();

	async function submit(model: HeroBuildModel) {
		await updateHero(data.hero.id, model);
		await goto(`/heroes/${data.hero.id}`);
	}
</script>

<svelte:head><title>Edit {data.hero.name} — NimbleSheets</title></svelte:head>

<HeroBuildForm
	initial={heroToBuildModel(data.hero)}
	ancestries={data.ancestries}
	backgrounds={data.backgrounds}
	submitLabel="Save changes"
	mode="edit"
	level={data.hero.level}
	onsubmit={submit}
/>
