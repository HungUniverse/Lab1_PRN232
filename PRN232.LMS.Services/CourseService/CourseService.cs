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

    public CourseService(IRepository<Course> courseRepository)
    {
        _courseRepository = courseRepository;
    }

    public async Task<ServiceResult<PagedResult<CourseModel>>> GetCoursesAsync(ListQueryModel request)
    {
        if (!QueryOptionsParser.TryParse(request, AllowedFields, AllowedFields, AllowedExpansions,
                out var options, out var errors))
            return ServiceResults.BadRequest<PagedResult<CourseModel>>(errors);

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

        var entities = await query
            .Skip((options.Page - 1) * options.Size)
            .Take(options.Size)
            .AsSplitQuery()
            .ToListAsync();

        return ServiceResults.Success(new PagedResult<CourseModel>
        {
            Items = entities.Select(entity => ToModel(entity, options.Expansions)).ToArray(),
            Pagination = Pagination(options, totalItems)
        });
    }

    public async Task<ServiceResult<CourseModel>> GetCourseByIdAsync(int id)
    {
        var entity = await _courseRepository.Query()
            .Include(course => course.Semester)
            .Include(course => course.Subject)
            .Include(course => course.Enrollments).ThenInclude(enrollment => enrollment.Student)
            .AsSplitQuery()
            .FirstOrDefaultAsync(course => course.CourseId == id);

        if (entity is null)
            return ServiceResults.NotFound<CourseModel>($"Course with id {id} was not found.");

        var expansions = new HashSet<string>(AllowedExpansions, StringComparer.OrdinalIgnoreCase);
        return ServiceResults.Success(ToModel(entity, expansions));
    }

    private static IQueryable<Course> ApplySorting(
        IQueryable<Course> query,
        IReadOnlyCollection<SortTerm> terms)
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

    private static CourseModel ToModel(Course entity, HashSet<string> expansions) => new()
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
                new CourseEnrollmentModel
                {
                    EnrollmentId = enrollment.EnrollmentId,
                    StudentId = enrollment.StudentId,
                    StudentName = enrollment.Student.FullName,
                    EnrollDate = enrollment.EnrollDate,
                    Status = enrollment.Status
                }).ToArray()
            : Array.Empty<CourseEnrollmentModel>()
    };

    private static PaginationMetadata Pagination(QueryOptions options, int totalItems) => new()
    {
        Page = options.Page,
        PageSize = options.Size,
        TotalItems = totalItems,
        TotalPages = (int)Math.Ceiling(totalItems / (double)options.Size)
    };
}
