<script lang="ts">
  let {
    name,
    ancestryName,
    className,
    level,
    subclass
  }: {
    name: string;
    ancestryName: string;
    className: string;
    level: number;
    subclass: string | null;
  } = $props();
</script>

<div class="bg-gradient-to-br from-blue-900 to-slate-900 px-5 py-5">
  <h1 class="text-2xl font-extrabold leading-tight text-white sm:text-3xl">{name}</h1>
  <p class="mt-1 text-sm text-blue-200">
    {ancestryName} · {className}{subclass ? ` (${subclass})` : ''} · Level {level}
  </p>
</div>
