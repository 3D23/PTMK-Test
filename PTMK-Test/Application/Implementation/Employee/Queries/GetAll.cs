using MediatR;
using Microsoft.EntityFrameworkCore;
using PTMK_Test.Application.Common;
using PTMK_Test.Application.Interface;

namespace PTMK_Test.Application.Implementation.Employee.Queries
{
    public readonly record struct GetAllEmployeesQuery(
        PaginationParameters Pagination) 
        : IRequest<RequestResult<List<Core.Implementation.Models.Employee>>>;

    public sealed class GetAllEmployeesHandler(IDbContext dbContext)
        : IRequestHandler<GetAllEmployeesQuery, RequestResult<List<Core.Implementation.Models.Employee>>>
    {
        private readonly IDbContext _dbContext = dbContext;

        public async Task<RequestResult<List<Core.Implementation.Models.Employee>>> Handle(
            GetAllEmployeesQuery request,
            CancellationToken ct)
        {
            var employees = await _dbContext.Employees
                .OrderBy(e => e.ID)
                .Skip((request.Pagination.PageNumber - 1) * request.Pagination.PageSize)
                .Take(request.Pagination.PageSize)
                .ToListAsync(ct);

            return RequestResult<List<Core.Implementation.Models.Employee>>.Success(employees);
        }
    }
}
