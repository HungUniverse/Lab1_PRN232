using Microsoft.EntityFrameworkCore;
using PRN232.Lab1.Repository.Entity;
using PRN232.Lab1.Repository.Repositories;
using PRN232.Lab1.Service.Base;
using PRN232.Lab1.Service.Business;

namespace PRN232.Lab1.Service.EnrollmentService;

public class EnrollmentService : IEnrollmentService
{
    private static readonly string[] AllowedFields = ["enrollmentId", "studentId", "courseId", "enrollDate", "status"];
    private static readonly string[] AllowedExpansions = ["student", "course"];
    private static readonly string[] AllowedStatuses = ["Active", "Completed", "Cancelled"];
    private readonly IRepository<Enrollment> _enrollmentRepository;
    private readonly IRepository<Student> _studentRepository;
    private readonly IRepository<Course> _courseRepository;

    public EnrollmentService(
        IRepository<Enrollment> enrollmentRepository,
        IRepository<Student> studentRepository,
        IRepository<Course> courseRepository)
    {
        _enrollmentRepository = enrollmentRepository;
        _studentRepository = studentRepository;
        _courseRepository = courseRepository;
    }

    public async Task<ServiceResult<PagedResult<Dictionary<string, object?>>>> GetEnrollmentsAsync(ListQueryRequest request)
    {
        if (!QueryOptionsParser.TryParse(request, AllowedFields, AllowedFields, AllowedExpansions,
                out var options, out var errors))
            return ServiceResults.BadRequest<PagedResult<Dictionary<string, object?>>>(errors);

        var query = _enrollmentRepository.Query();
        if (options.Search is not null)
            query = query.Where(enrollment =>
                enrollment.Status.Contains(options.Search) ||
                enrollment.Student.FullName.Contains(options.Search) ||
                enrollment.Course.CourseName.Contains(options.Search));

        var totalItems = await query.CountAsync();
        query = ApplySorting(query, options.SortTerms);
        if (options.Expansions.Contains("student")) query = query.Include(enrollment => enrollment.Student);
        if (options.Expansions.Contains("course"))
            query = query.Include(enrollment => enrollment.Course).ThenInclude(course => course.Semester)
                .Include(enrollment => enrollment.Course).ThenInclude(course => course.Subject);

        var entities = await query.Skip((options.Page - 1) * options.Size).Take(options.Size)
            .AsSplitQuery().ToListAsync();
        var fields = options.Fields.Count == 0
            ? AllowedFields.ToHashSet(StringComparer.OrdinalIgnoreCase)
            : options.Fields;
        var items = entities
            .Select(entity => ToBusinessModel(entity, options.Expansions))
            .Select(model => SelectFields(model, fields, options.Expansions))
            .ToArray();

        return ServiceResults.Success(new PagedResult<Dictionary<string, object?>>
        {
            Items = items,
            Pagination = Pagination(options, totalItems)
        });
    }

    public async Task<ServiceResult<Response.EnrollmentDetailResponse>> GetEnrollmentByIdAsync(int id)
    {
        var entity = await _enrollmentRepository.Query()
            .Include(enrollment => enrollment.Student)
            .Include(enrollment => enrollment.Course).ThenInclude(course => course.Semester)
            .Include(enrollment => enrollment.Course).ThenInclude(course => course.Subject)
            .AsSplitQuery()
            .FirstOrDefaultAsync(enrollment => enrollment.EnrollmentId == id);
        if (entity is null)
            return ServiceResults.NotFound<Response.EnrollmentDetailResponse>($"Enrollment with id {id} was not found.");

        var expansions = new HashSet<string>(AllowedExpansions, StringComparer.OrdinalIgnoreCase);
        return ServiceResults.Success(ToDetailResponse(ToBusinessModel(entity, expansions)));
    }

