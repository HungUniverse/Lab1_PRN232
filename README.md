# PRN232 Lab 1 - LMS REST API

ASP.NET Core 8 REST API for a Learning Management System using the existing
API -> Service -> Repository architecture.

## Run locally

1. Ensure SQL Server Express is available as `.\\SQLEXPRESS`.
2. From the solution directory, run:

   ```powershell
   dotnet ef database update --project PRN232.LMS.Repository --startup-project PRN232.LMS.API
   dotnet run --project PRN232.LMS.API
   ```

3. Open the Swagger URL printed by the application.

## Run with Docker Compose

```powershell
docker compose up --build
```

Swagger is available at <http://localhost:8080/swagger>.

Stop the containers with:

```powershell
docker compose down
```

Use `docker compose down -v` only when you intentionally want to delete the
SQL Server data volume.

## Resources

- `/api/students`
- `/api/semesters`
- `/api/subjects`
- `/api/courses`
- `/api/enrollments`

Every collection endpoint accepts `search`, `sort`, `page`, `size`, `fields`,
and `expand`. For example:

```text
GET /api/students?search=student&sort=fullName,-dateOfBirth&page=2&size=10&fields=studentId,fullName,email&expand=enrollments
GET /api/enrollments?search=active&sort=-enrollDate&page=1&size=20&fields=enrollmentId,status&expand=student,course
```
