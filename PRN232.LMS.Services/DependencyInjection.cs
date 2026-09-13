using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PRN232.Lab1.Repository;
using PRN232.Lab1.Service.CourseService;
using PRN232.Lab1.Service.EnrollmentService;
using PRN232.Lab1.Service.SemesterService;
using PRN232.Lab1.Service.StudentService;
using PRN232.Lab1.Service.SubjectService;

namespace PRN232.Lab1.Service;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IStudentService, StudentService.StudentService>();
        services.AddScoped<ISemesterService, SemesterService.SemesterService>();
        services.AddScoped<ISubjectService, SubjectService.SubjectService>();
        services.AddScoped<ICourseService, CourseService.CourseService>();
        services.AddScoped<IEnrollmentService, EnrollmentService.EnrollmentService>();
        services.AddSingleton<IDatabaseLifecycleService, DatabaseLifecycleService>();
        return services;
    }
}

public interface IDatabaseLifecycleService
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task<bool> IsReadyAsync(CancellationToken cancellationToken = default);
}

internal sealed class DatabaseLifecycleService : IDatabaseLifecycleService
{
    private const int MaximumAttempts = 10;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);
    private readonly IServiceScopeFactory _scopeFactory;

    public DatabaseLifecycleService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        Exception? lastException = null;

        for (var attempt = 1; attempt <= MaximumAttempts; attempt++)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDBContext>();
                await dbContext.Database.MigrateAsync(cancellationToken);
                await DatabaseSeeder.SeedIfEmptyAsync(dbContext, cancellationToken);

                if (await HasRequiredSeedDataAsync(dbContext, cancellationToken))
                    return;

                throw new InvalidOperationException("Database seed data is incomplete.");
            }
            catch (Exception exception) when (attempt < MaximumAttempts)
            {
                lastException = exception;
                await Task.Delay(RetryDelay, cancellationToken);
            }
        }

        throw new InvalidOperationException(
            $"Database initialization failed after {MaximumAttempts} attempts.",
            lastException);
    }

    public async Task<bool> IsReadyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDBContext>();
            return await dbContext.Database.CanConnectAsync(cancellationToken)
                   && await HasRequiredSeedDataAsync(dbContext, cancellationToken);
        }
        catch
        {
            return false;
        }
    }

    private static async Task<bool> HasRequiredSeedDataAsync(
        AppDBContext dbContext,
        CancellationToken cancellationToken)
    {
        return await dbContext.Semesters.CountAsync(cancellationToken) >= 5
               && await dbContext.Subjects.CountAsync(cancellationToken) >= 10
               && await dbContext.Courses.CountAsync(cancellationToken) >= 20
               && await dbContext.Students.CountAsync(cancellationToken) >= 50
               && await dbContext.Enrollments.CountAsync(cancellationToken) >= 500;
    }
}
