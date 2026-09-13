using Microsoft.AspNetCore.Mvc;
using PRN232.Lab1.API.RequestModels;
using PRN232.Lab1.API.ResponseModels;
using PRN232.Lab1.Service.CourseService;

namespace PRN232.Lab1.API.Controllers;

[Route("api/[controller]")]
[Produces("application/json")]
public class CoursesController : ApiControllerBase
{
    private readonly ICourseService _service;
    public CoursesController(ICourseService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(PagedApiResponse<CourseResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCourses([FromQuery] CourseQueryRequest query)
        => ToPagedActionResult(
            await _service.GetCoursesAsync(query.ToModel()),
            model => model.ToResponse(query.Fields, query.Expand));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CourseDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCourseById(int id)
        => ToActionResult(await _service.GetCourseByIdAsync(id), model => model.ToDetailResponse());

}
