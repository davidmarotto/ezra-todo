# Architecture Notes

## .NET / ASP.NET Core

ASP.NET Core is explicit where Rails is implicit. Dependencies, middleware, and routes are all wired up intentionally in `Program.cs` rather than inferred by convention. The architecture is similar to Spring Boot in Java.

| .NET | Rails equivalent | Notes |
|---|---|---|
| `TodoApi.csproj` | `Gemfile` | Declares dependencies; NuGet = RubyGems |
| `Program.cs` | `config/application.rb` + `config/routes.rb` | Single file for DI registration, middleware, and routing |
| `Controllers/` | `app/controllers/` | Same concept; inherit from `ControllerBase` instead of `ApplicationController` |
| `Models/` | `app/models/` | Plain data classes only — no business logic or querying |
| `Data/AppDbContext.cs` | ActiveRecord | EF Core ORM; migrations work similarly (`dotnet ef migrations add`) |
| `DTOs/` | `strong_parameters` + serializers | Define request/response shapes; decouple API contract from DB model |
| `Services/` | Service objects (optional in Rails) | Standard home for business logic; kept out of controllers and models |
| `appsettings.json` | `config/database.yml` + env files | `appsettings.Development.json` overrides base config locally |
| `BackgroundServices/` | Sidekiq | Long-running background work inside the same process; no separate worker needed |

**Dependency Injection** is the biggest conceptual shift from Rails. Rather than class-level methods and globals, dependencies are declared in constructors and supplied automatically by the framework. Services are registered once in `Program.cs` (`builder.Services.AddScoped<IMyService, MyService>()`) and injected wherever they're needed.

---

## Vue

Vue is a component-based frontend framework. The UI is built from self-contained components, each managing their own template, logic, and styles. When data changes, the UI updates automatically — no manual DOM manipulation.

| File / Directory | Rails equivalent | Notes |
|---|---|---|
| `index.html` | `app/views/layouts/application.html.erb` | The single HTML page; Vue mounts into a `<div id="app">` here |
| `main.js` | `config/application.rb` | Entry point; bootstraps the app and registers Pinia and Vue Router |
| `vite.config.js` | Asset pipeline / webpack | Dev server and build tool; handles hot reload in development |
| `App.vue` | Layout template | Root component; wraps all views, typically holds the `<RouterView>` |
| `router/index.js` | `config/routes.rb` | Defines client-side routes; navigation happens in the browser without page reloads |
| `stores/auth.js` | Session / global state | Pinia store; holds JWT token and current user, accessible from any component |
| `views/` | `app/views/` (page-level) | Page components tied to routes — `LoginView.vue`, `HomeView.vue`, etc. |
| `components/` | Rails partials (`_partial.html.erb`) | Reusable UI pieces used inside views — `TodoItem.vue`, `AddTodoForm.vue`, etc. |
| `services/api.js` | `app/services/` | Fetch wrapper that attaches the JWT `Authorization` header to every API request |

**Single File Components (`.vue` files)** are the biggest structural difference from Rails views. Each file has three sections:

```vue
<template>
  <!-- HTML markup for this component -->
</template>

<script setup>
  // JavaScript logic — data, computed values, event handlers
</script>

<style scoped>
  /* CSS scoped to this component only */
</style>
```

**Props and events** are how components communicate — a parent passes data down to a child via props (like locals in a Rails partial), and the child emits events back up to the parent.

**Reactivity** is handled automatically. Variables declared with `ref()` or `reactive()` are tracked by Vue — when they change, any part of the template that uses them re-renders.

---

## SQLite / Database Tools

The database is a single file (`backend/todo.db`) created automatically when migrations are applied.

**Command line:**
```bash
sqlite3 backend/todo.db
.tables            # list all tables
.schema Users      # show create statement for a table
.quit              # exit
```
