<script lang="ts">
	import HeroActionsScope from '$lib/sheet/components/HeroActionsScope.svelte';

	let { data } = $props();
</script>

<svelte:head><title>{data.vm.name} — NimbleSheets</title></svelte:head>

<div class="px-4 py-8">
	<div class="mx-auto mb-3 flex max-w-3xl justify-end">
		<a
			href={`/heroes/${data.heroId}/edit`}
			class="rounded border border-slate-700 bg-slate-800 px-3 py-1 text-xs font-semibold text-slate-200 hover:border-slate-600"
		>
			Edit
		</a>
	</div>
	{#key data.heroId}
		<HeroActionsScope vm={data.vm} heroId={data.heroId} />
	{/key}
</div>
