using Microsoft.AspNetCore.Mvc;
using PRN232.Lab1.API.ResponseModels;
using PRN232.Lab1.Service.Base;

namespace PRN232.Lab1.API.Controllers;

[ApiController]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult ToActionResult<TModel, TResponse>(
        ServiceResult<TModel> result,
        Func<TModel, TResponse> map)
    {
        return result.Status switch
        {
            ServiceStatus.Success => Ok(ApiResponse<TResponse>.Succeeded(map(result.Data!), result.Message)),
            ServiceStatus.Created => StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<TResponse>.Succeeded(map(result.Data!), result.Message)),
            ServiceStatus.BadRequest => BadRequest(ApiResponse<TResponse>.Failed(result.Message, result.Errors)),
            ServiceStatus.NotFound => NotFound(ApiResponse<TResponse>.Failed(result.Message, result.Errors)),
            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<TResponse>.Failed("An unexpected error occurred."))
        };
    }

    protected IActionResult ToActionResult<T>(ServiceResult<T> result)
        => ToActionResult(result, data => data);

    protected IActionResult ToPagedActionResult<TModel, TResponse>(
        ServiceResult<PagedResult<TModel>> result,
        Func<TModel, TResponse> map)
    {
        if (result.Status != ServiceStatus.Success || result.Data is null)
            return ToActionResult(result);

        return Ok(new PagedApiResponse<TResponse>
        {
            Success = true,
            Message = result.Message,
            Data = result.Data.Items.Select(map).ToArray(),
            Errors = null,
            Pagination = new PaginationResponse
            {
                Page = result.Data.Pagination.Page,
                PageSize = result.Data.Pagination.PageSize,
                TotalItems = result.Data.Pagination.TotalItems,
                TotalPages = result.Data.Pagination.TotalPages
            }
        });
    }

    protected IActionResult ToCreatedResult<TModel, TResponse>(
        ServiceResult<TModel> result,
        Func<TModel, TResponse> map,
        string actionName,
        object routeValues)
    {
        if (result.Status != ServiceStatus.Created)
            return ToActionResult(result, map);

        return CreatedAtAction(
            actionName,
            routeValues,
            ApiResponse<TResponse>.Succeeded(map(result.Data!), result.Message));
    }
}
