# FlipFlow Product Spec (Personal Edition)

Status: Draft v1, agreed through a brainstorming Q&A on 2026-10-30.
Companion document: [`implementation-plan.md`](./implementation-plan.md).
This spec supersedes the original multi-user SaaS framing in the README and the 7-phase roadmap there. Phases 1 and 2 of that roadmap (scaffold, auth, item CRUD, photo upload) are the starting point, not the destination.

## 1. Purpose

FlipFlow is a personal tool for turning things lying around the house into sold items with as little typing as possible. The owner takes photos on their phone, adds a few words of notes, and gets a ready-to-post listing for each marketplace. After that, the app tracks what is listed where, what sold, and what still needs to be taken down.

It is also a learning and portfolio project, so the code should stay clean and layered, but **usefulness to one real seller is the deciding factor** whenever there is a trade-off.

## 2. Decisions Made

| Topic | Decision |
| --- | --- |
| Audience | One person (the owner). No accounts, no sign-up. |
| Hosting | Runs on the owner's own machine (laptop or small home server). Opened from a phone on the home network. Not exposed to the internet. |
| Stack | Keep .NET 8 (ASP.NET Core) + React/TypeScript/Vite/Tailwind. Swap PostgreSQL for SQLite. |
| Item types | Mixed household goods plus electronics. Categories are open-ended. |
| Marketplaces | eBay and Facebook Marketplace. |
| Posting | Copy-ready text per marketplace first. Real eBay API posting is a later, optional milestone. Facebook Marketplace has no public posting API, so it stays copy-and-paste permanently. |
| AI listing writer | Local vision-capable model served by LM Studio on a **different machine on the home network**. Photos plus notes in, structured listing out. |
| Pricing | AI price estimate plus a one-tap link to eBay sold listings. Optionally save comparable sales; the app suggests a price from them. |
| After listing | Cross-post cleanup (delist elsewhere when something sells) and basic reports. |
| Sales data | Sold price, sold date, sold platform. Optional purchase cost. No fee or shipping tracking in v1. |
| Not in v1 | Price-drop reminders, repricing engine, background jobs, full profit accounting (fees and shipping), multi-user, cloud hosting. |

## 3. Users and Context

- One seller, technically comfortable, running everything themselves.
- Main capture device: a phone camera. Main review and editing device: phone or laptop.
- The app host and the LM Studio host are separate machines on the same network, so either can be offline independently. The app must degrade gracefully when the model machine is off.

## 4. Core Workflow

1. **Capture.** On the phone, tap "Add item", take or choose photos, type a short note ("Sony WH-1000XM4, works, small scuff on left cup, comes with case").
2. **Generate.** The app sends the photos and note to the LM Studio model. It returns a draft title, description, category, brand/model guess, condition, condition notes and a price estimate. The draft opens in an editable form. Nothing is saved as final until the owner reviews it.
3. **Price.** The owner sees the AI estimate, taps "Check sold prices on eBay" to open a pre-filled search, and can save 2 to 3 comparable sales. A suggested price based on those comps replaces the AI guess once comps exist.
4. **Prepare listings.** For each target marketplace (eBay, Facebook), the app produces formatted, copy-ready text and the photo set, with per-field copy buttons.
5. **Mark as listed.** The owner posts manually, then records the listing (platform, price, optional URL) so the app knows where the item is live.
6. **Sell.** When it sells, the owner records sold price, date and platform. Every other active listing for that item is flagged "Delist me" with a link and a checkbox.
7. **Review.** A reports page shows what sold, for how much, how fast, and what is still sitting.

## 5. Functional Requirements

### 5.1 Inventory (exists, to be adapted)

- Create, view, edit, delete items with: title, brand, model, category (free text with suggestions), condition, description, condition notes, asking price, optional purchase cost, status.
- Item statuses: `Draft`, `ReadyToList`, `Listed`, `Sold`, `Archived` (unchanged).
- List view with search and status filter (exists). Add category filter and sort by age.
- Photos: multiple per item, reorderable, one marked as cover, deletable.
- Remove `OwnerUserId` ownership checks everywhere.

### 5.2 Mobile intake

- All screens usable at phone width; add-item flow is thumb-friendly.
- Photo input uses the native camera/gallery picker (`<input type="file" accept="image/*" multiple>` with optional `capture`), so no HTTPS requirement.
- Large phone photos (often 5 to 12 MB) are downscaled on the server on upload (target: long edge about 2048 px for storage, about 1024 px copies for the model). Upload limits must be raised accordingly from the current Phase 2 rules.
- Upload progress and clear failure messages.

### 5.3 AI listing writer (LM Studio)

