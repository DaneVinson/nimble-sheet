<script lang="ts">
  import type { SheetViewModel } from '../viewmodel';
  import Panel from './Panel.svelte';

  let { vm }: { vm: SheetViewModel } = $props();
</script>

<div class="grid gap-3 sm:grid-cols-2">
  <Panel title="Weapons" empty={vm.weapons.length === 0} emptyText="No weapons.">
    <ul class="space-y-2">
      {#each vm.weapons as w (w.name)}
        <li class="text-sm text-slate-200">
          <span class="font-semibold text-white">{w.name}</span>
          <span class="text-slate-400">{w.damage} {w.damageType} · {w.statLabel}</span>
          {#if w.isTwoHanded}<span class="text-slate-500"> · two-handed</span>{/if}
          {#if w.notes}<div class="text-xs text-slate-500">{w.notes}</div>{/if}
        </li>
      {/each}
    </ul>
  </Panel>

  <Panel title="Armor" empty={vm.armorItems.length === 0} emptyText="No armor.">
    <ul class="space-y-2">
      {#each vm.armorItems as a (a.name)}
        <li class="text-sm text-slate-200">
          <span class="font-semibold text-white">{a.name}</span>
          <span class="text-slate-400">{a.type} · +{a.armorValue}</span>
          {#if a.isEquipped}<span class="text-green-400"> · equipped</span>{/if}
        </li>
      {/each}
    </ul>
  </Panel>

  <Panel title="Conditions" empty={vm.conditions.length === 0} emptyText="No active conditions.">
    <ul class="space-y-2">
      {#each vm.conditions as c (c.name)}
        <li class="text-sm text-slate-200">
          <span class="font-semibold text-white">{c.name}</span>
          {#if c.expiresAtEndOf}<span class="text-slate-400"> · expires {c.expiresAtEndOf}</span>{/if}
          <div class="text-xs text-slate-500">{c.description}</div>
        </li>
      {/each}
    </ul>
  </Panel>
</div>
