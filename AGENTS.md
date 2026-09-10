# Project Instructions

## Architecture

- Follow the existing API -> Service -> Repository architecture.
- Keep controllers in `PRN232.LMS.API`.
- Keep business logic and request/response models in `PRN232.LMS.Service`.
- Keep Entity Framework Core entities, mappings, migrations, and database access in `PRN232.LMS.Repository`.
- Do not change the existing architecture unless the user explicitly requests it.

## Database

- Use `AppDBContext` from `PRN232.LMS.Repository`.
- Read `DefaultConnection` from `PRN232.LMS.API/appsettings.json`.
- Never hard-code connection strings.
- Use asynchronous Entity Framework Core methods.
- Use `AsNoTracking()` for read-only queries.
- Order queries before applying `Skip()` and `Take()`.
- Do not create or remove migrations unless entities or EF Core mappings change.

## Coding Conventions

- Preserve the existing solution, namespace, and folder structure.
- Use dependency injection for services and `AppDBContext`.
- Use camelCase for parameters and local variables.
- Validate paging parameters before querying the database.
- Return DTOs instead of exposing EF Core entities directly from controllers.
- Avoid unrelated refactoring while completing a requested feature.

## Verification

- Run `dotnet build PRN232.LMS.sln` after changing C# code.
- When API behavior changes, verify the relevant endpoint and response payload.
- Clearly report any remaining build warnings or errors.
