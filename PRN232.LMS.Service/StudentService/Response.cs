namespace PRN232.Lab1.Service.StudentService;

public class Response
{
    public class StudentResponse
    {
        public int StudentId { get; init; }
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public DateTime DateOfBirth { get; init; }
    }

    public class StudentDetailResponse : StudentResponse
    {
        public IReadOnlyCollection<StudentEnrollmentResponse> Enrollments { get; init; } = Array.Empty<StudentEnrollmentResponse>();
    }

    public class StudentEnrollmentResponse
    {
        public int EnrollmentId { get; init; }
        public int CourseId { get; init; }
        public string CourseName { get; init; } = string.Empty;
        public DateTime EnrollDate { get; init; }
        public string Status { get; init; } = string.Empty;
    }
}
