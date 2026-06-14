<script lang="ts">
	import { setContext } from 'svelte';
	import type { SheetViewModel } from '../viewmodel';
	import { HERO_ACTIONS, createHeroActions } from '../heroActions.svelte';
	import HeroSheet from './HeroSheet.svelte';

	let { vm, heroId }: { vm: SheetViewModel; heroId: string } = $props();

	setContext(HERO_ACTIONS, createHeroActions(() => heroId));
</script>

<HeroSheet {vm} />
