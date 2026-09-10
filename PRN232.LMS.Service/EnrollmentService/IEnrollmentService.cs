using PRN232.Lab1.Service.Base;

namespace PRN232.Lab1.Service.EnrollmentService;

public interface IEnrollmentService
{
    Task<ServiceResult<PagedResult<Dictionary<string, object?>>>> GetEnrollmentsAsync(ListQueryRequest request);
    Task<ServiceResult<Response.EnrollmentDetailResponse>> GetEnrollmentByIdAsync(int id);
    Task<ServiceResult<Response.EnrollmentResponse>> CreateEnrollmentAsync(Request.CreateEnrollmentRequest request);
    Task<ServiceResult<Response.EnrollmentResponse>> UpdateEnrollmentAsync(int id, Request.UpdateEnrollmentRequest request);
    Task<ServiceResult<bool>> DeleteEnrollmentAsync(int id);
}
