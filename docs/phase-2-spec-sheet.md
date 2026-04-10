# FlipFlow Phase 2 Spec Sheet

## Scope

Phase 2 turns FlipFlow from an authenticated shell into a usable inventory workflow. The goal is to let a signed-in seller create, review, update, and delete items, then attach item photos that can be reused in later listing and pricing phases.

## User Outcomes

- A seller can create a structured item draft with the core resale fields.
- A seller can see all of their items in a single inventory view.
- A seller can open an item detail page to review and edit the record.
- A seller can upload item photos that are stored locally in development and displayed back in the UI.
- A seller can remove an item if it was created in error.

## Backend Features

### Item API

- `GET /api/items`
  - Returns the current user's items sorted by most recently updated.
  - Includes summary metadata for list rendering.
- `GET /api/items/{itemId}`
  - Returns the full detail record for one owned item.
- `POST /api/items`
  - Creates a new item owned by the current authenticated user.
- `PUT /api/items/{itemId}`
  - Updates the owned item.
- `DELETE /api/items/{itemId}`
  - Deletes the owned item and its tracked photo metadata.

### Photo API

- `POST /api/items/{itemId}/photos`
  - Accepts a multipart file upload for an owned item.
  - Validates content type and size before persisting.
  - Stores file metadata in the database and file contents in local storage.

## Data Contract Shape

### Item summary

- `id`
- `title`
- `brand`
- `model`
- `category`
- `condition`
- `askingPrice`
- `status`
- `photoCount`
- `primaryPhotoRelativePath`
- `updatedAtUtc`

### Item detail

- All summary fields
- `description`
- `createdAtUtc`
- `photos[]`

### Item photo

- `id`
- `fileName`
- `contentType`
- `fileSizeBytes`
- `sortOrder`
- `isPrimary`
- `relativePath`
- `createdAtUtc`

## Validation Rules

### Item form

- `title`: required, max 150 chars
- `brand`: required, max 100 chars
- `model`: required, max 100 chars
- `category`: required, max 100 chars
- `description`: required, max 4000 chars
- `askingPrice`: required, must be `>= 0`
- `condition`: required enum value
- `status`: required enum value

### Photo upload

- Accept only `image/jpeg`, `image/png`, `image/webp`
- Max upload size: 5 MB per file
- Max photo count per item: 8
- First uploaded photo becomes the primary photo automatically

## UX Notes

- `My Items` becomes the main inventory dashboard with empty, loading, and error states.
- `New Item` becomes a structured intake form.
- A new `Item Detail` route supports review, editing, and photo upload.
- Item cards should surface the most important scan data:
  - title
  - status
  - condition
  - price
  - photo count
  - last updated time

## Deliberate Non-Goals

- AI listing generation
- cross-platform publishing
- repricing jobs
- drag-and-drop photo ordering
- marketplace account management

## Risks And Follow-Ups

- Deleting an item removes its metadata and attempts to clean up local files, but cloud storage parity is still a future concern.
- Photo transformations such as resizing, thumbnails, or EXIF stripping are not included yet.
- Search, filters, and pagination are intentionally deferred until the inventory view has real usage patterns.
