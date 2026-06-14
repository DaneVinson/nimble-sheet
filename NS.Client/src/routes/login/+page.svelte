<script lang="ts">
	import { goto } from '$app/navigation';
	import { Button, Card, Input, Label } from 'flowbite-svelte';
	import { ApiError, createUser, login } from '$lib/api/client';
	import { setSession } from '$lib/auth/session';

	let mode = $state<'login' | 'create'>('login');
	let name = $state('');
	let email = $state('');
	let error = $state<string | null>(null);
	let busy = $state(false);

	async function submit() {
		error = null;
		busy = true;
		try {
			if (mode === 'create') {
				await createUser(name, email);
			}
			const result = await login(name);
			setSession({ name, token: result.token, userId: result.userId });
			await goto('/heroes');
		} catch (e) {
			error =
				e instanceof ApiError && e.status === 401
					? 'No user found with that name.'
					: e instanceof ApiError
						? e.message
						: 'Something went wrong. Please try again.';
		} finally {
			busy = false;
		}
	}
</script>

<svelte:head><title>Sign in — NimbleSheets</title></svelte:head>

<div class="dark flex min-h-screen items-center justify-center bg-slate-950 px-4">
	<Card class="w-full max-w-sm border-slate-800 bg-slate-900 p-6">
		<h1 class="mb-1 text-2xl font-bold text-white">NimbleSheets</h1>
		<p class="mb-4 text-sm text-slate-400">
			{mode === 'login' ? 'Sign in with your name.' : 'Create an account.'}
		</p>

		<form onsubmit={(e) => { e.preventDefault(); submit(); }} class="flex flex-col gap-4">
			<div>
				<Label for="name" class="mb-1 text-slate-300">Name</Label>
				<Input id="name" bind:value={name} required placeholder="Your name" />
			</div>

			{#if mode === 'create'}
				<div>
					<Label for="email" class="mb-1 text-slate-300">Email</Label>
					<Input id="email" type="email" bind:value={email} required placeholder="you@example.com" />
				</div>
			{/if}

			{#if error}
				<p class="text-sm text-red-400">{error}</p>
			{/if}

			<Button type="submit" color="primary" disabled={busy}>
				{mode === 'login' ? 'Log in' : 'Create account & log in'}
			</Button>
		</form>

		<button
			type="button"
			class="mt-4 text-sm text-slate-400 underline hover:text-slate-200"
			onclick={() => { mode = mode === 'login' ? 'create' : 'login'; error = null; }}
		>
			{mode === 'login' ? 'Need an account? Create one' : 'Already have an account? Log in'}
		</button>
	</Card>
</div>
