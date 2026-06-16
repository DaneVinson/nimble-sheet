<script lang="ts">
  import type { SheetViewModel } from '../viewmodel';
  import FeatureEditor from './FeatureEditor.svelte';

  let { vm }: { vm: SheetViewModel } = $props();
</script>

<FeatureEditor features={vm.features} heroClass={vm.className} heroLevel={vm.level} />
