namespace PRN232.Lab1.Service.SemesterService;

public class Request
{
    public class CreateSemesterRequest
    {
        public string SemesterName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class UpdateSemesterRequest : CreateSemesterRequest;
}
