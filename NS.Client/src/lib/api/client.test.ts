import { afterEach, describe, expect, it, vi } from 'vitest';
// `$app/navigation` (goto) resolves to src/test/app-stub.ts via the Vitest alias (Step 0).
import { ApiError, getHeroes, login } from './client';
import { clearSession } from '$lib/auth/session';

afterEach(() => {
	vi.restoreAllMocks();
	clearSession();
});

function mockFetch(status: number, body: unknown) {
	vi.stubGlobal('fetch', vi.fn(() =>
		Promise.resolve(new Response(body === undefined ? null : JSON.stringify(body), { status }))
	));
}

describe('apiFetch via login', () => {
	it('returns parsed JSON on 200', async () => {
		mockFetch(200, { token: 't', userId: 'u' });
		await expect(login('Caldra')).resolves.toEqual({ token: 't', userId: 'u' });
	});

	it('throws ApiError with the validation message on 400', async () => {
		mockFetch(400, { errors: { name: ['Name is required'] } });
		await expect(login('')).rejects.toMatchObject({ status: 400, message: 'Name is required' });
	});
});

describe('getHeroes error mapping', () => {
	it('throws ApiError on 500', async () => {
		mockFetch(500, { message: 'boom' });
		await expect(getHeroes()).rejects.toBeInstanceOf(ApiError);
	});
});
