namespace PRN232.Lab1.Service.EnrollmentService;

public class Request
{
    public class CreateEnrollmentRequest
    {
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public DateTime EnrollDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class UpdateEnrollmentRequest : CreateEnrollmentRequest;
}
