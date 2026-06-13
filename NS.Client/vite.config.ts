import { sveltekit } from '@sveltejs/kit/vite';
import tailwindcss from '@tailwindcss/vite';
import { defineConfig } from 'vite';

// API routes are unprefixed (/heroes, /users, /reference). In dev, Vite serves the
// SPA on its own port, so proxy the API routes to the NS.WebApp HTTP endpoint.
// Plain HTTP (5197) avoids the self-signed HTTPS cert. Production is same-origin
// (NS.WebApp serves the built SPA) and never hits this proxy.
const API_TARGET = 'http://localhost:5197';

export default defineConfig({
	plugins: [tailwindcss(), sveltekit()],
	server: {
		proxy: {
			'/heroes': API_TARGET,
			'/users': API_TARGET,
			'/reference': API_TARGET
		}
	}
});
