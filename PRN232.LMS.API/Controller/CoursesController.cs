using Microsoft.AspNetCore.Mvc;
using PRN232.Lab1.Service.Base;
using PRN232.Lab1.Service.CourseService;

namespace PRN232.Lab1.API.Controller;

[Route("api/courses")]
[Produces("application/json")]
public class CoursesController : ApiControllerBase
{
    private readonly ICourseService _service;
    public CoursesController(ICourseService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<Dictionary<string, object?>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCourses([FromQuery] ListQueryRequest query)
        => ToActionResult(await _service.GetCoursesAsync(query));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<Response.CourseDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCourseById(int id)
        => ToActionResult(await _service.GetCourseByIdAsync(id));

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Response.CourseResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCourse([FromBody] Request.CreateCourseRequest request)
    {
        var result = await _service.CreateCourseAsync(request);
        return ToCreatedResult(result, nameof(GetCourseById), new { id = result.Data?.CourseId });
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<Response.CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCourse(int id, [FromBody] Request.UpdateCourseRequest request)
        => ToActionResult(await _service.UpdateCourseAsync(id, request));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCourse(int id)
        => ToActionResult(await _service.DeleteCourseAsync(id));
}
