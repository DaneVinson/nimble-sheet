import adapter from '@sveltejs/adapter-static';

/** @type {import('@sveltejs/kit').Config} */
const config = {
	compilerOptions: {
		// Force runes mode for the project, except for libraries. Can be removed in svelte 6.
		runes: ({ filename }) => (filename.split(/[/\\]/).includes('node_modules') ? undefined : true)
	},
	kit: {
		// Pure SPA: build to static assets with a fallback page so client-side routing
		// handles all routes. The app talks to the NS.WebApp API over HTTP.
		adapter: adapter({
			fallback: 'index.html'
		})
	}
};

export default config;
