namespace PRN232.Lab1.Service.SubjectService;

public class Request
{
    public class CreateSubjectRequest
    {
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int Credit { get; set; }
    }

    public class UpdateSubjectRequest : CreateSubjectRequest;
}
