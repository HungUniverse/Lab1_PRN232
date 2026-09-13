using Microsoft.EntityFrameworkCore;
using PRN232.Lab1.Repository.Entity;

namespace PRN232.Lab1.Repository;

public static class DatabaseSeeder
{
    public static async Task SeedIfEmptyAsync(
        AppDBContext dbContext,
        CancellationToken cancellationToken = default)
    {
        if (!await dbContext.Semesters.AnyAsync(cancellationToken))
        {
            await dbContext.Semesters.AddRangeAsync(CreateSemesters(), cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (!await dbContext.Subjects.AnyAsync(cancellationToken))
        {
            await dbContext.Subjects.AddRangeAsync(CreateSubjects(), cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (!await dbContext.Students.AnyAsync(cancellationToken))
        {
            await dbContext.Students.AddRangeAsync(CreateStudents(), cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (!await dbContext.Courses.AnyAsync(cancellationToken))
        {
            var semesterIds = await dbContext.Semesters.OrderBy(item => item.SemesterId)
                .Select(item => item.SemesterId).Take(5).ToArrayAsync(cancellationToken);
            var subjects = await dbContext.Subjects.OrderBy(item => item.SubjectId)
                .Take(10).ToArrayAsync(cancellationToken);
            var courses = Enumerable.Range(1, 20).Select(index => new Course
            {
                SubjectId = subjects[(index - 1) % subjects.Length].SubjectId,
                CourseName = $"{subjects[(index - 1) % subjects.Length].SubjectCode} - Class {index:00}",
                SemesterId = semesterIds[(index - 1) % semesterIds.Length]
            });
            await dbContext.Courses.AddRangeAsync(courses, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (!await dbContext.Enrollments.AnyAsync(cancellationToken))
        {
            var studentIds = await dbContext.Students.OrderBy(item => item.StudentId)
                .Select(item => item.StudentId).Take(50).ToArrayAsync(cancellationToken);
            var courses = await dbContext.Courses.Include(item => item.Semester)
                .OrderBy(item => item.CourseId).Take(20).ToArrayAsync(cancellationToken);
            var statuses = new[] { "Active", "Completed", "Cancelled" };
            var enrollments = new List<Enrollment>(500);

            for (var studentIndex = 0; studentIndex < studentIds.Length; studentIndex++)
            {
                for (var offset = 0; offset < 10; offset++)
                {
                    var course = courses[(studentIndex * 7 + offset * 3) % courses.Length];
                    enrollments.Add(new Enrollment
                    {
                        StudentId = studentIds[studentIndex],
                        CourseId = course.CourseId,
                        EnrollDate = course.Semester.StartDate.AddDays((studentIndex + 1 + offset) % 30),
                        Status = statuses[(studentIndex + 1 + offset) % statuses.Length]
                    });
                }
            }

            await dbContext.Enrollments.AddRangeAsync(enrollments, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static Semester[] CreateSemesters() =>
    [
        new() { SemesterName = "Spring 2025", StartDate = new DateTime(2025, 1, 2), EndDate = new DateTime(2025, 4, 30) },
        new() { SemesterName = "Summer 2025", StartDate = new DateTime(2025, 5, 5), EndDate = new DateTime(2025, 8, 15) },
        new() { SemesterName = "Fall 2025", StartDate = new DateTime(2025, 9, 1), EndDate = new DateTime(2025, 12, 20) },
        new() { SemesterName = "Spring 2026", StartDate = new DateTime(2026, 1, 5), EndDate = new DateTime(2026, 4, 30) },
        new() { SemesterName = "Summer 2026", StartDate = new DateTime(2026, 5, 4), EndDate = new DateTime(2026, 8, 15) }
    ];

    private static Subject[] CreateSubjects()
    {
        var codes = new[] { "PRN232", "PRN221", "SWT301", "SWP391", "DBI202", "CSD201", "PRO192", "MAS291", "OSG202", "IOT102" };
        return codes.Select((code, index) => new Subject
        {
            SubjectCode = code,
            SubjectName = $"Subject {code}",
            Credit = index % 2 == 0 ? 3 : 4
        }).ToArray();
    }

    private static Student[] CreateStudents() => Enumerable.Range(1, 50).Select(index => new Student
    {
        FullName = $"Student {index:00}",
        Email = $"student{index:00}@fpt.edu.vn",
        DateOfBirth = new DateTime(2002 + index % 5, index % 12 + 1, index % 27 + 1)
    }).ToArray();
}
