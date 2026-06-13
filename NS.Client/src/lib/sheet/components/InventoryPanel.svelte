<script lang="ts">
  import type { SheetViewModel } from '../viewmodel';
  import Panel from './Panel.svelte';

  let { vm }: { vm: SheetViewModel } = $props();
</script>

<div class="grid gap-3 sm:grid-cols-2">
  <Panel title="Magic Items" empty={vm.magicItems.length === 0} emptyText="No magic items.">
    <ul class="space-y-2">
      {#each vm.magicItems as m (m.name)}
        <li class="text-sm text-slate-200">
          <span class="font-semibold text-white">{m.name}</span>
          <span class="text-slate-400">{m.rarity}</span>
          {#if m.charges}<span class="text-slate-400"> · {m.charges.remaining}/{m.charges.max} charges</span>{/if}
          {#if m.isEquipped}<span class="text-green-400"> · equipped</span>{/if}
          <div class="text-xs text-slate-500">{m.effect}</div>
        </li>
      {/each}
    </ul>
  </Panel>

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
