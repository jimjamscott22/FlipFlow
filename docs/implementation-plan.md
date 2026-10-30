# FlipFlow Implementation Plan (Personal Edition)

Companion to [`product-spec.md`](./product-spec.md). Milestones are named **M0 to M9** so they are not confused with the original README phases 1 to 7.

## Starting Point (verified 2026-10-28)

- Frontend builds cleanly (`npm run build`).
- Backend **does not compile**. Known errors:
  1. `FlipFlow.Application/DependencyInjection.cs` uses `IServiceCollection` but the project lacks `Microsoft.Extensions.DependencyInjection.Abstractions` (adding version 8.0.1 fixes it).
  2. `FlipFlow.Api/Middleware/GlobalExceptionMiddleware.cs` line 25: the `switch` expression returns different anonymous types per arm, so the tuple cannot be inferred. Fix by returning a common type (for example `(int StatusCode, object Payload)`).
  3. Further errors may appear once these are fixed; the backend has never been built end to end.
- No EF Core migrations exist, no test projects exist, and there is no solution (`.sln`) file.
- `src/frontend/tsconfig.app.tsbuildinfo` and `tsconfig.node.tsbuildinfo` are tracked in git and change on every build; they should be untracked and ignored.
- The Listings page is a placeholder; the content generator and pricing service are simple mocks; Hangfire is wired but has no jobs.

## Guiding Rules

- Every milestone ends with something the owner can use or see working, and a green build (`dotnet build`, `dotnet test`, `npm run build`).
- One milestone per branch and pull request, small commits, tests alongside behaviour changes.
- Prefer deleting code to adapting code that no longer serves the personal-tool goal.
- Keep interfaces at the seams that are likely to change (AI generator, pricing, marketplace formatter), and nowhere else.

## Milestone Overview

| # | Milestone | Outcome | Rough size |
| --- | --- | --- | --- |
| M0 | Make it build and run | App starts, one item can be added end to end | Small |
| M1 | Strip to personal tool | SQLite, no auth, single process, optional passcode | Medium |
| M2 | Mobile intake | Add an item with phone photos comfortably | Medium |
| M3 | Listing drafts and settings | Draft entity flow, settings page, template fallback | Small |
| M4 | LM Studio listing writer | Photos + notes produce a reviewable draft | Medium-large |
| M5 | Pricing and comps | AI range, eBay sold link, saved comps, suggestion | Medium |
| M6 | Marketplace copy and listings | eBay and Facebook copy-ready output, listing records | Medium |
| M7 | Sold flow and delisting | Mark sold, flag other listings, undo | Medium |
| M8 | Reports and dashboard | Monthly sales, sell-through, stale items, CSV | Medium |
| M9 | Hardening, docs, optional extras | Backups, run-as-service, README refresh, eBay API spike | Small-medium |

M0 to M2 make the app a usable inventory tool. M3 to M6 deliver the "photos in, listings out" value. M7 and M8 close the loop. Each can be paused after without leaving the app half broken.

---

## M0: Make It Build and Run

Goal: a healthy baseline before changing any design.

Tasks
- Add a `FlipFlow.sln` at `src/backend/` including all four projects.
- Fix the two known compile errors above and any that follow.
- Remove `*.tsbuildinfo` from git and add to `.gitignore`; also ignore `dist/`, `storage/`, `data/`.
- Generate the first EF Core migration against the **current** Postgres model only if needed to prove the model is valid; otherwise skip because M1 replaces the provider (see note).
- Run the app and the frontend together and walk through register, add item, upload photo, to confirm the existing flows work.
- Record any runtime bugs found in a short list in the PR description.

Note: because M1 moves to SQLite, avoid creating a Postgres migration that is immediately thrown away. If Postgres is not available in the environment, M0 only needs a successful `dotnet build` plus a smoke run against a throwaway database.

Done when: `dotnet build` and `npm run build` both succeed from a clean clone; a smoke test of the existing flows is documented.

## M1: Strip to Personal Tool

Goal: remove multi-user machinery and move to SQLite.

