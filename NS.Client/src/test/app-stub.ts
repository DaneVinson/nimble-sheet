// Minimal stand-ins for SvelteKit's $app/* virtual modules under Vitest.
import { writable } from 'svelte/store';

export const goto = async (_url?: string): Promise<void> => {};
export const navigating = writable(null);
export const page = writable({ status: 200, error: null });
