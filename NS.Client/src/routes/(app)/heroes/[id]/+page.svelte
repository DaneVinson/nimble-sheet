<script lang="ts">
	import HeroSheet from '$lib/sheet/components/HeroSheet.svelte';

	let { data } = $props();
</script>

<svelte:head><title>{data.vm.name} — NimbleSheets</title></svelte:head>

<div class="px-4 py-8">
	<HeroSheet vm={data.vm} />
</div>
