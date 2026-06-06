# TodoTaskManagement

A full-stack Kanban task manager. Drag cards between columns, archive completed work, and manage tasks across sessions with JWT-secured accounts.

**Stack:** .NET 10 Web API · EF Core + PostgreSQL · React + Vite + TypeScript · Tailwind CSS · TanStack Query · @dnd-kit

---

## Setup

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)

### Backend

**1. Generate a JWT secret** (run in PowerShell):

```powershell
[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Maximum 256 }))
```

**2. Create `TodoTaskManagement/TodoTaskManagement/appsettings.Development.json`** with the generated secret:

```json
{
  "Jwt": {
    "Secret": "<your-generated-secret>",
    "Issuer": "TodoTaskManagement",
    "Audience": "TodoTaskManagement",
    "AccessTokenExpiryMinutes": 60,
    "RefreshTokenExpiryDays": 7
  }
}
```

See `appsettings.json` for the full configuration reference.

**3. Run the API:**

```bash
cd TodoTaskManagement
dotnet restore
dotnet run
```

The API starts on `http://localhost:5268`. The SQLite database (`todotasks.db`) is created automatically on first run.

### Frontend

```bash
cd TodoTaskManagementUI
npm install
npm run dev
```

The Vite dev server starts on `http://localhost:5173` and proxies all `/api/*` requests to the backend — no CORS configuration needed and HttpOnly cookies work out of the box.

Open `http://localhost:5173`, sign up for an account, and start adding tasks.

---

## Architecture

Clean architecture across four projects:

| Project | Role |
|---|---|
| `TodoTaskManagement.Domain` | Entity models only (`User`, `Task`, `OAuthToken`, `UserTokens`) |
| `TodoTaskManagement.Infrastructure` | EF Core, JWT, BCrypt — repos and services alongside their interfaces |
| `TodoTaskManagement.Application` | Business logic (`AuthService`, `TaskService`) |
| `TodoTaskManagement` (API) | Controllers, request/response models, global exception middleware |

---

## Assumptions

These are the non-obvious decisions baked into the implementation.

### Auth

**HttpOnly cookies over `Authorization` headers.** Both access and refresh tokens are stored in HttpOnly cookies set by the server. JavaScript never sees the tokens. This eliminates XSS token theft at the cost of CSRF exposure, which `SameSite=Strict` neutralises. The trade-off favours XSS resistance because the app is a single-origin SPA.

**Stateless JWTs — no server-side token store.** Neither the access token nor the refresh token is persisted in the database. True revocation (blocklist) is out of scope. A stolen refresh token stays valid for up to 7 days. Access tokens expire in 60 minutes. This is an accepted trade-off for a take-home scope.

**No JS-readable auth flag.** There is no `isAuthenticated` boolean in localStorage or React state. The app probes auth state by attempting `GET /api/tasks` on first render. A 401 that survives a refresh attempt redirects to `/login`. This is the only correct approach when tokens are HttpOnly — any JS-readable flag would be a stale proxy for the real cookie state.

### API Design

**Single `TaskApi` model for create, update, and response.** All fields are nullable. Validation (`IValidatableObject`) enforces which fields are required for each operation. This avoids a `Requests/Responses/` folder split for what is effectively the same shape across all three uses.

**`Status` is always required from the client.** There is no server-side default for status on update. A PUT that omits `Status` returns 400. This eliminates the ambiguity of a silent `?? Todo` default that would overwrite an existing status on a partial update.

**`CreatedAt` is always server-set.** The client-supplied value is ignored. On updates, `TaskRepository` restores the original DB value via the EF Core `ChangeTracker` before writing, so the creation timestamp can never be overwritten.

### Frontend

**Column position is the canonical status.** The task edit form does not expose a status dropdown. The column the card lives in is the status — showing both creates a conflict case. Status is passed through transparently when saving.

**New tasks always start as `Todo`.** There is no way to create a card directly into `InProgress` or `Done`. The "Add" button always inserts at the top of the Todo column.

**Dates are local on the client, UTC at the boundary.** The date picker, display formatting, and validation all work in the browser's local timezone throughout. Conversion to a UTC ISO string happens exactly once — at the moment a value is sent to the backend. The backend stores and processes all dates in UTC. On read, the client parses dates using the local-date constructor (not `.toISOString().slice(0,10)`) to avoid off-by-one day errors in negative-offset timezones.

**Optimistic updates on every mutation.** Create, update, delete, move, and archive-all all apply the change to the TanStack Query cache before the network request completes. On failure the cache is rolled back and a toast is shown. Without optimistic updates, drag-and-drop would snap cards back to their origin column on every move while the PUT is in-flight.

**Query cache is cleared on logout.** Without this, a second user logging in on the same tab would briefly see the previous user's tasks from cache before a refetch completed.

---

## Scalability

The current implementation uses SQLite, which is appropriate for a take-home scope but not for production. An enterprise application should use PostgreSQL from the start.

**Switch to PostgreSQL.** PostgreSQL supports true concurrent writes from multiple processes, row-level locking, and read replicas. The EF Core Fluent API configuration is database-agnostic — the only change is the provider package and connection string. PostgreSQL can scale vertically by running on more powerful machines, and it can scale horizontally through read replicas and connection pooling.

**Scale API instances horizontally.** Because auth is stateless (no server-side token store, no in-memory session), adding more API instances behind a load balancer is straightforward — any instance can validate any JWT.

---

## Future Improvements

**Column-coloured task cards.** Cards in different columns would have a distinct background or left-border accent colour — blue for Todo, yellow for In Progress, green for Done. This gives instant visual status at a glance without reading the column header, which matters when a board is dense.

**Task grouping within a column.** Tasks could be grouped by a user-defined label or tag, preventing a long column from becoming an undifferentiated scroll.

**Confirmation dialog on delete.** Currently a card is permanently deleted on a single click with no undo. A brief confirmation or a short-window undo toast would prevent accidental data loss.
