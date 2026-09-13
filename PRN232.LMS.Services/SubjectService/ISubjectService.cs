using PRN232.Lab1.Service.Base;
using PRN232.Lab1.Service.Business;

namespace PRN232.Lab1.Service.SubjectService;

public interface ISubjectService
{
    Task<ServiceResult<PagedResult<SubjectModel>>> GetSubjectsAsync(ListQueryModel query);
    Task<ServiceResult<SubjectModel>> GetSubjectByIdAsync(int id);
}
