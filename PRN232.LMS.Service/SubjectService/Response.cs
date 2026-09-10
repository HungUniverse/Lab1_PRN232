namespace PRN232.Lab1.Service.SubjectService;

public class Response
{
    public class SubjectResponse
    {
        public int SubjectId { get; init; }
        public string SubjectCode { get; init; } = string.Empty;
        public string SubjectName { get; init; } = string.Empty;
        public int Credit { get; init; }
    }

    public class SubjectDetailResponse : SubjectResponse
    {
        public IReadOnlyCollection<CourseResponse> Courses { get; init; } = Array.Empty<CourseResponse>();
    }

    public class CourseResponse
    {
        public int CourseId { get; init; }
        public string CourseName { get; init; } = string.Empty;
        public int SemesterId { get; init; }
    }
}
