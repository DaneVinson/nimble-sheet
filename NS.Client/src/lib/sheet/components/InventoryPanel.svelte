<script lang="ts">
  import type { SheetViewModel } from '../viewmodel';
  import MagicItemEditor from './MagicItemEditor.svelte';
  import Panel from './Panel.svelte';

  let { vm }: { vm: SheetViewModel } = $props();
</script>

<div class="grid gap-3 sm:grid-cols-2">
  <MagicItemEditor magicItems={vm.magicItems} />

  <Panel title="Gear" empty={vm.gear.length === 0} emptyText="No gear.">
    <ul class="space-y-1">
      {#each vm.gear as g (g.name)}
        <li class="text-sm text-slate-200">
          <span class="font-semibold text-white">{g.name}</span>
          {#if g.quantity > 1}<span class="text-slate-400"> ×{g.quantity}</span>{/if}
        </li>
      {/each}
    </ul>
  </Panel>
</div>
