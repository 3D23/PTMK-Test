using Microsoft.EntityFrameworkCore;
using PTMK_Test.Application.Common;
using PTMK_Test.Core.Implementation.Models;
using PTMK_Test.Infrastructure.Implementation;
using System.Diagnostics;

namespace PTMK_Test.Web.Endpoints.Tests
{
    public static class DebugEnpointsMapper
    {
        public static void MapDebugs(this IEndpointRouteBuilder builder)
        {
            var debugGroup = builder.MapGroup("api/debug");

            debugGroup.MapGet("debug-index-check", CheckIndexWorking)
                .WithName("DebugIndexCheck");

            debugGroup.MapGet("benchmark-indexes-speed", BenchmarkIndexesSpeed)
                .WithName("BenchmarkIndexesSpeed");

            debugGroup.MapGet("benchmark-filter-endpoint", BenchmarkFilterEndpoint)
                .WithName("BenchmarkFilterEndpoint");
        }

        #region Endpoints Handlers

        private static async Task<IResult> CheckIndexWorking(AppDbContext context)
        {
            var connection = context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            using var command = connection.CreateCommand();

            command.CommandText = @"
                EXPLAIN QUERY PLAN
                SELECT * FROM Applications a
                INNER JOIN Employees e ON a.ExecutorId = e.ID
                WHERE a.ExecutorId = $execId 
                  AND a.Status = 'InProgress' 
                  AND a.Deadline < $now";

            var pExec = command.CreateParameter(); pExec.ParameterName = "$execId"; pExec.Value = Guid.NewGuid().ToString(); command.Parameters.Add(pExec);
            var pNow = command.CreateParameter(); pNow.ParameterName = "$now"; pNow.Value = DateTime.UtcNow.ToString("o"); command.Parameters.Add(pNow);

            var explanationLines = new List<string>();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                explanationLines.Add(reader.GetString(3));
            }

            return Results.Ok(new
            {
                Message = "Анализ плана выполнения запроса SQLite",
                QueryPlan = explanationLines
            });
        }

        private static async Task<IResult> BenchmarkIndexesSpeed(AppDbContext context, CancellationToken ct)
        {
            var connection = context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            var sampleExecutorId = await context.Set<Employee>()
                .Select(e => e.ID)
                .FirstOrDefaultAsync(ct);

            var nowStr = DateTime.UtcNow.ToString("o");

            using (var dropCommand = connection.CreateCommand())
            {
                dropCommand.CommandText = "DROP INDEX IF EXISTS IX_Applications_ExecutorId;";
                dropCommand.ExecuteNonQuery();
            }

            var stopwatchWithoutIndex = Stopwatch.StartNew();

            using (var queryCommand = connection.CreateCommand())
            {
                queryCommand.CommandText = @"
                    SELECT COUNT(*) FROM Applications 
                    WHERE ExecutorId = $execId AND Status = 'InProgress' AND Deadline < $now";

                var pExec = queryCommand.CreateParameter(); 
                pExec.ParameterName = "$execId";
                pExec.Value = sampleExecutorId.ToString();
                queryCommand.Parameters.Add(pExec);

                var pNow = queryCommand.CreateParameter();
                pNow.ParameterName = "$now";
                pNow.Value = nowStr; 
                queryCommand.Parameters.Add(pNow);

                queryCommand.ExecuteScalar();
            }

            stopwatchWithoutIndex.Stop();
            double timeWithoutIndexMs = stopwatchWithoutIndex.Elapsed.TotalMilliseconds;

            using (var createCommand = connection.CreateCommand())
            {
                createCommand.CommandText = "CREATE INDEX IF NOT EXISTS IX_Applications_ExecutorId ON Applications (ExecutorId);";
                createCommand.ExecuteNonQuery();
            }

            var stopwatchWithIndex = Stopwatch.StartNew();

            using (var queryCommand = connection.CreateCommand())
            {
                queryCommand.CommandText = @"
                    SELECT COUNT(*) FROM Applications 
                    WHERE ExecutorId = $execId AND Status = 'InProgress' AND Deadline < $now";

                var pExec = queryCommand.CreateParameter(); 
                pExec.ParameterName = "$execId"; 
                pExec.Value = sampleExecutorId.ToString(); 
                queryCommand.Parameters.Add(pExec);

                var pNow = queryCommand.CreateParameter(); 
                pNow.ParameterName = "$now"; 
                pNow.Value = nowStr; 
                queryCommand.Parameters.Add(pNow);

                queryCommand.ExecuteScalar();
            }

            stopwatchWithIndex.Stop();
            double timeWithIndexMs = stopwatchWithIndex.Elapsed.TotalMilliseconds;

            double speedupFactor = timeWithoutIndexMs / (timeWithIndexMs > 0 ? timeWithIndexMs : 0.1);

            return Results.Ok(new
            {
                Message = "Результаты сравнительного тестирования производительности на 1 000 000 строк",
                TargetExecutorId = sampleExecutorId,
                TimeWithoutIndex = $"{timeWithoutIndexMs:F2} мс (Full Table Scan)",
                TimeWithIndex = $"{timeWithIndexMs:F2} мс (Index Search)",
                Efficiency = $"Запрос с индексом работает быстрее в {speedupFactor:F1} раз(а)!"
            });
        }

