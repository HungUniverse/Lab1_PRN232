namespace PRN232.Lab1.Service.CourseService;

public class Response
{
    public class CourseResponse
    {
        public int CourseId { get; init; }
        public string CourseName { get; init; } = string.Empty;
        public int SemesterId { get; init; }
        public int SubjectId { get; init; }
    }

    public class CourseDetailResponse : CourseResponse
    {
        public SemesterResponse Semester { get; init; } = new();
        public SubjectResponse Subject { get; init; } = new();
        public IReadOnlyCollection<EnrollmentResponse> Enrollments { get; init; } = Array.Empty<EnrollmentResponse>();
    }

    public class SemesterResponse
    {
        public int SemesterId { get; init; }
        public string SemesterName { get; init; } = string.Empty;
    }

    public class SubjectResponse
    {
        public int SubjectId { get; init; }
        public string SubjectCode { get; init; } = string.Empty;
        public string SubjectName { get; init; } = string.Empty;
    }

    public class EnrollmentResponse
    {
        public int EnrollmentId { get; init; }
        public int StudentId { get; init; }
        public string StudentName { get; init; } = string.Empty;
        public DateTime EnrollDate { get; init; }
        public string Status { get; init; } = string.Empty;
    }
}
