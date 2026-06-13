import { defineConfig } from 'vitest/config';
import { fileURLToPath } from 'node:url';

export default defineConfig({
	test: {
		include: ['src/**/*.test.ts'],
		environment: 'node'
	},
	resolve: {
		alias: [
			{ find: '$lib', replacement: fileURLToPath(new URL('./src/lib', import.meta.url)) },
			{
				find: /^\$app\/(navigation|stores|state)$/,
				replacement: fileURLToPath(new URL('./src/test/app-stub.ts', import.meta.url))
			}
		]
	}
});
