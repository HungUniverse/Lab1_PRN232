using Microsoft.AspNetCore.Mvc;
using PRN232.Lab1.Service.Base;
using PRN232.Lab1.Service.SemesterService;

namespace PRN232.Lab1.API.Controller;

[Route("api/semesters")]
[Produces("application/json")]
public class SemestersController : ApiControllerBase
{
    private readonly ISemesterService _service;
    public SemestersController(ISemesterService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<Dictionary<string, object?>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSemesters([FromQuery] ListQueryRequest query)
        => ToActionResult(await _service.GetSemestersAsync(query));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<Response.SemesterDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSemesterById(int id)
        => ToActionResult(await _service.GetSemesterByIdAsync(id));

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Response.SemesterResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSemester([FromBody] Request.CreateSemesterRequest request)
    {
        var result = await _service.CreateSemesterAsync(request);
        return ToCreatedResult(result, nameof(GetSemesterById), new { id = result.Data?.SemesterId });
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<Response.SemesterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSemester(int id, [FromBody] Request.UpdateSemesterRequest request)
        => ToActionResult(await _service.UpdateSemesterAsync(id, request));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSemester(int id)
        => ToActionResult(await _service.DeleteSemesterAsync(id));
}
