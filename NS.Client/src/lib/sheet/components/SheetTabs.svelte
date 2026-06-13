<script lang="ts">
  import type { SheetViewModel } from '../viewmodel';
  import CombatPanel from './CombatPanel.svelte';
  import MagicPanel from './MagicPanel.svelte';
  import ClassResourcesPanel from './ClassResourcesPanel.svelte';
  import InventoryPanel from './InventoryPanel.svelte';
  import FeaturesPanel from './FeaturesPanel.svelte';

  let { vm }: { vm: SheetViewModel } = $props();

  const tabs = ['Combat', 'Magic', 'Class Resources', 'Inventory', 'Features'];
  let active = $state(0);
</script>

<div>
  <div class="flex flex-wrap gap-1 border-b border-slate-700">
    {#each tabs as label, i (label)}
      <button
        type="button"
        class="border-b-2 px-3 py-2 text-xs sm:text-sm {active === i ? 'border-blue-500 font-semibold text-white' : 'border-transparent text-slate-400 hover:text-slate-200'}"
        onclick={() => (active = i)}
      >
        {label}
      </button>
    {/each}
  </div>
  <div class="pt-4">
    {#if active === 0}
      <CombatPanel {vm} />
    {:else if active === 1}
      <MagicPanel {vm} />
    {:else if active === 2}
      <ClassResourcesPanel {vm} />
    {:else if active === 3}
      <InventoryPanel {vm} />
    {:else}
      <FeaturesPanel {vm} />
    {/if}
  </div>
</div>
