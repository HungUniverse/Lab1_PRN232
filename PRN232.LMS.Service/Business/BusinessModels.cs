namespace PRN232.Lab1.Service.Business;

public class StudentBusinessModel
{
    public int StudentId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime DateOfBirth { get; init; }
    public IReadOnlyCollection<StudentEnrollmentBusinessModel> Enrollments { get; init; } = Array.Empty<StudentEnrollmentBusinessModel>();
}

public class StudentEnrollmentBusinessModel
{
    public int EnrollmentId { get; init; }
    public int CourseId { get; init; }
    public string CourseName { get; init; } = string.Empty;
    public DateTime EnrollDate { get; init; }
    public string Status { get; init; } = string.Empty;
}

public class SemesterBusinessModel
{
    public int SemesterId { get; init; }
    public string SemesterName { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public IReadOnlyCollection<CourseSummaryBusinessModel> Courses { get; init; } = Array.Empty<CourseSummaryBusinessModel>();
}

public class SubjectBusinessModel
{
    public int SubjectId { get; init; }
    public string SubjectCode { get; init; } = string.Empty;
    public string SubjectName { get; init; } = string.Empty;
    public int Credit { get; init; }
    public IReadOnlyCollection<CourseSummaryBusinessModel> Courses { get; init; } = Array.Empty<CourseSummaryBusinessModel>();
}

public class CourseBusinessModel
{
    public int CourseId { get; init; }
    public string CourseName { get; init; } = string.Empty;
    public int SemesterId { get; init; }
    public string? SemesterName { get; init; }
    public int SubjectId { get; init; }
    public string? SubjectCode { get; init; }
    public string? SubjectName { get; init; }
    public IReadOnlyCollection<CourseEnrollmentBusinessModel> Enrollments { get; init; } = Array.Empty<CourseEnrollmentBusinessModel>();
}

public class CourseSummaryBusinessModel
{
    public int CourseId { get; init; }
    public string CourseName { get; init; } = string.Empty;
    public int SemesterId { get; init; }
    public int SubjectId { get; init; }
}

public class CourseEnrollmentBusinessModel
{
    public int EnrollmentId { get; init; }
    public int StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public DateTime EnrollDate { get; init; }
    public string Status { get; init; } = string.Empty;
}

public class EnrollmentBusinessModel
{
    public int EnrollmentId { get; init; }
    public int StudentId { get; init; }
    public int CourseId { get; init; }
    public DateTime EnrollDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public StudentSummaryBusinessModel? Student { get; init; }
    public EnrollmentCourseBusinessModel? Course { get; init; }
}

public class StudentSummaryBusinessModel
{
    public int StudentId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}

public class EnrollmentCourseBusinessModel
{
    public int CourseId { get; init; }
    public string CourseName { get; init; } = string.Empty;
    public int SemesterId { get; init; }
    public string SemesterName { get; init; } = string.Empty;
    public int SubjectId { get; init; }
    public string SubjectCode { get; init; } = string.Empty;
    public string SubjectName { get; init; } = string.Empty;
}
