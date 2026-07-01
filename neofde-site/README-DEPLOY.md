# neofde.com — Deploy Guide

A self-contained 3-page pitch site for the StatusNeo FDE charter. No build step, no framework, no dependency on the local `.NET` portal. Just static HTML you can host anywhere.

```
neofde-site/
  index.html   → landing / pitch (Founder + CTO facing)
  suite.html   → the complete Suite Index (scope proof; "full bodies on onboarding")
  deck.html    → the 9-slide board deck as a web page
```

Every page carries the confidential footer + attribution; `suite.html` and `deck.html` also carry a subtle diagonal watermark. All pages are set to `noindex,nofollow` so search engines won't list them — but **anyone with the URL can open them.** That's intentional: only the *scope* (section names) is here; the full document bodies are held until you're onboarded.

---

## 1. Pick the domain
- **neofde.com** — reads as a *practice / brand* ("Neo FDE"). Slightly stronger for pitching to build StatusNeo's practice; not self-promotional.
- **jeethfde.com** — clearly *personal portfolio*. Also fine.

Register at any registrar (Namecheap, Cloudflare Registrar, Google Domains/Squarespace, GoDaddy). ~$10–12/yr. *(You do this yourself — I can't register or pay for a domain.)*

## 2. Host it (free — pick one)

### Option A — Cloudflare Pages (recommended: free, fast, easy custom domain)
1. Push this `neofde-site/` folder to a GitHub repo (or use the "Direct Upload" option).
2. Cloudflare dashboard → **Workers & Pages** → **Create** → **Pages** → connect the repo (or drag-drop the folder).
3. Build settings: **no build command**, output directory = `/` (root of what you upload).
4. Deploy → you get a `*.pages.dev` URL immediately.
5. **Custom domain:** Pages project → **Custom domains** → add `neofde.com` and `www.neofde.com`. If the domain is on Cloudflare, DNS is auto-configured; otherwise follow the CNAME it shows you.

### Option B — Netlify (drag-and-drop, no Git needed)
1. app.netlify.com → **Add new site** → **Deploy manually** → drag the `neofde-site` folder onto the page.
2. Live on a `*.netlify.app` URL instantly.
3. **Domain settings** → **Add a custom domain** → `neofde.com` → follow the DNS records Netlify gives you (an `A`/`ALIAS` record at the apex + a `CNAME` for `www`).

### Option C — GitHub Pages
1. Create a repo, put these files at the repo root, push.
2. Repo → **Settings** → **Pages** → Source = `main` branch, `/root`.
3. Add a file named `CNAME` (no extension) containing just: `neofde.com`
4. At your registrar, point DNS: `www` → `CNAME` to `<username>.github.io`, and the apex `@` → the four GitHub Pages `A` records (185.199.108–111.153).

All three give **free HTTPS** on the custom domain automatically.

## 3. Share it
Send the Managing Partner / Founders / CTOs the single link:
`https://neofde.com` (landing) — they can navigate to the Suite Index and Deck from the top nav.
Direct links if useful: `https://neofde.com/suite.html` · `https://neofde.com/deck.html`

---

## Options you can ask me to add
- **A light password gate** (same pattern as the portal's `gate.js`) if you want the URL to require a key before viewing. *Note: it's a soft deterrent, not real security — but since only scope is on the site, it's optional.*
- **A short contact form** (via Formspree/Netlify Forms) instead of just mailto/tel.
- **A one-page "Coverage vs. the JD" page** (from `JD-to-Jeeth-Coverage-Map.html`) added to the site nav.
- **Analytics** (Cloudflare Web Analytics — privacy-friendly, no cookies) so you can see when leadership opens it.

## Keep it honest
Every page states the framing: a **proposal + validated demonstration** (Modutecture = proof of the motion, not a closed-and-scaled account), and **full document bodies shared on hiring and onboarding into StatusNeo.** Don't remove that line — it's what makes the confidence credible.
