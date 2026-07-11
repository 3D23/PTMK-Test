using MediatR;
using PTMK_Test.Application.Interface;
using Microsoft.EntityFrameworkCore;
using PTMK_Test.Application.Common;

namespace PTMK_Test.Application.Implementation.Application.Queries
{
    public readonly record struct GetAllApplicationsQuery(
        PaginationParameters Pagination) : IRequest<List<Core.Implementation.Models.Application>>;

    public sealed class GetAllApplicationsHandler(IDbContext dbContext)
        : IRequestHandler<GetAllApplicationsQuery, List<Core.Implementation.Models.Application>>
    {
        private readonly IDbContext _dbContext = dbContext;

        public async Task<List<Core.Implementation.Models.Application>> Handle(GetAllApplicationsQuery request, CancellationToken ct)
        {
            return await _dbContext.Applications
                .OrderBy(a => a.ID)
                .Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
                .Take(request.Pagination.PageSize)
                .ToListAsync(ct);
        }
    }
}