    public async Task<ServiceResult<Response.EnrollmentResponse>> CreateEnrollmentAsync(Request.CreateEnrollmentRequest request)
    {
        var normalizedStatus = NormalizeStatus(request.Status);
        var errors = await ValidateAsync(request.StudentId, request.CourseId, request.EnrollDate, normalizedStatus);
        if (errors.Count > 0) return ServiceResults.BadRequest<Response.EnrollmentResponse>(errors.ToArray());

        if (await _enrollmentRepository.Query().AnyAsync(enrollment =>
                enrollment.StudentId == request.StudentId && enrollment.CourseId == request.CourseId))
            return ServiceResults.BadRequest<Response.EnrollmentResponse>("The student is already enrolled in this course.");

        var entity = new Enrollment
        {
            StudentId = request.StudentId,
            CourseId = request.CourseId,
            EnrollDate = request.EnrollDate,
            Status = normalizedStatus
        };
        await _enrollmentRepository.AddAsync(entity);
        await _enrollmentRepository.SaveChangesAsync();
        return ServiceResults.Created(ToResponse(ToBusinessModel(entity, new HashSet<string>())));
    }

    public async Task<ServiceResult<Response.EnrollmentResponse>> UpdateEnrollmentAsync(
        int id,
        Request.UpdateEnrollmentRequest request)
    {
        var normalizedStatus = NormalizeStatus(request.Status);
        var errors = await ValidateAsync(request.StudentId, request.CourseId, request.EnrollDate, normalizedStatus);
        if (errors.Count > 0) return ServiceResults.BadRequest<Response.EnrollmentResponse>(errors.ToArray());

        var entity = await _enrollmentRepository.FindAsync(id);
        if (entity is null)
            return ServiceResults.NotFound<Response.EnrollmentResponse>($"Enrollment with id {id} was not found.");

        if (await _enrollmentRepository.Query().AnyAsync(enrollment => enrollment.EnrollmentId != id &&
                enrollment.StudentId == request.StudentId && enrollment.CourseId == request.CourseId))
            return ServiceResults.BadRequest<Response.EnrollmentResponse>("The student is already enrolled in this course.");

        entity.StudentId = request.StudentId;
        entity.CourseId = request.CourseId;
        entity.EnrollDate = request.EnrollDate;
        entity.Status = normalizedStatus;
        await _enrollmentRepository.SaveChangesAsync();
        return ServiceResults.Success(ToResponse(ToBusinessModel(entity, new HashSet<string>())), "Enrollment updated successfully");
    }

    public async Task<ServiceResult<bool>> DeleteEnrollmentAsync(int id)
    {
        var entity = await _enrollmentRepository.FindAsync(id);
        if (entity is null) return ServiceResults.NotFound<bool>($"Enrollment with id {id} was not found.");

        _enrollmentRepository.Remove(entity);
        await _enrollmentRepository.SaveChangesAsync();
        return ServiceResults.Success(true, "Enrollment deleted successfully");
    }

    private async Task<List<string>> ValidateAsync(int studentId, int courseId, DateTime enrollDate, string status)
    {
        var errors = new List<string>();
        if (!await _studentRepository.Query().AnyAsync(student => student.StudentId == studentId))
            errors.Add($"Student with id {studentId} does not exist.");
        if (!await _courseRepository.Query().AnyAsync(course => course.CourseId == courseId))
            errors.Add($"Course with id {courseId} does not exist.");
        if (enrollDate == default) errors.Add("EnrollDate is required.");
        if (!AllowedStatuses.Contains(status, StringComparer.Ordinal))
            errors.Add("Status must be Active, Completed, or Cancelled.");
        return errors;
    }

    private static string NormalizeStatus(string status)
    {
        var match = AllowedStatuses.FirstOrDefault(value => value.Equals(status?.Trim(), StringComparison.OrdinalIgnoreCase));
        return match ?? status?.Trim() ?? string.Empty;
    }