- Integration through LM Studio's OpenAI-compatible endpoint (`POST {BaseUrl}/v1/chat/completions`) with images sent as base64 data URLs in the message content.
- Configurable: base URL (for example `http://192.168.1.50:1234`), model name, request timeout, max images per request, temperature.
- The prompt asks for **JSON only**, matching a fixed schema (below). Responses are validated; invalid JSON triggers one retry with a repair prompt, then a clear error.
- Output fields: `title`, `description`, `category`, `brand`, `model`, `condition` (enum value), `conditionNotes`, `estimatedPriceLow`, `estimatedPriceHigh`, `confidence` notes or caveats, `missingInfo` (things the owner should check, such as "model number not visible").
- The result is always a **draft**: stored with `IsAiGenerated = true`, shown for review, never auto-published or auto-applied to the asking price.
- Failure modes handled explicitly: LM Studio unreachable, model not loaded, timeout, non-vision model (no image support), malformed output. On failure the app falls back to the existing template generator (`MockListingContentGenerator`, to be renamed `TemplateListingContentGenerator`) and tells the owner why.
- A "Test connection" check on a settings page calls `GET {BaseUrl}/v1/models` and reports whether the configured model is available.
- Privacy: photos never leave the home network.

Suggested response schema:

```json
{
  "title": "string, max 80 chars",
  "description": "string",
  "category": "string",
  "brand": "string",
  "model": "string",
  "condition": "New | LikeNew | Good | Fair | Poor | ForParts",
  "conditionNotes": "string",
  "estimatedPriceLow": 0,
  "estimatedPriceHigh": 0,
  "missingInfo": ["string"]
}
```

### 5.4 Pricing

