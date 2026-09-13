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
    private readonly IRepository<Enrollment> _enrollmentRepository;

    public EnrollmentService(IRepository<Enrollment> enrollmentRepository)
    {
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<ServiceResult<PagedResult<EnrollmentModel>>> GetEnrollmentsAsync(ListQueryModel request)
    {
        if (!QueryOptionsParser.TryParse(request, AllowedFields, AllowedFields, AllowedExpansions,
                out var options, out var errors))
            return ServiceResults.BadRequest<PagedResult<EnrollmentModel>>(errors);

        var query = _enrollmentRepository.Query();
        if (options.Search is not null)
            query = query.Where(enrollment =>
                enrollment.Status.Contains(options.Search) ||
                enrollment.Student.FullName.Contains(options.Search) ||
                enrollment.Course.CourseName.Contains(options.Search));

        var totalItems = await query.CountAsync();
        query = ApplySorting(query, options.SortTerms);
        if (options.Expansions.Contains("student"))
            query = query.Include(enrollment => enrollment.Student);
        if (options.Expansions.Contains("course"))
            query = query.Include(enrollment => enrollment.Course).ThenInclude(course => course.Semester)
                .Include(enrollment => enrollment.Course).ThenInclude(course => course.Subject);

        var entities = await query
            .Skip((options.Page - 1) * options.Size)
            .Take(options.Size)
            .AsSplitQuery()
            .ToListAsync();

        return ServiceResults.Success(new PagedResult<EnrollmentModel>
        {
            Items = entities.Select(entity => ToModel(entity, options.Expansions)).ToArray(),
            Pagination = Pagination(options, totalItems)
        });
    }

    public async Task<ServiceResult<EnrollmentModel>> GetEnrollmentByIdAsync(int id)
    {
        var entity = await _enrollmentRepository.Query()
            .Include(enrollment => enrollment.Student)
            .Include(enrollment => enrollment.Course).ThenInclude(course => course.Semester)
            .Include(enrollment => enrollment.Course).ThenInclude(course => course.Subject)
            .AsSplitQuery()
            .FirstOrDefaultAsync(enrollment => enrollment.EnrollmentId == id);

        if (entity is null)
            return ServiceResults.NotFound<EnrollmentModel>($"Enrollment with id {id} was not found.");

        var expansions = new HashSet<string>(AllowedExpansions, StringComparer.OrdinalIgnoreCase);
        return ServiceResults.Success(ToModel(entity, expansions));
    }

    private static IQueryable<Enrollment> ApplySorting(
        IQueryable<Enrollment> query,
        IReadOnlyCollection<SortTerm> terms)
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

    private static EnrollmentModel ToModel(Enrollment entity, HashSet<string> expansions) => new()
    {
        EnrollmentId = entity.EnrollmentId,
        StudentId = entity.StudentId,
        CourseId = entity.CourseId,
        EnrollDate = entity.EnrollDate,
        Status = entity.Status,
        Student = expansions.Contains("student")
            ? new StudentSummaryModel
            {
                StudentId = entity.Student.StudentId,
                FullName = entity.Student.FullName,
                Email = entity.Student.Email
            }
            : null,
        Course = expansions.Contains("course")
            ? new EnrollmentCourseModel
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

    private static PaginationMetadata Pagination(QueryOptions options, int totalItems) => new()
    {
        Page = options.Page,
        PageSize = options.Size,
        TotalItems = totalItems,
        TotalPages = (int)Math.Ceiling(totalItems / (double)options.Size)
    };
}