        private static async Task<IResult> BenchmarkFilterEndpoint(
            AppDbContext context,
            HttpContext httpContext,
            [AsParameters] PaginationParameters pagination,
            CancellationToken ct)
        {
            var connection = context.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            var sampleExecutorId = await context.Set<Employee>()
                .Select(e => e.ID)
                .FirstOrDefaultAsync(ct);

            var requestScheme = httpContext.Request.Scheme;
            var requestHost = httpContext.Request.Host;

            var filterUrl = $"{requestScheme}://{requestHost}/api/applications/filter" +
                            $"?status=InProgress" +
                            $"&executorId={sampleExecutorId}" +
                            $"&isOverdue=true" +
                            $"&pageNumber={pagination.PageNumber}" +
                            $"&pageSize={pagination.PageSize}";

            using var httpClient = new HttpClient();

            using (var dropCommand = connection.CreateCommand())
            {
                dropCommand.CommandText = "DROP INDEX IF EXISTS IX_Applications_ExecutorId;";
                dropCommand.ExecuteNonQuery();
            }

            await httpClient.GetAsync(filterUrl, ct);

            var swWithoutIndex = Stopwatch.StartNew();
            var responseWithoutIndex = await httpClient.GetAsync(filterUrl, ct);
            swWithoutIndex.Stop();
            double timeWithoutIndex = swWithoutIndex.Elapsed.TotalMilliseconds;

            using (var createCommand = connection.CreateCommand())
            {
                createCommand.CommandText = "CREATE INDEX IF NOT EXISTS IX_Applications_ExecutorId ON Applications (ExecutorId);";
                createCommand.ExecuteNonQuery();
            }

            var swWithIndex = Stopwatch.StartNew();
            var responseWithIndex = await httpClient.GetAsync(filterUrl, ct);
            swWithIndex.Stop();
            double timeWithIndex = swWithIndex.Elapsed.TotalMilliseconds;

            double ratio = timeWithoutIndex / (timeWithIndex > 0 ? timeWithIndex : 0.1);

            return Results.Ok(new
            {
                Message = "Сравнительный анализ скорости HTTP-эндпоинта фильтрации на 1 000 000 строк",
                TestedUrl = filterUrl,
                RequestedPageNumber = pagination.PageNumber,
                RequestedPageSize = pagination.PageSize,
                HttpResponseStatusWithoutIndex = responseWithoutIndex.StatusCode,
                HttpResponseStatusWithIndex = responseWithIndex.StatusCode,
                EndpointTimeWithoutIndex = $"{timeWithoutIndex:F2} мс (СУБД перебирает миллион строк)",
                EndpointTimeWithIndex = $"{timeWithIndex:F2} мс (СУБД использует B-Tree индекс)",
                PerformanceGain = $"Эндпоинт фильтрации с индексом стал быстрее в {ratio:F1} раз(а)!"
            });
        }

        #endregion
    }
}
