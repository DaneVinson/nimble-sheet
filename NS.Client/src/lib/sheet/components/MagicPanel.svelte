<script lang="ts">
  import type { SheetViewModel } from '../viewmodel';
  import Panel from './Panel.svelte';

  let { vm }: { vm: SheetViewModel } = $props();
</script>

<Panel title="Spells" empty={vm.spellsByTier.length === 0} emptyText="No spells known.">
  <div class="space-y-4">
    {#each vm.spellsByTier as group (group.tier)}
      <div>
        <div class="mb-1 text-xs font-semibold text-sky-300">Tier {group.tier}</div>
        <ul class="space-y-2">
          {#each group.spells as s (s.name)}
            <li class="text-sm text-slate-200">
              <span class="font-semibold text-white">{s.name}</span>
              <span class="text-slate-400">{s.school} · {s.manaCost} mana · {s.actionCost} action{s.actionCost === 1 ? '' : 's'}</span>
              {#if s.damage}<span class="text-slate-400"> · {s.damage} {s.damageType}</span>{/if}
              <div class="text-xs text-slate-500">{s.description}</div>
            </li>
          {/each}
        </ul>
      </div>
    {/each}
  </div>
</Panel>
