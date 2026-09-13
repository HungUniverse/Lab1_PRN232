using PRN232.Lab1.Service.Base;
using PRN232.Lab1.Service.Business;

namespace PRN232.Lab1.Service.CourseService;

public interface ICourseService
{
    Task<ServiceResult<PagedResult<CourseModel>>> GetCoursesAsync(ListQueryModel query);
    Task<ServiceResult<CourseModel>> GetCourseByIdAsync(int id);
}
