using Microsoft.EntityFrameworkCore;
using PRN232.Lab1.Repository.Entity;
using PRN232.Lab1.Repository.Repositories;
using PRN232.Lab1.Service.Base;
using PRN232.Lab1.Service.Business;

namespace PRN232.Lab1.Service.CourseService;

public class CourseService : ICourseService
{
    private static readonly string[] AllowedFields = ["courseId", "courseName", "semesterId", "subjectId"];
    private static readonly string[] AllowedExpansions = ["semester", "subject", "enrollments"];
    private readonly IRepository<Course> _courseRepository;
    private readonly IRepository<Semester> _semesterRepository;
    private readonly IRepository<Subject> _subjectRepository;
    private readonly IRepository<Enrollment> _enrollmentRepository;

    public CourseService(
        IRepository<Course> courseRepository,
        IRepository<Semester> semesterRepository,
        IRepository<Subject> subjectRepository,
        IRepository<Enrollment> enrollmentRepository)
    {
        _courseRepository = courseRepository;
        _semesterRepository = semesterRepository;
        _subjectRepository = subjectRepository;
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<ServiceResult<PagedResult<Dictionary<string, object?>>>> GetCoursesAsync(ListQueryRequest request)
    {
        if (!QueryOptionsParser.TryParse(request, AllowedFields, AllowedFields, AllowedExpansions,
                out var options, out var errors))
            return ServiceResults.BadRequest<PagedResult<Dictionary<string, object?>>>(errors);

        var query = _courseRepository.Query();
        if (options.Search is not null)
            query = query.Where(course =>
                course.CourseName.Contains(options.Search) ||
                course.Subject.SubjectCode.Contains(options.Search) ||
                course.Subject.SubjectName.Contains(options.Search));

        var totalItems = await query.CountAsync();
        query = ApplySorting(query, options.SortTerms);
        if (options.Expansions.Contains("semester")) query = query.Include(course => course.Semester);
        if (options.Expansions.Contains("subject")) query = query.Include(course => course.Subject);
        if (options.Expansions.Contains("enrollments"))
            query = query.Include(course => course.Enrollments).ThenInclude(enrollment => enrollment.Student);

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

    public async Task<ServiceResult<Response.CourseDetailResponse>> GetCourseByIdAsync(int id)
    {
        var entity = await _courseRepository.Query()
            .Include(course => course.Semester)
            .Include(course => course.Subject)
            .Include(course => course.Enrollments).ThenInclude(enrollment => enrollment.Student)
            .AsSplitQuery()
            .FirstOrDefaultAsync(course => course.CourseId == id);
        if (entity is null)
            return ServiceResults.NotFound<Response.CourseDetailResponse>($"Course with id {id} was not found.");

        var expansions = new HashSet<string>(AllowedExpansions, StringComparer.OrdinalIgnoreCase);
        return ServiceResults.Success(ToDetailResponse(ToBusinessModel(entity, expansions)));
    }

    public async Task<ServiceResult<Response.CourseResponse>> CreateCourseAsync(Request.CreateCourseRequest request)
    {
        var errors = await ValidateAsync(request.CourseName, request.SemesterId, request.SubjectId);
        if (errors.Count > 0) return ServiceResults.BadRequest<Response.CourseResponse>(errors.ToArray());

        var name = request.CourseName.Trim();
        if (await _courseRepository.Query().AnyAsync(course =>
                course.CourseName == name && course.SemesterId == request.SemesterId && course.SubjectId == request.SubjectId))
            return ServiceResults.BadRequest<Response.CourseResponse>("The course already exists in this semester and subject.");

        var entity = new Course
        {
            CourseName = name,
            SemesterId = request.SemesterId,
            SubjectId = request.SubjectId
        };
        await _courseRepository.AddAsync(entity);
        await _courseRepository.SaveChangesAsync();
        return ServiceResults.Created(ToResponse(ToBusinessModel(entity, new HashSet<string>())));
    }

    public async Task<ServiceResult<Response.CourseResponse>> UpdateCourseAsync(int id, Request.UpdateCourseRequest request)
    {
        var errors = await ValidateAsync(request.CourseName, request.SemesterId, request.SubjectId);
        if (errors.Count > 0) return ServiceResults.BadRequest<Response.CourseResponse>(errors.ToArray());

        var entity = await _courseRepository.FindAsync(id);
        if (entity is null) return ServiceResults.NotFound<Response.CourseResponse>($"Course with id {id} was not found.");

        var name = request.CourseName.Trim();
        if (await _courseRepository.Query().AnyAsync(course => course.CourseId != id &&
                course.CourseName == name && course.SemesterId == request.SemesterId && course.SubjectId == request.SubjectId))
            return ServiceResults.BadRequest<Response.CourseResponse>("The course already exists in this semester and subject.");

        entity.CourseName = name;
        entity.SemesterId = request.SemesterId;
        entity.SubjectId = request.SubjectId;
        await _courseRepository.SaveChangesAsync();
        return ServiceResults.Success(ToResponse(ToBusinessModel(entity, new HashSet<string>())), "Course updated successfully");
    }

    public async Task<ServiceResult<bool>> DeleteCourseAsync(int id)
    {
        var entity = await _courseRepository.FindAsync(id);
        if (entity is null) return ServiceResults.NotFound<bool>($"Course with id {id} was not found.");
        if (await _enrollmentRepository.Query().AnyAsync(enrollment => enrollment.CourseId == id))
            return ServiceResults.BadRequest<bool>("The course cannot be deleted because enrollments reference it.");

        _courseRepository.Remove(entity);
        await _courseRepository.SaveChangesAsync();
        return ServiceResults.Success(true, "Course deleted successfully");
    }

    private async Task<List<string>> ValidateAsync(string name, int semesterId, int subjectId)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 100)
            errors.Add("CourseName is required and must not exceed 100 characters.");
        if (!await _semesterRepository.Query().AnyAsync(semester => semester.SemesterId == semesterId))
            errors.Add($"Semester with id {semesterId} does not exist.");
        if (!await _subjectRepository.Query().AnyAsync(subject => subject.SubjectId == subjectId))
            errors.Add($"Subject with id {subjectId} does not exist.");
        return errors;
    }

