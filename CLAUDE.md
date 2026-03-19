# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

CollectorShop is a full-stack e-commerce platform for collectible items. The backend is a .NET 9 ASP.NET Core Web API with SQL Server. The frontend is an Angular 21 SPA. Both repos deploy to the same infrastructure (Azure VM at `api.maalsikube.dev` / `maalsikube.dev`, and a school K8s cluster).

## Repositories

- **Backend** (`CollectorShop.Back/`): This repo — .NET 9 API
- **Frontend** (`CollectorShop/`): Angular 21 SPA at `../CollectorShop`

---

## Backend (CollectorShop.Back)

### Commands

```bash
# Build
dotnet build CollectorShopPoc2.sln

# Run the API
dotnet run --project CollectorShop.API

# Lint (check formatting)
dotnet format CollectorShopPoc2.sln --verify-no-changes --severity warn

# Fix formatting
dotnet format CollectorShopPoc2.sln

# Run tests
dotnet test CollectorShopPoc2.sln

# EF Core migrations
dotnet ef migrations add <MigrationName> --project CollectorShop.Infrastructure --startup-project CollectorShop.API
dotnet ef database update --project CollectorShop.Infrastructure --startup-project CollectorShop.API
```

### Architecture

Three-layer Clean Architecture solution (`CollectorShopPoc2.sln`):

- **CollectorShop.Domain** — Entities, value objects (`Email`, `Address`, `PhoneNumber`), enums, domain events, and repository interfaces. No dependencies on other projects. Uses a generic `IRepository<T>` base with specialized interfaces per aggregate. Domain entities use rich models with behavior methods (e.g., `brand.Activate()`, `brand.UpdateDetails()`).

- **CollectorShop.Infrastructure** — EF Core implementation (SQL Server), ASP.NET Identity (`ApplicationUser`), repository implementations, and services (email, payment, storage). Registers itself via `AddInfrastructure()` extension method. Uses `IUnitOfWork` pattern — all repositories are accessed through `IUnitOfWork` (e.g., `_unitOfWork.Brands.GetByIdAsync()`). EF configurations live in `Data/Configurations/`. Seed data uses embedded JSON files in `Data/Seeding/SeedData/`.

- **CollectorShop.API** — Controllers, DTOs, FluentValidation validators, MediatR setup, JWT auth, Swagger, rate limiting, Serilog logging. Registers itself via `AddApiServices()` extension method. Controllers use `IUnitOfWork` directly. All API routes follow `api/[controller]` convention. Admin-only endpoints use `[Authorize(Roles = "Admin")]`.

### Key Patterns

- **Unit of Work**: All data access goes through `IUnitOfWork` — do not inject individual repositories into controllers.
- **Validation**: FluentValidation with auto-validation — validators in `Validators/` are auto-discovered from the API assembly.
- **Auth**: JWT Bearer in production. Set `"BypassAuth": true` in appsettings to use `DevAuthHandler` which auto-authenticates as an admin user (dev only). JWT secret key is stored in user secrets (UserSecretsId: `CollectorShop-API-2024`).
- **Roles**: `Admin`, `Customer`, `Manager` — seeded on startup in `Program.cs`.

### CI/CD

GitLab CI (`.gitlab-ci.yml`): lint → build → test → security (vulnerable packages) → SonarCloud → Docker (Nexus + GitLab registry) → deploy (K8s school cluster or Azure VM). Build uses `Release` config with `/warnaserror`.

---

## Frontend (CollectorShop)

### Commands

```bash
# Dev server (http://localhost:4200)
npm start

# Build for production
npm run build

# Run Vitest unit tests
npm test

# Lint (ESLint + Angular ESLint)
npm run lint

# Generate a component
ng generate component features/<feature>/components/<name>
```

### Architecture

Angular 21 standalone app (no NgModules). All components use `imports: []` directly.

```
src/app/
├── core/        # Singleton services, guards, interceptors, models
├── shared/      # Reusable components (layout, navbar, footer, product-card), pipes, directives
├── features/    # Lazy-loaded feature routes: home, auth, catalog, product-detail, cart, checkout, user, admin
└── state/       # Placeholder for centralized state (currently unused)
```

- **Routing**: Standalone `Routes` arrays per feature (`*.routes.ts`), lazy-loaded via `loadChildren`. Public routes wrapped in `LayoutComponent` (header/footer). Admin routes are separate (no shared layout).
- **Guards**: `authGuard` (authenticated), `adminGuard` (Admin role), `guestGuard` (unauthenticated only).

### Key Patterns

- **Signals for state**: All services use Angular signals (`signal()`, `computed()`, `asReadonly()`) — no NgRx or external state library.
- **Standalone components only**: No `.module.ts` files. Import dependencies directly in `@Component({ imports: [...] })`.
- **Component prefix**: `app-` selector prefix.
- **Styling**: SCSS + Tailwind CSS 4. Use `styleUrl` (singular) in components.
- **i18n**: `@ngx-translate` with translation JSON files in `src/assets/i18n/` (en, fr, de, it).
- **Testing**: Vitest (not Jasmine/Karma). Test files colocated as `*.spec.ts`.

### API Integration

- **Base URL** configured per environment in `src/environments/`:
  - Dev: `http://localhost:5000/api`
  - Prod: `https://api.maalsikube.dev/api`
  - VM: `http://172.16.47.13/api`
- **Auth interceptor**: Adds `Authorization: Bearer <token>` to all requests.
- **Error interceptor**: Global HTTP error handling — extracts ASP.NET `ValidationProblemDetails`, auto-logouts on 401.
- **Token storage**: `localStorage` (accessToken, refreshToken, user JSON).

### CI/CD

Both GitHub Actions (`.github/workflows/ci-cd.yml`) and GitLab CI (`.gitlab-ci.yml`): lint → build → test → security (npm audit) → SonarCloud → Docker (multi-stage Node + Nginx) → Trivy scan → deploy. Dockerized with Nginx for SPA routing (`nginx.conf`).
