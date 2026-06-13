<script lang="ts">
  import type { SheetViewModel } from '../viewmodel';
  import HpTile from './HpTile.svelte';
  import WoundTrack from './WoundTrack.svelte';

  let { vm }: { vm: SheetViewModel } = $props();
</script>

<div class="grid grid-cols-2 gap-2.5 sm:grid-cols-5">
  <HpTile current={vm.hp.current} max={vm.hp.max} temp={vm.hp.temp} />
  <WoundTrack current={vm.wounds.current} max={vm.wounds.max} isDead={vm.wounds.isDead} isDying={vm.wounds.isDying} />
  <div class="rounded-lg bg-slate-800 p-2.5 text-center">
    <div class="text-[9px] uppercase tracking-[0.14em] text-slate-400">Armor</div>
    <div class="mt-1 text-2xl font-extrabold text-white">{vm.armor}</div>
  </div>
  <div class="rounded-lg bg-slate-800 p-2.5 text-center">
    <div class="text-[9px] uppercase tracking-[0.14em] text-slate-400">Init</div>
    <div class="mt-1 text-2xl font-extrabold text-white">{vm.initiative}</div>
  </div>
  <div class="rounded-lg bg-slate-800 p-2.5 text-center">
    <div class="text-[9px] uppercase tracking-[0.14em] text-slate-400">Hit Dice</div>
    <div class="mt-1 text-2xl font-extrabold text-white">{vm.hitDice.die}</div>
    <div class="text-[10px] text-slate-400">{vm.hitDice.available} / {vm.hitDice.max}</div>
  </div>
</div>