Tasks
- Domain: delete `PlatformAccount`, `RepricingRecommendation`; remove `OwnerUserId` from `Item`.
- Infrastructure: remove Identity (`AppUser`, `AuthService`, `JwtTokenService`, `JwtOptions`, `CurrentUserService`), Hangfire, and Npgsql packages. Add `Microsoft.EntityFrameworkCore.Sqlite`.
- Decide and implement the money storage approach (decimal converter or integer cents) once, in the DbContext configuration.
- Replace `ApplicationDbInitializer` seeding with a small optional demo-data seeder (flag-controlled) and apply migrations on startup.
- API: delete `AuthController`; remove `[Authorize]` and ownership filters from `ItemsController` and `DashboardController`; simplify `Program.cs` and `ApiServiceCollectionExtensions`.
- Optional passcode: `AccessPasscodeMiddleware` reading `Access:Passcode` from configuration; if set, requires a cookie obtained from a tiny `POST /api/access` endpoint; if unset, no-op. Frontend gets one small passcode screen shown only when the API reports it is required.
- Serve the built frontend from `wwwroot` and add SPA fallback so one process serves everything. Add a root script (`run.sh` / `run.ps1`) that builds the frontend and starts the API.
- Frontend: remove `auth` feature, `ProtectedRoute`, JWT handling in `api-client.ts`; simplify the app shell (no user menu).
- Delete `docker-compose.yml` and the Postgres sections of the README.
- Create the first real SQLite migration (`InitialCreate`) and commit it.
- Add the test project (`FlipFlow.Tests`, xUnit) with an in-memory SQLite fixture and a first test: item create and read round trip.

Done when: fresh clone, run one script, open `http://localhost:5000`, add an item with a photo, restart, data is still there.

## M2: Mobile Intake

Goal: adding an item from a phone is quick and pleasant.

Tasks
- Responsive pass over app shell, items list, item form and item detail at 360 to 430 px widths; larger tap targets; bottom "Add item" action.
- Photo upload: multi-select, camera/gallery picker, per-file progress, retry on failure, delete, reorder, set cover. Add `SortOrder` and `IsCover` to `ItemPhoto` (migration).
- Server-side image processing (ImageSharp or similar): auto-orient from EXIF, strip location metadata, store a max ~2048 px version and a ~1024 px "model copy"; raise upload limits to realistic phone sizes while capping at a sane maximum.
- Validate content by magic bytes, not only declared content type.
- Network setup docs: how to bind to the LAN (`ASPNETCORE_URLS=http://0.0.0.0:5000`), find the machine's address, and open it on the phone; firewall note for Windows.
- Make the form tolerant of fast entry: only title and condition are required for a draft; everything else optional.

Tests: image downscale keeps orientation and size limits; upload rejects non-images with image-like extensions.

Done when: the owner adds a real item with 4 phone photos over Wi-Fi and the photos display correctly oriented.

## M3: Listing Drafts and Settings

Goal: the plumbing for AI output, still with the template generator.

Tasks
- Extend `ListingDraft` (category, brand, model, missing-info) and `Item` (condition notes, AI estimate fields, purchase cost) with a migration.
- API: `POST /api/items/{id}/draft/generate` (calls `IListingContentGenerator`, saves a draft), `GET`/`PUT` draft, `POST .../draft/apply` (copies title, description, condition notes etc. onto the item).
- Rename `MockListingContentGenerator` to `TemplateListingContentGenerator` and keep it as the fallback.
- `AppSettings` single-row entity, `ISettingsService`, `GET`/`PUT /api/settings`, and the settings page (LM Studio fields present but the test button is wired in M4).
- Frontend: "Draft" tab on item detail with editable fields, a "Generate" button, and an "Apply to item" action; clear indication that content is a draft.

Tests: draft generate/apply flow; apply never overwrites asking price.

Done when: Generate produces a template draft that can be edited and applied.

## M4: LM Studio Listing Writer

Goal: photos plus notes produce a good draft using the model on the other machine.

Tasks
- `LmStudioOptions` (base URL, model, timeout, max images, temperature) bound from configuration and overridable by `AppSettings`.
- `LmStudioListingContentGenerator : IListingContentGenerator` using `HttpClient` (typed client with Polly-style timeout and one retry on transient failures):
  - builds a chat-completions request with a system prompt, the owner's notes, and up to N model-copy images as base64 data URLs;
  - requests JSON output (use `response_format` with a JSON schema if the loaded model supports it; otherwise instruct JSON-only and extract the first JSON object);
  - parses and validates the response into `GeneratedListingContentDto`; one repair retry for malformed JSON;
  - maps errors to typed results (unreachable, timeout, no vision support, bad output) with owner-friendly messages.
