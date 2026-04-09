# FlipFlow

FlipFlow helps users sell their used stuff online faster by assisting with item intake, condition tracking, pricing suggestions, listing generation, listing management, and repricing recommendations. The first niche is electronics, but the architecture is designed to expand into other categories later.

## Architecture Proposal

FlipFlow is set up as a modular monolith. That is the right tradeoff for a solo portfolio project because it keeps deployment and local development simple, while still preserving the separation of concerns recruiters expect to see.

Backend layers:

- `FlipFlow.Api`: HTTP concerns only. Controllers, middleware, configuration, Swagger, auth pipeline, static file hosting.
- `FlipFlow.Application`: use-case contracts, DTOs, validators, and service abstractions. This layer defines what the system does.
- `FlipFlow.Domain`: core business entities, enums, and persistence-agnostic rules.
- `FlipFlow.Infrastructure`: EF Core, Identity, JWT implementation, file storage, seeded data, Hangfire wiring, and application service implementations.

Frontend approach:

- React + TypeScript + Vite for fast local development.
- Tailwind CSS + shadcn-style source components for a clean, reusable UI system.
- React Router for route structure.
- TanStack Query for server state.
- React Hook Form + Zod for form ergonomics and strong validation.

Why this architecture:

- Clean enough to demonstrate professional layering.
- Simple enough to study without drowning in abstractions.
- EF Core is used directly through `AppDbContext` instead of adding repositories on top of it, because repository wrappers would mostly duplicate EF behavior here.
- AI and pricing logic are abstracted behind interfaces from the start so the app can evolve without rewriting controllers or domain code.

## Project Structure

```text
FlipFlow/
├── src/
│   ├── backend/
│   │   ├── Directory.Build.props
│   │   ├── FlipFlow.Api/
│   │   ├── FlipFlow.Application/
│   │   ├── FlipFlow.Domain/
│   │   └── FlipFlow.Infrastructure/
│   └── frontend/
│       ├── public/
│       └── src/
│           ├── app/
│           ├── components/
│           ├── features/
│           └── lib/
└── README.md
```

Backend folder intent:

- `Domain/Common`: base entity types and audit support.
- `Domain/Entities`: persistent business entities.
- `Application/Abstractions`: interfaces for auth, pricing, AI content generation, storage, and dashboard use cases.
- `Application/Contracts`: request and response DTOs.
- `Infrastructure/Data`: `DbContext`, EF configurations, design-time context, and seeding.
- `Infrastructure/Auth`: Identity user model, JWT generation, and auth service implementation.
- `Api/Extensions`: service registration and upload static file hosting.
- `Api/Middleware`: global exception handling.

Frontend folder intent:

- `app`: top-level router and providers.
- `components`: reusable UI and layout pieces.
- `features`: route-level modules like auth, dashboard, and items.
- `lib`: shared utilities, environment access, and API client code.

## Entity And Schema Design

### `User`

Represents the authenticated seller. Implemented with ASP.NET Core Identity in infrastructure so password, claims, and future roles stay in the framework-supported model.

Key fields:

- `Id`
- `DisplayName`
- `Email`

### `Item`

The core inventory record owned by a user. This is the main source object for listing drafts, marketplace listings, photos, and repricing analysis.

Key fields:

- `OwnerUserId`
- `Title`
- `Brand`
- `Model`
- `Category`
- `Condition`
- `Description`
- `AskingPrice`
- `Status`
- `CreatedAtUtc`
- `UpdatedAtUtc`

### `ItemPhoto`

Metadata for a stored image file. Physical storage is abstracted so development can use local disk and production can later move to S3-compatible storage.

Key fields:

- `ItemId`
- `FileName`
- `StoredFileName`
- `RelativePath`
- `ContentType`
- `FileSizeBytes`
- `SortOrder`
- `IsPrimary`

### `ListingDraft`

Editable draft content for a marketplace-ready item listing. This is where AI-assisted content generation will plug in first.

Key fields:

- `ItemId`
- `Title`
- `Description`
- `IsAiGenerated`
- `GeneratedAtUtc`

### `MarketplaceListing`

Tracks a listing on a specific target platform such as eBay, Facebook Marketplace, or Mercari.

Key fields:

- `ItemId`
- `Platform`
- `Status`
- `ListedPrice`
- `ExternalListingId`
- `ListingUrl`
- `PublishedAtUtc`
- `SoldAtUtc`

### `RepricingRecommendation`

Stores suggested pricing changes produced by the repricing engine and later background jobs.

Key fields:

- `ItemId`
- `PreviousPrice`
- `RecommendedPrice`
- `Reason`
- `EvaluatedAtUtc`
- `IsApplied`
- `AppliedAtUtc`

