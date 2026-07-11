using MediatR;
using Microsoft.EntityFrameworkCore;
using PTMK_Test.Application.Common;
using PTMK_Test.Application.Interface;
using PTMK_Test.Core.Implementation.Enums;

namespace PTMK_Test.Application.Implementation.Queries
{
    public sealed record GetFilteredApplicationsQuery(
         IEnumerable<IApplicationSpecification> Specifications,
         PaginationParameters Pagination,
         DepartmentType? Department = null,
         bool OrderByDeadline = false) : IRequest<RequestResult<List<Core.Implementation.Models.Application>>>;

    public sealed class GetFilteredApplicationsHandler(IDbContext dbContext)
        : IRequestHandler<GetFilteredApplicationsQuery, RequestResult<List<Core.Implementation.Models.Application>>>
    {
        private readonly IDbContext _dbContext = dbContext;

        public async Task<RequestResult<List<Core.Implementation.Models.Application>>> Handle(
            GetFilteredApplicationsQuery request,
            CancellationToken ct)
        {
            var query = _dbContext.Applications.AsQueryable();

            if (request.Specifications != null)
            {
                foreach (var spec in request.Specifications)
                    query = query.Where(spec.ToExpression());
            }

            if (request.Department.HasValue)
            {
                query = from app in query
                        join emp in _dbContext.Employees on app.ExecutorId equals emp.ID
                        where emp.Department == request.Department.Value
                        select app;
            }

            var orderedQuery = request.OrderByDeadline
                ? query.OrderBy(a => a.Deadline)
                : query.OrderBy(a => a.ID);

            var applications = await orderedQuery
                .Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
                .Take(request.Pagination.PageSize)
                .ToListAsync(ct);

            return RequestResult<List<Core.Implementation.Models.Application>>.Success(applications);
        }
    }
}