- Prompt design lives in one versioned file (`Prompts/listing-writer.md` embedded resource) so it can be tuned without code changes; include rules: never invent model numbers, put uncertainty in `missingInfo`, title at most 80 characters, plain language, no marketing exaggeration.
- Fallback: on failure, return the template draft plus a warning banner; never lose the owner's notes.
- Async UX: `POST` returns a job id immediately; the frontend polls `GET /api/jobs/{id}` (in-memory job tracker, no Hangfire) and shows progress states: "Sending photos", "Waiting for model", "Done".
- Settings page "Test connection" calls `GET /v1/models` and reports reachable, model present, and approximate latency.
- Early spike (first task in this milestone): hit the owner's real LM Studio instance with 2 to 3 sample items to choose a model and tune the prompt before building the UI polish. Record findings in `docs/lm-studio-notes.md` (model name, image limits, time per request, typical failure cases).
- LM Studio setup docs: enable "Serve on Local Network", load a vision-capable model, note the port, firewall rules.

Tests: response parser against a library of sample outputs (valid, fenced in markdown, truncated, wrong types); request builder produces the expected payload; fake `HttpMessageHandler` covers timeout and unreachable cases.

Done when: from the phone, an item with photos and a note yields a sensible draft in about a minute, and unplugging the model machine produces a clear message and a usable template draft.

## M5: Pricing and Comps

Goal: help the owner pick a price they trust.

Tasks
- Persist `AiEstimateLow/High` from the generator onto the item; show them labelled "rough AI estimate".
- `EbaySoldSearchLinkBuilder`: builds the sold/completed search URL from brand, model and title (strip noise words, URL-encode); unit-tested.
- `ItemComp` entity, migration, and API (`GET/POST/DELETE /api/items/{id}/comps`).
- Replace `RuleBasedPricingSuggestionService` with `CompsPricingSuggestionService`:
  - no comps: return the AI range midpoint, flagged as low confidence;
  - one or more comps: median, min, max, with an optional condition adjustment;
  - always returns a short human-readable reason.
- Frontend: Pricing panel on item detail with the AI range, "Check sold prices on eBay" button, comps list with quick-add (price, note, URL), the computed suggestion, and a "Use this price" button that sets the asking price (explicit click only).

Tests: median, even/odd comp counts, outlier handling decision documented in code, link builder encoding.

Done when: the owner can price three real items using the panel and agrees the suggestion is reasonable.

## M6: Marketplace Copy and Listing Records

Goal: ready-to-paste text for eBay and Facebook, and a record of where each item is live.

Tasks
- Replace `MarketplacePlatform` values with `Ebay`, `FacebookMarketplace`, `Other`; adjust `MarketplaceListingStatus` to `Draft`, `Active`, `Sold`, `Ended`, `NeedsDelisting`; migration.
- `IMarketplaceFormatter` with `EbayFormatter` and `FacebookMarketplaceFormatter`:
  - eBay: title limited to 80 characters (truncate at word boundary and warn), condition label mapping, plain and simple-HTML description, item specifics block;
  - Facebook: short title, plain text description, condition mapping, pickup note from settings.
- `GET /api/items/{id}/export/{platform}` returns the formatted fields as JSON; frontend shows each field with its own Copy button and a character counter.
- `GET /api/items/{id}/photos.zip` for downloading the photo set.
- Listing CRUD: `POST /api/items/{id}/listings` (platform, price, URL, listed date); item status moves to `Listed` when the first listing becomes `Active`.
- Replace the placeholder Listings page with a table of all active listings (item, platform, price, age, URL) with filters.

Tests: formatter outputs for edge cases (long titles, missing brand or model, special characters); status transitions.

Done when: the owner can prepare, post (manually) and record an item on both marketplaces in a few minutes.

## M7: Sold Flow and Cross-Post Cleanup

Goal: never leave a sold item advertised somewhere else.

Tasks
- `ISalesService.MarkSold(itemId, soldPrice, soldAtUtc, platform, note)`:
  - sets item `Sold`, stores sold fields;
  - marks the matching listing `Sold`;
  - sets every other `Active` listing to `NeedsDelisting`;
  - runs in one transaction.
