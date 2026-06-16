<script lang="ts">
  import type { SheetViewModel } from '../viewmodel';
  import HeroBanner from './HeroBanner.svelte';
  import VitalsRow from './VitalsRow.svelte';
  import StatRow from './StatRow.svelte';
  import SkillsRow from './SkillsRow.svelte';
  import SheetTabs from './SheetTabs.svelte';
  import RestButton from './RestButton.svelte';
  import LevelUpControls from './LevelUpControls.svelte';

  let { vm }: { vm: SheetViewModel } = $props();
</script>

<article class="mx-auto max-w-3xl overflow-hidden rounded-xl border border-slate-800 bg-slate-900 shadow-xl">
  <HeroBanner
    name={vm.name}
    ancestryName={vm.ancestryName}
    className={vm.className}
    level={vm.level}
    subclass={vm.subclass}
  />

  <div class="space-y-4 bg-slate-900 px-5 py-4">
    <div class="flex flex-wrap items-center justify-end gap-2">
      <LevelUpControls {vm} />
      <RestButton />
    </div>
    <VitalsRow {vm} />
    <StatRow stats={vm.stats} />
    <SkillsRow skills={vm.skills} />
  </div>

  <div class="bg-slate-900 px-5 pb-6">
    <SheetTabs {vm} />
  </div>
</article>
