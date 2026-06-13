<script lang="ts">
  let {
    current,
    max,
    isDead,
    isDying
  }: {
    current: number;
    max: number;
    isDead: boolean;
    isDying: boolean;
  } = $props();

  const pips = $derived(Array.from({ length: max }, (_, i) => i < current));
</script>

<div class="rounded-lg bg-slate-800 p-2.5 text-center">
  <div class="text-[9px] uppercase tracking-[0.14em] text-slate-400">
    Wounds
    {#if isDead}<span class="ml-1 text-red-400">· Dead</span>
    {:else if isDying}<span class="ml-1 text-amber-400">· Dying</span>{/if}
  </div>
  <div class="mt-2 flex items-center justify-center gap-1">
    {#each pips as filled, i (i)}
      <span class="h-3 w-3 rounded-full border-2 {filled ? 'border-red-500 bg-red-500' : 'border-slate-500'}"></span>
    {/each}
    <span class="ml-0.5 text-sm text-slate-400">☠</span>
  </div>
</div>