### `PlatformAccount`

Future-ready record for connecting seller accounts to external marketplaces.

Key fields:

- `OwnerUserId`
- `Platform`
- `DisplayName`
- `ExternalAccountId`
- `IsConnected`

### Relationships

- One user owns many items.
- One item has many photos.
- One item has many listing drafts over time.
- One item can have many marketplace listings, but only one per platform.
- One item can have many repricing recommendations over time.
- One user can eventually connect many platform accounts.

### Enum design

- `ItemStatus`: `Draft`, `ReadyToList`, `Listed`, `Sold`, `Archived`
- `ItemCondition`: `New`, `LikeNew`, `Good`, `Fair`, `Poor`, `ForParts`
- `MarketplaceListingStatus`: `Draft`, `PendingPublish`, `Published`, `Expired`, `Sold`, `Failed`
- `MarketplacePlatform`: `Ebay`, `FacebookMarketplace`, `Mercari`

## Phased Roadmap

### Phase 1: Solution setup, architecture, auth, base database

Build the project skeleton, Identity-based auth, JWT flow, core domain entities, EF Core setup, Swagger, exception handling, seed data, and a protected frontend dashboard shell.

### Phase 2: Item CRUD and photo upload

Add item create/read/update flows, photo upload API and UI, file validation, local file storage, and item detail screens.

### Phase 3: Listing drafts and AI content abstraction

Introduce listing draft CRUD, mocked AI generation through `IListingContentGenerator`, and draft editing UX.

### Phase 4: Marketplace listing tracking

Add per-platform listing records, platform status views, filters, and future-ready integration seams.

### Phase 5: Repricing engine and Hangfire jobs

Wire `IPricingSuggestionService`, repricing recommendation generation, scheduled checks, and recommendation management.

### Phase 6: Frontend polish, validation, error states, demo data

Improve UX details, empty states, optimistic hints, form polish, seeded workflows, and visual consistency.

### Phase 7: README, cleanup, and portfolio readiness

Finalize documentation, screenshots, setup guidance, architecture notes, and recruiter-facing polish.

## Phase 1 Implementation

### What was built

- Backend project boundaries for API, Application, Domain, and Infrastructure
- ASP.NET Core Identity + JWT auth contract and implementation
- Core EF Core entity model for all MVP domain concepts
- PostgreSQL-ready `AppDbContext` and entity configurations
- Hangfire registration and dashboard plumbing
- Development seed path with a demo user and demo item
- React frontend scaffold with protected routing and auth screens
- Dashboard page hooked to a protected API endpoint
- README roadmap and setup instructions

### Files added in Phase 1

Backend:

- `src/backend/Directory.Build.props`
- `src/backend/FlipFlow.Domain/*`
- `src/backend/FlipFlow.Application/*`
- `src/backend/FlipFlow.Infrastructure/*`
- `src/backend/FlipFlow.Api/*`

Frontend:

- `src/frontend/package.json`
- `src/frontend/tsconfig*.json`
- `src/frontend/vite.config.ts`
- `src/frontend/tailwind.config.ts`
- `src/frontend/components.json`
- `src/frontend/src/*`

### Run commands

Frontend:

```bash
cd src/frontend
npm install
npm run dev
```

Backend:

```bash
cd src/backend/FlipFlow.Api
dotnet restore
dotnet run
```

### Database and migrations

Once the .NET SDK is installed, create the initial migration from `src/backend`:

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate \
  --project FlipFlow.Infrastructure \
  --startup-project FlipFlow.Api \
  --output-dir Data/Migrations

dotnet ef database update \
  --project FlipFlow.Infrastructure \
  --startup-project FlipFlow.Api
```

Local PostgreSQL defaults used in `appsettings.json`:

```text
Host=localhost;Port=5432;Database=flipflow;Username=postgres;Password=postgres
```

Seeded demo credentials:

```text
email: demo@flipflow.local
password: FlipFlow123
```

### Practical note about this environment

The current Codex environment does not have the `.NET` SDK or `psql` installed, so Phase 1 source code and setup files were created, but the backend was not compiled here and EF migrations were not generated locally. The commands above are the next step on a machine with the SDK installed.

## What could improve later in a production SaaS version

- Refresh tokens and token revocation instead of access-token-only auth
- Structured permissions and richer role/claim modeling
- Cloud object storage with signed URLs
- Better observability with request tracing and metrics
- Stronger background job orchestration and retry policies
- External marketplace integration adapters per platform
- More explicit application use-case handlers when workflows become larger

## Milestone Summary

`feat(phase-1): scaffold modular monolith with auth, base domain model, dashboard shell, and local development setup`
