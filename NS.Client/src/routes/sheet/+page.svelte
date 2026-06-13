<script lang="ts">
  import { caldra, referenceData } from '$lib/fixtures/caldra';
  import { resolveSheet } from '$lib/sheet/resolve';
  import HeroSheet from '$lib/sheet/components/HeroSheet.svelte';

  const vm = resolveSheet(caldra, referenceData);
</script>

<svelte:head>
  <title>{vm.name} — NimbleSheets</title>
</svelte:head>

<div class="dark min-h-screen bg-slate-950 px-4 py-8">
  <HeroSheet {vm} />
</div>
