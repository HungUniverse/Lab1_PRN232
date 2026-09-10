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
    private readonly IRepository<Course> _courseRepository;

    public SemesterService(IRepository<Semester> semesterRepository, IRepository<Course> courseRepository)
    {
        _semesterRepository = semesterRepository;
        _courseRepository = courseRepository;
    }

    public async Task<ServiceResult<PagedResult<Dictionary<string, object?>>>> GetSemestersAsync(ListQueryRequest request)
    {
        if (!QueryOptionsParser.TryParse(request, AllowedFields, AllowedFields, AllowedExpansions,
                out var options, out var errors))
            return ServiceResults.BadRequest<PagedResult<Dictionary<string, object?>>>(errors);

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

    public async Task<ServiceResult<Response.SemesterDetailResponse>> GetSemesterByIdAsync(int id)
    {
        var entity = await _semesterRepository.Query()
            .Include(semester => semester.Courses)
            .AsSplitQuery()
            .FirstOrDefaultAsync(semester => semester.SemesterId == id);
        if (entity is null)
            return ServiceResults.NotFound<Response.SemesterDetailResponse>($"Semester with id {id} was not found.");

        return ServiceResults.Success(ToDetailResponse(ToBusinessModel(entity, true)));
    }

    public async Task<ServiceResult<Response.SemesterResponse>> CreateSemesterAsync(Request.CreateSemesterRequest request)
    {
        var errors = Validate(request.SemesterName, request.StartDate, request.EndDate);
        if (errors.Count > 0)
            return ServiceResults.BadRequest<Response.SemesterResponse>(errors.ToArray());

        var name = request.SemesterName.Trim();
        if (await _semesterRepository.Query().AnyAsync(semester => semester.SemesterName == name))
            return ServiceResults.BadRequest<Response.SemesterResponse>("SemesterName already exists.");

        var entity = new Semester
        {
            SemesterName = name,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };
        await _semesterRepository.AddAsync(entity);
        await _semesterRepository.SaveChangesAsync();
        return ServiceResults.Created(ToResponse(ToBusinessModel(entity, false)));
    }

    public async Task<ServiceResult<Response.SemesterResponse>> UpdateSemesterAsync(
        int id,
        Request.UpdateSemesterRequest request)
    {
        var errors = Validate(request.SemesterName, request.StartDate, request.EndDate);
        if (errors.Count > 0)
            return ServiceResults.BadRequest<Response.SemesterResponse>(errors.ToArray());

        var entity = await _semesterRepository.FindAsync(id);
        if (entity is null)
            return ServiceResults.NotFound<Response.SemesterResponse>($"Semester with id {id} was not found.");

        var name = request.SemesterName.Trim();
        if (await _semesterRepository.Query().AnyAsync(semester =>
                semester.SemesterName == name && semester.SemesterId != id))
            return ServiceResults.BadRequest<Response.SemesterResponse>("SemesterName already exists.");

        entity.SemesterName = name;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        await _semesterRepository.SaveChangesAsync();
        return ServiceResults.Success(ToResponse(ToBusinessModel(entity, false)), "Semester updated successfully");
    }

    public async Task<ServiceResult<bool>> DeleteSemesterAsync(int id)
    {
        var entity = await _semesterRepository.FindAsync(id);
        if (entity is null)
            return ServiceResults.NotFound<bool>($"Semester with id {id} was not found.");
        if (await _courseRepository.Query().AnyAsync(course => course.SemesterId == id))
            return ServiceResults.BadRequest<bool>("The semester cannot be deleted because courses reference it.");

        _semesterRepository.Remove(entity);
        await _semesterRepository.SaveChangesAsync();
        return ServiceResults.Success(true, "Semester deleted successfully");
    }

    private static IQueryable<Semester> ApplySorting(IQueryable<Semester> query, IReadOnlyCollection<SortTerm> terms)
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

    private static List<string> Validate(string name, DateTime startDate, DateTime endDate)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 100)
            errors.Add("SemesterName is required and must not exceed 100 characters.");
        if (startDate == default) errors.Add("StartDate is required.");
        if (endDate <= startDate) errors.Add("EndDate must be later than StartDate.");
        return errors;
    }

    private static SemesterBusinessModel ToBusinessModel(Semester entity, bool includeCourses)
    {
        return new SemesterBusinessModel
        {
            SemesterId = entity.SemesterId,
            SemesterName = entity.SemesterName,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
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
    }

    private static Response.SemesterResponse ToResponse(SemesterBusinessModel model) => new()
    {
        SemesterId = model.SemesterId,
        SemesterName = model.SemesterName,
        StartDate = model.StartDate,
        EndDate = model.EndDate
    };

    private static Response.SemesterDetailResponse ToDetailResponse(SemesterBusinessModel model) => new()
    {
        SemesterId = model.SemesterId,
        SemesterName = model.SemesterName,
        StartDate = model.StartDate,
        EndDate = model.EndDate,
        Courses = model.Courses.Select(course => new Response.CourseResponse
        {
            CourseId = course.CourseId,
            CourseName = course.CourseName,
            SubjectId = course.SubjectId
        }).ToArray()
    };

    private static Dictionary<string, object?> SelectFields(
        SemesterBusinessModel model,
        HashSet<string> fields,
        HashSet<string> expansions)
    {
        var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        if (fields.Contains("semesterId")) result["semesterId"] = model.SemesterId;
        if (fields.Contains("semesterName")) result["semesterName"] = model.SemesterName;
        if (fields.Contains("startDate")) result["startDate"] = model.StartDate;
        if (fields.Contains("endDate")) result["endDate"] = model.EndDate;
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
