# Live API Integration — Design Spec

**Date:** 2026-06-13
**Component:** `NS.Client` (SvelteKit SPA)
**Status:** Approved for planning

## Goal

Replace the static fixture that drives the character sheet with **live data from the API**. Add the minimum auth and navigation scaffolding required to get there: a login/create-user flow, a hero list, and a hero detail page at `/heroes/[id]` that fetches a real `Hero` plus the reference data it needs, then renders the **existing** resolver + `HeroSheet` components unchanged. This is a **front-end-only slice — no backend changes** (server-side reference caching is explicitly deferred).

## Scope

**In scope**
- `/login` page: log in by name **and** create a new user (name + email), with auto-login on create.
- localStorage-backed session store (JWT) that survives refresh and browser restarts.
- A shared API client with bearer-token auth and central 401 handling.
- A guarded `(app)` route group with a layout auth guard.
- Hero list page (`/heroes`) scoped to the authenticated user.
- Hero detail page (`/heroes/[id]`) that fetches the hero, lazily fetches + caches only the reference collections the hero references, resolves, and renders the existing sheet.
- Dev-time Vite proxy so `npm run dev` runs against the live API.
- Unit tests (written after implementation) for the reference-assembly logic and API-error mapping.

**Out of scope (deferred to later slices)**
- Server-side caching of reference data (backend stays as-is).
- Create/edit hero (build) UI — a full `HeroBuildRequest` form is its own slice.
- The HP damage/heal popover and all live-play mutation wiring.
- Passwords / real auth hardening (login remains name-only per the POC).
- Backend reference/user seeding as a product feature (see Verification Dependency).

## Decisions (from brainstorming)

| Decision | Choice |
|---|---|
| Auth scope | **Login + create user** (name-only login; create = name + email → auto-login) |
| Token storage | **localStorage** — persists across refresh and restarts; hydrate store on load |
| Reference fetch strategy | **Lazy, per collection**, cached client-side for the session |
| Server-side reference caching | **Deferred** — client cache only this slice |
| Data-loading architecture | **Approach A** — SvelteKit `load` functions (client-side, `ssr=false`); fetch out of components |
| `/sheet` fixture route | **Deleted** — superseded by `/heroes/[id]`; fixture retained for tests |
| Create-hero UI | **Deferred** — not in this slice |
| Root route `/` | Redirect to `/heroes` (guard bounces to `/login` if unauthenticated) |
| Tests | Vitest unit tests for reference-assembly + API-error mapping, written after implementation |

## Architecture

The pipeline for an authenticated sheet view:

```
+layout.ts guard (token?) ──no──▶ redirect /login
        │ yes
        ▼
/heroes/[id]/+page.ts  load:
    getHero(id) ──▶ assembleReferenceData(hero) ──▶ resolveSheet(hero, refs)
        │                     │ (lazy, cached)            │
        ▼                     ▼                           ▼
   ApiError 404         GET /reference/{used}        SheetViewModel
   → +error.svelte      (cache hits skipped)         → <HeroSheet/>
```

The resolver (`$lib/sheet/resolve.ts`) and all sheet components are **unchanged** — they already accept `(Hero, ReferenceData)`. Only the source of those two inputs changes (fixture → fetch).

### Modules

**`$lib/auth/session.ts` — session store**
- Svelte writable of `{ token: string; userId: string } | null`.
- Hydrates from `localStorage` on module init; `setSession(s)` persists, `clearSession()` removes.
- A `localStorage` key (e.g. `ns.session`) holds the JSON blob.

**`$lib/api/client.ts` — API client**
- `apiFetch(path, init?)`: relative same-origin path; attaches `Authorization: Bearer <token>` from the session store when present; sets `Content-Type: application/json` for bodied requests; parses JSON.
- Non-2xx → throws `ApiError(status, message)`.
- **401 → `clearSession()` + redirect to `/login`** (central), then rethrow/handled so guarded loads bail out.
- Typed wrappers:
  - `login(name): Promise<{ token; userId }>` → `POST /users/login`
  - `createUser(name, email): Promise<{ id }>` → `POST /users`
  - `getHeroes(): Promise<Hero[]>` → `GET /heroes`
  - `getHero(id): Promise<Hero>` → `GET /heroes/{id}`
  - `getReferenceCollection<T>(resource): Promise<T[]>` → `GET /reference/{resource}`

