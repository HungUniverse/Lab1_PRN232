using PRN232.Lab1.Service.Base;

namespace PRN232.Lab1.Service.SubjectService;

public interface ISubjectService
{
    Task<ServiceResult<PagedResult<Dictionary<string, object?>>>> GetSubjectsAsync(ListQueryRequest request);
    Task<ServiceResult<Response.SubjectDetailResponse>> GetSubjectByIdAsync(int id);
    Task<ServiceResult<Response.SubjectResponse>> CreateSubjectAsync(Request.CreateSubjectRequest request);
    Task<ServiceResult<Response.SubjectResponse>> UpdateSubjectAsync(int id, Request.UpdateSubjectRequest request);
    Task<ServiceResult<bool>> DeleteSubjectAsync(int id);
}
