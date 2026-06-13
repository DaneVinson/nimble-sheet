<script lang="ts">
  import type { StatViewModel } from '../viewmodel';
  import StatBlock from './StatBlock.svelte';

  let { stats }: { stats: StatViewModel[] } = $props();
</script>

<div class="grid grid-cols-4 gap-2">
  {#each stats as stat (stat.type)}
    <StatBlock {stat} />
  {/each}
</div>
