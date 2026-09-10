using Microsoft.AspNetCore.Mvc;
using PRN232.Lab1.Service.Base;
using PRN232.Lab1.Service.StudentService;

namespace PRN232.Lab1.API.Controller;

[Route("api/students")]
[Produces("application/json")]
public class StudentsController : ApiControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<Dictionary<string, object?>>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetStudents([FromQuery] ListQueryRequest query)
        => ToActionResult(await _studentService.GetStudentsAsync(query));

    [HttpGet("{id:int}", Name = nameof(GetStudentById))]
    [ProducesResponseType(typeof(ApiResponse<Response.StudentDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentById(int id)
        => ToActionResult(await _studentService.GetStudentByIdAsync(id));

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Response.StudentResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateStudent([FromBody] Request.CreateStudentRequest request)
    {
        var result = await _studentService.CreateStudentAsync(request);
        return ToCreatedResult(result, nameof(GetStudentById), new { id = result.Data?.StudentId });
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<Response.StudentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStudent(int id, [FromBody] Request.UpdateStudentRequest request)
        => ToActionResult(await _studentService.UpdateStudentAsync(id, request));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteStudent(int id)
        => ToActionResult(await _studentService.DeleteStudentAsync(id));
}
