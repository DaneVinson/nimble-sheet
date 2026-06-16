<script lang="ts">
  import type { SheetViewModel } from '../viewmodel';
  import ArmorEditor from './ArmorEditor.svelte';
  import ConditionEditor from './ConditionEditor.svelte';
  import WeaponEditor from './WeaponEditor.svelte';

  let { vm }: { vm: SheetViewModel } = $props();
</script>

<div class="grid gap-3 sm:grid-cols-2">
  <WeaponEditor weapons={vm.weapons} />

  <ArmorEditor armorItems={vm.armorItems} />

  <ConditionEditor conditions={vm.conditions} />
</div>
