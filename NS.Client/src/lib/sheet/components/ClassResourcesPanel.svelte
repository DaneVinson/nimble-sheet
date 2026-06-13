<script lang="ts">
  import type { SheetViewModel } from '../viewmodel';
  import Panel from './Panel.svelte';

  let { vm }: { vm: SheetViewModel } = $props();
</script>

<Panel title="Class Resources" empty={vm.classResources.length === 0} emptyText="No class resources.">
  <dl class="grid grid-cols-2 gap-3 sm:grid-cols-3">
    {#each vm.classResources as r (r.label)}
      <div class="rounded-md bg-slate-900/60 p-3 text-center">
        <dt class="text-[10px] uppercase tracking-wide text-slate-400">{r.label}</dt>
        <dd class="mt-1 text-lg font-bold text-white">{r.value}</dd>
      </div>
    {/each}
  </dl>
</Panel>
