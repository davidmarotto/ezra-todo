# Discussion Notes

Architecture decisions, design tradeoffs, and scalability considerations for interview discussion.

---

## Async/Await

The backend is written using .NET's async/await pattern throughout. All I/O-bound operations (database reads/writes) are non-blocking — when a thread is waiting on the database it is released to handle other incoming requests rather than sitting idle.

**Tradeoff vs Rails:** Rails (with Puma) handles concurrency via multiple threads/processes, each of which may block on I/O. This is simpler to write but less efficient under high concurrency — each blocked thread still consumes memory and OS resources. .NET's async model means fewer threads can serve more simultaneous requests, which is one reason .NET tends to benchmark well for raw throughput.

**In our context:** For a single-user todo app the difference is negligible. But async is the .NET standard pattern and writing it consistently means the app scales without refactoring if load increases.

---

## Database Indexes

EF Core creates several indexes automatically by convention:

- **Primary key indexes** on every `Id` (`Guid`) column — unique clustered index, created for all four tables
- **Foreign key indexes** on all FK columns (`OwnerId`, `TodoListId`, `UserId`) — created automatically when EF Core detects a navigation property + FK pair

We additionally defined two **unique constraint indexes** explicitly in `AppDbContext.OnModelCreating`:

- `Users.Email` — prevents duplicate accounts at the database level, not just the application level
- `ListPermissions(TodoListId, UserId)` — ensures a user can only have one permission entry per list; prevents duplicate shares

**Scalability note:** As the todo list grows, the FK index on `TodoItems.TodoListId` will be important for query performance — fetching all todos for a given list is a frequent operation and this index makes it O(log n) rather than a full table scan. Similarly, the composite index on `ListPermissions` makes permission checks fast when looking up whether a user has access to a given list.

---

## Automatic Model Validation

Decorating controllers with `[ApiController]` enables automatic request validation in ASP.NET Core. If a required field is missing or the request body is malformed, the framework returns a 400 Bad Request with a `ProblemDetails` response before the controller method is even called — no manual validation code needed.

**Tradeoff:** This handles structural validation (missing fields, wrong types) for free, but domain-level validation (e.g. "title must not exceed 200 chars") still needs to be handled explicitly — either via Data Annotations on the DTO or in the service layer. We use Data Annotations for simple constraints and keep more complex business rules in the service.

**What we'd add in production:** A validation library like FluentValidation gives more expressive, testable validation rules and keeps validation logic out of both the DTO and the service.

---

## ProblemDetails (RFC 7807)

All API error responses use the IETF ProblemDetails standard rather than a custom error format. This gives a consistent JSON shape across all endpoints:

```json
{ "title": "Email already in use.", "status": 409 }
```

ASP.NET Core's `[ApiController]` returns ProblemDetails automatically for 400 validation failures. We follow the same format for domain errors (409 Conflict, 401 Unauthorized, 403 Forbidden, 404 Not Found). The frontend can handle errors with a single consistent pattern regardless of which endpoint it's calling.
