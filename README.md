# PRN232 Lab 1 - LMS REST API

- Student: SE193324 - Vũ Đức Hùng - SE1920
- Database: Microsoft SQL Server 2022
- Architecture: `PRN232.LMS.API` → `PRN232.LMS.Services` → `PRN232.LMS.Repositories`

## Run

```powershell
docker compose up --build -d
```

Swagger: <http://localhost:8080/swagger>  
Health check: <http://localhost:8080/health>

The application automatically retries the database connection, applies EF Core migrations, and seeds data during startup. Seed data contains 5 semesters, 10 subjects, 20 courses, 50 students, and 500 enrollments. Known test IDs for all five resources start at `1`.

All collection endpoints support `search`, `sort`, `page`, `size`, `fields`, and `expand`. Stop and remove the Lab 1 containers with `docker compose down -v`.

Known limitations: none.
