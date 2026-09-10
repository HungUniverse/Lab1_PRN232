using Microsoft.EntityFrameworkCore;
using PRN232.Lab1.Repository.Entity;

namespace PRN232.Lab1.Repository;

public class AppDBContext : DbContext
{
    public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
    {
    }

    public DbSet<Semester> Semesters => Set<Semester>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureSemester(modelBuilder);
        ConfigureStudent(modelBuilder);
        ConfigureSubject(modelBuilder);
        ConfigureCourse(modelBuilder);
        ConfigureEnrollment(modelBuilder);
        SeedData(modelBuilder);
    }

    private static void ConfigureSemester(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Semester>(builder =>
        {
            builder.ToTable("Semester", table =>
                table.HasCheckConstraint("CK_Semester_DateRange", "[EndDate] > [StartDate]"));

            builder.HasKey(x => x.SemesterId);
            builder.Property(x => x.SemesterId).ValueGeneratedOnAdd();
            builder.Property(x => x.SemesterName)
                .HasColumnType("nvarchar(100)")
                .HasMaxLength(100)
                .IsRequired();
            builder.Property(x => x.StartDate)
                .HasColumnType("datetime")
                .IsRequired();
            builder.Property(x => x.EndDate)
                .HasColumnType("datetime")
                .IsRequired();

            builder.HasIndex(x => x.SemesterName)
                .IsUnique()
                .HasDatabaseName("UX_Semester_SemesterName");
        });
    }

    private static void ConfigureStudent(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>(builder =>
        {
            builder.ToTable("Student");
            builder.HasKey(x => x.StudentId);
            builder.Property(x => x.StudentId).ValueGeneratedOnAdd();
            builder.Property(x => x.FullName)
                .HasColumnType("nvarchar(100)")
                .HasMaxLength(100)
                .IsRequired();
            builder.Property(x => x.Email)
                .HasColumnType("varchar(100)")
                .HasMaxLength(100)
                .IsUnicode(false)
                .IsRequired();
            builder.Property(x => x.DateOfBirth)
                .HasColumnType("datetime")
                .IsRequired();

            builder.HasIndex(x => x.Email)
                .IsUnique()
                .HasDatabaseName("UX_Student_Email");
        });
    }

    private static void ConfigureSubject(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Subject>(builder =>
        {
            builder.ToTable("Subject", table =>
                table.HasCheckConstraint("CK_Subject_Credit", "[Credit] BETWEEN 1 AND 10"));

            builder.HasKey(x => x.SubjectId);
            builder.Property(x => x.SubjectId).ValueGeneratedOnAdd();
            builder.Property(x => x.SubjectCode)
                .HasColumnType("varchar(20)")
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsRequired();
            builder.Property(x => x.SubjectName)
                .HasColumnType("nvarchar(100)")
                .HasMaxLength(100)
                .IsRequired();
            builder.Property(x => x.Credit).IsRequired();

            builder.HasIndex(x => x.SubjectCode)
                .IsUnique()
                .HasDatabaseName("UX_Subject_SubjectCode");
        });
    }

    private static void ConfigureCourse(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Course>(builder =>
        {
            builder.ToTable("Course");
            builder.HasKey(x => x.CourseId);
            builder.Property(x => x.CourseId).ValueGeneratedOnAdd();
            builder.Property(x => x.CourseName)
                .HasColumnType("nvarchar(100)")
                .HasMaxLength(100)
                .IsRequired();
            builder.Property(x => x.SemesterId).IsRequired();
            builder.Property(x => x.SubjectId).IsRequired();

            builder.HasOne(x => x.Semester)
                .WithMany(x => x.Courses)
                .HasForeignKey(x => x.SemesterId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Course_Semester");

            builder.HasOne(x => x.Subject)
                .WithMany(x => x.Courses)
                .HasForeignKey(x => x.SubjectId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Course_Subject");

            builder.HasIndex(x => new { x.SemesterId, x.SubjectId, x.CourseName })
                .IsUnique()
                .HasDatabaseName("UX_Course_Semester_Subject_CourseName");
        });
    }

    private static void ConfigureEnrollment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Enrollment>(builder =>
        {
            builder.ToTable("Enrollment", table =>
                table.HasCheckConstraint(
                    "CK_Enrollment_Status",
                    "[Status] IN ('Active', 'Completed', 'Cancelled')"));

            builder.HasKey(x => x.EnrollmentId);
            builder.Property(x => x.EnrollmentId).ValueGeneratedOnAdd();
            builder.Property(x => x.StudentId).IsRequired();
            builder.Property(x => x.CourseId).IsRequired();
            builder.Property(x => x.EnrollDate)
                .HasColumnType("datetime")
                .IsRequired();
            builder.Property(x => x.Status)
                .HasColumnType("varchar(20)")
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsRequired();

            builder.HasOne(x => x.Student)
                .WithMany(x => x.Enrollments)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Enrollment_Student");

            builder.HasOne(x => x.Course)
                .WithMany(x => x.Enrollments)
                .HasForeignKey(x => x.CourseId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Enrollment_Course");

            builder.HasIndex(x => new { x.StudentId, x.CourseId })
                .IsUnique()
                .HasDatabaseName("UX_Enrollment_Student_Course");
        });
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var semesters = new[]
        {
            new Semester { SemesterId = 1, SemesterName = "Spring 2025", StartDate = new DateTime(2025, 1, 2), EndDate = new DateTime(2025, 4, 30) },
            new Semester { SemesterId = 2, SemesterName = "Summer 2025", StartDate = new DateTime(2025, 5, 5), EndDate = new DateTime(2025, 8, 15) },
            new Semester { SemesterId = 3, SemesterName = "Fall 2025", StartDate = new DateTime(2025, 9, 1), EndDate = new DateTime(2025, 12, 20) },
            new Semester { SemesterId = 4, SemesterName = "Spring 2026", StartDate = new DateTime(2026, 1, 5), EndDate = new DateTime(2026, 4, 30) },
            new Semester { SemesterId = 5, SemesterName = "Summer 2026", StartDate = new DateTime(2026, 5, 4), EndDate = new DateTime(2026, 8, 15) }
        };

        var subjectCodes = new[]
        {
            "PRN232", "PRN221", "SWT301", "SWP391", "DBI202",
            "CSD201", "PRO192", "MAS291", "OSG202", "IOT102"
        };
        var subjects = subjectCodes.Select((code, index) => new Subject
        {
            SubjectId = index + 1,
            SubjectCode = code,
            SubjectName = $"Subject {code}",
            Credit = index % 2 == 0 ? 3 : 4
        }).ToArray();

        var students = Enumerable.Range(1, 50).Select(index => new Student
        {
            StudentId = index,
            FullName = $"Student {index:00}",
            Email = $"student{index:00}@fpt.edu.vn",
            DateOfBirth = new DateTime(2002 + index % 5, index % 12 + 1, index % 27 + 1)
        }).ToArray();

        var courses = Enumerable.Range(1, 20).Select(index => new Course
        {
            CourseId = index,
            SubjectId = (index - 1) % subjects.Length + 1,
            CourseName = $"{subjects[(index - 1) % subjects.Length].SubjectCode} - Class {index:00}",
            SemesterId = (index - 1) % semesters.Length + 1
        }).ToArray();

        var statuses = new[] { "Active", "Completed", "Cancelled" };
        var enrollments = new List<Enrollment>(500);
        var enrollmentId = 1;

        for (var studentId = 1; studentId <= students.Length; studentId++)
        {
            for (var offset = 0; offset < 10; offset++)
            {
                var courseId = ((studentId - 1) * 7 + offset * 3) % courses.Length + 1;
                var semesterId = courses[courseId - 1].SemesterId;
                var semesterStart = semesters[semesterId - 1].StartDate;

                enrollments.Add(new Enrollment
                {
                    EnrollmentId = enrollmentId,
                    StudentId = studentId,
                    CourseId = courseId,
                    EnrollDate = semesterStart.AddDays((studentId + offset) % 30),
                    Status = statuses[(studentId + offset) % statuses.Length]
                });
                enrollmentId++;
            }
        }

        modelBuilder.Entity<Semester>().HasData(semesters);
        modelBuilder.Entity<Subject>().HasData(subjects);
        modelBuilder.Entity<Student>().HasData(students);
        modelBuilder.Entity<Course>().HasData(courses);
        modelBuilder.Entity<Enrollment>().HasData(enrollments);
    }
}
