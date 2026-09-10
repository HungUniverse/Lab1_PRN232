using Microsoft.AspNetCore.Mvc;
using PRN232.Lab1.Service.Base;
using PRN232.Lab1.Service.EnrollmentService;

namespace PRN232.Lab1.API.Controller;

[Route("api/enrollments")]
[Produces("application/json")]
public class EnrollmentsController : ApiControllerBase
{
    private readonly IEnrollmentService _service;
    public EnrollmentsController(IEnrollmentService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<Dictionary<string, object?>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetEnrollments([FromQuery] ListQueryRequest query)
        => ToActionResult(await _service.GetEnrollmentsAsync(query));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<Response.EnrollmentDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEnrollmentById(int id)
        => ToActionResult(await _service.GetEnrollmentByIdAsync(id));

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Response.EnrollmentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateEnrollment([FromBody] Request.CreateEnrollmentRequest request)
    {
        var result = await _service.CreateEnrollmentAsync(request);
        return ToCreatedResult(result, nameof(GetEnrollmentById), new { id = result.Data?.EnrollmentId });
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<Response.EnrollmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEnrollment(int id, [FromBody] Request.UpdateEnrollmentRequest request)
        => ToActionResult(await _service.UpdateEnrollmentAsync(id, request));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEnrollment(int id)
        => ToActionResult(await _service.DeleteEnrollmentAsync(id));
}
