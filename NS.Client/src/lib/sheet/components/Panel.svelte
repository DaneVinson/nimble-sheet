<script lang="ts">
  import type { Snippet } from 'svelte';

  let {
    title,
    empty = false,
    emptyText = 'Nothing here yet.',
    children
  }: {
    title: string;
    empty?: boolean;
    emptyText?: string;
    children: Snippet;
  } = $props();
</script>

<section class="rounded-lg bg-slate-800 p-4">
  <h3 class="mb-3 text-[10px] font-semibold uppercase tracking-[0.1em] text-sky-300">{title}</h3>
  {#if empty}
    <p class="text-sm italic text-slate-500">{emptyText}</p>
  {:else}
    {@render children()}
  {/if}
</section>
