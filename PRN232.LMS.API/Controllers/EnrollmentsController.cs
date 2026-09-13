using Microsoft.AspNetCore.Mvc;
using PRN232.Lab1.API.RequestModels;
using PRN232.Lab1.API.ResponseModels;
using PRN232.Lab1.Service.EnrollmentService;

namespace PRN232.Lab1.API.Controllers;

[Route("api/[controller]")]
[Produces("application/json")]
public class EnrollmentsController : ApiControllerBase
{
    private readonly IEnrollmentService _service;
    public EnrollmentsController(IEnrollmentService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(PagedApiResponse<EnrollmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetEnrollments([FromQuery] EnrollmentQueryRequest query)
        => ToPagedActionResult(
            await _service.GetEnrollmentsAsync(query.ToModel()),
            model => model.ToResponse(query.Fields, query.Expand));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEnrollmentById(int id)
        => ToActionResult(await _service.GetEnrollmentByIdAsync(id), model => model.ToDetailResponse());

}
