using PRN232.Lab1.Service.Business;

namespace PRN232.Lab1.API.RequestModels;

public abstract class ListQueryRequest
{
    public string? Search { get; set; }
    public string? Sort { get; set; }
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string? Fields { get; set; }
    public string? Expand { get; set; }

    public ListQueryModel ToModel() => new()
    {
        Search = Search,
        Sort = Sort,
        Page = Page,
        Size = Size,
        Fields = Fields,
        Expand = Expand
    };
}

public sealed class StudentQueryRequest : ListQueryRequest;
public sealed class SemesterQueryRequest : ListQueryRequest;
public sealed class SubjectQueryRequest : ListQueryRequest;
public sealed class CourseQueryRequest : ListQueryRequest;
public sealed class EnrollmentQueryRequest : ListQueryRequest;

public sealed class CreateStudentRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }

    public CreateStudentModel ToModel() => new()
    {
        FullName = FullName,
        Email = Email,
        DateOfBirth = DateOfBirth
    };
}

public sealed class UpdateStudentRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }

    public UpdateStudentModel ToModel() => new()
    {
        FullName = FullName,
        Email = Email,
        DateOfBirth = DateOfBirth
    };
}
