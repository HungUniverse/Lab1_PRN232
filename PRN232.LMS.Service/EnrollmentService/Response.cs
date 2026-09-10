namespace PRN232.Lab1.Service.EnrollmentService;

public class Response
{
    public class EnrollmentResponse
    {
        public int EnrollmentId { get; init; }
        public int StudentId { get; init; }
        public int CourseId { get; init; }
        public DateTime EnrollDate { get; init; }
        public string Status { get; init; } = string.Empty;
    }

    public class EnrollmentDetailResponse : EnrollmentResponse
    {
        public StudentResponse Student { get; init; } = new();
        public CourseResponse Course { get; init; } = new();
    }

    public class StudentResponse
    {
        public int StudentId { get; init; }
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
    }

    public class CourseResponse
    {
        public int CourseId { get; init; }
        public string CourseName { get; init; } = string.Empty;
        public int SemesterId { get; init; }
        public string SemesterName { get; init; } = string.Empty;
        public int SubjectId { get; init; }
        public string SubjectCode { get; init; } = string.Empty;
        public string SubjectName { get; init; } = string.Empty;
    }
}
