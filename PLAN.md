# Implementation Plan

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
| `Role` | `enum` | `Editor`, `Viewer` (Owner is implicit via TodoList.OwnerId) |
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

**API Endpoints:**

Auth:
| Method | Path | Description |
|---|---|---|
| `POST` | `/auth/register` | Create account |
| `POST` | `/auth/login` | Login, returns JWT |

Todo Lists:
| Method | Path | Description |
|---|---|---|
| `GET` | `/lists` | Get all lists the current user owns or has access to |
| `POST` | `/lists` | Create a list |
| `GET` | `/lists/{id}` | Get a single list |
| `PUT` | `/lists/{id}` | Update list name (owner only) |
| `DELETE` | `/lists/{id}` | Delete list (owner only) |
| `POST` | `/lists/{id}/share` | Share list with a user by email + role |
| `DELETE` | `/lists/{id}/share/{userId}` | Revoke access (owner only) |

Todos (all scoped to a list, permission-checked):
| Method | Path | Description |
|---|---|---|
| `GET` | `/lists/{listId}/todos` | List todos; optional `?status=active\|completed` |
| `POST` | `/lists/{listId}/todos` | Create a todo (Editor+ only) |
| `GET` | `/lists/{listId}/todos/{id}` | Get a single todo |
| `PUT` | `/lists/{listId}/todos/{id}` | Update todo (Editor+ only) |
| `DELETE` | `/lists/{listId}/todos/{id}` | Delete todo (Editor+ only) |

**Permission rules:**
- Owner: full control over list, todos, and shares
- Editor: read + write todos; cannot manage shares or delete list
- Viewer: read todos only

**Error handling:** `ProblemDetails` responses (RFC 7807) for 400, 401, 403, 404, 500.

**Reminder service:**
- `ReminderBackgroundService` runs on a configurable interval (default: every hour)
- Queries for todos where `DueDate` is within the reminder window (default: 24h), `IsCompleted = false`, `ReminderSentAt = null`
- Calls `INotificationService.SendReminderAsync(todo)`
- Sets `ReminderSentAt` to prevent re-firing

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

**UI features:**
- Register / login screens
- Sidebar listing all accessible todo lists
- Selected list view with todos, filter tabs (All / Active / Completed)
- Add/edit/delete todos with optional due date
- Share list modal: enter email + role
- Error and loading states throughout

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
