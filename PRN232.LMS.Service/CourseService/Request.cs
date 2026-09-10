namespace PRN232.Lab1.Service.CourseService;

public class Request
{
    public class CreateCourseRequest
    {
        public string CourseName { get; set; } = string.Empty;
        public int SemesterId { get; set; }
        public int SubjectId { get; set; }
    }

    public class UpdateCourseRequest : CreateCourseRequest;
}
