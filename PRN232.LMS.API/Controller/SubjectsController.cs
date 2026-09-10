using Microsoft.AspNetCore.Mvc;
using PRN232.Lab1.Service.Base;
using PRN232.Lab1.Service.SubjectService;

namespace PRN232.Lab1.API.Controller;

[Route("api/subjects")]
[Produces("application/json")]
public class SubjectsController : ApiControllerBase
{
    private readonly ISubjectService _service;
    public SubjectsController(ISubjectService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<Dictionary<string, object?>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSubjects([FromQuery] ListQueryRequest query)
        => ToActionResult(await _service.GetSubjectsAsync(query));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<Response.SubjectDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubjectById(int id)
        => ToActionResult(await _service.GetSubjectByIdAsync(id));

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Response.SubjectResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSubject([FromBody] Request.CreateSubjectRequest request)
    {
        var result = await _service.CreateSubjectAsync(request);
        return ToCreatedResult(result, nameof(GetSubjectById), new { id = result.Data?.SubjectId });
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<Response.SubjectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSubject(int id, [FromBody] Request.UpdateSubjectRequest request)
        => ToActionResult(await _service.UpdateSubjectAsync(id, request));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSubject(int id)
        => ToActionResult(await _service.DeleteSubjectAsync(id));
}
