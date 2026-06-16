<script lang="ts">
  import type { SheetViewModel } from '../viewmodel';
  import SpellEditor from './SpellEditor.svelte';

  let { vm }: { vm: SheetViewModel } = $props();
</script>

<SpellEditor spellsByTier={vm.spellsByTier} />
