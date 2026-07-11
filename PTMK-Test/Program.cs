using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PTMK_Test.Application.Interface;
using PTMK_Test.Core.Implementation.Enums;
using PTMK_Test.Core.Implementation.Models;
using PTMK_Test.Infrastructure.Implementation;
using PTMK_Test.Web.Endpoints;

#if DEBUG
using PTMK_Test.Web.Endpoints.Tests;
#endif

#region Builder Registrations

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

builder.Services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
{
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

builder.Services.AddSwaggerGen();

var sqliteConnection = new SqliteConnection("Data Source=PtmkInMemoryDb;Mode=Memory;Cache=Shared");
sqliteConnection.Open();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(sqliteConnection));

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

#endregion

#region DI

builder.Services.AddScoped<IDbContext>(provider =>
    provider.GetRequiredService<AppDbContext>());

#endregion

#region App Middlewares

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();

    if (!context.Set<Employee>().Any())
    {
        var connection = context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            connection.Open();

        using var transaction = connection.BeginTransaction();

        var employeeIds = new List<Guid>(1000);

        var departments = Enum.GetNames<DepartmentType>();
        var positions = Enum.GetNames<PositionType>();

        using (var empCommand = connection.CreateCommand())
        {
            empCommand.Transaction = transaction;
            empCommand.CommandText = @"
                INSERT INTO Employees (ID, FirstName, Surname, MiddleName, Department, Position) 
                VALUES ($id, $firstName, $surname, $middleName, $department, $position);";

            var pId = empCommand.CreateParameter(); pId.ParameterName = "$id"; empCommand.Parameters.Add(pId);
            var pFn = empCommand.CreateParameter(); pFn.ParameterName = "$firstName"; empCommand.Parameters.Add(pFn);
            var pSn = empCommand.CreateParameter(); pSn.ParameterName = "$surname"; empCommand.Parameters.Add(pSn);
            var pMn = empCommand.CreateParameter(); pMn.ParameterName = "$middleName"; empCommand.Parameters.Add(pMn);
            var pDp = empCommand.CreateParameter(); pDp.ParameterName = "$department"; empCommand.Parameters.Add(pDp);
            var pPs = empCommand.CreateParameter(); pPs.ParameterName = "$position"; empCommand.Parameters.Add(pPs);

            for (int i = 0; i < 1000; i++)
            {
                var empId = Guid.NewGuid();
                employeeIds.Add(empId);

                pId.Value = empId;
                pFn.Value = $"Имя_{i}";
                pSn.Value = $"Фамилия_{i}";
                pMn.Value = i % 2 == 0 ? $"Отчество_{i}" : "";
                pDp.Value = departments[i % departments.Length];
                pPs.Value = positions[i % positions.Length];

                empCommand.ExecuteNonQuery();
            }
        }

        var statuses = new[] { "New", "InProgress", "Completed" };
        var now = DateTime.UtcNow;

        using (var appCommand = connection.CreateCommand())
        {
            appCommand.Transaction = transaction;
            appCommand.CommandText = @"
                INSERT INTO Applications (Id, Number, CreatedAt, AuthorId, ExecutorId, Description, Deadline, Status) 
                VALUES ($id, $number, $createdAt, $authorId, $executorId, $description, $deadline, $status);";

            var pAppId = appCommand.CreateParameter(); pAppId.ParameterName = "$id"; appCommand.Parameters.Add(pAppId);
            var pNum = appCommand.CreateParameter(); pNum.ParameterName = "$number"; appCommand.Parameters.Add(pNum);
            var pCreated = appCommand.CreateParameter(); pCreated.ParameterName = "$createdAt"; appCommand.Parameters.Add(pCreated);
            var pAuth = appCommand.CreateParameter(); pAuth.ParameterName = "$authorId"; appCommand.Parameters.Add(pAuth);
            var pExec = appCommand.CreateParameter(); pExec.ParameterName = "$executorId"; appCommand.Parameters.Add(pExec);
            var pDesc = appCommand.CreateParameter(); pDesc.ParameterName = "$description"; appCommand.Parameters.Add(pDesc);
            var pDead = appCommand.CreateParameter(); pDead.ParameterName = "$deadline"; appCommand.Parameters.Add(pDead);
            var pStat = appCommand.CreateParameter(); pStat.ParameterName = "$status"; appCommand.Parameters.Add(pStat);

            for (int i = 1; i <= 1_000_000; i++)
            {
                var authorId = employeeIds[i % employeeIds.Count];
                var executorId = employeeIds[(i + 1) % employeeIds.Count];

                bool isOverdue = i % 5 == 0;
                var createdAt = now.AddDays(-10);
                var deadline = isOverdue ? now.AddDays(-2) : now.AddDays(5);
                var status = isOverdue ? "InProgress" : statuses[i % statuses.Length];

                pAppId.Value = Guid.NewGuid();
                pNum.Value = $"ЗАВ-{i:D7}";
                pCreated.Value = createdAt.ToString("o");
                pAuth.Value = authorId;
                pExec.Value = executorId;
                pDesc.Value = $"Описание автоматической тестовой заявки номер {i}";
                pDead.Value = deadline.ToString("o");
                pStat.Value = status;

                appCommand.ExecuteNonQuery();
            }
        }

        transaction.Commit();
    }
}

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (ArgumentException ex) 
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            Type = "https://ietf.org",
            Title = "Ошибка валидации бизнес-правил",
            Status = 400,
            Detail = ex.Message
        });
    }
    catch (DbUpdateException ex) 
        when (ex.InnerException is SqliteException sqliteEx 
            && sqliteEx.SqliteErrorCode == 19)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            Title = "Ошибка ссылочной целостности",
            Status = 400
        });
    }
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler();
}

app.UseStatusCodePages();
app.UseHttpsRedirection();

#region Mapping Endpoints

#if DEBUG
app.MapDebugs();
#endif

app.MapEmployees();
app.MapApplications();

#endregion

app.Run();

#endregion