    private static IQueryable<Course> ApplySorting(IQueryable<Course> query, IReadOnlyCollection<SortTerm> terms)
    {
        if (terms.Count == 0) return query.OrderBy(course => course.CourseId);
        IOrderedQueryable<Course>? ordered = null;
        foreach (var term in terms)
        {
            ordered = term.Field.ToLowerInvariant() switch
            {
                "courseid" => QueryOrdering.Apply(query, ordered, course => course.CourseId, term.Descending),
                "coursename" => QueryOrdering.Apply(query, ordered, course => course.CourseName, term.Descending),
                "semesterid" => QueryOrdering.Apply(query, ordered, course => course.SemesterId, term.Descending),
                "subjectid" => QueryOrdering.Apply(query, ordered, course => course.SubjectId, term.Descending),
                _ => ordered
            };
        }
        return ordered ?? query.OrderBy(course => course.CourseId);
    }

    private static CourseBusinessModel ToBusinessModel(Course entity, HashSet<string> expansions) => new()
    {
        CourseId = entity.CourseId,
        CourseName = entity.CourseName,
        SemesterId = entity.SemesterId,
        SemesterName = expansions.Contains("semester") ? entity.Semester.SemesterName : null,
        SubjectId = entity.SubjectId,
        SubjectCode = expansions.Contains("subject") ? entity.Subject.SubjectCode : null,
        SubjectName = expansions.Contains("subject") ? entity.Subject.SubjectName : null,
        Enrollments = expansions.Contains("enrollments")
            ? entity.Enrollments.OrderBy(enrollment => enrollment.EnrollmentId).Select(enrollment =>
                new CourseEnrollmentBusinessModel
                {
                    EnrollmentId = enrollment.EnrollmentId,
                    StudentId = enrollment.StudentId,
                    StudentName = enrollment.Student.FullName,
                    EnrollDate = enrollment.EnrollDate,
                    Status = enrollment.Status
                }).ToArray()
            : Array.Empty<CourseEnrollmentBusinessModel>()
    };

    private static Response.CourseResponse ToResponse(CourseBusinessModel model) => new()
    {
        CourseId = model.CourseId,
        CourseName = model.CourseName,
        SemesterId = model.SemesterId,
        SubjectId = model.SubjectId
    };

    private static Response.CourseDetailResponse ToDetailResponse(CourseBusinessModel model) => new()
    {
        CourseId = model.CourseId,
        CourseName = model.CourseName,
        SemesterId = model.SemesterId,
        SubjectId = model.SubjectId,
        Semester = new Response.SemesterResponse { SemesterId = model.SemesterId, SemesterName = model.SemesterName! },
        Subject = new Response.SubjectResponse
        {
            SubjectId = model.SubjectId,
            SubjectCode = model.SubjectCode!,
            SubjectName = model.SubjectName!
        },
        Enrollments = model.Enrollments.Select(enrollment => new Response.EnrollmentResponse
        {
            EnrollmentId = enrollment.EnrollmentId,
            StudentId = enrollment.StudentId,
            StudentName = enrollment.StudentName,
            EnrollDate = enrollment.EnrollDate,
            Status = enrollment.Status
        }).ToArray()
    };

    private static Dictionary<string, object?> SelectFields(
        CourseBusinessModel model,
        HashSet<string> fields,
        HashSet<string> expansions)
    {
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        if (fields.Contains("courseId")) result["courseId"] = model.CourseId;
        if (fields.Contains("courseName")) result["courseName"] = model.CourseName;
        if (fields.Contains("semesterId")) result["semesterId"] = model.SemesterId;
        if (fields.Contains("subjectId")) result["subjectId"] = model.SubjectId;
        if (expansions.Contains("semester"))
            result["semester"] = new { model.SemesterId, model.SemesterName };
        if (expansions.Contains("subject"))
            result["subject"] = new { model.SubjectId, model.SubjectCode, model.SubjectName };
        if (expansions.Contains("enrollments")) result["enrollments"] = model.Enrollments;
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
