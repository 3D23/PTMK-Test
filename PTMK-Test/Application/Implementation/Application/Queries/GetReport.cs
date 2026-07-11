using MediatR;
using Microsoft.EntityFrameworkCore;
using PTMK_Test.Application.Common;
using PTMK_Test.Application.Implementation.Specifications.Application;
using PTMK_Test.Application.Interface;
using PTMK_Test.Core.Implementation.Enums;

namespace PTMK_Test.Application.Implementation.Application.Queries
{
    public record struct ExecutorReportItem(Guid ExecutorId, int CompletedCount);

    public sealed record ApplicationsReportDto(
        Dictionary<string, int> CountByStatus,
        int TotalOverdueCount,
        List<ExecutorReportItem> CompletedByExecutors);

    public readonly record struct GetApplicationsReportQuery : 
        IRequest<RequestResult<ApplicationsReportDto>>;

    public sealed class GetApplicationsReportHandler(IDbContext dbContext)
       : IRequestHandler<GetApplicationsReportQuery, RequestResult<ApplicationsReportDto>>
    {
        private readonly IDbContext _dbContext = dbContext;

        public async Task<RequestResult<ApplicationsReportDto>> Handle(GetApplicationsReportQuery request, CancellationToken ct)
        {
            var newSpec = new StatusSpecification(ApplicationStatusType.New).ToExpression();
            var inProgressSpec = new StatusSpecification(ApplicationStatusType.InProgress).ToExpression();
            var completedSpec = new StatusSpecification(ApplicationStatusType.Completed).ToExpression();
            var overdueSpec = new OverdueSpecification(isOverdue: true).ToExpression();

            var summaryMetrics = await _dbContext.Applications
                .GroupBy(a => 1)
                .Select(g => new
                {
                    NewCount = g.AsQueryable().Where(newSpec).Count(),
                    InProgressCount = g.AsQueryable().Where(inProgressSpec).Count(),
                    CompletedCount = g.AsQueryable().Where(completedSpec).Count(),
                    OverdueCount = g.AsQueryable().Where(overdueSpec).Count()
                })
                .FirstOrDefaultAsync(ct);

            if (summaryMetrics == null)
                return RequestResult<ApplicationsReportDto>.Failure("Не удалось рассчитать метрики для отчета.");

            var reportByStatus = new Dictionary<string, int>
            {
                { nameof(ApplicationStatusType.New), summaryMetrics.NewCount },
                { nameof(ApplicationStatusType.InProgress), summaryMetrics.InProgressCount },
                { nameof(ApplicationStatusType.Completed), summaryMetrics.CompletedCount }
            };

            int totalOverdue = summaryMetrics.OverdueCount;

            var completedExecutorIds = await _dbContext.Applications
                .Where(completedSpec)
                .Select(a => a.ExecutorId)
                .ToListAsync(ct);

            var completedByExecutors = completedExecutorIds
                .GroupBy(id => id)
                .Select(g => new ExecutorReportItem(g.Key, g.Count()))
                .OrderByDescending(item => item.CompletedCount)
                .ToList();

            var dto = new ApplicationsReportDto(reportByStatus, totalOverdue, completedByExecutors);

            return RequestResult<ApplicationsReportDto>.Success(dto);
        }
    }
}
