# FlipFlow Phase 2 Implementation Plan

## Goal

Deliver a complete first pass of item management so the app has a meaningful post-auth workflow and a realistic base for later listing and repricing phases.

## Workstreams

### 1. Backend application contracts

- Add item request and response DTOs under `FlipFlow.Application.Contracts.Items`
- Add `IItemService` under `FlipFlow.Application.Abstractions.Items`
- Add FluentValidation rules for item create and update payloads

### 2. Backend item service

- Implement item querying and ownership checks in infrastructure
- Map domain entities to detail and summary DTOs
- Add create, update, delete, and photo-upload operations
- Enforce the Phase 2 photo limits in service-level logic

### 3. Backend API surface

- Add `ItemsController` with secured CRUD endpoints
- Add multipart upload endpoint for item photos
- Extend global exception handling to support `404 Not Found`

### 4. Frontend data layer

- Add an items API client module
- Update the generic API helper so multipart form data works
- Centralize enum options and item/photo view-model helpers

### 5. Frontend views

- Replace the placeholder `My Items` page with a real list
- Replace the placeholder `New Item` page with a validated intake form
- Add an `Item Detail` page with edit and photo upload support
- Update routing and navigation links to include the detail flow

### 6. Verification

- Run frontend build
- Run backend build
- If available, generate EF migrations for the current schema state
- Update README to reflect the new Phase 2 capability and dev commands

## Acceptance Criteria

- An authenticated user can create an item and immediately land on its detail page.
- The created item appears in `My Items`.
- An authenticated user can edit an existing item.
- An authenticated user can upload a supported image to an item and see it in the detail page.
- Unsupported upload types and oversized uploads are rejected with useful messages.
- The backend and frontend build successfully in the local environment.
