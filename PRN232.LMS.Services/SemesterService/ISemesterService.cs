using PRN232.Lab1.Service.Base;
using PRN232.Lab1.Service.Business;

namespace PRN232.Lab1.Service.SemesterService;

public interface ISemesterService
{
    Task<ServiceResult<PagedResult<SemesterModel>>> GetSemestersAsync(ListQueryModel query);
    Task<ServiceResult<SemesterModel>> GetSemesterByIdAsync(int id);
}
