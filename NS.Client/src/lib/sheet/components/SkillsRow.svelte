<script lang="ts">
  import type { SkillViewModel } from '../viewmodel';

  let { skills }: { skills: SkillViewModel[] } = $props();
</script>

<div class="grid grid-cols-2 gap-1.5 sm:grid-cols-5">
  {#each skills as skill (skill.name)}
    <div class="rounded-md bg-slate-800/70 py-1.5 text-center">
      <div class="text-[9px] uppercase tracking-wide text-slate-400">{skill.name}</div>
      <div class="text-sm font-bold text-white">{skill.display}</div>
      <div class="text-[8px] text-slate-500">{skill.statLabel}</div>
    </div>
  {/each}
</div>
