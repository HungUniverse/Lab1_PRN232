using Microsoft.EntityFrameworkCore;
using PRN232.Lab1.Repository.Entity;
using PRN232.Lab1.Repository.Repositories;
using PRN232.Lab1.Service.Base;
using PRN232.Lab1.Service.Business;

namespace PRN232.Lab1.Service.SubjectService;

public class SubjectService : ISubjectService
{
    private static readonly string[] AllowedFields = ["subjectId", "subjectCode", "subjectName", "credit"];
    private static readonly string[] AllowedExpansions = ["courses"];
    private readonly IRepository<Subject> _subjectRepository;

    public SubjectService(IRepository<Subject> subjectRepository)
    {
        _subjectRepository = subjectRepository;
    }

    public async Task<ServiceResult<PagedResult<SubjectModel>>> GetSubjectsAsync(ListQueryModel request)
    {
        if (!QueryOptionsParser.TryParse(request, AllowedFields, AllowedFields, AllowedExpansions,
                out var options, out var errors))
            return ServiceResults.BadRequest<PagedResult<SubjectModel>>(errors);

        var query = _subjectRepository.Query();
        if (options.Search is not null)
            query = query.Where(subject =>
                subject.SubjectCode.Contains(options.Search) || subject.SubjectName.Contains(options.Search));

        var totalItems = await query.CountAsync();
        query = ApplySorting(query, options.SortTerms);
        if (options.Expansions.Contains("courses"))
            query = query.Include(subject => subject.Courses);

        var entities = await query
            .Skip((options.Page - 1) * options.Size)
            .Take(options.Size)
            .AsSplitQuery()
            .ToListAsync();

        return ServiceResults.Success(new PagedResult<SubjectModel>
        {
            Items = entities.Select(entity => ToModel(entity, options.Expansions.Contains("courses"))).ToArray(),
            Pagination = Pagination(options, totalItems)
        });
    }

    public async Task<ServiceResult<SubjectModel>> GetSubjectByIdAsync(int id)
    {
        var entity = await _subjectRepository.Query()
            .Include(subject => subject.Courses)
            .AsSplitQuery()
            .FirstOrDefaultAsync(subject => subject.SubjectId == id);

        return entity is null
            ? ServiceResults.NotFound<SubjectModel>($"Subject with id {id} was not found.")
            : ServiceResults.Success(ToModel(entity, true));
    }

    private static IQueryable<Subject> ApplySorting(
        IQueryable<Subject> query,
        IReadOnlyCollection<SortTerm> terms)
    {
        if (terms.Count == 0) return query.OrderBy(subject => subject.SubjectId);
        IOrderedQueryable<Subject>? ordered = null;
        foreach (var term in terms)
        {
            ordered = term.Field.ToLowerInvariant() switch
            {
                "subjectid" => QueryOrdering.Apply(query, ordered, subject => subject.SubjectId, term.Descending),
                "subjectcode" => QueryOrdering.Apply(query, ordered, subject => subject.SubjectCode, term.Descending),
                "subjectname" => QueryOrdering.Apply(query, ordered, subject => subject.SubjectName, term.Descending),
                "credit" => QueryOrdering.Apply(query, ordered, subject => subject.Credit, term.Descending),
                _ => ordered
            };
        }
        return ordered ?? query.OrderBy(subject => subject.SubjectId);
    }

    private static SubjectModel ToModel(Subject entity, bool includeCourses) => new()
    {
        SubjectId = entity.SubjectId,
        SubjectCode = entity.SubjectCode,
        SubjectName = entity.SubjectName,
        Credit = entity.Credit,
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
