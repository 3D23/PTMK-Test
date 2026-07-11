using MediatR;
using Microsoft.EntityFrameworkCore;
using PTMK_Test.Application.Common;
using PTMK_Test.Application.Interface;

namespace PTMK_Test.Application.Implementation.Application.Commands
{
    public readonly record struct ChangeExecutorCommand(Guid ApplicationId, Guid NewExecutorId) 
        : IRequest<RequestResult>;

    public sealed class ChangeExecutorHandler(IDbContext dbContext) : IRequestHandler<ChangeExecutorCommand, RequestResult>
    {
        private readonly IDbContext _dbContext = dbContext;

        public async Task<RequestResult> Handle(ChangeExecutorCommand request, CancellationToken ct)
        {
            bool executorExists = await _dbContext.Employees.AnyAsync(e => e.ID == request.NewExecutorId, ct);
            if (!executorExists)
                return RequestResult.Failure($"Исполнитель с ID {request.NewExecutorId} не найден в системе.");

            var app = await _dbContext.Applications.FirstOrDefaultAsync(a => a.ID == request.ApplicationId, ct);
            if (app == null)
                return RequestResult.NotFound($"Заявка с ID {request.ApplicationId} не найдена.");

            app.ChangeExecutor(request.NewExecutorId);

            await _dbContext.SaveChangeAsync(ct);

            return RequestResult.Success();
        }
    }
}
