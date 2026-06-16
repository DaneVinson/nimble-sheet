# Collection Editing (Equipment / Spells / Conditions) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add inline add/remove of weapons, armor, magic items, gear, spells, and conditions on the hero sheet, plus proper equip/unequip for weapons, armor, and magic items.

**Architecture:** The 14 add/remove endpoints already exist; this plan adds three equip endpoints (domain + API), client wrappers + `heroActions` entries for the full set, reference-id fields on the view models, and per-collection editor components the panels compose. Controls consume the existing `HERO_ACTIONS` context optionally (read-only when absent); server owns all rules, client POSTs then `invalidateAll()`.

**Tech Stack:** C# 14 / .NET 10, FastEndpoints 8.x, xUnit (NS.Tests); SvelteKit 2 / Svelte 5 runes, Vitest (NS.Client).

---

## Conventions (read before every task)

- **C#:** `sealed` classes; `var` locals; explicit access modifiers; `_camelCase` private fields; XML docs on all public members; braces on all control flow; no per-file `using` (global usings live in `_GlobalUsings.cs`, already set up). Members ordered alphabetically within their group.
- **FastEndpoints 8.x:** respond via the `Send` property (`await Send.NoContentAsync(ct)`, `await Send.NotFoundAsync(ct)`). Routes are declared **without** the `/api` prefix in `Configure()` — the global `RoutePrefix = "api"` adds it. Request record + endpoint in one file.
- **Hero ownership:** every hero-by-id endpoint loads via `_heroes.GetOwnedByIdAsync(req.HeroId, User.GetUserId())` and returns 404 when null.
- **Svelte:** runes mode; dark Tailwind utilities directly (no `dark:`); follow existing component idioms.
- **Testing order (project preference — NO TDD):** implement first, then write the test, then run it. Do **not** write failing tests first.
- **Commits:** one commit at the end of each slice (the plan marks the commit step). Work happens on branch `feat/collection-editing` (already created; the spec is its first commit).
- **Reference data for pickers:** use the existing `getCollection<T>(resource)` from `src/lib/reference/cache.ts` (session-cached, evict-on-failure). No new cache function needed.

---

## File Structure

**Create (NS.FastEndpoints/Heroes/):** `SetWeaponEquippedEndpoint.cs`, `SetArmorEquippedEndpoint.cs`, `SetMagicItemEquippedEndpoint.cs`

**Create (NS.Client/src/lib/sheet/components/):** `WeaponEditor.svelte`, `ArmorEditor.svelte`, `MagicItemEditor.svelte`, `SpellEditor.svelte`, `GearEditor.svelte`, `ConditionEditor.svelte`

**Modify:**
- `NS.Domain/Heroes/Hero.cs` — add `SetArmorEquipped`, `SetMagicItemEquipped`, `SetWeaponEquipped`
- `NS.Tests/HeroTests.cs` — equip method tests
- `NS.Client/src/lib/api/client.ts` — 15 collection wrappers
- `NS.Client/src/lib/api/client.test.ts` — wrapper tests
- `NS.Client/src/lib/sheet/heroActions.svelte.ts` — extend `HeroActions`
- `NS.Client/src/lib/sheet/viewmodel.ts` — add reference-id fields
- `NS.Client/src/lib/sheet/resolve.ts` — populate the ids
- `NS.Client/src/lib/sheet/resolve.test.ts` — assert the ids
- `NS.Client/src/lib/sheet/components/CombatPanel.svelte` — compose Weapon/Armor/Condition editors
- `NS.Client/src/lib/sheet/components/MagicPanel.svelte` — compose SpellEditor
- `NS.Client/src/lib/sheet/components/InventoryPanel.svelte` — compose MagicItem/Gear editors
- `CLAUDE.md` — route table + NS.Client notes

---

## Slice 1 — Weapons (establishes the full pattern)

### Task 1.1: Domain `SetWeaponEquipped` + test

**Files:**
- Modify: `NS.Domain/Heroes/Hero.cs` (the `Set*` mutators region, ~line 302)
- Test: `NS.Tests/HeroTests.cs`

- [ ] **Step 1: Add the three equip methods to `Hero`.** Place them alphabetically among the existing mutators — `SetArmorEquipped` and `SetMagicItemEquipped` before `SetSubclass`, `SetWeaponEquipped` after it. (Armor/MagicItem bodies are filled in Slices 2–3; add all three signatures now so the file compiles once and ordering is final.)

```csharp
/// <summary>Sets whether the referenced armor item is equipped; no-op if the hero does not have it.</summary>
public void SetArmorEquipped(Guid armorId, bool isEquipped)
{
    var index = _armor.FindIndex(a => a.ArmorId == armorId);
    if (index >= 0)
    {
        _armor[index] = _armor[index] with { IsEquipped = isEquipped };
    }
}

/// <summary>Sets whether the referenced magic item is equipped; no-op if the hero does not have it.</summary>
public void SetMagicItemEquipped(Guid magicItemId, bool isEquipped)
{
    var index = _magicItems.FindIndex(m => m.MagicItemId == magicItemId);
    if (index >= 0)
    {
        _magicItems[index] = _magicItems[index] with { IsEquipped = isEquipped };
    }
}
```

```csharp
/// <summary>Sets whether the referenced weapon is equipped; no-op if the hero does not have it.</summary>
public void SetWeaponEquipped(Guid weaponId, bool isEquipped)
{
    var index = _weapons.FindIndex(w => w.WeaponId == weaponId);
    if (index >= 0)
    {
        _weapons[index] = _weapons[index] with { IsEquipped = isEquipped };
    }
}
```

- [ ] **Step 2: Add tests to `HeroTests.cs`.**

```csharp
/// <summary>Equipping a weapon the hero owns flips its equipped flag.</summary>
[Fact]
public void SetWeaponEquipped_WhenWeaponPresent_UpdatesFlag()
{
    var hero = TestHero.Create();
    var weaponId = Guid.CreateVersion7();
    hero.AddWeapon(new HeroWeapon(hero.Id, false, null, weaponId));

    hero.SetWeaponEquipped(weaponId, true);

    Assert.True(hero.Weapons.Single().IsEquipped);
}

/// <summary>Setting equipped on an unknown weapon id changes nothing.</summary>
[Fact]
public void SetWeaponEquipped_WhenWeaponAbsent_IsNoOp()
{
    var hero = TestHero.Create();
    hero.AddWeapon(new HeroWeapon(hero.Id, true, null, Guid.CreateVersion7()));

    hero.SetWeaponEquipped(Guid.CreateVersion7(), false);

    Assert.True(hero.Weapons.Single().IsEquipped);
}
```

- [ ] **Step 3: Build and run the tests.**

Run: `dotnet test NS.Tests/NS.Tests.csproj --filter "FullyQualifiedName~SetWeaponEquipped"`
Expected: PASS (2 tests). The full solution builds with 0 errors (the armor/magic-item method bodies exist even though their endpoints come later).

### Task 1.2: `SetWeaponEquippedEndpoint`

**Files:**
- Create: `NS.FastEndpoints/Heroes/SetWeaponEquippedEndpoint.cs`

- [ ] **Step 1: Write the endpoint** (model it on `AddWeaponEndpoint.cs`).

```csharp
namespace NSFastEndpoints;

/// <summary>Sets whether a weapon in the hero's equipment is equipped.</summary>
public sealed class SetWeaponEquippedEndpoint : Endpoint<SetWeaponEquippedRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public SetWeaponEquippedEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/set-weapon-equipped");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(SetWeaponEquippedRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetOwnedByIdAsync(req.HeroId, User.GetUserId());
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.SetWeaponEquipped(req.WeaponId, req.IsEquipped);
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for setting a weapon's equipped state.</summary>
/// <param name="HeroId">The hero's unique identifier (route).</param>
/// <param name="WeaponId">The identifier of the weapon to update.</param>
/// <param name="IsEquipped">Whether the weapon should be equipped.</param>
public sealed record SetWeaponEquippedRequest(Guid HeroId, Guid WeaponId, bool IsEquipped);
```

- [ ] **Step 2: Build.**

Run: `dotnet build NS.WebApp/NS.WebApp.csproj`
Expected: Build succeeded, 0 warnings. (Endpoint auto-discovered; total endpoint count rises by 1.)

