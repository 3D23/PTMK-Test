using MediatR;
using PTMK_Test.Application.Common;
using PTMK_Test.Application.Implementation.Employee.Queries;
using PTMK_Test.Web.Extensions;

namespace PTMK_Test.Web.Endpoints
{
    public static class EmployeeEndpointsMapper
    {
        public static void MapEmployees(this IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup("api/employees");

            group.MapGet("", GetAllEmployees)
                .WithName("GetAllEmployees");
        }

        #region Endpoints Handlers

        private static async Task<IResult> GetAllEmployees(
            IMediator mediator,
            [AsParameters] PaginationParameters pagination,
            CancellationToken ct)
        {
            var query = new GetAllEmployeesQuery(pagination);
            var result = await mediator.Send(query, ct);
            return result.ToHttpResponse();
        }

        #endregion
    }
}
