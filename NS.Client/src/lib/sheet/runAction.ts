import { ApiError } from '$lib/api/client';

/**
 * Run a hero mutation: flag busy, clear the previous error, perform the action, then refresh.
 * On failure the error message is surfaced and the refresh is skipped. Kept free of runes so it
 * is unit-testable; the reactive bindings live in heroActions.svelte.ts.
 */
export async function runAction(
	action: () => Promise<void>,
	refresh: () => Promise<void>,
	setBusy: (busy: boolean) => void,
	setError: (error: string | null) => void
): Promise<void> {
	setBusy(true);
	setError(null);
	try {
		await action();
		await refresh();
	} catch (e) {
		setError(e instanceof ApiError ? e.message : 'Action failed.');
	} finally {
		setBusy(false);
	}
}
