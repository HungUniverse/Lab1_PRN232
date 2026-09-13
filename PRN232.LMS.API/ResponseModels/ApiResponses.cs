using System.Text.Json.Serialization;

namespace PRN232.Lab1.API.ResponseModels;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public IReadOnlyCollection<string>? Errors { get; init; }

    public static ApiResponse<T> Succeeded(T data, string message) => new()
    {
        Success = true,
        Message = message,
        Data = data,
        Errors = null
    };

    public static ApiResponse<T> Failed(
        string message,
        IReadOnlyCollection<string>? errors = null) => new()
    {
        Success = false,
        Message = message,
        Data = default,
        Errors = errors ?? new[] { message }
    };
}

public sealed class PagedApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public IReadOnlyCollection<T> Data { get; init; } = Array.Empty<T>();
    public IReadOnlyCollection<string>? Errors { get; init; }
    public PaginationResponse Pagination { get; init; } = new();
}

public sealed class PaginationResponse
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages { get; init; }
}

public sealed class HealthResponse
{
    public string Status { get; init; } = string.Empty;
}

public class StudentResponse
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? StudentId { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? FullName { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? Email { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public DateTime? DateOfBirth { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<StudentEnrollmentResponse>? Enrollments { get; init; }
}

public sealed class StudentDetailResponse : StudentResponse;

public sealed class StudentEnrollmentResponse
{
    public int EnrollmentId { get; init; }
    public int CourseId { get; init; }
    public string CourseName { get; init; } = string.Empty;
    public DateTime EnrollDate { get; init; }
    public string Status { get; init; } = string.Empty;
}

public class SemesterResponse
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? SemesterId { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? SemesterName { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public DateTime? StartDate { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public DateTime? EndDate { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<CourseSummaryResponse>? Courses { get; init; }
}

public sealed class SemesterDetailResponse : SemesterResponse;

public class SubjectResponse
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? SubjectId { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? SubjectCode { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? SubjectName { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? Credit { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<CourseSummaryResponse>? Courses { get; init; }
}

public sealed class SubjectDetailResponse : SubjectResponse;

public sealed class CourseSummaryResponse
{
    public int CourseId { get; init; }
    public string CourseName { get; init; } = string.Empty;
    public int SemesterId { get; init; }
    public int SubjectId { get; init; }
}

public class CourseResponse
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? CourseId { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? CourseName { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? SemesterId { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? SubjectId { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public SemesterSummaryResponse? Semester { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public SubjectSummaryResponse? Subject { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<CourseEnrollmentResponse>? Enrollments { get; init; }
}

public sealed class CourseDetailResponse : CourseResponse;

public sealed class SemesterSummaryResponse
{
    public int SemesterId { get; init; }
    public string SemesterName { get; init; } = string.Empty;
}

public sealed class SubjectSummaryResponse
{
    public int SubjectId { get; init; }
    public string SubjectCode { get; init; } = string.Empty;
    public string SubjectName { get; init; } = string.Empty;
}

public sealed class CourseEnrollmentResponse
{
    public int EnrollmentId { get; init; }
    public int StudentId { get; init; }
    public string StudentName { get; init; } = string.Empty;
    public DateTime EnrollDate { get; init; }
    public string Status { get; init; } = string.Empty;
}

public class EnrollmentResponse
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? EnrollmentId { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? StudentId { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public int? CourseId { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public DateTime? EnrollDate { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? Status { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public StudentSummaryResponse? Student { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public EnrollmentCourseResponse? Course { get; init; }
}

public sealed class EnrollmentDetailResponse : EnrollmentResponse;

public sealed class StudentSummaryResponse
{
    public int StudentId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}

public sealed class EnrollmentCourseResponse
{
    public int CourseId { get; init; }
    public string CourseName { get; init; } = string.Empty;
    public int SemesterId { get; init; }
    public string SemesterName { get; init; } = string.Empty;
    public int SubjectId { get; init; }
    public string SubjectCode { get; init; } = string.Empty;
    public string SubjectName { get; init; } = string.Empty;
}
