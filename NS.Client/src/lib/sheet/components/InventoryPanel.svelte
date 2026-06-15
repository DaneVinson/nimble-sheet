<script lang="ts">
  import type { SheetViewModel } from '../viewmodel';
  import GearEditor from './GearEditor.svelte';
  import MagicItemEditor from './MagicItemEditor.svelte';

  let { vm }: { vm: SheetViewModel } = $props();
</script>

<div class="grid gap-3 sm:grid-cols-2">
  <MagicItemEditor magicItems={vm.magicItems} />
  <GearEditor gear={vm.gear} />
</div>
