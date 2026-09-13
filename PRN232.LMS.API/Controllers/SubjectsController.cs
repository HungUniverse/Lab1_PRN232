using Microsoft.AspNetCore.Mvc;
using PRN232.Lab1.API.RequestModels;
using PRN232.Lab1.API.ResponseModels;
using PRN232.Lab1.Service.SubjectService;

namespace PRN232.Lab1.API.Controllers;

[Route("api/[controller]")]
[Produces("application/json")]
public class SubjectsController : ApiControllerBase
{
    private readonly ISubjectService _service;
    public SubjectsController(ISubjectService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(PagedApiResponse<SubjectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSubjects([FromQuery] SubjectQueryRequest query)
        => ToPagedActionResult(
            await _service.GetSubjectsAsync(query.ToModel()),
            model => model.ToResponse(query.Fields, query.Expand));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<SubjectDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubjectById(int id)
        => ToActionResult(await _service.GetSubjectByIdAsync(id), model => model.ToDetailResponse());

}
