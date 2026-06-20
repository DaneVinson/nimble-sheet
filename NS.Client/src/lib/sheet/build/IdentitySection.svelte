<script lang="ts">
	import type { Ancestry, Background, HeroClass } from '$lib/api/types';
	import { playableClasses } from './classDefs';

	let {
		name = $bindable(),
		ancestryId = $bindable(),
		backgroundId = $bindable(),
		heroClass = $bindable(),
		ancestries,
		backgrounds,
		classLocked,
		errors
	}: {
		name: string;
		ancestryId: string;
		backgroundId: string | null;
		heroClass: HeroClass | '';
		ancestries: Ancestry[];
		backgrounds: Background[];
		classLocked: boolean;
		errors: { name?: string; ancestryId?: string; heroClass?: string };
	} = $props();

	const field = 'mt-1 w-full rounded bg-slate-900 px-2 py-1 text-sm text-white disabled:opacity-60';
	const lbl = 'block text-xs text-slate-400';
</script>

<section class="rounded-lg bg-slate-800 p-4">
	<h2 class="mb-3 text-sm font-semibold uppercase tracking-wide text-slate-300">Identity</h2>
	<div class="grid gap-3 sm:grid-cols-2">
		<label class={lbl}>
			Name
			<input type="text" bind:value={name} class={field} />
			{#if errors.name}<span class="mt-1 block text-[11px] text-red-400">{errors.name}</span>{/if}
		</label>
		<label class={lbl}>
			Class
			<select bind:value={heroClass} class={field} disabled={classLocked}>
				<option value="">— select —</option>
				{#each playableClasses as c (c)}<option value={c}>{c}</option>{/each}
			</select>
			{#if classLocked}<span class="mt-1 block text-[11px] text-slate-500">Class is set at creation and cannot be changed.</span>{/if}
			{#if errors.heroClass}<span class="mt-1 block text-[11px] text-red-400">{errors.heroClass}</span>{/if}
		</label>
		<label class={lbl}>
			Ancestry
			<select bind:value={ancestryId} class={field}>
				<option value="">— select —</option>
				{#each ancestries as a (a.id)}<option value={a.id}>{a.name}</option>{/each}
			</select>
			{#if errors.ancestryId}<span class="mt-1 block text-[11px] text-red-400">{errors.ancestryId}</span>{/if}
		</label>
		<label class={lbl}>
			Background
			<select bind:value={backgroundId} class={field}>
				<option value={null}>— none —</option>
				{#each backgrounds as b (b.id)}<option value={b.id}>{b.name}</option>{/each}
			</select>
		</label>
	</div>
</section>
