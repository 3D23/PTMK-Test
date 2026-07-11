using MediatR;
using Microsoft.AspNetCore.Mvc;
using PTMK_Test.Application.Common;
using PTMK_Test.Application.Implementation.Application.Commands;
using PTMK_Test.Application.Implementation.Application.Queries;
using PTMK_Test.Application.Implementation.Queries;
using PTMK_Test.Application.Implementation.Specifications.Application;
using PTMK_Test.Application.Interface;
using PTMK_Test.Core.Implementation.Enums;
using PTMK_Test.Web.Extensions; 

namespace PTMK_Test.Web.Endpoints
{
    public static class ApplicationEndpointsMapper
    {
        public static void MapApplications(this IEndpointRouteBuilder builder)
        {
            var appGroup = builder.MapGroup("api/applications");

            appGroup.MapGet("", GetAllApplications)
                .WithName("GetAllApplications");

            appGroup.MapGet("filter", GetFilteredApplications)
                .WithName("GetFilteredApplications");

            appGroup.MapGet("overdue-in-progress/{executorId:guid}", GetOverdueInProgressByExecutor)
                .WithName("GetOverdueInProgressByExecutor");

            appGroup.MapGet("analytics-report", GetAnalyticsReport)
                .WithName("GetAnalyticsReport");

            appGroup.MapPatch("{id:guid}/set-in-progress", PatchSetInProgress)
                .WithName("SetApplicationInProgress");

            appGroup.MapPatch("{id:guid}/set-completed", PatchSetCompleted)
                .WithName("SetApplicationCompleted");

            appGroup.MapPatch("{id:guid}/change-executor", PatchChangeExecutor)
                .WithName("ChangeApplicationExecutor");

            appGroup.MapPost("", CreateApplication)
                .WithName("CreateApplication");
        }

        #region Endpoints Handlers

        private static async Task<IResult> GetAllApplications(
            IMediator mediator,
            [AsParameters] PaginationParameters pagination,
            CancellationToken ct = default)
        {
            var query = new GetAllApplicationsQuery(pagination);
            var result = await mediator.Send(query, ct);
            return result.ToHttpResponse();
        }

        private static async Task<IResult> GetFilteredApplications(
            IMediator mediator,
            [AsParameters] PaginationParameters pagination,
            [FromQuery] ApplicationStatusType? status,
            [FromQuery] Guid? executorId,
            [FromQuery] DepartmentType? department,
            [FromQuery] bool? isOverdue,
            CancellationToken ct)
        {
            var specs = new List<IApplicationSpecification>();

            if (status.HasValue) specs.Add(new StatusSpecification(status.Value));
            if (executorId.HasValue) specs.Add(new ExecutorSpecification(executorId.Value));
            if (isOverdue.HasValue) specs.Add(new OverdueSpecification(isOverdue.Value));

            var query = new GetFilteredApplicationsQuery(specs, pagination, department);
            var result = await mediator.Send(query, ct);
            return result.ToHttpResponse();
        }

        private static async Task<IResult> GetOverdueInProgressByExecutor(
            IMediator mediator,
            Guid executorId,
            [AsParameters] PaginationParameters pagination,
            CancellationToken ct = default)
        {
            var specs = new List<IApplicationSpecification>
            {
                new ExecutorSpecification(executorId),
                new StatusSpecification(ApplicationStatusType.InProgress),
                new OverdueSpecification(isOverdue: true)
            };

            var query = new GetFilteredApplicationsQuery(
                specs,
                pagination,
                Department: null,
                OrderByDeadline: true);

            var result = await mediator.Send(query, ct);
            return result.ToHttpResponse();
        }

        private static async Task<IResult> GetAnalyticsReport(
            IMediator mediator,
            CancellationToken ct)
        {
            var result = await mediator.Send(new GetApplicationsReportQuery(), ct);
            return result.ToHttpResponse();
        }

        private static async Task<IResult> PatchSetInProgress(IMediator mediator, Guid id, CancellationToken ct)
        {
            var command = new TrySetInProgressCommand(id);
            var result = await mediator.Send(command, ct);
            return result.ToHttpResponse();
        }

        private static async Task<IResult> CreateApplication(
            IMediator mediator,
            [FromBody] CreateApplicationCommand command,
            CancellationToken ct)
        {
            var result = await mediator.Send(command, ct);
            return result.ToHttpCreated("/api/applications/{0}");
        }

        private static async Task<IResult> PatchSetCompleted(IMediator mediator, Guid id, CancellationToken ct)
        {
            var command = new TrySetCompletedCommand(id);
            var result = await mediator.Send(command, ct);
            return result.ToHttpResponse();
        }

        private static async Task<IResult> PatchChangeExecutor(
            IMediator mediator,
            Guid id,
            [FromQuery] Guid newExecutorId,
            CancellationToken ct)
        {
            var command = new ChangeExecutorCommand(id, newExecutorId);
            var result = await mediator.Send(command, ct);
            return result.ToHttpResponse();
        }

        #endregion
    }
}
