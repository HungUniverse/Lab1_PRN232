using PRN232.Lab1.Service.Base;
using PRN232.Lab1.Service.Business;

namespace PRN232.Lab1.Service.EnrollmentService;

public interface IEnrollmentService
{
    Task<ServiceResult<PagedResult<EnrollmentModel>>> GetEnrollmentsAsync(ListQueryModel query);
    Task<ServiceResult<EnrollmentModel>> GetEnrollmentByIdAsync(int id);
}
