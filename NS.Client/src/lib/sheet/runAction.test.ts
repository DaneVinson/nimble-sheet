import { describe, expect, it, vi } from 'vitest';
import { runAction } from './runAction';
import { ApiError } from '$lib/api/client';

describe('runAction', () => {
	it('toggles busy true→false and refreshes on success', async () => {
		const busy: boolean[] = [];
		const refresh = vi.fn(() => Promise.resolve());
		await runAction(() => Promise.resolve(), refresh, (b) => busy.push(b), () => {});
		expect(busy).toEqual([true, false]);
		expect(refresh).toHaveBeenCalledOnce();
	});

	it('surfaces an ApiError message and skips refresh on failure', async () => {
		let error: string | null = 'stale';
		const refresh = vi.fn(() => Promise.resolve());
		await runAction(
			() => Promise.reject(new ApiError(400, 'Not enough mana')),
			refresh,
			() => {},
			(e) => (error = e)
		);
		expect(error).toBe('Not enough mana');
		expect(refresh).not.toHaveBeenCalled();
	});

	it('uses a generic message for a non-ApiError failure', async () => {
		let error: string | null = null;
		await runAction(() => Promise.reject(new Error('boom')), () => Promise.resolve(), () => {}, (e) => (error = e));
		expect(error).toBe('Action failed.');
	});
});
