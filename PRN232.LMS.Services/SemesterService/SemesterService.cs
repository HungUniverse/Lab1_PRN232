using Microsoft.EntityFrameworkCore;
using PRN232.Lab1.Repository.Entity;
using PRN232.Lab1.Repository.Repositories;
using PRN232.Lab1.Service.Base;
using PRN232.Lab1.Service.Business;

namespace PRN232.Lab1.Service.SemesterService;

public class SemesterService : ISemesterService
{
    private static readonly string[] AllowedFields = ["semesterId", "semesterName", "startDate", "endDate"];
    private static readonly string[] AllowedExpansions = ["courses"];
    private readonly IRepository<Semester> _semesterRepository;

    public SemesterService(IRepository<Semester> semesterRepository)
    {
        _semesterRepository = semesterRepository;
    }

    public async Task<ServiceResult<PagedResult<SemesterModel>>> GetSemestersAsync(ListQueryModel request)
    {
        if (!QueryOptionsParser.TryParse(request, AllowedFields, AllowedFields, AllowedExpansions,
                out var options, out var errors))
            return ServiceResults.BadRequest<PagedResult<SemesterModel>>(errors);

        var query = _semesterRepository.Query();
        if (options.Search is not null)
            query = query.Where(semester => semester.SemesterName.Contains(options.Search));

        var totalItems = await query.CountAsync();
        query = ApplySorting(query, options.SortTerms);
        if (options.Expansions.Contains("courses"))
            query = query.Include(semester => semester.Courses);

        var entities = await query
            .Skip((options.Page - 1) * options.Size)
            .Take(options.Size)
            .AsSplitQuery()
            .ToListAsync();

        return ServiceResults.Success(new PagedResult<SemesterModel>
        {
            Items = entities.Select(entity => ToModel(entity, options.Expansions.Contains("courses"))).ToArray(),
            Pagination = Pagination(options, totalItems)
        });
    }

    public async Task<ServiceResult<SemesterModel>> GetSemesterByIdAsync(int id)
    {
        var entity = await _semesterRepository.Query()
            .Include(semester => semester.Courses)
            .AsSplitQuery()
            .FirstOrDefaultAsync(semester => semester.SemesterId == id);

        return entity is null
            ? ServiceResults.NotFound<SemesterModel>($"Semester with id {id} was not found.")
            : ServiceResults.Success(ToModel(entity, true));
    }

    private static IQueryable<Semester> ApplySorting(
        IQueryable<Semester> query,
        IReadOnlyCollection<SortTerm> terms)
    {
        if (terms.Count == 0) return query.OrderBy(semester => semester.SemesterId);
        IOrderedQueryable<Semester>? ordered = null;
        foreach (var term in terms)
        {
            ordered = term.Field.ToLowerInvariant() switch
            {
                "semesterid" => QueryOrdering.Apply(query, ordered, semester => semester.SemesterId, term.Descending),
                "semestername" => QueryOrdering.Apply(query, ordered, semester => semester.SemesterName, term.Descending),
                "startdate" => QueryOrdering.Apply(query, ordered, semester => semester.StartDate, term.Descending),
                "enddate" => QueryOrdering.Apply(query, ordered, semester => semester.EndDate, term.Descending),
                _ => ordered
            };
        }
        return ordered ?? query.OrderBy(semester => semester.SemesterId);
    }

    private static SemesterModel ToModel(Semester entity, bool includeCourses) => new()
    {
        SemesterId = entity.SemesterId,
        SemesterName = entity.SemesterName,
        StartDate = entity.StartDate,
        EndDate = entity.EndDate,
        Courses = includeCourses
            ? entity.Courses.OrderBy(course => course.CourseId).Select(course => new CourseSummaryModel
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                SemesterId = course.SemesterId,
                SubjectId = course.SubjectId
            }).ToArray()
            : Array.Empty<CourseSummaryModel>()
    };

    private static PaginationMetadata Pagination(QueryOptions options, int totalItems) => new()
    {
        Page = options.Page,
        PageSize = options.Size,
        TotalItems = totalItems,
        TotalPages = (int)Math.Ceiling(totalItems / (double)options.Size)
    };
}
