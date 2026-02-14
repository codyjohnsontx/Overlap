# Local Development Troubleshooting

## Why these errors happen

### 1) `:4173/favicon.ico 404 (File not found)`
Your dev server is trying to load `/favicon.ico`, but no favicon file exists at your app root/public path. This is harmless for app logic.

### 2) `net::ERR_NAME_NOT_RESOLVED` for `floridamanapi.com`
DNS cannot resolve `floridamanapi.com` from your machine. In practice this usually means:
- the domain is no longer active,
- it is temporarily down, or
- the hostname is incorrect.

### 3) CORS blocked for `https://floridamanapi.herokuapp.com/...`
The browser is enforcing cross-origin rules because your app runs on `http://localhost:4173` but the API response does not include an `Access-Control-Allow-Origin` header allowing your localhost origin.

### 4) `net::ERR_FAILED 404` on Heroku endpoints
The URL shape is likely wrong for that service (for example `/11/15` and `/?date=11/15` returning 404), or that deployment no longer serves those routes.

## Practical fix strategy

1. **Use a single API base URL that is currently alive** and verify the exact route format with `curl` before wiring it into the frontend.
2. **Proxy API requests through your local dev server** so browser requests stay same-origin.
   - For Vite, configure `server.proxy` and call relative paths from the browser.
3. **Add a fallback message in UI** for date lookups that fail.
4. **Add `public/favicon.ico`** (or remove favicon link) to silence the favicon 404.

## Vite proxy example

```js
// vite.config.js
import { defineConfig } from 'vite';

export default defineConfig({
  server: {
    proxy: {
      '/api': {
        target: 'https://<working-api-host>',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/api/, ''),
      },
    },
  },
});
```

Then in frontend code:

```js
const res = await fetch(`/api/v1/date/${monthDay}`);
```

This avoids browser-side CORS issues during development because the browser only talks to `localhost`.
