using PRN232.Lab1.Service.Base;

namespace PRN232.Lab1.Service.SemesterService;

public interface ISemesterService
{
    Task<ServiceResult<PagedResult<Dictionary<string, object?>>>> GetSemestersAsync(ListQueryRequest request);
    Task<ServiceResult<Response.SemesterDetailResponse>> GetSemesterByIdAsync(int id);
    Task<ServiceResult<Response.SemesterResponse>> CreateSemesterAsync(Request.CreateSemesterRequest request);
    Task<ServiceResult<Response.SemesterResponse>> UpdateSemesterAsync(int id, Request.UpdateSemesterRequest request);
    Task<ServiceResult<bool>> DeleteSemesterAsync(int id);
}
