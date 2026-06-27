# Implementation Plan

## Phases

### Phase 1 — Core Backend
Full data model, database setup, authentication, and REST API for lists and todos. Error handling included per milestone.

- **1.1 Data model & database setup**
  - Install EF Core + SQLite packages
  - Create `User`, `TodoList`, `TodoItem`, `ListPermission` models
  - Create `AppDbContext`
  - Create and apply initial migration

- **1.2 Auth**
  - `POST /auth/register` — create account with hashed password
  - `POST /auth/login` — validate credentials, return JWT
  - JWT middleware configured in `Program.cs`
  - All subsequent endpoints require a valid token

- **1.3 Todo Lists API**
  - `GET /lists` — lists owned by or shared with current user
  - `POST /lists` — create a list
  - `GET /lists/{id}` — get a single list (permission-checked)
  - `PUT /lists/{id}` — update name (owner only)
  - `DELETE /lists/{id}` — delete list (owner only)

- **1.4 Todo Items API**
  - `GET /lists/{listId}/todos` — list todos, optional `?status=active|completed`
  - `POST /lists/{listId}/todos` — create todo (Editor+ only)
  - `GET /lists/{listId}/todos/{id}` — get a single todo
  - `PUT /lists/{listId}/todos/{id}` — update todo (Editor+ only)
  - `DELETE /lists/{listId}/todos/{id}` — delete todo (Editor+ only)

- **1.5 Tests**
  - xUnit project setup
  - Auth: register, login, duplicate email, invalid credentials
  - Lists: CRUD happy paths, owner-only enforcement (403 cases)
  - Todos: CRUD happy paths, 404 on missing list/todo, validation failures

---

### Phase 2 — Frontend
Basic UI connected to the real backend.

- Login and register screens
- CORS configured on backend
- API service layer (attaches JWT to requests)
- List sidebar: view, create, select lists
- Todo view: create, edit, mark complete, delete todos
- Filter tabs: All / Active / Completed

---

### Phase 3 — Scheduled Reminders
- `ReminderBackgroundService`: polls on configurable interval, queries todos due within reminder window, calls `INotificationService`
- `INotificationService` / `NotificationService`: stub that logs to console, interface designed for SMS/email swap
- Configurable via `appsettings.json`

---

### Phase 4 — Sharing
- `POST /lists/{id}/share` — share list with a user by email + role
- `DELETE /lists/{id}/share/{userId}` — revoke access (owner only)
- Share modal in frontend: enter email, select Editor/Viewer role
- Shared lists appear in recipient's list sidebar

---

### Phase 5 — Polish
- Frontend loading and error states
- Form validation feedback
- Empty states (no lists, no todos)
- UX cleanup and consistency pass

---

## Data Model

**User**
| Field | Type | Notes |
|---|---|---|
| `Id` | `Guid` | Primary key |
| `Email` | `string` | Unique, required |
| `PasswordHash` | `string` | Bcrypt hashed |
| `CreatedAt` | `DateTime` | Set on register |

**TodoList**
| Field | Type | Notes |
|---|---|---|
| `Id` | `Guid` | Primary key |
| `Name` | `string` | Required, max 100 chars |
| `OwnerId` | `Guid` | FK → User |
| `CreatedAt` | `DateTime` | Set on create |
| `UpdatedAt` | `DateTime` | Updated on every write |

**ListPermission**
| Field | Type | Notes |
|---|---|---|
| `Id` | `Guid` | Primary key |
| `TodoListId` | `Guid` | FK → TodoList |
| `UserId` | `Guid` | FK → User |
| `Role` | `enum` | `Editor`, `Viewer` (Owner implicit via `TodoList.OwnerId`) |
| `InvitedAt` | `DateTime` | When the share was created |

**TodoItem**
| Field | Type | Notes |
|---|---|---|
| `Id` | `Guid` | Primary key |
| `TodoListId` | `Guid` | FK → TodoList |
| `Title` | `string` | Required, max 200 chars |
| `IsCompleted` | `bool` | Default false |
| `DueDate` | `DateTime?` | Optional |
| `ReminderSentAt` | `DateTime?` | Null until reminder fires; prevents duplicate notifications |
| `CreatedAt` | `DateTime` | Set on create |
| `UpdatedAt` | `DateTime` | Updated on every write |

## Backend

**Stack:** ASP.NET Core 10, EF Core, SQLite, JWT authentication

**Structure:**
```
backend/
├── Controllers/
│   ├── AuthController.cs           # Register, login
│   ├── TodoListsController.cs      # List CRUD + sharing
│   └── TodosController.cs          # Todo CRUD (scoped to a list)
├── Models/
│   ├── User.cs
│   ├── TodoList.cs
│   ├── TodoItem.cs
│   └── ListPermission.cs
├── DTOs/
│   ├── Auth/
│   │   ├── RegisterRequest.cs
│   │   ├── LoginRequest.cs
│   │   └── AuthResponse.cs
│   ├── TodoLists/
│   │   ├── CreateTodoListRequest.cs
│   │   ├── UpdateTodoListRequest.cs
│   │   └── ShareTodoListRequest.cs
│   └── Todos/
│       ├── CreateTodoRequest.cs
│       └── UpdateTodoRequest.cs
├── Data/
│   └── AppDbContext.cs
├── Services/
│   ├── IAuthService.cs
│   ├── AuthService.cs              # Password hashing, JWT generation
│   ├── ITodoListService.cs
│   ├── TodoListService.cs          # List logic + permission checks
│   ├── ITodoService.cs
│   ├── TodoService.cs
│   ├── INotificationService.cs
│   └── NotificationService.cs     # Stub — logs to console
├── BackgroundServices/
│   └── ReminderBackgroundService.cs
└── Program.cs
```

**Permission rules:**
- Owner: full control over list, todos, and shares
- Editor: read + write todos; cannot manage shares or delete list
- Viewer: read todos only

**Error handling:** `ProblemDetails` responses (RFC 7807) for 400, 401, 403, 404, 500.

## Frontend

**Stack:** Vue 3, Vite, Vue Router, Pinia

**Structure:**
```
frontend/src/
├── components/
│   ├── TodoList.vue
│   ├── TodoItem.vue
│   ├── AddTodoForm.vue
│   └── ShareListModal.vue
├── views/
│   ├── LoginView.vue
│   ├── RegisterView.vue
│   └── HomeView.vue
├── router/
│   └── index.js                   # Vue Router: public vs protected routes
├── stores/
│   └── auth.js                    # Pinia: JWT token + current user
├── services/
│   └── api.js                     # Fetch wrapper; attaches Authorization header
├── App.vue
└── main.js
```

Vue Router and Pinia were added via `npm install vue-router pinia` and wired up in `main.js`.

## Testing

- Backend: xUnit integration tests covering auth, CRUD happy paths, permission enforcement, and key error cases (401, 403, 404, invalid input)
- Frontend: not included in scope

## Configuration (`appsettings.json`)

```json
"Jwt": {
  "Secret": "<long-random-secret>",
  "ExpiryHours": 24,
  "Issuer": "TodoApp"
},
"Reminders": {
  "PollingIntervalMinutes": 60,
  "LeadTimeHours": 24
}
```
