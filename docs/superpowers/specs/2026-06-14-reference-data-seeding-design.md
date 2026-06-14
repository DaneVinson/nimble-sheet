# Reference-Data Seeding — Design

**Date:** 2026-06-14
**Status:** Approved (brainstorming) — ready for implementation plan

## Problem

The SoloDB database starts empty and the reference endpoints (`GET /reference/*`) are deliberately read-only, so there is no way to get reference rows (ancestries, armor, weapons, features, spells, …) into the database through the running application. Consequently the live `/heroes/[id]` sheet cannot resolve a hero's ID-referenced collections to real reference **names**, and the app cannot be exercised or visually verified end-to-end. This slice adds a seeding mechanism that populates a curated starter set of reference data at application startup.

## Goals

- Populate every reference collection with a small, curated **starter set** sufficient to resolve two example heroes end-to-end: Caldra (Oathsworn, non-caster) and a Mage (caster).
- Preserve the deliberate **GET-only** reference-data contract — seeding must not add a write method to `IReferenceDataService<T>`.
- Be **idempotent** on restart via a per-collection "seed only when empty" check.
- Keep all SoloDB collection access inside NS.SoloDB.

## Non-Goals

- Authoring the complete real Nimble ruleset (deferred; see the rules-reference notes for open gaps such as spell schools and the mana formula).
- A runtime admin/write API for reference data.
- Seeding users or heroes (heroes are user-owned and created via the build API).
- Upsert/overwrite of existing seed rows (explicitly out: chosen behavior is seed-when-empty).

## Decisions (from brainstorming)

| Question | Decision |
|---|---|
| Data scope | Demonstrable starter set (resolves Caldra + one caster end-to-end) |
| Seed format | Hardcoded C# positional-record literals with fixed `Guid` constants |
| Idempotency | Seed only when the target collection is empty (never overwrite) |
| Architecture | Dedicated internal seeder in NS.SoloDB (Approach A) |

## Architecture (Approach A)

All new persistence code lives in **NS.SoloDB**, where the `SoloCollections.Of<T>` internals already are.

### `IReferenceDataSeeder` (public interface, NS.SoloDB)

```csharp
/// <summary>Populates reference collections with the curated starter data set.</summary>
public interface IReferenceDataSeeder
{
    /// <summary>Seeds each reference collection that is currently empty. Idempotent.</summary>
    Task SeedAsync(CancellationToken cancellationToken = default);
}
```

### `SoloReferenceDataSeeder` (internal sealed, implements the interface)

- Constructed with the `SoloDB` singleton.
- `SeedAsync` calls a private generic helper for each of the 10 reference types:

  ```csharp
  private void SeedIfEmpty<T>(IReadOnlyList<T> rows) where T : class
  {
      var collection = SoloCollections.Of<T>(_db);
      if (collection.Any())
      {
          return;
      }
      foreach (var row in rows)
      {
          collection.Insert(new SoloDocument<T> { Data = row });
      }
  }
  ```

  (Exact emptiness check — `Any()` vs count — finalized in the plan against the SoloDB collection API; intent is "if the collection has any rows, skip it".)
- Per-collection check, so a partially-populated DB leaves already-populated collections untouched and still fills any empty ones.

### `SeedData` (internal static, NS.SoloDB)

- A block of **fixed `Guid` constants** (stable, hand-written GUIDs — **not** `Guid.CreateVersion7()`) so heroes can reference seeded rows by a known id and re-seeding is deterministic.
- One `IReadOnlyList<T>` per reference type, built from positional-record literals: `Ancestries`, `Armor`, `Weapons`, `Features`, `Spells`, `Conditions`, `Backgrounds`, `Actions`, `MagicItems`, `Rules`.
- The rows that Caldra references — Human (ancestry), Rusty Mail + Wooden Buckler (armor), Mace (weapon), Radiant Judgment + Lay on Hands (features) — **reuse the exact Guids from the client `NS.Client/src/lib/fixtures/caldra.ts` fixture**, so the fixture and the real seed share identity.

## Startup wiring & idempotency

- `ServiceCollectionExtensions.AddSoloDBDataServices` registers `IReferenceDataSeeder` → `SoloReferenceDataSeeder` as a **singleton**, alongside the existing data services.
- `NS.WebApp/Program.cs`, after `var app = builder.Build();` and before `app.Run();`:

  ```csharp
  await app.Services.GetRequiredService<IReferenceDataSeeder>().SeedAsync();
  ```

  This runs once, synchronously, before the server begins listening, guaranteeing reference data is present for the first request. (Chosen over an `IHostedService`, which starts after the server is already accepting requests and would race the first calls.)
- Idempotency comes entirely from the per-collection empty check. Editing seed data later requires a fresh database (acceptable for the POC).

## Seed content scope (the "demonstrable starter set")

Counts are targets; exact wording/stats for non-Caldra rows are filled in during implementation. The design fixes the **shape and counts**, not every string.

| Type | Count | Must-haves / examples |
|---|---|---|
| Ancestries | ~3 | Human (Caldra), Elf, Dwarf |
| Armor | ~4 | Rusty Mail + Wooden Buckler (Caldra), one Cloth, one Leather (widen `ArmorType` coverage) |
| Weapons | ~3 | Mace (Caldra), a ranged (e.g. Shortbow), a two-handed (e.g. Greatsword) |
| Features | ~3 | Radiant Judgment + Lay on Hands (Oathsworn L1, Caldra), one Mage L1 feature |
| Spells | ~4 | One Mage spell per `SpellSchool` (Fire/Ice/Lightning/Radiant) so the Magic tab + mana resolve |
| Conditions | ~3 | e.g. Prone, Bleeding, Dazed |
| Backgrounds | ~2 | any two |
| Actions | ~3 | a few common `ActionReference`s |
| MagicItems | ~2 | one with `MaxCharges`, one without |
| Rules | ~3 | across distinct `RuleCategory` values |

End-to-end verification target: create a user, create a hero via the build API referencing these seeded Guids (Caldra-shaped and a Mage-shaped hero), then view `/heroes/[id]` and confirm names resolve and the Magic tab populates for the caster.

## Testing (NS.Tests, xUnit — tests-after)

- **Populates all collections:** `SeedAsync` against a fresh in-memory SoloDB makes every reference type's `GetAllAsync().Count > 0`.
- **Known id resolves:** a fixed seed Guid (e.g. Human ancestry) resolves via the matching `IReferenceDataService<T>.GetByIdAsync`.
- **Idempotent:** running `SeedAsync` twice leaves row counts unchanged (no duplication).

## Files

**New**
- `NS.SoloDB/IReferenceDataSeeder.cs`
- `NS.SoloDB/SoloReferenceDataSeeder.cs`
- `NS.SoloDB/SeedData.cs`
- `NS.Tests/SeedingTests.cs`

**Modified**
- `NS.SoloDB/ServiceCollectionExtensions.cs` — register the seeder
- `NS.WebApp/Program.cs` — invoke `SeedAsync()` at startup
- `NS.SoloDB/_GlobalUsings.cs` — only if a new namespace is required

## Risks / open items

- **Seed content accuracy:** non-Caldra rows are representative, not canonical Nimble data; the full ruleset remains a separate, larger effort.
- **Emptiness-check API:** the exact SoloDB call for "collection has any rows" is confirmed against the SoloDB collection surface during implementation.
- **C# conventions:** `sealed`, positional records, member ordering, XML docs, `_GlobalUsings.cs`, and **fixed Guids (not `CreateVersion7()`)** for seed identity all apply per the project's conventions.
