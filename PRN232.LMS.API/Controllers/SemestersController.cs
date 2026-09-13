using Microsoft.AspNetCore.Mvc;
using PRN232.Lab1.API.RequestModels;
using PRN232.Lab1.API.ResponseModels;
using PRN232.Lab1.Service.SemesterService;

namespace PRN232.Lab1.API.Controllers;

[Route("api/[controller]")]
[Produces("application/json")]
public class SemestersController : ApiControllerBase
{
    private readonly ISemesterService _service;
    public SemestersController(ISemesterService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(PagedApiResponse<SemesterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSemesters([FromQuery] SemesterQueryRequest query)
        => ToPagedActionResult(
            await _service.GetSemestersAsync(query.ToModel()),
            model => model.ToResponse(query.Fields, query.Expand));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SemesterDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSemesterById(int id)
        => ToActionResult(await _service.GetSemesterByIdAsync(id), model => model.ToDetailResponse());

}
