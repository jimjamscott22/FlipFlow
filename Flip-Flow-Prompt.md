You are a senior full-stack software engineer helping me build a polished portfolio project step-by-step.

I want to build a full-stack application called “FlipFlow” using the following stack:

- Frontend: React + TypeScript + Vite
- UI: Tailwind CSS + shadcn/ui
- Backend: ASP.NET Core Web API (.NET 8 or latest stable)
- Authentication: ASP.NET Core Identity with JWT auth
- Database: PostgreSQL
- ORM: Entity Framework Core
- Background jobs: Hangfire
- File/image storage: local storage for development, designed so it can later be swapped to S3-compatible storage
- API communication: REST
- Architecture: clean, modular, production-style structure suitable for learning and portfolio use

The app idea:
FlipFlow helps users sell their used stuff online faster by assisting with item intake, condition tracking, pricing suggestions, listing generation, listing management, and repricing recommendations. The first niche should be electronics, but the architecture should allow future expansion into other categories.

I want this project to be educational. Please generate code that is:
- organized
- readable
- strongly typed where appropriate
- commented only where useful
- not overengineered
- realistic enough to reflect professional architecture
- easy for me to study afterward

Important instructions:
1. Build the project in phases.
2. Before generating large chunks of code, explain the folder structure and architectural decisions briefly.
3. Prefer small, coherent files over giant files.
4. Use good naming conventions.
5. Avoid placeholder pseudocode unless absolutely necessary.
6. When something is not implemented yet, stub it cleanly with TODO comments.
7. Keep the UI modern and polished, but not overly flashy.
8. Assume this is a portfolio-quality app I may show recruiters.
9. Use DTOs, service layers, and separation of concerns.
10. Use migrations for database setup.
11. Add seed/demo data where useful.
12. Include a README section as the project evolves.
13. Use conventional commit-style milestone summaries when finishing each phase.

Project goals for MVP:
- User authentication and registration
- Dashboard for a signed-in user
- Create an item listing draft
- Upload item photos
- Store structured item details:
  - title
  - brand
  - model
  - category
  - condition
  - description
  - asking price
  - status
- Generate AI-assisted listing title and description with a placeholder abstraction layer for AI
- Track listing status across platforms
- Support platform targets such as:
  - eBay
  - Facebook Marketplace
  - Mercari
  These do not need full live integrations yet. Build the system so platform integrations can be added later.
- Repricing recommendation engine:
  - suggest a lower price if an item has been unsold for X days
- Background jobs for repricing checks
- Item detail page
- Listing management page
- Clean REST API
- Clean database schema

Core domain concepts:
- User
- Item
- ItemPhoto
- MarketplaceListing
- ListingDraft
- RepricingRecommendation
- PlatformAccount (future-ready, may be minimal now)
- Audit fields (createdAt, updatedAt where appropriate)

Suggested statuses:
- Draft
- ReadyToList
- Listed
- Sold
- Archived

Suggested item conditions:
- New
- LikeNew
- Good
- Fair
- Poor
- ForParts

Suggested marketplace listing statuses:
- Draft
- PendingPublish
- Published
- Expired
- Sold
- Failed

Technical preferences:
- Backend should follow a structure similar to:
  - FlipFlow.Api
  - FlipFlow.Application
  - FlipFlow.Domain
  - FlipFlow.Infrastructure

If you think a simpler modular monolith structure is better for a solo portfolio project, explain why and use it, but still keep separation between domain, application, infrastructure, and API concerns.

Backend expectations:
- ASP.NET Core Web API
- Identity-based auth with JWT
- EF Core with PostgreSQL
- FluentValidation if appropriate
- AutoMapper only if truly useful, otherwise map manually
- Swagger/OpenAPI enabled
- Global exception handling
- Configuration via appsettings + environment variables
- Repository pattern only if it adds real value. Do not add unnecessary abstraction on top of EF Core.
- Services for business logic
- Background job service with Hangfire
- Dev-friendly local setup

Frontend expectations:
- React + TypeScript + Vite
- Tailwind + shadcn/ui
- React Router
- TanStack Query
- React Hook Form + Zod
- Auth flow with JWT
- Protected routes
- Pages/views:
  - Login/Register
  - Dashboard
  - My Items
  - New Item
  - Item Detail
  - Listings
- Reusable components
- Good loading, error, and empty states
- Nice card-based layout
- Modern but professional look

Please design the app so that AI functionality is abstracted behind an interface, for example:
- IListingContentGenerator
This should initially return mocked/generated content locally so the app works without external AI API keys. Later it should be easy to swap in a real provider such as OpenAI.

Please also design the pricing/repricing system behind an interface, for example:
- IPricingSuggestionService
Initially, implement a simple rules-based version using category, condition, and age of listing. It does not need external market data yet.

Please include image upload support:
- backend endpoint for upload
- file validation
- storing file paths/URLs in database
- serve uploaded images in development
- frontend upload UI

Please think carefully about the database model and relationships before scaffolding everything.

What I want from you first:
1. Propose the architecture
2. Propose the folder/project structure
3. Propose the database schema/entities
4. Propose the implementation phases
5. Then begin implementing Phase 1

Implementation phases should look roughly like:
- Phase 1: Solution setup, architecture, auth, base database
- Phase 2: Item CRUD and photo upload
- Phase 3: Listing drafts and AI-generated content abstraction
- Phase 4: Marketplace listing tracking
- Phase 5: Repricing engine and Hangfire jobs
- Phase 6: Frontend polish, validation, error states, demo data
- Phase 7: README, cleanup, and portfolio readiness

For each phase:
- briefly explain what is being built
- generate the needed code
- show all file contents clearly
- include commands to run the project
- include migration commands where needed

Coding style:
- favor clarity over cleverness
- use practical enterprise-style patterns
- keep files manageable
- use explicit names
- avoid magic behavior
- use consistent DTOs and request/response models
- make it easy for me to learn from reading the code

Also include:
- a short domain explanation for each major entity
- why certain architectural choices were made
- what could be improved later in a production SaaS version

Please start now with:
A. architecture proposal
B. project structure
C. entity/schema design
D. phased roadmap
E. initial implementation of Phase 1

**Please make the project local-development friendly with Docker Compose for PostgreSQL and pgAdmin. Application containers are optional at first, but the database should be easy to run locally with one command.**

Do not skip straight to random code without first presenting the plan.