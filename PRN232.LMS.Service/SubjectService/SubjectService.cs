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
    private readonly IRepository<Course> _courseRepository;

    public SubjectService(IRepository<Subject> subjectRepository, IRepository<Course> courseRepository)
    {
        _subjectRepository = subjectRepository;
        _courseRepository = courseRepository;
    }

    public async Task<ServiceResult<PagedResult<Dictionary<string, object?>>>> GetSubjectsAsync(ListQueryRequest request)
    {
        if (!QueryOptionsParser.TryParse(request, AllowedFields, AllowedFields, AllowedExpansions,
                out var options, out var errors))
            return ServiceResults.BadRequest<PagedResult<Dictionary<string, object?>>>(errors);

        var query = _subjectRepository.Query();
        if (options.Search is not null)
            query = query.Where(subject =>
                subject.SubjectCode.Contains(options.Search) || subject.SubjectName.Contains(options.Search));

        var totalItems = await query.CountAsync();
        query = ApplySorting(query, options.SortTerms);
        if (options.Expansions.Contains("courses")) query = query.Include(subject => subject.Courses);

        var entities = await query.Skip((options.Page - 1) * options.Size).Take(options.Size)
            .AsSplitQuery().ToListAsync();
        var fields = options.Fields.Count == 0
            ? AllowedFields.ToHashSet(StringComparer.OrdinalIgnoreCase)
            : options.Fields;
        var items = entities
            .Select(entity => ToBusinessModel(entity, options.Expansions.Contains("courses")))
            .Select(model => SelectFields(model, fields, options.Expansions))
            .ToArray();

        return ServiceResults.Success(new PagedResult<Dictionary<string, object?>>
        {
            Items = items,
            Pagination = Pagination(options, totalItems)
        });
    }

    public async Task<ServiceResult<Response.SubjectDetailResponse>> GetSubjectByIdAsync(int id)
    {
        var entity = await _subjectRepository.Query().Include(subject => subject.Courses).AsSplitQuery()
            .FirstOrDefaultAsync(subject => subject.SubjectId == id);
        if (entity is null)
            return ServiceResults.NotFound<Response.SubjectDetailResponse>($"Subject with id {id} was not found.");
        return ServiceResults.Success(ToDetailResponse(ToBusinessModel(entity, true)));
    }

    public async Task<ServiceResult<Response.SubjectResponse>> CreateSubjectAsync(Request.CreateSubjectRequest request)
    {
        var errors = Validate(request.SubjectCode, request.SubjectName, request.Credit);
        if (errors.Count > 0) return ServiceResults.BadRequest<Response.SubjectResponse>(errors.ToArray());

        var code = request.SubjectCode.Trim().ToUpperInvariant();
        if (await _subjectRepository.Query().AnyAsync(subject => subject.SubjectCode == code))
            return ServiceResults.BadRequest<Response.SubjectResponse>("SubjectCode already exists.");

        var entity = new Subject
        {
            SubjectCode = code,
            SubjectName = request.SubjectName.Trim(),
            Credit = request.Credit
        };
        await _subjectRepository.AddAsync(entity);
        await _subjectRepository.SaveChangesAsync();
        return ServiceResults.Created(ToResponse(ToBusinessModel(entity, false)));
    }

    public async Task<ServiceResult<Response.SubjectResponse>> UpdateSubjectAsync(
        int id,
        Request.UpdateSubjectRequest request)
    {
        var errors = Validate(request.SubjectCode, request.SubjectName, request.Credit);
        if (errors.Count > 0) return ServiceResults.BadRequest<Response.SubjectResponse>(errors.ToArray());

        var entity = await _subjectRepository.FindAsync(id);
        if (entity is null)
            return ServiceResults.NotFound<Response.SubjectResponse>($"Subject with id {id} was not found.");

        var code = request.SubjectCode.Trim().ToUpperInvariant();
        if (await _subjectRepository.Query().AnyAsync(subject => subject.SubjectCode == code && subject.SubjectId != id))
            return ServiceResults.BadRequest<Response.SubjectResponse>("SubjectCode already exists.");

        entity.SubjectCode = code;
        entity.SubjectName = request.SubjectName.Trim();
        entity.Credit = request.Credit;
        await _subjectRepository.SaveChangesAsync();
        return ServiceResults.Success(ToResponse(ToBusinessModel(entity, false)), "Subject updated successfully");
    }

    public async Task<ServiceResult<bool>> DeleteSubjectAsync(int id)
    {
        var entity = await _subjectRepository.FindAsync(id);
        if (entity is null) return ServiceResults.NotFound<bool>($"Subject with id {id} was not found.");
        if (await _courseRepository.Query().AnyAsync(course => course.SubjectId == id))
            return ServiceResults.BadRequest<bool>("The subject cannot be deleted because courses reference it.");

        _subjectRepository.Remove(entity);
        await _subjectRepository.SaveChangesAsync();
        return ServiceResults.Success(true, "Subject deleted successfully");
    }

    private static IQueryable<Subject> ApplySorting(IQueryable<Subject> query, IReadOnlyCollection<SortTerm> terms)
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

    private static List<string> Validate(string code, string name, int credit)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(code) || code.Trim().Length > 20)
            errors.Add("SubjectCode is required and must not exceed 20 characters.");
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 100)
            errors.Add("SubjectName is required and must not exceed 100 characters.");
        if (credit is < 1 or > 10) errors.Add("Credit must be between 1 and 10.");
        return errors;
    }

    private static SubjectBusinessModel ToBusinessModel(Subject entity, bool includeCourses) => new()
    {
        SubjectId = entity.SubjectId,
        SubjectCode = entity.SubjectCode,
        SubjectName = entity.SubjectName,
        Credit = entity.Credit,
        Courses = includeCourses
            ? entity.Courses.OrderBy(course => course.CourseId).Select(course => new CourseSummaryBusinessModel
            {
                CourseId = course.CourseId,
                CourseName = course.CourseName,
                SemesterId = course.SemesterId,
                SubjectId = course.SubjectId
            }).ToArray()
            : Array.Empty<CourseSummaryBusinessModel>()
    };

    private static Response.SubjectResponse ToResponse(SubjectBusinessModel model) => new()
    {
        SubjectId = model.SubjectId,
        SubjectCode = model.SubjectCode,
        SubjectName = model.SubjectName,
        Credit = model.Credit
    };

    private static Response.SubjectDetailResponse ToDetailResponse(SubjectBusinessModel model) => new()
    {
        SubjectId = model.SubjectId,
        SubjectCode = model.SubjectCode,
        SubjectName = model.SubjectName,
        Credit = model.Credit,
        Courses = model.Courses.Select(course => new Response.CourseResponse
        {
            CourseId = course.CourseId,
            CourseName = course.CourseName,
            SemesterId = course.SemesterId
        }).ToArray()
    };

    private static Dictionary<string, object?> SelectFields(
        SubjectBusinessModel model,
        HashSet<string> fields,
        HashSet<string> expansions)
    {
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        if (fields.Contains("subjectId")) result["subjectId"] = model.SubjectId;
        if (fields.Contains("subjectCode")) result["subjectCode"] = model.SubjectCode;
        if (fields.Contains("subjectName")) result["subjectName"] = model.SubjectName;
        if (fields.Contains("credit")) result["credit"] = model.Credit;
        if (expansions.Contains("courses")) result["courses"] = model.Courses;
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
