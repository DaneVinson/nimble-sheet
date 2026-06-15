# Hero Mutation Validation — Design

**Date:** 2026-06-14
**Status:** Approved (brainstorming) — ready for implementation plan

## Problem

The hero play-mutation endpoints have no request validation, and the `Hero` domain methods clamp with `Math.Max`/`Math.Min` but never reject negative amounts. Via the API this silently inverts intent:

- `TakeDamage(-5)` → `Math.Max(hp - (-5), 0)` = **heals**
- `Heal(-5)` → reduces HP
- `SpendMana(-5)` → **restores** mana
- `SpendHitDice(-1, …)` → **gains** a hit die; a negative `HealingAmount` damages the hero

The "server owns the rules" premise is therefore false. This slice closes the gap with two enforcement layers.

## Goals

- Reject invalid amounts at the API boundary with a **400 + field message** (FluentValidation, matching the existing `HeroBuildValidator`).
- Make the `Hero` domain authoritative for any caller by throwing on negative amounts (defense-in-depth).
- Keep the existing clamping behavior for valid inputs unchanged.

## Non-Goals

- No upper-bound / "exceeds available" rules (the domain already floors hit dice and HP at 0; over-large amounts are harmless).
- No changes to the no-body endpoints (`gain-wound`, `heal-wound`, `recover-all-resources`) — they take no numeric input and are already safe.
- No new endpoint integration-test harness; validators are unit-tested directly.
- No client changes (the client already surfaces FastEndpoints validation messages via `readErrorMessage`).

## Decisions (from brainstorming)

| Question | Decision |
|---|---|
| Enforcement location | **Both** API validators and domain guards (defense-in-depth) |
| Domain floor | Uniformly "no negatives" (`< 0` throws `ArgumentOutOfRangeException`) |
| API bounds | Stricter where a zero is meaningless (see table); `grant-temp-hp` and `HealingAmount` allow 0 |

## Architecture

Two layers with a deliberate division of responsibility:

1. **API validators** — one `Validator<TRequest>` per amount-bearing request, enforcing the *UX-level* bound (a meaningful action). A failing validator short-circuits to a 400 before `HandleAsync` runs, so the endpoint handlers are unchanged.
2. **Domain guards** — a guard clause at the top of each affected `Hero` method enforcing the *corruption* invariant (no negatives). The existing clamping logic stays after the guard.

The bounds differ on purpose: the domain forbids only the dangerous case (negatives); the API additionally forbids no-op zeros where they are meaningless.

### The rules

Only the amount-bearing requests are affected.

| Request field | API validator (400) | Domain guard (throws) |
|---|---|---|
| `TakeDamageRequest.Amount` | `GreaterThan(0)` | `< 0` |
| `HealRequest.Amount` | `GreaterThan(0)` | `< 0` |
| `GrantTempHpRequest.Amount` | `GreaterThanOrEqualTo(0)` | `< 0` |
| `SpendManaRequest.Amount` | `GreaterThan(0)` | `< 0` |
| `SpendHitDiceRequest.Count` | `GreaterThan(0)` | `< 0` |
| `SpendHitDiceRequest.HealingAmount` | `GreaterThanOrEqualTo(0)` | `< 0` |

`grant-temp-hp` and `HealingAmount` allow `0` (a harmless no-op the client already permits); the rest require a positive action. The domain floor is uniformly "no negatives."

### Components

- **Validators** — each `Validator<TRequest>` is co-located in the same endpoint file as its request record, following the repo's one-class-per-file convention (as `HeroBuildValidator` sits in `HeroBuildRequest.cs`). FastEndpoints auto-discovers them. Naming: `TakeDamageValidator`, `HealValidator`, `GrantTempHpValidator`, `SpendManaValidator`, `SpendHitDiceValidator`.
  - Note: the validators must validate only the numeric field(s), not `HeroId` (which is bound from the route).
- **Domain guards** — `ArgumentOutOfRangeException.ThrowIfNegative(value)` (.NET 8+) at the top of `Hero.TakeDamage`, `Heal`, `GrantTempHp`, `SpendMana`, and `SpendHitDice` (guarding both `count` and `healingAmount`). The parameter name flows through automatically for a clear exception message.

### Behavior / error handling

- A negative amount, or a zero where the validator requires a positive, returns **400** with a FluentValidation message (e.g. `"'Amount' must be greater than '0'."`). The client's `readErrorMessage` already extracts FastEndpoints validation errors, so the play popovers and forms show a real message.
- The domain guards are a safety net; in normal HTTP operation the validator rejects bad input first, so the exception path is reached only by direct domain misuse (or a future non-HTTP caller).

## Testing (xUnit, tests-after)

- **Domain guard tests** (`NS.Tests`, matching the existing `HeroTests` style with the `TestHero` factory): each of `TakeDamage`, `Heal`, `GrantTempHp`, `SpendMana`, `SpendHitDice` throws `ArgumentOutOfRangeException` on a negative argument; a valid argument still mutates as before; `GrantTempHp(0)` and `SpendHitDice(1, 0)` do **not** throw.
- **Validator tests** (`NS.Tests/MutationValidationTests.cs`): instantiate each validator directly (e.g. `new TakeDamageValidator().Validate(new TakeDamageRequest(id, -1))`) and assert `IsValid` is false for negatives and zeros-where-required, and true for valid values. No HTTP/integration harness needed.

## Files

**Modify**
- `NS.FastEndpoints/Heroes/TakeDamageEndpoint.cs` — add `TakeDamageValidator`
- `NS.FastEndpoints/Heroes/HealEndpoint.cs` — add `HealValidator`
- `NS.FastEndpoints/Heroes/GrantTempHpEndpoint.cs` — add `GrantTempHpValidator`
- `NS.FastEndpoints/Heroes/SpendManaEndpoint.cs` — add `SpendManaValidator`
- `NS.FastEndpoints/Heroes/SpendHitDiceEndpoint.cs` — add `SpendHitDiceValidator`
- `NS.Domain/Heroes/Hero.cs` — guard clauses in the five methods

**Create**
- `NS.Tests/MutationValidationTests.cs` — validator unit tests
- Guard-clause tests added to the existing hero domain test file (`NS.Tests/HeroTests.cs`)

## Risks / open items

- **FastEndpoints validator discovery:** validators are auto-discovered from the endpoints assembly (already configured); co-locating each validator in its endpoint file requires no new registration.
- **`ThrowIfNegative` availability:** `ArgumentOutOfRangeException.ThrowIfNegative` exists in .NET 8+; this project targets .NET 10, so it is available.
- **C# conventions:** `sealed` validators, XML docs on the public validator types and their constructors, member ordering, and the `_GlobalUsings.cs` convention all apply (FluentValidation is already a global using in NS.FastEndpoints).
