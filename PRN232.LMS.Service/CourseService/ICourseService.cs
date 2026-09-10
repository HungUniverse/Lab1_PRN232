using PRN232.Lab1.Service.Base;

namespace PRN232.Lab1.Service.CourseService;

public interface ICourseService
{
    Task<ServiceResult<PagedResult<Dictionary<string, object?>>>> GetCoursesAsync(ListQueryRequest request);
    Task<ServiceResult<Response.CourseDetailResponse>> GetCourseByIdAsync(int id);
    Task<ServiceResult<Response.CourseResponse>> CreateCourseAsync(Request.CreateCourseRequest request);
    Task<ServiceResult<Response.CourseResponse>> UpdateCourseAsync(int id, Request.UpdateCourseRequest request);
    Task<ServiceResult<bool>> DeleteCourseAsync(int id);
}
