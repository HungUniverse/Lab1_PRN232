namespace PRN232.Lab1.Service.SemesterService;

public class Response
{
    public class SemesterResponse
    {
        public int SemesterId { get; init; }
        public string SemesterName { get; init; } = string.Empty;
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
    }

    public class SemesterDetailResponse : SemesterResponse
    {
        public IReadOnlyCollection<CourseResponse> Courses { get; init; } = Array.Empty<CourseResponse>();
    }

    public class CourseResponse
    {
        public int CourseId { get; init; }
        public string CourseName { get; init; } = string.Empty;
        public int SubjectId { get; init; }
    }
}
