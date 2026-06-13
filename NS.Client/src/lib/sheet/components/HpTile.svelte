<script lang="ts">
  let {
    current,
    max,
    temp
  }: {
    current: number;
    max: number;
    temp: number;
  } = $props();
</script>

<div class="rounded-lg bg-gradient-to-b from-red-900 to-red-950 p-2.5 text-center">
  <div class="text-[9px] uppercase tracking-[0.14em] text-red-200">Hit Points</div>
  <div class="text-3xl font-black leading-none text-white">{current}</div>
  <div class="mt-1 text-[10px] text-red-200">+{temp} temp · {max} max</div>
</div>