    private static IQueryable<Enrollment> ApplySorting(IQueryable<Enrollment> query, IReadOnlyCollection<SortTerm> terms)
    {
        if (terms.Count == 0) return query.OrderBy(enrollment => enrollment.EnrollmentId);
        IOrderedQueryable<Enrollment>? ordered = null;
        foreach (var term in terms)
        {
            ordered = term.Field.ToLowerInvariant() switch
            {
                "enrollmentid" => QueryOrdering.Apply(query, ordered, enrollment => enrollment.EnrollmentId, term.Descending),
                "studentid" => QueryOrdering.Apply(query, ordered, enrollment => enrollment.StudentId, term.Descending),
                "courseid" => QueryOrdering.Apply(query, ordered, enrollment => enrollment.CourseId, term.Descending),
                "enrolldate" => QueryOrdering.Apply(query, ordered, enrollment => enrollment.EnrollDate, term.Descending),
                "status" => QueryOrdering.Apply(query, ordered, enrollment => enrollment.Status, term.Descending),
                _ => ordered
            };
        }
        return ordered ?? query.OrderBy(enrollment => enrollment.EnrollmentId);
    }

    private static EnrollmentBusinessModel ToBusinessModel(Enrollment entity, HashSet<string> expansions) => new()
    {
        EnrollmentId = entity.EnrollmentId,
        StudentId = entity.StudentId,
        CourseId = entity.CourseId,
        EnrollDate = entity.EnrollDate,
        Status = entity.Status,
        Student = expansions.Contains("student")
            ? new StudentSummaryBusinessModel
            {
                StudentId = entity.Student.StudentId,
                FullName = entity.Student.FullName,
                Email = entity.Student.Email
            }
            : null,
        Course = expansions.Contains("course")
            ? new EnrollmentCourseBusinessModel
            {
                CourseId = entity.Course.CourseId,
                CourseName = entity.Course.CourseName,
                SemesterId = entity.Course.SemesterId,
                SemesterName = entity.Course.Semester.SemesterName,
                SubjectId = entity.Course.SubjectId,
                SubjectCode = entity.Course.Subject.SubjectCode,
                SubjectName = entity.Course.Subject.SubjectName
            }
            : null
    };

    private static Response.EnrollmentResponse ToResponse(EnrollmentBusinessModel model) => new()
    {
        EnrollmentId = model.EnrollmentId,
        StudentId = model.StudentId,
        CourseId = model.CourseId,
        EnrollDate = model.EnrollDate,
        Status = model.Status
    };

    private static Response.EnrollmentDetailResponse ToDetailResponse(EnrollmentBusinessModel model) => new()
    {
        EnrollmentId = model.EnrollmentId,
        StudentId = model.StudentId,
        CourseId = model.CourseId,
        EnrollDate = model.EnrollDate,
        Status = model.Status,
        Student = new Response.StudentResponse
        {
            StudentId = model.Student!.StudentId,
            FullName = model.Student.FullName,
            Email = model.Student.Email
        },
        Course = new Response.CourseResponse
        {
            CourseId = model.Course!.CourseId,
            CourseName = model.Course.CourseName,
            SemesterId = model.Course.SemesterId,
            SemesterName = model.Course.SemesterName,
            SubjectId = model.Course.SubjectId,
            SubjectCode = model.Course.SubjectCode,
            SubjectName = model.Course.SubjectName
        }
    };

    private static Dictionary<string, object?> SelectFields(
        EnrollmentBusinessModel model,
        HashSet<string> fields,
        HashSet<string> expansions)
    {
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        if (fields.Contains("enrollmentId")) result["enrollmentId"] = model.EnrollmentId;
        if (fields.Contains("studentId")) result["studentId"] = model.StudentId;
        if (fields.Contains("courseId")) result["courseId"] = model.CourseId;
        if (fields.Contains("enrollDate")) result["enrollDate"] = model.EnrollDate;
        if (fields.Contains("status")) result["status"] = model.Status;
        if (expansions.Contains("student")) result["student"] = model.Student;
        if (expansions.Contains("course")) result["course"] = model.Course;
        return result;
    }

    private static PaginationMetadata Pagination(QueryOptions options, int totalItems) => new()
    {
        Page = options.Page,
        PageSize = options.Size,
        TotalItems = totalItems,
        TotalPages = (int)Math.Ceiling(totalItems / (double)options.Size)
    };
}