### Task 1.3: Client wrappers for weapons + tests

**Files:**
- Modify: `NS.Client/src/lib/api/client.ts` (after the play-mutation wrappers, before `createHero`)
- Test: `NS.Client/src/lib/api/client.test.ts`

- [ ] **Step 1: Add the three weapon wrappers** (place them grouped under a `// --- collection mutations ---` comment).

```ts
/** POST /heroes/{id}/add-weapon — add a weapon from the reference catalog. */
export function addWeapon(heroId: string, weaponId: string, isEquipped: boolean, notes: string | null): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/add-weapon`, {
		method: 'POST',
		body: JSON.stringify({ weaponId, isEquipped, notes })
	});
}

/** POST /heroes/{id}/remove-weapon — remove a weapon by its reference id. */
export function removeWeapon(heroId: string, weaponId: string): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/remove-weapon`, {
		method: 'POST',
		body: JSON.stringify({ weaponId })
	});
}

/** POST /heroes/{id}/set-weapon-equipped — equip or unequip a weapon. */
export function setWeaponEquipped(heroId: string, weaponId: string, isEquipped: boolean): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/set-weapon-equipped`, {
		method: 'POST',
		body: JSON.stringify({ weaponId, isEquipped })
	});
}
```

- [ ] **Step 2: Add wrapper tests** to `client.test.ts` (inside a new `describe('collection wrappers', ...)`; reuse the existing `captureFetch` helper).

```ts
describe('collection wrappers', () => {
	it('addWeapon posts weaponId/isEquipped/notes', async () => {
		const fetchMock = captureFetch(204);
		await addWeapon('h1', 'w1', true, null);
		expect(fetchMock).toHaveBeenCalledWith(
			'/api/heroes/h1/add-weapon',
			expect.objectContaining({ method: 'POST', body: JSON.stringify({ weaponId: 'w1', isEquipped: true, notes: null }) })
		);
	});

	it('removeWeapon posts the weaponId', async () => {
		const fetchMock = captureFetch(204);
		await removeWeapon('h1', 'w1');
		expect(fetchMock).toHaveBeenCalledWith(
			'/api/heroes/h1/remove-weapon',
			expect.objectContaining({ method: 'POST', body: JSON.stringify({ weaponId: 'w1' }) })
		);
	});

	it('setWeaponEquipped posts weaponId/isEquipped', async () => {
		const fetchMock = captureFetch(204);
		await setWeaponEquipped('h1', 'w1', false);
		expect(fetchMock).toHaveBeenCalledWith(
			'/api/heroes/h1/set-weapon-equipped',
			expect.objectContaining({ method: 'POST', body: JSON.stringify({ weaponId: 'w1', isEquipped: false }) })
		);
	});
});
```

Add the new names to the existing import at the top of `client.test.ts`:
```ts
import { ApiError, addWeapon, createHero, gainWound, getHeroes, login, removeWeapon, setWeaponEquipped, spendHitDice, takeDamage, updateHero } from './client';
```

- [ ] **Step 3: Run the tests.**

Run (from `NS.Client/`): `npm test`
Expected: PASS, test count up by 3.

### Task 1.4: Extend `heroActions` with weapon methods

**Files:**
- Modify: `NS.Client/src/lib/sheet/heroActions.svelte.ts`

- [ ] **Step 1: Import the new wrappers.** Extend the existing import:

```ts
import {
	addWeapon, gainWound, grantTempHp, heal, healWound, recoverAll,
	removeWeapon, setWeaponEquipped, spendHitDice, spendMana, takeDamage
} from '$lib/api/client';
```

- [ ] **Step 2: Add to the `HeroActions` interface** (after `recoverAll()`):

```ts
	addWeapon(weaponId: string, isEquipped: boolean, notes: string | null): Promise<void>;
	removeWeapon(weaponId: string): Promise<void>;
	setWeaponEquipped(weaponId: string, isEquipped: boolean): Promise<void>;
```

- [ ] **Step 3: Add to the returned object** in `createHeroActions` (after `recoverAll: ...`):

```ts
		addWeapon: (weaponId, isEquipped, notes) => run(() => addWeapon(getHeroId(), weaponId, isEquipped, notes)),
		removeWeapon: (weaponId) => run(() => removeWeapon(getHeroId(), weaponId)),
		setWeaponEquipped: (weaponId, isEquipped) => run(() => setWeaponEquipped(getHeroId(), weaponId, isEquipped)),
```

- [ ] **Step 4: Type-check.**

Run (from `NS.Client/`): `npm run check`
Expected: 0 errors / 0 warnings.

### Task 1.5: `weaponId` on the view model + resolver

**Files:**
- Modify: `NS.Client/src/lib/sheet/viewmodel.ts`
- Modify: `NS.Client/src/lib/sheet/resolve.ts`
- Test: `NS.Client/src/lib/sheet/resolve.test.ts`

- [ ] **Step 1: Add `weaponId` to `WeaponViewModel`** (first field):

```ts
export interface WeaponViewModel {
  weaponId: string;
  name: string;
  damage: string;        // '1d6+2'
  damageType: DamageType;
  statLabel: string;     // 'STR'
  reach: number;
  range: number | null;
  isTwoHanded: boolean;
  isEquipped: boolean;
  notes: string | null;
}
```

- [ ] **Step 2: Populate it in `resolve.ts`** — in the `weapons:` mapper, add `weaponId: w.weaponId,` as the first property of the returned object.

- [ ] **Step 3: Add a resolver test** to `resolve.test.ts` (the file already imports `caldra` and `resolveSheet` with reference data — reuse that setup; if the existing test builds a `vm`, add an assertion, otherwise add this test):

```ts
it('carries the weapon reference id for editing', () => {
	const vm = resolveSheet(caldra, referenceFixture);
	expect(vm.weapons[0].weaponId).toBe(caldra.weapons[0].weaponId);
});
```

> If `resolve.test.ts` names its reference bundle differently than `referenceFixture`, use that name. Check the top of the file for how it assembles `ReferenceData`.

- [ ] **Step 4: Run tests + check.**

Run (from `NS.Client/`): `npm test && npm run check`
Expected: PASS, 0 type errors.

### Task 1.6: `WeaponEditor` component + wire into `CombatPanel`

**Files:**
- Create: `NS.Client/src/lib/sheet/components/WeaponEditor.svelte`
- Modify: `NS.Client/src/lib/sheet/components/CombatPanel.svelte`

- [ ] **Step 1: Create `WeaponEditor.svelte`.** It renders the weapon list with per-row equip toggle + remove when the actions context is present, plus a "+ Add" popover that lazily loads the full weapon catalog and excludes already-owned weapons. When no actions context, it renders the read-only list (current behavior).

```svelte
<script lang="ts">
	import { getContext } from 'svelte';
	import type { Weapon } from '$lib/api/types';
	import type { WeaponViewModel } from '../viewmodel';
	import { getCollection } from '$lib/reference/cache';
	import { HERO_ACTIONS, type HeroActions } from '../heroActions.svelte';
	import Panel from './Panel.svelte';
	import TilePopover from './TilePopover.svelte';

	let { weapons }: { weapons: WeaponViewModel[] } = $props();

	const actions = getContext<HeroActions | undefined>(HERO_ACTIONS);

	let catalog = $state<Weapon[]>([]);
	let selectedId = $state('');
	let equipped = $state(false);

	const ownedIds = $derived(new Set(weapons.map((w) => w.weaponId)));
	const available = $derived(catalog.filter((w) => !ownedIds.has(w.id)));

	const btn = 'rounded bg-slate-700 px-2 py-1 text-xs font-semibold text-white hover:bg-slate-600 disabled:opacity-50';

	async function loadCatalog() {
		selectedId = '';
		equipped = false;
		if (catalog.length === 0) {
			catalog = await getCollection<Weapon>('weapons');
		}
	}

	async function add() {
		if (!actions || selectedId === '') return;
		await actions.addWeapon(selectedId, equipped, null);
		selectedId = '';
		equipped = false;
	}
</script>