**`$lib/reference/cache.ts` — reference cache + assembly**
- Module-level `Map<resource, Promise<unknown[]>>`; `getCollection(resource)` returns the cached promise or starts a fetch and caches it (dedupes concurrent calls).
- `assembleReferenceData(hero): Promise<ReferenceData>`:
  - Determines needed collections from the hero: **ancestries always**; `backgrounds` if `backgroundId`; `armor` if `armor.length`; `weapons` if `weapons.length`; `conditions` if `activeConditions.length`; `features` if `features.length`; `magicItems` if `magicItems.length`; `spells` if `knownSpells.length`.
  - Fetches missing collections in parallel (`Promise.all`), reuses cached ones, fills unused collections with `[]`.
  - Returns a complete `ReferenceData` bundle for `resolveSheet`.

### Routes

| Route | Auth | `+page.ts` load | `+page.svelte` |
|---|---|---|---|
| `/login` | anonymous | — | login / create-account toggle |
| `/` | (redirect) | redirect → `/heroes` | — |
| `(app)/heroes` | guarded | `getHeroes()` | hero cards → link to detail |
| `(app)/heroes/[id]` | guarded | `getHero` → `assembleReferenceData` → `resolveSheet` | `<HeroSheet {vm}/>` |

- `(app)/+layout.ts`: reads the session store; if no token, `redirect(302, '/login')`. All guarded pages live under this group.
- `(app)/+layout.svelte`: thin dark top bar — app name, current user name, **Logout** (`clearSession` → `/login`).
- `/sheet` route is removed.

### Login page (`/login`)

- Two modes via a toggle: **Log in** (name field) and **Create account** (name + email).
- Log in: `login(name)` → `setSession` → redirect `/heroes`. `401` → inline "name not found".
- Create: `createUser(name, email)` → `login(name)` → `setSession` → `/heroes`. Validation / name-taken errors shown inline.

### Dev proxy (`vite.config.ts`)

- `server.proxy` maps `/heroes`, `/users`, `/reference` → `http://localhost:5197` (plain HTTP, avoids the self-signed HTTPS cert), so `npm run dev` exercises the live API with HMR. Production (served same-origin by `NS.WebApp`) is unaffected.

### Loading / error UX

- Navigation loading indicator driven by SvelteKit's `$navigating` store.
- `+error.svelte` boundary renders friendly messages for thrown `ApiError`s (notably 404 on an unknown/unowned hero).
- 401 is handled centrally in the API client (clear session + redirect), so individual loads don't each special-case it.

## Testing

Written **after** implementation (per project preference):
- `assembleReferenceData`: selects exactly the collections a hero references; empty/absent collections are not fetched; unused collections come back `[]`.
- `ApiError` mapping: non-2xx responses produce the right status/message; 2xx parse correctly.
- Existing resolver tests (`resolve.test.ts`) remain green and unchanged.

## Verification Dependency

The SoloDB database starts **empty** (no `.cs` seeder, no reference rows, no users). To verify the live flow end-to-end we need a user, some reference data, and at least one hero in the DB. The exact mechanism (a small dev-seed script, a throwaway set of API calls, or a minimal seeded fixture loaded at startup in Development) will be decided in the implementation plan. This is a test-harness concern, not a product feature in this slice.

## Non-Goals / Constraints

- No backend code changes in this slice.
- The resolver and all `HeroSheet` components must remain unchanged — if the live data forces a resolver change, that is a signal the fixture diverged from the API and should be reconciled, not worked around in components.
- Follow SvelteKit/TypeScript idioms (the C# conventions do not apply to `NS.Client`).