- **AI estimate.** Low/high range from the listing writer. Displayed with a label that it is a rough estimate from a local model and can be wrong.
- **Sold-comps link.** A button builds an eBay search URL for the item (brand + model + key words) with the sold and completed filters on, for example `https://www.ebay.com/sch/i.html?_nkw=<query>&LH_Sold=1&LH_Complete=1`. Opens in a new tab. No scraping, no API key needed.
- **Saved comps.** The owner can attach comparable sales to an item: price, optional note, optional URL, optional date. With one or more comps the app shows a suggested asking price (median of comps, adjusted by an optional condition factor relative to the comps' condition), plus min/max.
- The existing `RuleBasedPricingSuggestionService` (condition multiplier on the asking price) is replaced by this comps-based service. The condition multiplier idea can be kept as an optional adjustment.
- The owner always sets the final asking price manually.

### 5.5 Listings and marketplace copy

- An item can have many marketplace listings, at most one active listing per platform.
- Platforms in v1: `Ebay`, `FacebookMarketplace`. The enum stays extensible (`Other`).
- Per-platform formatted output with copy buttons:
  - **eBay:** title (hard 80-character limit, enforced and counted live), category suggestion, condition, price, description (plain text and simple HTML variants), item specifics (brand, model).
  - **Facebook Marketplace:** title, price, category, condition, description (plain text, no HTML), pickup/shipping note placeholder.
- A "Download photos" action (zip of the item's photos) for easy transfer to the phone's gallery before posting through the marketplace's own app.
- Listing records: platform, status (`Draft`, `Active`, `Sold`, `Ended`), listed price, listing URL, listed date.
- Reuse the existing `MarketplaceListing` entity; drop `PendingPublish`, `Failed` and `ExternalListingId` unless the eBay API milestone is built.

### 5.6 Sold flow and cross-post cleanup

- "Mark sold" captures: sold price, sold date (default today), sold platform (default: the platform of the listing the owner is on), optional note.
- On save: item status becomes `Sold`; the winning listing becomes `Sold`; **all other active listings for the item become `NeedsDelisting`**.
- A "Needs delisting" panel on the dashboard and item page shows each pending listing with its URL and a "Done, delisted" button, which sets the listing to `Ended`.
- Undo: a sold item can be reverted to `Listed` (mistakes happen), restoring listing statuses where possible.

### 5.7 Reports

Basic, read-only, calculated from existing data:

- Items sold per month, total revenue per month.
- Sell-through rate (sold / ever listed), average days from first listed to sold.
- Revenue and count by platform and by category.
- Currently listed: count, total asking value, oldest listings first.
- If purchase cost was entered: simple gross profit (sold price minus cost) for those items, labelled as excluding fees and shipping.
- CSV export of sold items.

### 5.8 Dashboard

Replace the current placeholder cards with: items by status, needs-delisting list, oldest active listings, this month's sales, and quick "Add item" button.

### 5.9 Settings

A single settings page for LM Studio connection (base URL, model, test button), default location/pickup note text for listings, and currency symbol. Stored in the database (single row) with `appsettings` defaults.

## 6. Data Model Changes

Current entities: `Item`, `ItemPhoto`, `ListingDraft`, `MarketplaceListing`, `RepricingRecommendation`, `PlatformAccount`, plus Identity `AppUser`.

| Entity | Change |
| --- | --- |
| `AppUser` / Identity | Remove. |
| `PlatformAccount` | Remove (no connected accounts in v1). |
| `RepricingRecommendation` | Remove (price-drop reminders are out of scope). Can be re-added later with its own migration. |
| `Item` | Remove `OwnerUserId`. Add `ConditionNotes`, `PurchaseCost?`, `SoldPrice?`, `SoldAtUtc?`, `SoldPlatform?`, `AiEstimateLow?`, `AiEstimateHigh?`. |
| `ItemPhoto` | Add `SortOrder`, `IsCover`. |
| `ListingDraft` | Keep for stored AI/template drafts; add `Category`, `Brand`, `Model`, `MissingInfoJson`. |
| `MarketplaceListing` | Statuses become `Draft`, `Active`, `Sold`, `Ended`, `NeedsDelisting`. Remove `ExternalListingId`, `PublishedAtUtc` becomes `ListedAtUtc`. |
| `ItemComp` (new) | `ItemId`, `Price`, `Note?`, `Url?`, `SoldOnUtc?`. |
| `AppSettings` (new, single row) | LM Studio base URL, model, timeout, default pickup note, currency. |

Database: SQLite file (for example `data/flipflow.db`). Decimals are stored via an EF value converter or as integer cents to avoid SQLite's lack of a native decimal type. Money handling is centralised so this choice is made once.

## 7. Architecture Impact

Keep the four-project layout (`Api`, `Application`, `Domain`, `Infrastructure`) and the interface-first approach.

- Remove: Identity, JWT, `AuthController`, `CurrentUserService`, Hangfire, Npgsql, `docker-compose.yml` (or keep it only as an optional Postgres profile; recommendation is to delete it).
- Keep and extend: `IListingContentGenerator` gets a real `LmStudioListingContentGenerator`; `IPricingSuggestionService` gets a comps-based implementation; `IFileStorageService` gains image downscaling.
- Add: `IMarketplaceFormatter` (one implementation per platform), `ISalesService` (mark sold, delisting logic), `IReportService`, `ISettingsService`.
- The API serves the built frontend from `wwwroot` so the whole app is one process on one port, which is the simplest thing to run on a home machine and to open from a phone.
- Frontend: remove auth pages and `ProtectedRoute`; add listing-draft review, comps, mark-sold, delisting, reports and settings pages.

## 8. Security and Network Posture

- The app has no login. It must therefore only be reachable on the trusted home network. Binding to `0.0.0.0` is opt-in via configuration, and the README states the risk.
- **Recommended safeguard (confirm before building):** an optional single shared passcode, set through configuration, checked by a small middleware and remembered in a cookie. Off by default. This is far lighter than the current Identity/JWT stack but stops casual access from other devices on the network.
- No secrets in the repo. The current development JWT signing key goes away with JWT.
- LM Studio's server is only enabled for the local network on the model machine; the app never exposes it.
- Uploaded files are validated by content type and magic bytes, not just file extension, and are served only from the uploads path.
- Back up `data/flipflow.db` and the uploads folder together; document a one-line backup command.

## 9. Non-Functional Requirements

- **Setup:** from a clean clone, install .NET 8 and Node, run one command to start. No Docker.
- **Performance:** item list under 500 ms for a few hundred items; AI generation allowed to take up to roughly a minute on local hardware, so it runs as an async request with visible progress, not a blocking page.
- **Reliability:** the app is fully usable with the model machine offline (manual entry plus template fallback).
- **Testing:** unit tests for pricing, marketplace formatters, sales/delisting rules, report calculations and LM Studio response parsing (using a fake HTTP handler). A couple of API integration tests against in-memory SQLite. Frontend type-check and build in CI.
- **Code quality:** nullable reference types on, warnings as errors for new code, consistent formatting.

## 10. Out of Scope for v1

- Price-drop reminders and automatic repricing (the earlier "Phase 5").
- Fee, shipping and full profit tracking.
- Real posting to eBay, Facebook, Mercari or others. (eBay API posting is the first candidate after v1; see plan milestone M9.)
- Multi-user accounts, roles, sign-up, email.
- Hosting on the public internet, push notifications, native mobile app.
- Barcode scanning, shipping label purchase, messaging buyers.

## 11. Success Criteria

v1 is successful when the owner can, from a phone and without touching a laptop:

1. Photograph an item and have a reviewable draft listing in under two minutes (when the model machine is on).
2. Copy eBay and Facebook ready text and post both.
3. Record a sale and be told exactly which other listing to take down.
4. Open a report and see what sold this month and what is stale.

## 12. Open Questions

1. Optional passcode: build it in M1 or skip it? (Recommendation: build it; it is small.)
2. Which vision model is loaded in LM Studio, and on what hardware? Generation speed and image-count limits depend on it. A test with the owner's real model should happen early in M4.
3. Preferred currency and locale (assumed USD, `en-US`).
4. Item specifics for eBay: is a free-text "key details" field enough, or should category-specific fields come later?
5. Should sold items' photos be archived or deleted after some time to save disk space?
6. Is Mercari (already in the enum) wanted later? It is dropped from v1.
