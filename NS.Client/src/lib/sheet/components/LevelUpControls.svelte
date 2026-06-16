<script lang="ts">
	import { getContext } from 'svelte';
	import type { HeroSkills, StatType } from '$lib/api/types';
	import type { SheetViewModel } from '../viewmodel';
	import { HERO_ACTIONS, type HeroActions } from '../heroActions.svelte';
	import { editorButton } from './styles';
	import TilePopover from './TilePopover.svelte';
	import { SKILLS, spentPoints, canIncrement, canDecrement, canFinalize } from '../levelUp/skillAllocation';

	let { vm }: { vm: SheetViewModel } = $props();

	const actions = getContext<HeroActions | undefined>(HERO_ACTIONS);

	const stats: StatType[] = ['Strength', 'Dexterity', 'Intelligence', 'Will'];

	let hpGained = $state(0);
	let subclass = $state('');
	// svelte-ignore state_referenced_locally
	let working = $state<HeroSkills>({ ...vm.skillValues });

	async function confirmLevelUp() {
		if (!actions) return;
		await actions.levelUp(hpGained);
		hpGained = 0;
	}

	function resetSkills() {
		working = { ...vm.skillValues };
	}

	async function finalizeSkills() {
		if (!actions) return;
		await actions.finalizeSkillAllocation({ ...working });
	}

	async function confirmSubclass() {
		if (!actions || subclass.trim() === '') return;
		await actions.setSubclass(subclass.trim());
		subclass = '';
	}

	const pending = 'bg-amber-700 hover:bg-amber-600';
</script>

{#if actions}
	<TilePopover label="Level up" onopen={() => (hpGained = 0)}>
		{#snippet trigger()}<span class={editorButton}>Level Up</span>{/snippet}
		{#snippet content()}
			<p class="mb-1 text-[11px] text-slate-300">Level up to {vm.level + 1}</p>
			<label class="block text-xs text-slate-300">HP gained
				<input type="number" min="0" bind:value={hpGained} class="mt-1 w-full rounded bg-slate-900 px-1.5 py-1 text-xs text-white" aria-label="HP gained" />
			</label>
			<button type="button" class={`${editorButton} mt-2 w-full`} disabled={actions.busy} onclick={confirmLevelUp}>Confirm level up</button>
			{#if actions.error}<p class="mt-1 text-[11px] text-red-400">{actions.error}</p>{/if}
		{/snippet}
	</TilePopover>

	{#if vm.pendingStatIncrease}
		<TilePopover label="Choose stat increase">
			{#snippet trigger()}<span class={`${editorButton} ${pending}`}>Stat +1</span>{/snippet}
			{#snippet content()}
				<p class="mb-1 text-[11px] text-slate-300">Choose a stat to increase</p>
				<div class="grid grid-cols-2 gap-1">
					{#each stats as s (s)}
						<button type="button" class={editorButton} disabled={actions.busy} onclick={() => actions.applyStatIncrease(s)}>{s.slice(0, 3).toUpperCase()}</button>
					{/each}
				</div>
				{#if actions.error}<p class="mt-1 text-[11px] text-red-400">{actions.error}</p>{/if}
			{/snippet}
		</TilePopover>
	{/if}

	{#if vm.unspentSkillPoints > 0}
		<TilePopover label="Allocate skill points" onopen={resetSkills}>
			{#snippet trigger()}<span class={`${editorButton} ${pending}`}>Skills +{vm.unspentSkillPoints}</span>{/snippet}
			{#snippet content()}
				<p class="mb-1 text-[11px] text-slate-300">Spent {spentPoints(vm.skillValues, working)} of {vm.unspentSkillPoints}</p>
				<div class="max-h-48 space-y-1 overflow-y-auto pr-1">
					{#each SKILLS as { key, label } (key)}
						<div class="flex items-center justify-between gap-1 text-xs text-slate-200">
							<span>{label}</span>
							<div class="flex items-center gap-1">
								<button type="button" class={editorButton} disabled={!canDecrement(vm.skillValues, working, key)} onclick={() => (working[key] -= 1)}>−</button>
								<span class="min-w-5 text-center">{working[key]}</span>
								<button type="button" class={editorButton} disabled={!canIncrement(vm.skillValues, working, key, vm.unspentSkillPoints)} onclick={() => (working[key] += 1)}>+</button>
							</div>
						</div>
					{/each}
				</div>
				<button type="button" class={`${editorButton} mt-2 w-full`} disabled={actions.busy || !canFinalize(vm.skillValues, working, vm.unspentSkillPoints)} onclick={finalizeSkills}>Finalize</button>
				{#if actions.error}<p class="mt-1 text-[11px] text-red-400">{actions.error}</p>{/if}
			{/snippet}
		</TilePopover>
	{/if}

	{#if vm.needsSubclass}
		<TilePopover label="Choose subclass" onopen={() => (subclass = '')}>
			{#snippet trigger()}<span class={`${editorButton} ${pending}`}>Subclass</span>{/snippet}
			{#snippet content()}
				<input type="text" bind:value={subclass} placeholder="Subclass name" class="w-full rounded bg-slate-900 px-1.5 py-1 text-xs text-white" aria-label="Subclass name" />
				<button type="button" class={`${editorButton} mt-2 w-full`} disabled={actions.busy || subclass.trim() === ''} onclick={confirmSubclass}>Set subclass</button>
				{#if actions.error}<p class="mt-1 text-[11px] text-red-400">{actions.error}</p>{/if}
			{/snippet}
		</TilePopover>
	{/if}
{/if}
