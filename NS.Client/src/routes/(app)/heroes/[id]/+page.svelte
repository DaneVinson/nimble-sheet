<script lang="ts">
	import HeroActionsScope from '$lib/sheet/components/HeroActionsScope.svelte';

	let { data } = $props();
</script>

<svelte:head><title>{data.vm.name} — NimbleSheets</title></svelte:head>

<div class="px-4 py-8">
	{#key data.heroId}
		<HeroActionsScope vm={data.vm} heroId={data.heroId} />
	{/key}
</div>
