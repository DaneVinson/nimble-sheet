<script lang="ts">
	import type { Snippet } from 'svelte';

	let {
		label,
		trigger,
		content,
		onopen
	}: {
		label: string;
		trigger: Snippet;
		content: Snippet;
		onopen?: () => void;
	} = $props();

	let open = $state(false);
	let root = $state<HTMLElement | null>(null);

	function toggle() {
		open = !open;
		if (open) {
			onopen?.();
		}
	}

	function handleWindowClick(event: MouseEvent) {
		if (open && root && !root.contains(event.target as Node)) {
			open = false;
		}
	}

	function handleKeydown(event: KeyboardEvent) {
		if (event.key === 'Escape') {
			open = false;
		}
	}
</script>

<svelte:window onclick={handleWindowClick} onkeydown={handleKeydown} />

<div bind:this={root} class="relative">
	<button type="button" aria-label={label} class="block w-full text-left" onclick={toggle}>
		{@render trigger()}
	</button>
	{#if open}
		<div
			role="dialog"
			class="absolute left-1/2 z-30 mt-1 w-44 -translate-x-1/2 rounded-lg border border-slate-700 bg-slate-800 p-2 shadow-xl"
		>
			{@render content()}
		</div>
	{/if}
</div>
