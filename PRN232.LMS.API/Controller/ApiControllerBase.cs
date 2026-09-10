using Microsoft.AspNetCore.Mvc;
using PRN232.Lab1.Service.Base;

namespace PRN232.Lab1.API.Controller;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult ToActionResult<T>(ServiceResult<T> result)
    {
        return result.Status switch
        {
            ServiceStatus.Success => Ok(ApiResponse<T>.Succeeded(result.Data!, result.Message)),
            ServiceStatus.Created => StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<T>.Succeeded(result.Data!, result.Message)),
            ServiceStatus.BadRequest => BadRequest(ApiResponse<T>.Failed(result.Message, result.Errors)),
            ServiceStatus.NotFound => NotFound(ApiResponse<T>.Failed(result.Message, result.Errors)),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<T>.Failed("An unexpected error occurred."))
        };
    }

    protected IActionResult ToCreatedResult<T>(
        ServiceResult<T> result,
        string actionName,
        object routeValues)
    {
        if (result.Status != ServiceStatus.Created)
            return ToActionResult(result);

        return CreatedAtAction(
            actionName,
            routeValues,
            ApiResponse<T>.Succeeded(result.Data!, result.Message));
    }
}
