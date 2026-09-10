namespace PRN232.Lab1.Service.Base;

public enum ServiceStatus
{
    Success,
    Created,
    BadRequest,
    NotFound
}

public class ServiceResult<T>
{
    public ServiceStatus Status { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public IReadOnlyCollection<string>? Errors { get; init; }
}

public static class ServiceResults
{
    public static ServiceResult<T> Success<T>(T data, string message = "Request processed successfully")
        => new() { Status = ServiceStatus.Success, Data = data, Message = message };

    public static ServiceResult<T> Created<T>(T data, string message = "Resource created successfully")
        => new() { Status = ServiceStatus.Created, Data = data, Message = message };

    public static ServiceResult<T> BadRequest<T>(params string[] errors)
        => new()
        {
            Status = ServiceStatus.BadRequest,
            Message = "The request is invalid",
            Errors = errors
        };

    public static ServiceResult<T> NotFound<T>(string message)
        => new()
        {
            Status = ServiceStatus.NotFound,
            Message = message,
            Errors = new[] { message }
        };
}
