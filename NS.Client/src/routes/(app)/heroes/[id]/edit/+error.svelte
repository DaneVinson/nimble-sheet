<script lang="ts">
	import { page } from '$app/state';
</script>

<div class="mx-auto max-w-3xl px-4 py-16 text-center">
	<h1 class="mb-2 text-2xl font-bold text-white">{page.status}</h1>
	<p class="mb-6 text-slate-400">{page.error?.message ?? 'Something went wrong.'}</p>
	<a href="/heroes" class="text-sm text-blue-400 underline hover:text-blue-300">Back to heroes</a>
</div>
