using PRN232.Lab1.Service.Base;

namespace PRN232.Lab1.Service.StudentService;

public interface IStudentService
{
    Task<ServiceResult<PagedResult<Dictionary<string, object?>>>> GetStudentsAsync(ListQueryRequest request);
    Task<ServiceResult<Response.StudentDetailResponse>> GetStudentByIdAsync(int id);
    Task<ServiceResult<Response.StudentResponse>> CreateStudentAsync(Request.CreateStudentRequest request);
    Task<ServiceResult<Response.StudentResponse>> UpdateStudentAsync(int id, Request.UpdateStudentRequest request);
    Task<ServiceResult<bool>> DeleteStudentAsync(int id);
}
