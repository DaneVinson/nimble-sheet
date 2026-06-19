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

	// Dismiss on pointerdown rather than click. A native <select> inside the popover renders
	// its option list as an OS-level popup; choosing an option emits a fall-through `click`
	// whose target is outside the small popover, which a click listener would treat as
	// "clicked outside" and close the popover before the user can act. Native option selection
	// does not dispatch a page-level pointerdown, so pointerdown-based outside-detection is
	// immune to it while still dismissing on genuine outside interaction.
	function handleWindowPointerDown(event: PointerEvent) {
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

<svelte:window onpointerdown={handleWindowPointerDown} onkeydown={handleKeydown} />

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
