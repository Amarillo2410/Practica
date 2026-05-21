using Api.Helpers.Errors;
using Application;
using Infrastructure;
using Infrastructure.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
var postgresConnectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("Connection string 'Postgres' is required.");

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var problemDetails = new ValidationProblemDetails(context.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Bad Request",
            Detail = "One or more validation errors occurred.",
            Instance = context.HttpContext.Request.Path
        };

        return new BadRequestObjectResult(problemDetails)
        {
            ContentTypes = { "application/json" }
        };
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await EnsureDatabaseExistsAsync(postgresConnectionString);

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

try
{
    await dbContext.Database.MigrateAsync();
    logger.LogInformation("Database migration completed successfully.");
}
catch (Exception ex)
{
    logger.LogError(ex, "Database migration failed during startup.");
    throw new InvalidOperationException("Failed to run database migrations during startup.", ex);
}

app.Run();

static async Task EnsureDatabaseExistsAsync(string connectionString, CancellationToken ct = default)
{
    var appConnectionString = new NpgsqlConnectionStringBuilder(connectionString);

    if (string.IsNullOrWhiteSpace(appConnectionString.Database))
    {
        throw new InvalidOperationException("Database name is required in the Postgres connection string.");
    }

    var maintenanceConnectionString = new NpgsqlConnectionStringBuilder(connectionString)
    {
        Database = "postgres"
    };

    await using var connection = new NpgsqlConnection(maintenanceConnectionString.ConnectionString);
    await connection.OpenAsync(ct);

    await using var existsCommand = connection.CreateCommand();
    existsCommand.CommandText = "SELECT 1 FROM pg_database WHERE datname = @databaseName";
    existsCommand.Parameters.AddWithValue("databaseName", appConnectionString.Database);

    var exists = await existsCommand.ExecuteScalarAsync(ct) is not null;
    if (exists)
    {
        return;
    }

    var escapedDatabaseName = "\"" + appConnectionString.Database.Replace("\"", "\"\"") + "\"";
    await using var createCommand = connection.CreateCommand();
    createCommand.CommandText = $"CREATE DATABASE {escapedDatabaseName}";
    await createCommand.ExecuteNonQueryAsync(ct);
}