- `ISalesService.Undo(itemId)` restoring `Listed` and the previous listing statuses (store a minimal snapshot or recompute from timestamps).
- `POST /api/items/{id}/sold`, `POST /api/items/{id}/sold/undo`, `POST /api/listings/{id}/delisted`.
- Frontend: "Mark sold" sheet (sold price, date, platform) reachable from item detail and the listings table; "Needs delisting" panel on the dashboard and item page with link-out and "Done" buttons.

Tests: all combinations of one, two, zero other active listings; undo; idempotency (marking sold twice).

Done when: selling an item listed on both platforms results in exactly one "Delist me" task that disappears once confirmed.

## M8: Reports and Dashboard

Goal: see how selling is going.

Tasks
- `IReportService` with queries computed in SQL where practical: sold per month, revenue per month, sell-through rate, average days to sell, by platform, by category, stale active listings (oldest first), optional gross profit where cost is present.
- Replace `DashboardService` outputs with: counts by status, needs-delisting list, oldest listings, this month's sales.
- Reports page with a simple chart per metric (a lightweight chart library or plain SVG) and a table; date range selector.
- `GET /api/reports/sold.csv` export.

Tests: report calculations over a seeded in-memory dataset with hand-checked expected values.

Done when: the numbers on the reports page match a manual tally of the owner's real sales.

## M9: Hardening, Docs, Optional Extras

Tasks
- README rewrite for the personal edition: purpose, quick start, phone access, LM Studio setup, backup and restore, troubleshooting. Remove stale phase narrative and mark the old roadmap as historical.
- Backup script (copy `data/flipflow.db` and the uploads folder to a dated archive).
- Run-as-service instructions (systemd on Linux or a scheduled task/NSSM on Windows) so the app starts at boot.
- GitHub Actions CI: `dotnet build`, `dotnet test`, `npm ci`, `npm run build`.
- Error handling review: friendly messages in the UI for every network failure; log files with rotation.
- Accessibility and polish pass (focus states, contrast, empty states).
- Optional spike (time-boxed): eBay Sell API (OAuth, inventory item, offer, publish) behind a new `IMarketplacePublisher` interface, to decide whether real posting is worth building. Facebook stays manual.

Done when: someone else could clone the repo, follow the README, and have it running with a working backup.

---

## Cross-Cutting Concerns

**Testing strategy**
- `FlipFlow.Tests` (xUnit): pure logic (pricing, formatters, parsers, sales rules, reports) as plain unit tests; service and API tests on in-memory SQLite.
- LM Studio is never called in automated tests; a fake handler stands in.
- Frontend: TypeScript strictness and build in CI; add Vitest for formatting helpers only if they grow logic.

**Migrations**
- One migration per milestone that changes the schema, with descriptive names. Development databases are disposable; the owner's real database is backed up before each upgrade (documented in M9).

**Configuration**
- `appsettings.json` holds defaults only. Owner-specific values (LM Studio address, passcode) go in `appsettings.Local.json` (git-ignored) or environment variables, with the in-app settings page overriding LM Studio fields.

**Risks and mitigations**

| Risk | Mitigation |
| --- | --- |
| Local vision model gives weak or invented details | Draft-only output, `missingInfo` list, prompt rules against inventing model numbers, early model spike in M4 |
| Model machine often offline | Template fallback, clear status messages, manual entry always works |
| SQLite and `decimal` quirks | Central money converter, tested in M1 |
| Phone photo sizes and orientation | Server-side downscale and EXIF handling in M2 |
| Unauthenticated app on the LAN | Bind to localhost by default, optional passcode, documented risk |
| eBay changes sold-search URL parameters | Link builder isolated and tested; failure is a broken link, not data loss |
| Scope creep back toward SaaS | Out-of-scope list in the spec; revisit only after M8 |

**Suggested order if time is short:** M0, M1, M4 (with M3 as needed), M6. That sequence alone gives phone photos in, copy-ready listings out. M2 is strongly recommended before M4 because the photo pipeline feeds the model.

## Definition of Done (every milestone)

- `dotnet build`, `dotnet test`, and `npm run build` pass.
- New behaviour has tests, or a stated reason why not.
- Migration included and applies cleanly to a fresh database.
- README or docs updated where behaviour or setup changed.
- Manually exercised on a phone-sized viewport.
