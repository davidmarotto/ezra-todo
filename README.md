# Todo App

A multi-user todo list web application with a Vue 3 frontend and ASP.NET Core REST API backend.

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


## Features

**Authentication**
- User registration and login with JWT-based authentication

**Todo Lists/Items**
- Create, rename, and delete lists and items
- Each list is owned by the creating user
- Optional due date per item
- Filter items by status
- Items sorted by status/due date

**Sharing**
- List owners can share lists with other registered users by email
- Lists shared with you appear in a separate sidebar section
- Owners can revoke access at any time
- Permission model designed to support multiple roles (Editor/Viewer)

**Reminder Notifications**
- Background polling service checks for todos due within a configurable window
- Sends reminders via a stub notification service (logs to console)
- Interface designed for swap-in with a real email/SMS provider (e.g. SendGrid, Twilio)

## Assumptions
- No server side rendering. Juice not worth the squeeze here since there's no need for SEO on private/authenticated lists

## Future Improvements
- Responsive design/mobile web
- Native mobile built on top of the same backend API
- Integrate with email/SMS provider for reminder notifications (currently stubbed to logs)
- Email verification on registration and password reset flow
- Support sharing to users who don't yet have an account via email invitations
- Searching across lists
- Pagination on list/todo endpoints
- Alternate authentication methods (OAuth / Social login)

## Scalability

**Horizontal Scaling**
The backend API is stateless — auth is JWT-based with no server-side session, so instances can be easily be scaled horizontally behind a load balancer. We can autoscale the primary service based on http request response time and latency. 

**Database Indexes**
EF Core creates primary key and foreign key indexes automatically by convention. We additionally define:

- **`Users.Email`** unique index enforces uniqueness at the database level and makes login lookups fast
- **`ListPermissions(TodoListId, UserId)`** unique composite index prevents duplicate shares and makes permission checks fast when determining whether a user can access a list
- **`TodoItems(DueDate, ReminderSentAt, IsCompleted)`** composite index makes the reminder polling query efficient; without this, every poll cycle would do a full table scan on `TodoItems`

The FK index on `TodoItems.TodoListId` is critical since fetching all todos for a given list is the most frequent query.

**Pagination**
List and todo endpoints return all records — acceptable at MVP scale but would require cursor or offset pagination as data grows.

**Database**
A single relational database becomes a write bottleneck at high scale. As the scale of the system grows I'd recommend the following scalability improvements:
- enable read replicas (most traffic is reads)
- vertical scaling to increase the CPU and number of connections supported by the database
- implement a caching layer (eg, Redis) for hot data.

If this becomes the most popular Todo app in the world we may need to consider sharding but this introduces quite a bit of complexity. Sharding on `userId` is the natural choice — a user's lists and todos stay co-located on one shard. Shared lists complicate this since they may reside on a different shard but we could work around this through denormalizing or by keeping a global unsharded permissions table for cross-shard lookups.

**Reminder Notifications**
The current polling approach scans the `TodoItems` table on a fixed interval. At scale this becomes expensive, and running multiple backend instances would cause duplicate reminders without clever locking.

A more scalable approach is event-driven: when a todo is created or updated with a due date, publish a `TodoScheduled` event. A consumer schedules the reminder to fire at `dueDate - leadTime` without any polling. Message brokers (eg, Azure or Kafka) should support this. This approach also decouples reminder delivery from the main API, allowing each to scale independently based on queue size or duration.

An alternative (maybe simpler) approach would be to use an in-memory sorted set (eg Redis ZSET). In response to the `TodoScheduled` event we push the item onto the set sorted descending by duedate timestamp. NotificationService instances pop items off the set and process them.

## Error Handling

All error responses use ProblemDetails, giving a consistent shape across all endpoints:

```json
{ "title": "No user found with that email.", "status": 404 }
```

Validation errors (missing fields, wrong types) is handled automatically by `[ApiController]`. Domain errors from the service layer are mapped to appropriate HTTP status codes (eg, `KeyNotFoundException` to 404, `InvalidOperationException` to 409). Unauthorized access returns 404 rather than 403 to avoid leaking resource existence.

**Database constraints**
Unique constraints are enforced at both the application and database level — for example, duplicate email registration is caught in the service layer before hitting the database, but the unique index on `Users.Email` acts as a safety net. EF surfaces constraint violations as exceptions which are caught and mapped to appropriate response codes.

**Frontend error handling**
The `api.js` service layer catches all non-2xx responses and throws a structured `{ status, message }` error. Components catch these and display the message inline near the relevant form or action. Unhandled exceptions (eg, network failure) surface as a generic "An unexpected error occurred."

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
Request and response shapes are defined as separate DTO classes rather than exposing models directly. This decouples the API contract from the database schema.

## Architecture

**Overview**

The app is split into two independently projects:

- **Backend** (`/backend`): ASP.NET Core Web API. Handles authentication, business logic, and data persistence via EF Core + SQLite. Exposes a REST API consumed by the frontend.
- **Frontend** (`/frontend`): Vue 3 SPA served by Vite. Communicates with the backend exclusively over HTTP — no server-side rendering.

**Authentication Flow**

1. User registers or logs in via `POST /auth/register` or `POST /auth/login`
2. Backend validates credentials, generates a signed JWT, and returns it in the response body
3. Frontend stores the token in `localStorage` and attaches it to every subsequent request as an `Authorization: Bearer <token>` header
4. Backend JWT middleware validates the token on every protected endpoint
5. The token encodes the user's ID and email, which controllers extract to identify the requesting user without an additional database lookup. Note: in a larger microservices environment we may want to encode additional permission details in the JWT to avoid repeated lookups in each service.

