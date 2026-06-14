<script lang="ts">
	import { setContext } from 'svelte';
	import HeroSheet from '$lib/sheet/components/HeroSheet.svelte';
	import { HERO_ACTIONS, createHeroActions } from '$lib/sheet/heroActions.svelte';

	let { data } = $props();

	setContext(HERO_ACTIONS, createHeroActions(() => data.heroId));
</script>

<svelte:head><title>{data.vm.name} — NimbleSheets</title></svelte:head>

<div class="px-4 py-8">
	<HeroSheet vm={data.vm} />
</div>
