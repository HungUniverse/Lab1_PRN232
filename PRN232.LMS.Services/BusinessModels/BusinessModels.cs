namespace PRN232.Lab1.Service.Business;

public class ListQueryModel
{
    public string? Search { get; init; }
    public string? Sort { get; init; }
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 10;
    public string? Fields { get; init; }
    public string? Expand { get; init; }
}

public class CreateStudentModel
{
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime DateOfBirth { get; init; }
}

public class UpdateStudentModel : CreateStudentModel;

public class StudentModel
{
    public int StudentId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime DateOfBirth { get; init; }
    public IReadOnlyCollection<StudentEnrollmentModel> Enrollments { get; init; } = Array.Empty<StudentEnrollmentModel>();
}

public class StudentEnrollmentModel
{
    public int EnrollmentId { get; init; }
    public int CourseId { get; init; }
    public string CourseName { get; init; } = string.Empty;
    public DateTime EnrollDate { get; init; }
    public string Status { get; init; } = string.Empty;
}

public class SemesterModel
{
    public int SemesterId { get; init; }
    public string SemesterName { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public IReadOnlyCollection<CourseSummaryModel> Courses { get; init; } = Array.Empty<CourseSummaryModel>();
}

public class SubjectModel
{
    public int SubjectId { get; init; }
    public string SubjectCode { get; init; } = string.Empty;
    public string SubjectName { get; init; } = string.Empty;
    public int Credit { get; init; }
    public IReadOnlyCollection<CourseSummaryModel> Courses { get; init; } = Array.Empty<CourseSummaryModel>();
}

public class CourseModel
{
    public int CourseId { get; init; }
    public string CourseName { get; init; } = string.Empty;
    public int SemesterId { get; init; }
    public string? SemesterName { get; init; }
    public int SubjectId { get; init; }
    public string? SubjectCode { get; init; }
    public string? SubjectName { get; init; }
    public IReadOnlyCollection<CourseEnrollmentModel> Enrollments { get; init; } = Array.Empty<CourseEnrollmentModel>();
}

public class CourseSummaryModel
{
    public int CourseId { get; init; }
    public string CourseName { get; init; } = string.Empty;
    public int SemesterId { get; init; }
    public int SubjectId { get; init; }
}

public class CourseEnrollmentModel
{
    public int EnrollmentId { get; init; }
    public int StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public DateTime EnrollDate { get; init; }
    public string Status { get; init; } = string.Empty;
}

public class EnrollmentModel
{
    public int EnrollmentId { get; init; }
    public int StudentId { get; init; }
    public int CourseId { get; init; }
    public DateTime EnrollDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public StudentSummaryModel? Student { get; init; }
    public EnrollmentCourseModel? Course { get; init; }
}

public class StudentSummaryModel
{
    public int StudentId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}

public class EnrollmentCourseModel
{
    public int CourseId { get; init; }
    public string CourseName { get; init; } = string.Empty;
    public int SemesterId { get; init; }
    public string SemesterName { get; init; } = string.Empty;
    public int SubjectId { get; init; }
    public string SubjectCode { get; init; } = string.Empty;
    public string SubjectName { get; init; } = string.Empty;
}
