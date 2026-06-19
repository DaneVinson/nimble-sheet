<script lang="ts">
	import type { Snippet } from 'svelte';
	import { tick } from 'svelte';

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

	const GAP = 4; // px between trigger and popover
	const MARGIN = 8; // min px from any viewport edge

	let open = $state(false);
	let root = $state<HTMLElement | null>(null);
	let dialog = $state<HTMLElement | null>(null);
	let top = $state(0);
	let left = $state(0);
	let ready = $state(false); // hides the popover for the first frame, until positioned

	// The popover is rendered position:fixed and anchored to the trigger so it escapes any
	// ancestor `overflow-hidden` (the hero sheet's <article> clips its content, which previously
	// hid the bottom of downward-opening popovers — e.g. the inline editors' "Add" button). Fixed
	// positioning is viewport-relative, so we also flip above the trigger when there isn't room
	// below, and clamp horizontally, to keep the whole popover on-screen.
	function reposition() {
		if (!open || !root || !dialog) {
			return;
		}
		const anchor = root.getBoundingClientRect();
		const height = dialog.offsetHeight;
		const width = dialog.offsetWidth;

		// Horizontal: centered on the trigger, clamped into the viewport.
		const centered = anchor.left + anchor.width / 2 - width / 2;
		left = Math.max(MARGIN, Math.min(centered, window.innerWidth - width - MARGIN));

		// Vertical: prefer below; flip above when below would overflow and above has room.
		const below = anchor.bottom + GAP;
		const fitsBelow = below + height <= window.innerHeight - MARGIN;
		const fitsAbove = anchor.top - GAP - height >= MARGIN;
		top = fitsBelow || !fitsAbove ? below : anchor.top - GAP - height;
	}

	async function toggle() {
		open = !open;
		if (open) {
			ready = false;
			onopen?.();
			await tick(); // wait for the dialog to render so we can measure it
			reposition();
			ready = true;
		}
	}

	// Dismiss on pointerdown rather than click. A native <select> inside the popover renders its
	// option list as an OS-level popup; choosing an option emits a fall-through `click` whose target
	// is outside the popover, which a click listener would treat as "clicked outside" and close it
	// before the user can act. Native option selection dispatches no page-level pointerdown, so
	// pointerdown-based outside-detection is immune to it while still dismissing on genuine
	// outside interaction. (The fixed dialog is still a DOM descendant of `root`.)
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

<svelte:window
	onpointerdown={handleWindowPointerDown}
	onkeydown={handleKeydown}
	onresize={reposition}
	onscroll={reposition}
/>

<div bind:this={root} class="relative">
	<button type="button" aria-label={label} class="block w-full text-left" onclick={toggle}>
		{@render trigger()}
	</button>
	{#if open}
		<div
			bind:this={dialog}
			role="dialog"
			style="position: fixed; top: {top}px; left: {left}px; opacity: {ready ? 1 : 0};"
			class="z-30 w-44 rounded-lg border border-slate-700 bg-slate-800 p-2 shadow-xl"
		>
			{@render content()}
		</div>
	{/if}
</div>
