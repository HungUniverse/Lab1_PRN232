using PRN232.Lab1.API.ResponseModels;
using PRN232.Lab1.Repository;
using PRN232.Lab1.Service;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? "The request body is invalid."
                    : error.ErrorMessage)
                .ToArray();
            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(
                ApiResponse<object>.Failed("The request is invalid", errors));
        };
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    options.CustomSchemaIds(type => (type.FullName ?? type.Name).Replace('+', '.')));

builder.Services.AddRepositories(connectionString);
builder.Services.AddServices();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

var databaseLifecycle = app.Services.GetRequiredService<IDatabaseLifecycleService>();
await databaseLifecycle.InitializeAsync(app.Lifetime.ApplicationStopping);

app.MapGet("/health", async (CancellationToken cancellationToken) =>
    await databaseLifecycle.IsReadyAsync(cancellationToken)
        ? Results.Ok(ApiResponse<HealthResponse>.Succeeded(
            new HealthResponse { Status = "Healthy" },
            "Database is ready"))
        : Results.Json(
            ApiResponse<object>.Failed("Database is not ready"),
            statusCode: StatusCodes.Status503ServiceUnavailable))
    .Produces<ApiResponse<HealthResponse>>(StatusCodes.Status200OK)
    .Produces<ApiResponse<object>>(StatusCodes.Status503ServiceUnavailable)
    .WithName("Health");

app.Run();
