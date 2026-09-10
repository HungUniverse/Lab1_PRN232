using Microsoft.EntityFrameworkCore;
using PRN232.Lab1.Repository.Entity;
using PRN232.Lab1.Repository.Repositories;
using PRN232.Lab1.Service.Base;
using PRN232.Lab1.Service.Business;

namespace PRN232.Lab1.Service.StudentService;

public class StudentService : IStudentService
{
    private static readonly string[] AllowedFields = ["studentId", "fullName", "email", "dateOfBirth"];
    private static readonly string[] AllowedExpansions = ["enrollments"];
    private readonly IRepository<Student> _studentRepository;
    private readonly IRepository<Enrollment> _enrollmentRepository;

    public StudentService(
        IRepository<Student> studentRepository,
        IRepository<Enrollment> enrollmentRepository)
    {
        _studentRepository = studentRepository;
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<ServiceResult<PagedResult<Dictionary<string, object?>>>> GetStudentsAsync(ListQueryRequest request)
    {
        if (!QueryOptionsParser.TryParse(request, AllowedFields, AllowedFields, AllowedExpansions,
                out var options, out var errors))
            return ServiceResults.BadRequest<PagedResult<Dictionary<string, object?>>>(errors);

        var query = _studentRepository.Query();
        if (options.Search is not null)
            query = query.Where(student =>
                student.FullName.Contains(options.Search) || student.Email.Contains(options.Search));

        var totalItems = await query.CountAsync();
        query = ApplySorting(query, options.SortTerms);

        if (options.Expansions.Contains("enrollments"))
            query = query.Include(student => student.Enrollments).ThenInclude(enrollment => enrollment.Course);

        var entities = await query
            .Skip((options.Page - 1) * options.Size)
            .Take(options.Size)
            .AsSplitQuery()
            .ToListAsync();

        var selectedFields = options.Fields.Count == 0
            ? AllowedFields.ToHashSet(StringComparer.OrdinalIgnoreCase)
            : options.Fields;
        var items = entities
            .Select(entity => ToBusinessModel(entity, options.Expansions.Contains("enrollments")))
            .Select(model => SelectFields(model, selectedFields, options.Expansions))
            .ToArray();

        return ServiceResults.Success(new PagedResult<Dictionary<string, object?>>
        {
            Items = items,
            Pagination = CreatePagination(options, totalItems)
        });
    }

    public async Task<ServiceResult<Response.StudentDetailResponse>> GetStudentByIdAsync(int id)
    {
        var entity = await _studentRepository.Query()
            .Include(student => student.Enrollments)
            .ThenInclude(enrollment => enrollment.Course)
            .AsSplitQuery()
            .FirstOrDefaultAsync(student => student.StudentId == id);

        if (entity is null)
            return ServiceResults.NotFound<Response.StudentDetailResponse>($"Student with id {id} was not found.");

        return ServiceResults.Success(ToDetailResponse(ToBusinessModel(entity, true)));
    }

    public async Task<ServiceResult<Response.StudentResponse>> CreateStudentAsync(Request.CreateStudentRequest request)
    {
        var errors = Validate(request.FullName, request.Email, request.DateOfBirth);
        if (errors.Count > 0)
            return ServiceResults.BadRequest<Response.StudentResponse>(errors.ToArray());

        var normalizedEmail = request.Email.Trim();
        if (await _studentRepository.Query().AnyAsync(student => student.Email == normalizedEmail))
            return ServiceResults.BadRequest<Response.StudentResponse>("Email already exists.");

        var entity = new Student
        {
            FullName = request.FullName.Trim(),
            Email = normalizedEmail,
            DateOfBirth = request.DateOfBirth
        };
        await _studentRepository.AddAsync(entity);
        await _studentRepository.SaveChangesAsync();

        return ServiceResults.Created(ToResponse(ToBusinessModel(entity, false)));
    }

    public async Task<ServiceResult<Response.StudentResponse>> UpdateStudentAsync(
        int id,
        Request.UpdateStudentRequest request)
    {
        var errors = Validate(request.FullName, request.Email, request.DateOfBirth);
        if (errors.Count > 0)
            return ServiceResults.BadRequest<Response.StudentResponse>(errors.ToArray());

        var entity = await _studentRepository.FindAsync(id);
        if (entity is null)
            return ServiceResults.NotFound<Response.StudentResponse>($"Student with id {id} was not found.");

        var normalizedEmail = request.Email.Trim();
        if (await _studentRepository.Query().AnyAsync(student =>
                student.Email == normalizedEmail && student.StudentId != id))
            return ServiceResults.BadRequest<Response.StudentResponse>("Email already exists.");

        entity.FullName = request.FullName.Trim();
        entity.Email = normalizedEmail;
        entity.DateOfBirth = request.DateOfBirth;
        await _studentRepository.SaveChangesAsync();

        return ServiceResults.Success(ToResponse(ToBusinessModel(entity, false)), "Student updated successfully");
    }

    public async Task<ServiceResult<bool>> DeleteStudentAsync(int id)
    {
        var entity = await _studentRepository.FindAsync(id);
        if (entity is null)
            return ServiceResults.NotFound<bool>($"Student with id {id} was not found.");

        if (await _enrollmentRepository.Query().AnyAsync(enrollment => enrollment.StudentId == id))
            return ServiceResults.BadRequest<bool>("The student cannot be deleted because enrollments reference it.");

        _studentRepository.Remove(entity);
        await _studentRepository.SaveChangesAsync();
        return ServiceResults.Success(true, "Student deleted successfully");
    }

    private static IQueryable<Student> ApplySorting(IQueryable<Student> query, IReadOnlyCollection<SortTerm> terms)
    {
        if (terms.Count == 0)
            return query.OrderBy(student => student.StudentId);

        IOrderedQueryable<Student>? ordered = null;
        foreach (var term in terms)
        {
            ordered = term.Field.ToLowerInvariant() switch
            {
                "studentid" => QueryOrdering.Apply(query, ordered, student => student.StudentId, term.Descending),
                "fullname" => QueryOrdering.Apply(query, ordered, student => student.FullName, term.Descending),
                "email" => QueryOrdering.Apply(query, ordered, student => student.Email, term.Descending),
                "dateofbirth" => QueryOrdering.Apply(query, ordered, student => student.DateOfBirth, term.Descending),
                _ => ordered
            };
        }

        return ordered ?? query.OrderBy(student => student.StudentId);
    }

    private static List<string> Validate(string fullName, string email, DateTime dateOfBirth)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(fullName) || fullName.Trim().Length > 100)
            errors.Add("FullName is required and must not exceed 100 characters.");
        if (string.IsNullOrWhiteSpace(email) || email.Trim().Length > 100 || !email.Contains('@'))
            errors.Add("Email is required, must be valid, and must not exceed 100 characters.");
        if (dateOfBirth == default || dateOfBirth.Date >= DateTime.UtcNow.Date)
            errors.Add("DateOfBirth must be a date in the past.");
        return errors;
    }

    private static StudentBusinessModel ToBusinessModel(Student entity, bool includeEnrollments)
    {
        return new StudentBusinessModel
        {
            StudentId = entity.StudentId,
            FullName = entity.FullName,
            Email = entity.Email,
            DateOfBirth = entity.DateOfBirth,
            Enrollments = includeEnrollments
                ? entity.Enrollments.OrderBy(enrollment => enrollment.EnrollmentId).Select(enrollment =>
                    new StudentEnrollmentBusinessModel
                    {
                        EnrollmentId = enrollment.EnrollmentId,
                        CourseId = enrollment.CourseId,
                        CourseName = enrollment.Course.CourseName,
                        EnrollDate = enrollment.EnrollDate,
                        Status = enrollment.Status
                    }).ToArray()
                : Array.Empty<StudentEnrollmentBusinessModel>()
        };
    }

    private static Response.StudentResponse ToResponse(StudentBusinessModel model)
    {
        return new Response.StudentResponse
        {
            StudentId = model.StudentId,
            FullName = model.FullName,
            Email = model.Email,
            DateOfBirth = model.DateOfBirth
        };
    }

    private static Response.StudentDetailResponse ToDetailResponse(StudentBusinessModel model)
    {
        return new Response.StudentDetailResponse
        {
            StudentId = model.StudentId,
            FullName = model.FullName,
            Email = model.Email,
            DateOfBirth = model.DateOfBirth,
            Enrollments = model.Enrollments.Select(enrollment => new Response.StudentEnrollmentResponse
            {
                EnrollmentId = enrollment.EnrollmentId,
                CourseId = enrollment.CourseId,
                CourseName = enrollment.CourseName,
                EnrollDate = enrollment.EnrollDate,
                Status = enrollment.Status
            }).ToArray()
        };
    }

    private static Dictionary<string, object?> SelectFields(
        StudentBusinessModel model,
        HashSet<string> fields,
        HashSet<string> expansions)
    {
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        if (fields.Contains("studentId")) result["studentId"] = model.StudentId;
        if (fields.Contains("fullName")) result["fullName"] = model.FullName;
        if (fields.Contains("email")) result["email"] = model.Email;
        if (fields.Contains("dateOfBirth")) result["dateOfBirth"] = model.DateOfBirth;
        if (expansions.Contains("enrollments")) result["enrollments"] = model.Enrollments;
        return result;
    }

    private static PaginationMetadata CreatePagination(QueryOptions options, int totalItems)
    {
        return new PaginationMetadata
        {
            Page = options.Page,
            PageSize = options.Size,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)options.Size)
        };
    }
}
