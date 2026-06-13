<script lang="ts">
  import type { StatViewModel } from '../viewmodel';

  let { stat }: { stat: StatViewModel } = $props();
</script>

<div class="relative rounded-lg bg-slate-800 py-2 text-center">
  {#if stat.save === 'advantage'}
    <span class="absolute right-1.5 top-1 text-[8px] font-bold tracking-wide text-green-400" title="Advantage on {stat.label} saves">SAVE▲</span>
  {:else if stat.save === 'disadvantage'}
    <span class="absolute right-1.5 top-1 text-[8px] font-bold tracking-wide text-red-400" title="Disadvantage on {stat.label} saves">SAVE▼</span>
  {/if}
  <div class="text-2xl font-extrabold text-white">{stat.value}</div>
  <div class="mt-0.5 text-[10px] tracking-[0.12em] text-sky-300">{stat.label}</div>
</div>