<Panel title="Weapons" empty={weapons.length === 0 && !actions} emptyText="No weapons.">
	<ul class="space-y-2">
		{#each weapons as w (w.weaponId)}
			<li class="flex items-start justify-between gap-2 text-sm text-slate-200">
				<div>
					<span class="font-semibold text-white">{w.name}</span>
					<span class="text-slate-400">{w.damage} {w.damageType} · {w.statLabel}</span>
					{#if w.isTwoHanded}<span class="text-slate-500"> · two-handed</span>{/if}
					{#if w.isEquipped}<span class="text-green-400"> · equipped</span>{/if}
					{#if w.notes}<div class="text-xs text-slate-500">{w.notes}</div>{/if}
				</div>
				{#if actions}
					<div class="flex shrink-0 gap-1">
						<button type="button" class={btn} disabled={actions.busy} onclick={() => actions.setWeaponEquipped(w.weaponId, !w.isEquipped)}>
							{w.isEquipped ? 'Unequip' : 'Equip'}
						</button>
						<button type="button" class={btn} disabled={actions.busy} aria-label={`Remove ${w.name}`} onclick={() => actions.removeWeapon(w.weaponId)}>✕</button>
					</div>
				{/if}
			</li>
		{/each}
	</ul>

	{#if actions}
		<div class="mt-2">
			<TilePopover label="Add weapon" onopen={loadCatalog}>
				{#snippet trigger()}<span class={btn}>+ Add</span>{/snippet}
				{#snippet content()}
					<select bind:value={selectedId} class="w-full rounded bg-slate-900 px-1.5 py-1 text-xs text-white" aria-label="Weapon to add">
						<option value="">— select —</option>
						{#each available as w (w.id)}<option value={w.id}>{w.name}</option>{/each}
					</select>
					<label class="mt-2 flex items-center gap-1 text-xs text-slate-300">
						<input type="checkbox" bind:checked={equipped} /> Equipped
					</label>
					<button type="button" class={`${btn} mt-2 w-full`} disabled={actions.busy || selectedId === ''} onclick={add}>Add</button>
					{#if actions.error}<p class="mt-1 text-[11px] text-red-400">{actions.error}</p>{/if}
				{/snippet}
			</TilePopover>
		</div>
	{/if}
</Panel>
```

- [ ] **Step 2: Use it in `CombatPanel.svelte`.** Replace the inline Weapons `<Panel>…</Panel>` block with `<WeaponEditor weapons={vm.weapons} />` and add the import. (Armor/Conditions stay inline for now — Slices 2 and 6 replace them.)

```svelte
<script lang="ts">
  import type { SheetViewModel } from '../viewmodel';
  import Panel from './Panel.svelte';
  import WeaponEditor from './WeaponEditor.svelte';

  let { vm }: { vm: SheetViewModel } = $props();
</script>

<div class="grid gap-3 sm:grid-cols-2">
  <WeaponEditor weapons={vm.weapons} />

  <Panel title="Armor" empty={vm.armorItems.length === 0} emptyText="No armor.">
    <!-- unchanged armor list -->
  </Panel>

  <Panel title="Conditions" empty={vm.conditions.length === 0} emptyText="No active conditions.">
    <!-- unchanged conditions list -->
  </Panel>
</div>
```

- [ ] **Step 3: Type-check + build the SPA.**

Run (from `NS.Client/`): `npm run check && npm run build`
Expected: 0 errors; build writes `build/`.

### Task 1.7: Commit Slice 1

- [ ] **Step 1: Commit.**

```bash
git add NS.Domain/Heroes/Hero.cs NS.Tests/HeroTests.cs NS.FastEndpoints/Heroes/SetWeaponEquippedEndpoint.cs NS.Client/src/lib/api/client.ts NS.Client/src/lib/api/client.test.ts NS.Client/src/lib/sheet/heroActions.svelte.ts NS.Client/src/lib/sheet/viewmodel.ts NS.Client/src/lib/sheet/resolve.ts NS.Client/src/lib/sheet/resolve.test.ts NS.Client/src/lib/sheet/components/WeaponEditor.svelte NS.Client/src/lib/sheet/components/CombatPanel.svelte
git commit -m "feat: inline weapon add/remove/equip on the sheet"
```

---

## Slice 2 — Armor

`SetArmorEquipped` already exists from Task 1.1. Request shapes: `AddArmorRequest(HeroId, ArmorId, IsEquipped)`, `RemoveArmorRequest(HeroId, ArmorId)`.

### Task 2.1: `SetArmorEquippedEndpoint`

**Files:** Create `NS.FastEndpoints/Heroes/SetArmorEquippedEndpoint.cs`

- [ ] **Step 1: Write the endpoint.**

```csharp
namespace NSFastEndpoints;

/// <summary>Sets whether an armor item the hero carries is equipped.</summary>
public sealed class SetArmorEquippedEndpoint : Endpoint<SetArmorEquippedRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public SetArmorEquippedEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/set-armor-equipped");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(SetArmorEquippedRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetOwnedByIdAsync(req.HeroId, User.GetUserId());
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.SetArmorEquipped(req.ArmorId, req.IsEquipped);
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for setting an armor item's equipped state.</summary>
/// <param name="HeroId">The hero's unique identifier (route).</param>
/// <param name="ArmorId">The identifier of the armor item to update.</param>
/// <param name="IsEquipped">Whether the armor should be equipped.</param>
public sealed record SetArmorEquippedRequest(Guid HeroId, Guid ArmorId, bool IsEquipped);
```

- [ ] **Step 2: Add a domain test** to `HeroTests.cs` (mirror the weapon no-op/update pair).

```csharp
/// <summary>Equipping armor the hero owns flips its equipped flag.</summary>
[Fact]
public void SetArmorEquipped_WhenArmorPresent_UpdatesFlag()
{
    var hero = TestHero.Create();
    var armorId = Guid.CreateVersion7();
    hero.AddArmor(new HeroArmor(armorId, hero.Id, false));

    hero.SetArmorEquipped(armorId, true);

    Assert.True(hero.Armor.Single().IsEquipped);
}
```

- [ ] **Step 3: Build + test.** Run: `dotnet test NS.Tests/NS.Tests.csproj --filter "FullyQualifiedName~SetArmorEquipped"` → PASS.

### Task 2.2: Armor client wrappers + actions

**Files:** Modify `client.ts`, `client.test.ts`, `heroActions.svelte.ts`

- [ ] **Step 1: Add wrappers** to `client.ts`:

```ts
/** POST /heroes/{id}/add-armor — add armor from the reference catalog. */
export function addArmor(heroId: string, armorId: string, isEquipped: boolean): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/add-armor`, {
		method: 'POST',
		body: JSON.stringify({ armorId, isEquipped })
	});
}

/** POST /heroes/{id}/remove-armor — remove armor by its reference id. */
export function removeArmor(heroId: string, armorId: string): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/remove-armor`, {
		method: 'POST',
		body: JSON.stringify({ armorId })
	});
}

/** POST /heroes/{id}/set-armor-equipped — equip or unequip armor. */
export function setArmorEquipped(heroId: string, armorId: string, isEquipped: boolean): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/set-armor-equipped`, {
		method: 'POST',
		body: JSON.stringify({ armorId, isEquipped })
	});
}
```

- [ ] **Step 2: Add one wrapper test** to the `collection wrappers` describe (and import the names):

```ts
it('addArmor posts armorId/isEquipped', async () => {
	const fetchMock = captureFetch(204);
	await addArmor('h1', 'a1', true);
	expect(fetchMock).toHaveBeenCalledWith(
		'/api/heroes/h1/add-armor',
		expect.objectContaining({ method: 'POST', body: JSON.stringify({ armorId: 'a1', isEquipped: true }) })
	);
});
```

- [ ] **Step 3: Extend `heroActions`** — import `addArmor, removeArmor, setArmorEquipped`; add to the interface and returned object:

```ts
	addArmor(armorId: string, isEquipped: boolean): Promise<void>;
	removeArmor(armorId: string): Promise<void>;
	setArmorEquipped(armorId: string, isEquipped: boolean): Promise<void>;
```
```ts
		addArmor: (armorId, isEquipped) => run(() => addArmor(getHeroId(), armorId, isEquipped)),
		removeArmor: (armorId) => run(() => removeArmor(getHeroId(), armorId)),
		setArmorEquipped: (armorId, isEquipped) => run(() => setArmorEquipped(getHeroId(), armorId, isEquipped)),
```

- [ ] **Step 4: Run** `npm test && npm run check` → PASS, 0 errors.

### Task 2.3: `armorId` on view model + `ArmorEditor` + wire

**Files:** Modify `viewmodel.ts`, `resolve.ts`; Create `ArmorEditor.svelte`; Modify `CombatPanel.svelte`

- [ ] **Step 1:** Add `armorId: string;` as the first field of `ArmorViewModel`, and `armorId: a.armorId,` as the first property in the `armorItems:` mapper in `resolve.ts`.

- [ ] **Step 2: Create `ArmorEditor.svelte`** (same shape as `WeaponEditor`, armor fields):

```svelte
<script lang="ts">
	import { getContext } from 'svelte';
	import type { Armor } from '$lib/api/types';
	import type { ArmorViewModel } from '../viewmodel';
	import { getCollection } from '$lib/reference/cache';
	import { HERO_ACTIONS, type HeroActions } from '../heroActions.svelte';
	import Panel from './Panel.svelte';
	import TilePopover from './TilePopover.svelte';

	let { armorItems }: { armorItems: ArmorViewModel[] } = $props();

	const actions = getContext<HeroActions | undefined>(HERO_ACTIONS);

	let catalog = $state<Armor[]>([]);
	let selectedId = $state('');
	let equipped = $state(false);

	const ownedIds = $derived(new Set(armorItems.map((a) => a.armorId)));
	const available = $derived(catalog.filter((a) => !ownedIds.has(a.id)));

	const btn = 'rounded bg-slate-700 px-2 py-1 text-xs font-semibold text-white hover:bg-slate-600 disabled:opacity-50';

	async function loadCatalog() {
		selectedId = '';
		equipped = false;
		if (catalog.length === 0) catalog = await getCollection<Armor>('armor');
	}

	async function add() {
		if (!actions || selectedId === '') return;
		await actions.addArmor(selectedId, equipped);
		selectedId = '';
		equipped = false;
	}
</script>

<Panel title="Armor" empty={armorItems.length === 0 && !actions} emptyText="No armor.">
	<ul class="space-y-2">
		{#each armorItems as a (a.armorId)}
			<li class="flex items-start justify-between gap-2 text-sm text-slate-200">
				<div>
					<span class="font-semibold text-white">{a.name}</span>
					<span class="text-slate-400">{a.type} · +{a.armorValue}</span>
					{#if a.isEquipped}<span class="text-green-400"> · equipped</span>{/if}
				</div>
				{#if actions}
					<div class="flex shrink-0 gap-1">
						<button type="button" class={btn} disabled={actions.busy} onclick={() => actions.setArmorEquipped(a.armorId, !a.isEquipped)}>
							{a.isEquipped ? 'Unequip' : 'Equip'}
						</button>
						<button type="button" class={btn} disabled={actions.busy} aria-label={`Remove ${a.name}`} onclick={() => actions.removeArmor(a.armorId)}>✕</button>
					</div>
				{/if}
			</li>
		{/each}
	</ul>

	{#if actions}
		<div class="mt-2">
			<TilePopover label="Add armor" onopen={loadCatalog}>
				{#snippet trigger()}<span class={btn}>+ Add</span>{/snippet}
				{#snippet content()}
					<select bind:value={selectedId} class="w-full rounded bg-slate-900 px-1.5 py-1 text-xs text-white" aria-label="Armor to add">
						<option value="">— select —</option>
						{#each available as a (a.id)}<option value={a.id}>{a.name}</option>{/each}
					</select>
					<label class="mt-2 flex items-center gap-1 text-xs text-slate-300">
						<input type="checkbox" bind:checked={equipped} /> Equipped
					</label>
					<button type="button" class={`${btn} mt-2 w-full`} disabled={actions.busy || selectedId === ''} onclick={add}>Add</button>
					{#if actions.error}<p class="mt-1 text-[11px] text-red-400">{actions.error}</p>{/if}
				{/snippet}
			</TilePopover>
		</div>
	{/if}
</Panel>
```

- [ ] **Step 3:** In `CombatPanel.svelte`, replace the inline Armor `<Panel>` with `<ArmorEditor armorItems={vm.armorItems} />` and add the import.

- [ ] **Step 4:** `npm test && npm run check && npm run build` → PASS, 0 errors.

### Task 2.4: Commit Slice 2

```bash
git add NS.FastEndpoints/Heroes/SetArmorEquippedEndpoint.cs NS.Tests/HeroTests.cs NS.Client/src/lib/api/client.ts NS.Client/src/lib/api/client.test.ts NS.Client/src/lib/sheet/heroActions.svelte.ts NS.Client/src/lib/sheet/viewmodel.ts NS.Client/src/lib/sheet/resolve.ts NS.Client/src/lib/sheet/components/ArmorEditor.svelte NS.Client/src/lib/sheet/components/CombatPanel.svelte
git commit -m "feat: inline armor add/remove/equip on the sheet"
```

---

## Slice 3 — Magic items

`SetMagicItemEquipped` exists from Task 1.1. Request shapes: `AddMagicItemRequest(HeroId, MagicItemId, IsEquipped, ChargesRemaining)`, `RemoveMagicItemRequest(HeroId, MagicItemId)`.

### Task 3.1: `SetMagicItemEquippedEndpoint` + test

**Files:** Create `NS.FastEndpoints/Heroes/SetMagicItemEquippedEndpoint.cs`; modify `HeroTests.cs`

- [ ] **Step 1: Write the endpoint** (identical structure; `set-magic-item-equipped`, `hero.SetMagicItemEquipped(req.MagicItemId, req.IsEquipped)`).

```csharp
namespace NSFastEndpoints;

/// <summary>Sets whether a magic item the hero carries is equipped.</summary>
public sealed class SetMagicItemEquippedEndpoint : Endpoint<SetMagicItemEquippedRequest>
{
    private readonly IHeroDataService _heroes;

    /// <summary>Initializes the endpoint with the hero data service.</summary>
    public SetMagicItemEquippedEndpoint(IHeroDataService heroes) => _heroes = heroes;

    /// <inheritdoc/>
    public override void Configure()
    {
        Post("heroes/{heroId}/set-magic-item-equipped");
    }

    /// <inheritdoc/>
    public override async Task HandleAsync(SetMagicItemEquippedRequest req, CancellationToken ct)
    {
        var hero = await _heroes.GetOwnedByIdAsync(req.HeroId, User.GetUserId());
        if (hero is null) { await Send.NotFoundAsync(ct); return; }
        hero.SetMagicItemEquipped(req.MagicItemId, req.IsEquipped);
        await _heroes.SaveAsync(hero);
        await Send.NoContentAsync(ct);
    }
}

/// <summary>Request for setting a magic item's equipped state.</summary>
/// <param name="HeroId">The hero's unique identifier (route).</param>
/// <param name="MagicItemId">The identifier of the magic item to update.</param>
/// <param name="IsEquipped">Whether the magic item should be equipped.</param>
public sealed record SetMagicItemEquippedRequest(Guid HeroId, Guid MagicItemId, bool IsEquipped);
```

- [ ] **Step 2: Add a domain test** to `HeroTests.cs`:

```csharp
/// <summary>Equipping a magic item the hero owns flips its equipped flag.</summary>
[Fact]
public void SetMagicItemEquipped_WhenItemPresent_UpdatesFlag()
{
    var hero = TestHero.Create();
    var itemId = Guid.CreateVersion7();
    hero.AddMagicItem(new HeroMagicItem(null, hero.Id, false, itemId));

    hero.SetMagicItemEquipped(itemId, true);

    Assert.True(hero.MagicItems.Single().IsEquipped);
}
```

- [ ] **Step 3:** `dotnet test NS.Tests/NS.Tests.csproj --filter "FullyQualifiedName~SetMagicItemEquipped"` → PASS.

### Task 3.2: Magic-item client wrappers + actions

**Files:** Modify `client.ts`, `client.test.ts`, `heroActions.svelte.ts`

- [ ] **Step 1: Add wrappers** to `client.ts`:

```ts
/** POST /heroes/{id}/add-magic-item — add a magic item from the reference catalog. */
export function addMagicItem(heroId: string, magicItemId: string, isEquipped: boolean, chargesRemaining: number | null): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/add-magic-item`, {
		method: 'POST',
		body: JSON.stringify({ magicItemId, isEquipped, chargesRemaining })
	});
}

/** POST /heroes/{id}/remove-magic-item — remove a magic item by its reference id. */
export function removeMagicItem(heroId: string, magicItemId: string): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/remove-magic-item`, {
		method: 'POST',
		body: JSON.stringify({ magicItemId })
	});
}

/** POST /heroes/{id}/set-magic-item-equipped — equip or unequip a magic item. */
export function setMagicItemEquipped(heroId: string, magicItemId: string, isEquipped: boolean): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/set-magic-item-equipped`, {
		method: 'POST',
		body: JSON.stringify({ magicItemId, isEquipped })
	});
}
```

- [ ] **Step 2: Add one wrapper test** (import the names):

```ts
it('addMagicItem posts magicItemId/isEquipped/chargesRemaining', async () => {
	const fetchMock = captureFetch(204);
	await addMagicItem('h1', 'm1', false, 3);
	expect(fetchMock).toHaveBeenCalledWith(
		'/api/heroes/h1/add-magic-item',
		expect.objectContaining({ method: 'POST', body: JSON.stringify({ magicItemId: 'm1', isEquipped: false, chargesRemaining: 3 }) })
	);
});
```

- [ ] **Step 3: Extend `heroActions`** — import the three; add interface + object entries:

```ts
	addMagicItem(magicItemId: string, isEquipped: boolean, chargesRemaining: number | null): Promise<void>;
	removeMagicItem(magicItemId: string): Promise<void>;
	setMagicItemEquipped(magicItemId: string, isEquipped: boolean): Promise<void>;
```
```ts
		addMagicItem: (magicItemId, isEquipped, chargesRemaining) => run(() => addMagicItem(getHeroId(), magicItemId, isEquipped, chargesRemaining)),
		removeMagicItem: (magicItemId) => run(() => removeMagicItem(getHeroId(), magicItemId)),
		setMagicItemEquipped: (magicItemId, isEquipped) => run(() => setMagicItemEquipped(getHeroId(), magicItemId, isEquipped)),
```

- [ ] **Step 4:** `npm test && npm run check` → PASS.

### Task 3.3: `magicItemId` on view model + `MagicItemEditor` + wire into `InventoryPanel`

**Files:** Modify `viewmodel.ts`, `resolve.ts`; Create `MagicItemEditor.svelte`; Modify `InventoryPanel.svelte`

- [ ] **Step 1:** Add `magicItemId: string;` as the first field of `MagicItemViewModel`, and `magicItemId: m.magicItemId,` as the first property in the `magicItems:` mapper in `resolve.ts`.

- [ ] **Step 2: Create `MagicItemEditor.svelte`.** Default the charges input to the selected item's `maxCharges` when present; send `null` charges when the field is blank.

```svelte
<script lang="ts">
	import { getContext } from 'svelte';
	import type { MagicItem } from '$lib/api/types';
	import type { MagicItemViewModel } from '../viewmodel';
	import { getCollection } from '$lib/reference/cache';
	import { HERO_ACTIONS, type HeroActions } from '../heroActions.svelte';
	import Panel from './Panel.svelte';
	import TilePopover from './TilePopover.svelte';

	let { magicItems }: { magicItems: MagicItemViewModel[] } = $props();

	const actions = getContext<HeroActions | undefined>(HERO_ACTIONS);

	let catalog = $state<MagicItem[]>([]);
	let selectedId = $state('');
	let equipped = $state(false);
	let charges = $state<number | null>(null);

	const ownedIds = $derived(new Set(magicItems.map((m) => m.magicItemId)));
	const available = $derived(catalog.filter((m) => !ownedIds.has(m.id)));

	const btn = 'rounded bg-slate-700 px-2 py-1 text-xs font-semibold text-white hover:bg-slate-600 disabled:opacity-50';

	async function loadCatalog() {
		selectedId = '';
		equipped = false;
		charges = null;
		if (catalog.length === 0) catalog = await getCollection<MagicItem>('magic-items');
	}

	function onSelect() {
		const ref = catalog.find((m) => m.id === selectedId);
		charges = ref?.maxCharges ?? null;
	}

	async function add() {
		if (!actions || selectedId === '') return;
		await actions.addMagicItem(selectedId, equipped, charges === null || Number.isNaN(charges) ? null : charges);
		selectedId = '';
		equipped = false;
		charges = null;
	}
</script>

<Panel title="Magic Items" empty={magicItems.length === 0 && !actions} emptyText="No magic items.">
	<ul class="space-y-2">
		{#each magicItems as m (m.magicItemId)}
			<li class="flex items-start justify-between gap-2 text-sm text-slate-200">
				<div>
					<span class="font-semibold text-white">{m.name}</span>
					<span class="text-slate-400">{m.rarity}</span>
					{#if m.charges}<span class="text-slate-400"> · {m.charges.remaining}/{m.charges.max} charges</span>{/if}
					{#if m.isEquipped}<span class="text-green-400"> · equipped</span>{/if}
					<div class="text-xs text-slate-500">{m.effect}</div>
				</div>
				{#if actions}
					<div class="flex shrink-0 gap-1">
						<button type="button" class={btn} disabled={actions.busy} onclick={() => actions.setMagicItemEquipped(m.magicItemId, !m.isEquipped)}>
							{m.isEquipped ? 'Unequip' : 'Equip'}
						</button>
						<button type="button" class={btn} disabled={actions.busy} aria-label={`Remove ${m.name}`} onclick={() => actions.removeMagicItem(m.magicItemId)}>✕</button>
					</div>
				{/if}
			</li>
		{/each}
	</ul>

	{#if actions}
		<div class="mt-2">
			<TilePopover label="Add magic item" onopen={loadCatalog}>
				{#snippet trigger()}<span class={btn}>+ Add</span>{/snippet}
				{#snippet content()}
					<select bind:value={selectedId} onchange={onSelect} class="w-full rounded bg-slate-900 px-1.5 py-1 text-xs text-white" aria-label="Magic item to add">
						<option value="">— select —</option>
						{#each available as m (m.id)}<option value={m.id}>{m.name}</option>{/each}
					</select>
					<label class="mt-2 flex items-center gap-1 text-xs text-slate-300">
						<input type="checkbox" bind:checked={equipped} /> Equipped
					</label>
					<label class="mt-2 block text-xs text-slate-300">Charges (blank = none)
						<input type="number" min="0" bind:value={charges} class="mt-1 w-full rounded bg-slate-900 px-1.5 py-1 text-xs text-white" />
					</label>
					<button type="button" class={`${btn} mt-2 w-full`} disabled={actions.busy || selectedId === ''} onclick={add}>Add</button>
					{#if actions.error}<p class="mt-1 text-[11px] text-red-400">{actions.error}</p>{/if}
				{/snippet}
			</TilePopover>
		</div>
	{/if}
</Panel>
```

- [ ] **Step 3:** In `InventoryPanel.svelte`, replace the inline Magic Items `<Panel>` with `<MagicItemEditor magicItems={vm.magicItems} />` and add the import (Gear stays inline until Slice 4).

- [ ] **Step 4:** `npm test && npm run check && npm run build` → PASS.

### Task 3.4: Commit Slice 3

```bash
git add NS.FastEndpoints/Heroes/SetMagicItemEquippedEndpoint.cs NS.Tests/HeroTests.cs NS.Client/src/lib/api/client.ts NS.Client/src/lib/api/client.test.ts NS.Client/src/lib/sheet/heroActions.svelte.ts NS.Client/src/lib/sheet/viewmodel.ts NS.Client/src/lib/sheet/resolve.ts NS.Client/src/lib/sheet/components/MagicItemEditor.svelte NS.Client/src/lib/sheet/components/InventoryPanel.svelte
git commit -m "feat: inline magic item add/remove/equip on the sheet"
```

---

## Slice 4 — Spells (add/remove, no equip)

No domain/endpoint work (`AddSpell`/`RemoveSpell` exist). Request shapes: `AddSpellRequest(HeroId, SpellId, TierUnlocked, Notes)`, `RemoveSpellRequest(HeroId, SpellId)`. The view model groups spells by tier; add `spellId` to `SpellViewModel`.

### Task 4.1: Spell client wrappers + actions

**Files:** Modify `client.ts`, `client.test.ts`, `heroActions.svelte.ts`

- [ ] **Step 1: Add wrappers** to `client.ts`:

```ts
/** POST /heroes/{id}/add-spell — learn a spell from the reference catalog. */
export function addSpell(heroId: string, spellId: string, tierUnlocked: number, notes: string | null): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/add-spell`, {
		method: 'POST',
		body: JSON.stringify({ spellId, tierUnlocked, notes })
	});
}

/** POST /heroes/{id}/remove-spell — forget a spell by its reference id. */
export function removeSpell(heroId: string, spellId: string): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/remove-spell`, {
		method: 'POST',
		body: JSON.stringify({ spellId })
	});
}
```

- [ ] **Step 2: Add one wrapper test** (import the names):

```ts
it('addSpell posts spellId/tierUnlocked/notes', async () => {
	const fetchMock = captureFetch(204);
	await addSpell('h1', 's1', 2, null);
	expect(fetchMock).toHaveBeenCalledWith(
		'/api/heroes/h1/add-spell',
		expect.objectContaining({ method: 'POST', body: JSON.stringify({ spellId: 's1', tierUnlocked: 2, notes: null }) })
	);
});
```

- [ ] **Step 3: Extend `heroActions`** — import `addSpell, removeSpell`; add:

```ts
	addSpell(spellId: string, tierUnlocked: number, notes: string | null): Promise<void>;
	removeSpell(spellId: string): Promise<void>;
```
```ts
		addSpell: (spellId, tierUnlocked, notes) => run(() => addSpell(getHeroId(), spellId, tierUnlocked, notes)),
		removeSpell: (spellId) => run(() => removeSpell(getHeroId(), spellId)),
```

- [ ] **Step 4:** `npm test && npm run check` → PASS.

### Task 4.2: `spellId` on view model + `SpellEditor` + wire into `MagicPanel`

**Files:** Modify `viewmodel.ts`, `resolve.ts`; Create `SpellEditor.svelte`; Modify `MagicPanel.svelte`

- [ ] **Step 1:** Add `spellId: string;` as the first field of `SpellViewModel`. In `resolve.ts` `buildSpellsByTier`, add `spellId: known.spellId,` to **both** returned objects (the resolved branch and the "Unknown spell" fallback).

- [ ] **Step 2: Read `MagicPanel.svelte`** first to see how it renders `vm.spellsByTier` (tier groups). Create `SpellEditor.svelte` that renders the existing tier-grouped list, adds a per-spell ✕ when actions present, and a single "+ Add" popover (tier defaults to the selected spell's own tier):

```svelte
<script lang="ts">
	import { getContext } from 'svelte';
	import type { Spell } from '$lib/api/types';
	import type { SpellTierGroup } from '../viewmodel';
	import { getCollection } from '$lib/reference/cache';
	import { HERO_ACTIONS, type HeroActions } from '../heroActions.svelte';
	import Panel from './Panel.svelte';
	import TilePopover from './TilePopover.svelte';

	let { spellsByTier }: { spellsByTier: SpellTierGroup[] } = $props();

	const actions = getContext<HeroActions | undefined>(HERO_ACTIONS);

	let catalog = $state<Spell[]>([]);
	let selectedId = $state('');
	let tier = $state(1);

	const ownedIds = $derived(new Set(spellsByTier.flatMap((g) => g.spells.map((s) => s.spellId))));
	const available = $derived(catalog.filter((s) => !ownedIds.has(s.id)));
	const isEmpty = $derived(spellsByTier.length === 0);

	const btn = 'rounded bg-slate-700 px-2 py-1 text-xs font-semibold text-white hover:bg-slate-600 disabled:opacity-50';

	async function loadCatalog() {
		selectedId = '';
		if (catalog.length === 0) catalog = await getCollection<Spell>('spells');
	}

	function onSelect() {
		const ref = catalog.find((s) => s.id === selectedId);
		if (ref) tier = ref.tier;
	}

	async function add() {
		if (!actions || selectedId === '') return;
		await actions.addSpell(selectedId, tier, null);
		selectedId = '';
	}
</script>

<Panel title="Spells" empty={isEmpty && !actions} emptyText="No spells known.">
	<div class="space-y-3">
		{#each spellsByTier as group (group.tier)}
			<div>
				<div class="text-xs font-semibold uppercase tracking-wide text-slate-400">Tier {group.tier}</div>
				<ul class="mt-1 space-y-1">
					{#each group.spells as s (s.spellId)}
						<li class="flex items-start justify-between gap-2 text-sm text-slate-200">
							<div>
								<span class="font-semibold text-white">{s.name}</span>
								<span class="text-slate-400">{s.manaCost} mana · {s.actionCost} action</span>
								{#if s.damage}<span class="text-slate-400"> · {s.damage} {s.damageType}</span>{/if}
							</div>
							{#if actions}
								<button type="button" class={btn} disabled={actions.busy} aria-label={`Remove ${s.name}`} onclick={() => actions.removeSpell(s.spellId)}>✕</button>
							{/if}
						</li>
					{/each}
				</ul>
			</div>
		{/each}
	</div>

	{#if actions}
		<div class="mt-2">
			<TilePopover label="Add spell" onopen={loadCatalog}>
				{#snippet trigger()}<span class={btn}>+ Add</span>{/snippet}
				{#snippet content()}
					<select bind:value={selectedId} onchange={onSelect} class="w-full rounded bg-slate-900 px-1.5 py-1 text-xs text-white" aria-label="Spell to add">
						<option value="">— select —</option>
						{#each available as s (s.id)}<option value={s.id}>{s.name} (T{s.tier})</option>{/each}
					</select>
					<label class="mt-2 block text-xs text-slate-300">Tier unlocked
						<input type="number" min="1" bind:value={tier} class="mt-1 w-full rounded bg-slate-900 px-1.5 py-1 text-xs text-white" />
					</label>
					<button type="button" class={`${btn} mt-2 w-full`} disabled={actions.busy || selectedId === ''} onclick={add}>Add</button>
					{#if actions.error}<p class="mt-1 text-[11px] text-red-400">{actions.error}</p>{/if}
				{/snippet}
			</TilePopover>
		</div>
	{/if}
</Panel>
```

- [ ] **Step 3:** In `MagicPanel.svelte`, replace the inline spells rendering with `<SpellEditor spellsByTier={vm.spellsByTier} />` and add the import. (If `MagicPanel` also renders class resources or other content, leave that untouched — replace only the spells section.)

- [ ] **Step 4:** `npm test && npm run check && npm run build` → PASS.

### Task 4.3: Commit Slice 4

```bash
git add NS.Client/src/lib/api/client.ts NS.Client/src/lib/api/client.test.ts NS.Client/src/lib/sheet/heroActions.svelte.ts NS.Client/src/lib/sheet/viewmodel.ts NS.Client/src/lib/sheet/resolve.ts NS.Client/src/lib/sheet/components/SpellEditor.svelte NS.Client/src/lib/sheet/components/MagicPanel.svelte
git commit -m "feat: inline spell add/remove on the sheet"
```

---

## Slice 5 — Gear (free-text, no reference picker, no equip)

Request shapes: `AddGearItemRequest(HeroId, Name, Quantity)`, `RemoveGearItemRequest(HeroId, Name)`. Gear keys by **name** (no reference id). `GearViewModel` already has `name`/`quantity`.

### Task 5.1: Gear client wrappers + actions

**Files:** Modify `client.ts`, `client.test.ts`, `heroActions.svelte.ts`

- [ ] **Step 1: Add wrappers** to `client.ts`:

```ts
/** POST /heroes/{id}/add-gear-item — add a free-text gear item. */
export function addGearItem(heroId: string, name: string, quantity: number): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/add-gear-item`, {
		method: 'POST',
		body: JSON.stringify({ name, quantity })
	});
}

/** POST /heroes/{id}/remove-gear-item — remove a gear item by name. */
export function removeGearItem(heroId: string, name: string): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/remove-gear-item`, {
		method: 'POST',
		body: JSON.stringify({ name })
	});
}
```

- [ ] **Step 2: Add one wrapper test** (import the names):

```ts
it('addGearItem posts name/quantity', async () => {
	const fetchMock = captureFetch(204);
	await addGearItem('h1', 'Torch', 5);
	expect(fetchMock).toHaveBeenCalledWith(
		'/api/heroes/h1/add-gear-item',
		expect.objectContaining({ method: 'POST', body: JSON.stringify({ name: 'Torch', quantity: 5 }) })
	);
});
```

- [ ] **Step 3: Extend `heroActions`** — import `addGearItem, removeGearItem`; add:

```ts
	addGearItem(name: string, quantity: number): Promise<void>;
	removeGearItem(name: string): Promise<void>;
```
```ts
		addGearItem: (name, quantity) => run(() => addGearItem(getHeroId(), name, quantity)),
		removeGearItem: (name) => run(() => removeGearItem(getHeroId(), name)),
```

- [ ] **Step 4:** `npm test && npm run check` → PASS.

### Task 5.2: `GearEditor` + wire into `InventoryPanel`

**Files:** Create `GearEditor.svelte`; Modify `InventoryPanel.svelte`

- [ ] **Step 1: Create `GearEditor.svelte`** (free-text name + quantity; no catalog fetch). Guard against blank names client-side; quantity defaults to 1.

```svelte
<script lang="ts">
	import { getContext } from 'svelte';
	import type { GearViewModel } from '../viewmodel';
	import { HERO_ACTIONS, type HeroActions } from '../heroActions.svelte';
	import Panel from './Panel.svelte';
	import TilePopover from './TilePopover.svelte';

	let { gear }: { gear: GearViewModel[] } = $props();

	const actions = getContext<HeroActions | undefined>(HERO_ACTIONS);

	let name = $state('');
	let quantity = $state(1);

	const canAdd = $derived(name.trim() !== '' && quantity > 0);
	const btn = 'rounded bg-slate-700 px-2 py-1 text-xs font-semibold text-white hover:bg-slate-600 disabled:opacity-50';

	function reset() {
		name = '';
		quantity = 1;
	}

	async function add() {
		if (!actions || !canAdd) return;
		await actions.addGearItem(name.trim(), quantity);
		reset();
	}
</script>

<Panel title="Gear" empty={gear.length === 0 && !actions} emptyText="No gear.">
	<ul class="space-y-1">
		{#each gear as g (g.name)}
			<li class="flex items-center justify-between gap-2 text-sm text-slate-200">
				<span>
					<span class="font-semibold text-white">{g.name}</span>
					{#if g.quantity > 1}<span class="text-slate-400"> ×{g.quantity}</span>{/if}
				</span>
				{#if actions}
					<button type="button" class={btn} disabled={actions.busy} aria-label={`Remove ${g.name}`} onclick={() => actions.removeGearItem(g.name)}>✕</button>
				{/if}
			</li>
		{/each}
	</ul>

	{#if actions}
		<div class="mt-2">
			<TilePopover label="Add gear" onopen={reset}>
				{#snippet trigger()}<span class={btn}>+ Add</span>{/snippet}
				{#snippet content()}
					<input type="text" bind:value={name} placeholder="Item name" class="w-full rounded bg-slate-900 px-1.5 py-1 text-xs text-white" aria-label="Gear name" />
					<label class="mt-2 block text-xs text-slate-300">Quantity
						<input type="number" min="1" bind:value={quantity} class="mt-1 w-full rounded bg-slate-900 px-1.5 py-1 text-xs text-white" />
					</label>
					<button type="button" class={`${btn} mt-2 w-full`} disabled={actions.busy || !canAdd} onclick={add}>Add</button>
					{#if actions.error}<p class="mt-1 text-[11px] text-red-400">{actions.error}</p>{/if}
				{/snippet}
			</TilePopover>
		</div>
	{/if}
</Panel>
```

- [ ] **Step 2:** In `InventoryPanel.svelte`, replace the inline Gear `<Panel>` with `<GearEditor gear={vm.gear} />` and add the import. After this, `InventoryPanel` imports only `MagicItemEditor` and `GearEditor` (no more inline `<ul>`s) — remove the now-unused `Panel` import if nothing else uses it.

- [ ] **Step 3:** `npm test && npm run check && npm run build` → PASS.

### Task 5.3: Commit Slice 5

```bash
git add NS.Client/src/lib/api/client.ts NS.Client/src/lib/api/client.test.ts NS.Client/src/lib/sheet/heroActions.svelte.ts NS.Client/src/lib/sheet/components/GearEditor.svelte NS.Client/src/lib/sheet/components/InventoryPanel.svelte
git commit -m "feat: inline gear add/remove on the sheet"
```

---

## Slice 6 — Conditions (reference-backed, add/remove, no equip)

Request shapes: `AddConditionRequest(HeroId, ConditionId, ExpiresAtEndOf)`, `RemoveConditionRequest(HeroId, ConditionId)`.

### Task 6.1: Condition client wrappers + actions

**Files:** Modify `client.ts`, `client.test.ts`, `heroActions.svelte.ts`

- [ ] **Step 1: Add wrappers** to `client.ts`:

```ts
/** POST /heroes/{id}/add-condition — apply a condition from the reference catalog. */
export function addCondition(heroId: string, conditionId: string, expiresAtEndOf: string | null): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/add-condition`, {
		method: 'POST',
		body: JSON.stringify({ conditionId, expiresAtEndOf })
	});
}

/** POST /heroes/{id}/remove-condition — clear a condition by its reference id. */
export function removeCondition(heroId: string, conditionId: string): Promise<void> {
	return apiFetch<void>(`/heroes/${heroId}/remove-condition`, {
		method: 'POST',
		body: JSON.stringify({ conditionId })
	});
}
```

- [ ] **Step 2: Add one wrapper test** (import the names):

```ts
it('addCondition posts conditionId/expiresAtEndOf', async () => {
	const fetchMock = captureFetch(204);
	await addCondition('h1', 'c1', null);
	expect(fetchMock).toHaveBeenCalledWith(
		'/api/heroes/h1/add-condition',
		expect.objectContaining({ method: 'POST', body: JSON.stringify({ conditionId: 'c1', expiresAtEndOf: null }) })
	);
});
```

- [ ] **Step 3: Extend `heroActions`** — import `addCondition, removeCondition`; add:

```ts
	addCondition(conditionId: string, expiresAtEndOf: string | null): Promise<void>;
	removeCondition(conditionId: string): Promise<void>;
```
```ts
		addCondition: (conditionId, expiresAtEndOf) => run(() => addCondition(getHeroId(), conditionId, expiresAtEndOf)),
		removeCondition: (conditionId) => run(() => removeCondition(getHeroId(), conditionId)),
```

- [ ] **Step 4:** `npm test && npm run check` → PASS.

### Task 6.2: `conditionId` on view model + `ConditionEditor` + wire into `CombatPanel`

**Files:** Modify `viewmodel.ts`, `resolve.ts`; Create `ConditionEditor.svelte`; Modify `CombatPanel.svelte`

- [ ] **Step 1:** Add `conditionId: string;` as the first field of `ConditionViewModel`, and `conditionId: c.conditionId,` as the first property in the `conditions:` mapper in `resolve.ts`.

- [ ] **Step 2: Create `ConditionEditor.svelte`** (reference picker + optional "expires at end of" text):

```svelte
<script lang="ts">
	import { getContext } from 'svelte';
	import type { Condition } from '$lib/api/types';
	import type { ConditionViewModel } from '../viewmodel';
	import { getCollection } from '$lib/reference/cache';
	import { HERO_ACTIONS, type HeroActions } from '../heroActions.svelte';
	import Panel from './Panel.svelte';
	import TilePopover from './TilePopover.svelte';

	let { conditions }: { conditions: ConditionViewModel[] } = $props();

	const actions = getContext<HeroActions | undefined>(HERO_ACTIONS);

	let catalog = $state<Condition[]>([]);
	let selectedId = $state('');
	let expires = $state('');

	const ownedIds = $derived(new Set(conditions.map((c) => c.conditionId)));
	const available = $derived(catalog.filter((c) => !ownedIds.has(c.id)));

	const btn = 'rounded bg-slate-700 px-2 py-1 text-xs font-semibold text-white hover:bg-slate-600 disabled:opacity-50';

	async function loadCatalog() {
		selectedId = '';
		expires = '';
		if (catalog.length === 0) catalog = await getCollection<Condition>('conditions');
	}

	async function add() {
		if (!actions || selectedId === '') return;
		await actions.addCondition(selectedId, expires.trim() === '' ? null : expires.trim());
		selectedId = '';
		expires = '';
	}
</script>

<Panel title="Conditions" empty={conditions.length === 0 && !actions} emptyText="No active conditions.">
	<ul class="space-y-2">
		{#each conditions as c (c.conditionId)}
			<li class="flex items-start justify-between gap-2 text-sm text-slate-200">
				<div>
					<span class="font-semibold text-white">{c.name}</span>
					{#if c.expiresAtEndOf}<span class="text-slate-400"> · expires {c.expiresAtEndOf}</span>{/if}
					<div class="text-xs text-slate-500">{c.description}</div>
				</div>
				{#if actions}
					<button type="button" class={btn} disabled={actions.busy} aria-label={`Remove ${c.name}`} onclick={() => actions.removeCondition(c.conditionId)}>✕</button>
				{/if}
			</li>
		{/each}
	</ul>

	{#if actions}
		<div class="mt-2">
			<TilePopover label="Add condition" onopen={loadCatalog}>
				{#snippet trigger()}<span class={btn}>+ Add</span>{/snippet}
				{#snippet content()}
					<select bind:value={selectedId} class="w-full rounded bg-slate-900 px-1.5 py-1 text-xs text-white" aria-label="Condition to add">
						<option value="">— select —</option>
						{#each available as c (c.id)}<option value={c.id}>{c.name}</option>{/each}
					</select>
					<input type="text" bind:value={expires} placeholder="Expires at end of (optional)" class="mt-2 w-full rounded bg-slate-900 px-1.5 py-1 text-xs text-white" aria-label="Expires at end of" />
					<button type="button" class={`${btn} mt-2 w-full`} disabled={actions.busy || selectedId === ''} onclick={add}>Add</button>
					{#if actions.error}<p class="mt-1 text-[11px] text-red-400">{actions.error}</p>{/if}
				{/snippet}
			</TilePopover>
		</div>
	{/if}
</Panel>
```

- [ ] **Step 3:** In `CombatPanel.svelte`, replace the inline Conditions `<Panel>` with `<ConditionEditor conditions={vm.conditions} />` and add the import. After this `CombatPanel` composes `WeaponEditor`, `ArmorEditor`, `ConditionEditor` — remove the now-unused `Panel` import if nothing else uses it.

- [ ] **Step 4:** `npm test && npm run check && npm run build` → PASS.

### Task 6.3: Commit Slice 6

```bash
git add NS.Client/src/lib/sheet/viewmodel.ts NS.Client/src/lib/sheet/resolve.ts NS.Client/src/lib/api/client.ts NS.Client/src/lib/api/client.test.ts NS.Client/src/lib/sheet/heroActions.svelte.ts NS.Client/src/lib/sheet/components/ConditionEditor.svelte NS.Client/src/lib/sheet/components/CombatPanel.svelte
git commit -m "feat: inline condition add/remove on the sheet"
```

---

## Slice 7 — Browser verification + docs

### Task 7.1: Full-stack verification in a browser

- [ ] **Step 1: Build the SPA into wwwroot and run the server.** (Plain `dotnet build` skips the SPA rebuild when `wwwroot/index.html` exists — rebuild explicitly.)

```bash
cd NS.Client && npm run build && cd ..
rm -rf NS.WebApp/wwwroot && mkdir -p NS.WebApp/wwwroot && cp -r NS.Client/build/* NS.WebApp/wwwroot/
rm -f NS.WebApp/nimble-sheet.db
ASPNETCORE_ENVIRONMENT=Development dotnet run --project NS.WebApp/NS.WebApp.csproj --no-launch-profile
```
Server listens on `http://localhost:5000`.

- [ ] **Step 2: Drive the flow** (Playwright, headless Chromium — reuse the scratch setup from the prior verification: `npx playwright install chromium`, a temp dir with `npm i playwright`). Script: login → create user → New hero (Human) → open the sheet → in Combat, add a weapon, equip it (verify "equipped" appears), remove it (verify it disappears); add a spell (Magic tab); add gear "Torch" ×5 (Inventory); add a condition (Combat). Screenshot each stage; capture console/network errors. Each mutation should round-trip (POST under `/api`, then the re-fetched sheet reflects it).

- [ ] **Step 3: Record results.** PASS only if each add/remove/equip is visible on the re-fetched sheet with no JS/page errors. Stop the server when done.

### Task 7.2: Update docs + final commit

**Files:** Modify `CLAUDE.md`

- [ ] **Step 1: Update the Hero endpoint routes table** — add the three equip routes:

```
| POST | `/heroes/{heroId}/set-weapon-equipped` | `SetWeaponEquippedEndpoint` |
| POST | `/heroes/{heroId}/set-armor-equipped` | `SetArmorEquippedEndpoint` |
| POST | `/heroes/{heroId}/set-magic-item-equipped` | `SetMagicItemEquippedEndpoint` |
```

- [ ] **Step 2: Update the NS.Domain mutation-methods list** — add `SetArmorEquipped`, `SetMagicItemEquipped`, `SetWeaponEquipped` (alphabetical), with a one-line note that they replace the matching record's `IsEquipped` via `with` and no-op when absent.

- [ ] **Step 3: Update the NS.Client section** — note that the sheet panels now support inline add/remove (and equip for weapons/armor/magic items) via per-collection editor components that consume `HERO_ACTIONS` optionally (read-only when absent); the client has wrappers for all add/remove/equip endpoints; "Add" pickers lazily use `getCollection`. Remove the "equipment/spell collections are not in the form" caveat's implication that there's no way to edit them — clarify the build form still excludes collections but the sheet now edits them.

- [ ] **Step 4: Commit.**

```bash
git add CLAUDE.md
git commit -m "docs: document inline collection editing + equip endpoints"
```

### Task 7.3: Finish the branch

- [ ] **Step 1:** Use the `superpowers:finishing-a-development-branch` skill to merge `feat/collection-editing` into `main` locally and delete the branch (the user's established flow: `--no-ff` merge, delete branch, do not push).

---

## Self-Review

**Spec coverage:** add/remove for weapons (S1), armor (S2), magic items (S3), spells (S4), gear (S5), conditions (S6) ✓; equip/unequip endpoints for weapons/armor/magic items (S1 domain + S1/S2/S3 endpoints) ✓; inline on the sheet via per-collection editors consuming `HERO_ACTIONS` optionally ✓; lazy reference loading via `getCollection` ✓; duplicate prevention via owned-id filtering ✓; view-model ids ✓; tests (domain equip, client wrappers, resolver ids) ✓; browser verification ✓; docs ✓. Features excluded ✓.

**Placeholder scan:** No TBD/TODO. Two steps say "read the file first" (`resolve.test.ts` reference-bundle name; `MagicPanel.svelte` spell rendering) because those files weren't fully read while planning — each gives the concrete action to take once read. All code steps include full code.

**Type consistency:** `HeroActions` method names match their `client.ts` wrappers and the `actions.*` calls in the editors (`addWeapon`/`removeWeapon`/`setWeaponEquipped`, etc.). View-model id field names (`weaponId`/`armorId`/`magicItemId`/`spellId`/`conditionId`) match the resolver source records (`w.weaponId`, `a.armorId`, `m.magicItemId`, `known.spellId`, `c.conditionId`) and the `Hero*` types in `api/types.ts`. Domain method signatures match the endpoint call sites and the request records. Reference resource strings (`'weapons'`, `'armor'`, `'magic-items'`, `'spells'`, `'conditions'`) match `ReferenceResource`.
