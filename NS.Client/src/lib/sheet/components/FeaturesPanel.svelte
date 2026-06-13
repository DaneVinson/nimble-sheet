<script lang="ts">
  import type { SheetViewModel } from '../viewmodel';
  import Panel from './Panel.svelte';

  let { vm }: { vm: SheetViewModel } = $props();
</script>

<Panel title="Features" empty={vm.features.length === 0} emptyText="No features.">
  <div class="space-y-4">
    {#each vm.features as group (group.level)}
      <div>
        <div class="mb-1 text-xs font-semibold text-sky-300">Level {group.level}</div>
        <ul class="space-y-2">
          {#each group.features as f (f.name)}
            <li class="text-sm text-slate-200">
              <span class="font-semibold text-white">{f.name}</span>
              {#if f.subclass}<span class="text-slate-400"> · {f.subclass}</span>{/if}
              {#if f.frequencyLimit}<span class="text-slate-500"> · {f.frequencyLimit}</span>{/if}
              <div class="text-xs text-slate-500">{f.description}</div>
              {#if f.choices.length > 0}<div class="text-xs text-sky-400">Chosen: {f.choices.join(', ')}</div>{/if}
            </li>
          {/each}
        </ul>
      </div>
    {/each}
  </div>
</Panel>
