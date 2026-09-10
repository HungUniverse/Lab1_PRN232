namespace PRN232.Lab1.Service.StudentService;

public class Request
{
    public class CreateStudentRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
    }

    public class UpdateStudentRequest : CreateStudentRequest;
}
