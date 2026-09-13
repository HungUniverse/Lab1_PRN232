using PRN232.Lab1.Service.Base;
using PRN232.Lab1.Service.Business;

namespace PRN232.Lab1.Service.StudentService;

public interface IStudentService
{
    Task<ServiceResult<PagedResult<StudentModel>>> GetStudentsAsync(ListQueryModel query);
    Task<ServiceResult<StudentModel>> GetStudentByIdAsync(int id);
    Task<ServiceResult<StudentModel>> CreateStudentAsync(CreateStudentModel model);
    Task<ServiceResult<StudentModel>> UpdateStudentAsync(int id, UpdateStudentModel model);
    Task<ServiceResult<bool>> DeleteStudentAsync(int id);
}
