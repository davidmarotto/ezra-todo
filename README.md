# Todo App

A multi-user todo list web application with a Vue 3 frontend and ASP.NET Core REST API backend.

## Features

**Authentication**
- User registration and login with JWT-based authentication

**Todo Lists/Items**
- Create, rename, and delete lists and items
- Each list is owned by the creating user
- Optional due date per item
- Filter items by status
- Sort items by status/due date

**Sharing**
- List owners can share lists with other registered users by email
- Lists shared with you appear in a separate sidebar section
- Owners can revoke access at any time
- Permission model designed to support multiple roles (Editor/Viewer)

**Reminder Notifications**
- Background polling service checks for todos due within a configurable lead time window
- Sends reminders via a stub notification service (logs to console)
- Interface designed for swap-in with a real email/SMS provider (e.g. SendGrid, Twilio)

## Assumptions
- No server side rendering. Juice not worth the squeeze here since there's no need for SEO on private/authenticated lists

## Future
- Responsive design/mobile web
- Native mobile built on top of the same backend API
- Build out email notifications for due date reminders
- Email verification on registration and password reset flow
- Support sharing to users who don't yet have an account via email invitations
- Searching across lists
- Pagination on list/todo endpoints
- Alternate authentication methods (OAuth / Social login)

## Scalability

**Horizontal Scaling**
The backend API is stateless — auth is JWT-based with no server-side session, so instances can be scaled horizontally behind a load balancer without coordination. Once reminders are event-driven, the notification service is similarly stateless and horizontally scalable, with the message broker distributing work across consumers.

**Database Indexes**
EF Core creates primary key and foreign key indexes automatically by convention. We additionally define:

- **`Users.Email`** (unique) — enforces uniqueness at the database level, also makes login lookups fast
- **`ListPermissions(TodoListId, UserId)`** (unique composite) — prevents duplicate shares and makes permission checks fast when determining whether a user can access a list
- **`TodoItems(DueDate, ReminderSentAt, IsCompleted)`** (composite) — makes the reminder polling query efficient; without this, every poll cycle would do a full table scan on `TodoItems`

The FK index on `TodoItems.TodoListId` (automatic) is particularly important in practice — fetching all todos for a given list is the most frequent query in the app and this keeps it O(log n) rather than a full scan.

**Pagination**
List and todo endpoints return all records — acceptable at MVP scale but would require cursor or offset pagination as data grows.

**Database**
A single relational database becomes a write bottleneck at high scale. The practical scaling path is: read replicas first (most traffic is reads), then vertical scaling, then a caching layer (Redis) for hot data. Sharding is a last resort due to operational complexity.

Sharding on `userId` is the natural choice — a user's lists and todos stay co-located on one shard. The complication is shared lists: if Alice (shard 1) shares a list with Bob (shard 2), Bob's shard has no knowledge of it. This can be handled by denormalizing the permission to Bob's shard on share, or by keeping a global unsharded permissions table for cross-shard lookups. Sharding on `listId` instead avoids this problem but means a user's own lists may span shards.

**Reminder Notifications**
The current polling approach scans the `TodoItems` table on a fixed interval. At scale this becomes expensive, and running multiple backend instances would cause duplicate reminders without a distributed lock.

A more scalable approach is event-driven: when a todo is created or updated with a due date, publish a `TodoScheduled` event. A consumer schedules the reminder to fire at `dueDate - leadTime` without any polling. **Azure Service Bus** is a natural fit here — it supports scheduled message delivery natively, so the broker itself acts as the scheduler. This also decouples reminder delivery from the main API, allowing each to scale independently.

## Error Handling

All error responses follow the RFC 7807 ProblemDetails standard, giving a consistent JSON shape across all endpoints:

```json
{ "title": "No user found with that email.", "status": 404 }
```

Structural validation (missing fields, wrong types) is handled automatically by `[ApiController]` before reaching controller logic. Domain errors from the service layer are mapped to appropriate HTTP status codes — `KeyNotFoundException` → 404, `InvalidOperationException` → 409. Unauthorized access returns 404 rather than 403 to avoid leaking resource existence.

**Database constraints**
Unique constraints are enforced at both the application and database level — for example, duplicate email registration is caught in the service layer before hitting the database, but the unique index on `Users.Email` acts as a safety net. EF Core surfaces constraint violations as exceptions which are caught and mapped to 409 Conflict responses.

**Frontend error handling**
The `api.js` service layer catches all non-2xx responses and throws a structured `{ status, message }` error. Components catch these and display the message inline near the relevant form or action. Unhandled exceptions (e.g. network failure) surface as a generic "An unexpected error occurred." message — in production these would additionally be logged to an error tracking service.

## API Design

The API follows REST conventions — resources are nouns, HTTP verbs express intent, and routes reflect resource hierarchy:

```
POST   /auth/register
POST   /auth/login

GET    /lists
POST   /lists
PUT    /lists/{id}
DELETE /lists/{id}

GET    /lists/{listId}/todos
POST   /lists/{listId}/todos
PUT    /lists/{listId}/todos/{id}
DELETE /lists/{listId}/todos/{id}

GET    /lists/{listId}/permissions
POST   /lists/{listId}/permissions
DELETE /lists/{listId}/permissions/{userId}
```

**DTOs**
Request and response shapes are defined as separate DTO classes rather than exposing models directly. This decouples the API contract from the database schema — the two can evolve independently, and sensitive fields (e.g. `PasswordHash`) are never accidentally serialized.

**Trusted vs. client-supplied data**
The authenticated `userId` is always derived server-side from the JWT claim — never accepted from the request body. Similarly, resource identifiers like `listId` come from the route, not the body. This enforces a clean boundary: the request body contains only what the client is allowed to supply.

**404 instead of 403**
When a user requests a resource they don't have access to, the API returns 404 rather than 403. This avoids leaking whether a resource exists to unauthorized callers.

## Architecture

**Overview**

The app is split into two independently runnable projects:

- **Backend** (`/backend`): ASP.NET Core Web API. Handles authentication, business logic, and data persistence via EF Core + SQLite. Exposes a REST API consumed by the frontend.
- **Frontend** (`/frontend`): Vue 3 SPA served by Vite. Communicates with the backend exclusively over HTTP — no server-side rendering.

**Authentication Flow**

1. User registers or logs in via `POST /auth/register` or `POST /auth/login`
2. Backend validates credentials, generates a signed JWT, and returns it in the response body
3. Frontend stores the token in `localStorage` and attaches it to every subsequent request as an `Authorization: Bearer <token>` header
4. Backend JWT middleware validates the token on every protected endpoint — invalid or missing tokens are rejected with 401 before reaching any controller logic
5. The token encodes the user's ID (`sub` claim) and email, which controllers extract to identify the requesting user without an additional database lookup

## Prerequisites

- [.NET 10 SDK](https://dot.net)
- [Node.js](https://nodejs.org) (v18+)
- [dotnet-ef CLI tool](https://learn.microsoft.com/en-us/ef/core/cli/dotnet): `dotnet tool install --global dotnet-ef`

## Running the Backend

```bash
cd backend
dotnet ef database update   # creates todo.db on first run
dotnet run
```

API available at `http://localhost:5049` — API explorer at `http://localhost:5049/scalar/v1`

## Running the Frontend

```bash
cd frontend
npm install   # first time only
npm run dev
```

App available at `http://localhost:5173`
