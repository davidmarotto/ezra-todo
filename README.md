# Todo App

A simple todo list web application with a Vue 3 frontend and ASP.NET Core REST API backend.

## Architecture

- **Frontend** (`/frontend`): Vue 3 SPA served by Vite dev server, communicates with the backend via HTTP
- **Backend** (`/backend`): ASP.NET Core Web API exposing REST endpoints for todo CRUD operations

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
