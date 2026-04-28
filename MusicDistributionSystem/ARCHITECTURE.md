# Clean Architecture Layout

This project now follows a practical Clean Architecture layout inside the existing ASP.NET MVC application.

## Layers

- `Domain`
  - Core business entities, enums, and constants.
  - No dependency on MVC, EF Core, or infrastructure concerns.
- `Application`
  - Use-case logic, DTOs, and contracts.
  - Services coordinate workflows through abstractions, not concrete persistence details.
- `Infrastructure`
  - EF Core persistence, repositories, configuration, logging, notifications, and security implementations.
  - This layer depends on `Application` contracts and `Domain` types.
- `Presentation`
  - MVC-facing presentation models.
  - `Controllers`, `Views`, and `wwwroot` remain the web entry point and UI layer.

## Active Source Mapping

- `Domain/Entities`
- `Domain/Enums`
- `Domain/Constants`
- `Application/DTOs`
- `Application/Contracts`
- `Application/Services`
- `Infrastructure/Persistence`
- `Infrastructure/Persistence/Repositories`
- `Infrastructure/Configuration`
- `Infrastructure/Logging`
- `Infrastructure/Notifications`
- `Infrastructure/Security`
- `Controllers`
- `Views`
- `wwwroot`

## Request Flow

1. A request enters through an MVC `Controller`.
2. The controller delegates to an `Application` service.
3. The service uses `Application` contracts for repositories, notifications, and security helpers.
4. `Infrastructure` provides the concrete implementations.
5. `Domain` types remain the core business model shared across the workflow.

## Guardrails

- Keep controllers thin and HTTP-focused.
- Keep business rules in `Application/Services`.
- Keep EF Core and external integrations in `Infrastructure`.
- Keep `Domain` free from framework-specific code.
- Treat the old flat folders as legacy only; the new layered folders are the active architecture.
