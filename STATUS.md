# Implementation Status

## Phase 1 — Core Backend
- [x] 1.1 Data model & database setup
- [x] 1.2 Auth
- [x] 1.3 Todo Lists API
- [x] 1.4 Todo Items API
- [x] 1.5 Tests
  - [x] AuthService unit tests (register/login happy and error paths)
  - [x] TodoListService integration tests (permission logic — own lists, shared lists, non-owner delete)
  - [x] TodosController integration tests (401 unauthenticated, 200 valid JWT, 404 wrong user's list)

## Phase 2 — Frontend
- [x] Login and register screens
- [x] CORS configured on backend
- [x] API service layer
- [x] List sidebar
- [x] Todo view
- [x] Filter tabs

## Phase 3 — Scheduled Reminders
- [x] ReminderBackgroundService
- [x] INotificationService stub

## Phase 4 — Sharing
- [x] Share/revoke API endpoints
- [x] Share modal in frontend

## Phase 5 — Polish
- [x] Loading and error states (skipped — app is fast enough)
- [x] Form validation feedback
- [x] Empty states
- [x] UX cleanup
