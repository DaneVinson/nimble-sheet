<script lang="ts">
	import { Card } from 'flowbite-svelte';

	let { data } = $props();
</script>

<svelte:head><title>Heroes — NimbleSheets</title></svelte:head>

<div class="mx-auto max-w-3xl px-4 py-8">
	<div class="mb-6 flex items-center justify-between">
		<h1 class="text-2xl font-bold text-white">Your Heroes</h1>
		<a href="/heroes/new" class="rounded bg-blue-700 px-3 py-1.5 text-sm font-semibold text-white hover:bg-blue-600">New hero</a>
	</div>

	{#if data.heroes.length === 0}
		<p class="text-slate-400">You don't have any heroes yet.</p>
	{:else}
		<ul class="flex flex-col gap-3">
			{#each data.heroes as hero (hero.id)}
				<li>
					<a href={`/heroes/${hero.id}`} class="block">
						<Card class="border-slate-800 bg-slate-900 p-4 hover:border-slate-600">
							<div class="flex items-baseline justify-between">
								<span class="text-lg font-semibold text-white">{hero.name}</span>
								<span class="text-sm text-slate-400">Level {hero.level}</span>
							</div>
							<div class="mt-1 text-sm text-slate-400">
								{hero.class}{hero.subclass ? ` · ${hero.subclass}` : ''} · HP {hero.currentHp}/{hero.maxHp}
							</div>
						</Card>
					</a>
				</li>
			{/each}
		</ul>
	{/if}
</div>
