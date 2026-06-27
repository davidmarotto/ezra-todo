# Todo App

A multi-user todo list web application with a Vue 3 frontend and ASP.NET Core REST API backend.

## Features

- JWT-based user registration and login
- Create and manage multiple named todo lists
- Share lists with other users via email, with role-based permissions (Editor / Viewer)
- Create, edit, and delete todos within a list
- Mark todos complete/incomplete
- Optional due date per todo
- Filter todos by status (all / active / completed)
- Background reminder service: detects todos due within a configurable window and fires a notification event — currently logs to console, designed for easy swap to SMS/email

## Scope & Assumptions

- Auth uses JWT (no refresh tokens — acceptable for MVP)
- Sharing invite emails are stubbed via the same notification interface as reminders
- Reminders are triggered by a polling background service (not push/webhooks)
- No pagination — acceptable for MVP scale
- Password reset and email verification are out of scope

## What I'd add next

- JWT refresh tokens
- Email verification on registration
- Password reset flow
- Real notification delivery (e.g. SendGrid for email, Twilio for SMS)
- Due date recurrence
- Pagination and sorting on list/todo endpoints
- Frontend tests

## Architecture

- **Frontend** (`/frontend`): Vue 3 SPA served by Vite dev server, communicates with the backend via HTTP using JWT for auth
- **Backend** (`/backend`): ASP.NET Core Web API with JWT authentication, EF Core + SQLite for persistence

## Prerequisites

- [.NET 10 SDK](https://dot.net)
- [Node.js](https://nodejs.org) (v18+)

## Running the Backend

```bash
cd backend
dotnet run
```

API available at `http://localhost:5049` — API explorer at `http://localhost:5049/scalar/v1`

## Running the Frontend

```bash
cd frontend
npm install  # first time only
npm run dev
```

App available at `http://localhost:5173`
