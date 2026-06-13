<script lang="ts">
	import { goto } from '$app/navigation';
	import { navigating } from '$app/stores';
	import { clearSession } from '$lib/auth/session';

	let { children } = $props();

	function logout() {
		clearSession();
		goto('/login');
	}
</script>

<div class="dark min-h-screen bg-slate-950 text-slate-200">
	{#if $navigating}
		<div class="fixed inset-x-0 top-0 z-50 h-0.5 animate-pulse bg-blue-500"></div>
	{/if}
	<header class="flex items-center justify-between border-b border-slate-800 px-4 py-3">
		<a href="/heroes" class="text-lg font-bold text-white">NimbleSheets</a>
		<button
			type="button"
			class="text-sm text-slate-400 underline hover:text-slate-200"
			onclick={logout}
		>
			Log out
		</button>
	</header>
	<main>
		{@render children()}
	</main>
</div>
