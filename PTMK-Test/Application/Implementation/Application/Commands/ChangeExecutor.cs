using MediatR;
using Microsoft.EntityFrameworkCore;
using PTMK_Test.Application.Interface;

namespace PTMK_Test.Application.Implementation.Application.Commands
{
    public readonly record struct ChangeExecutorCommand(Guid ApplicationId, Guid NewExecutorId) : IRequest;

    public sealed class ChangeExecutorHandler(IDbContext dbContext) : IRequestHandler<ChangeExecutorCommand>
    {
        private readonly IDbContext _dbContext = dbContext;

        public async Task Handle(ChangeExecutorCommand request, CancellationToken ct)
        {
            bool executorExists = await _dbContext.Employees.AnyAsync(e => e.ID == request.NewExecutorId, ct);
            if (!executorExists)
                throw new ArgumentException($"{nameof(ChangeExecutorHandler)} Исполнитель с ID {request.NewExecutorId} не найден в системе.", nameof(request.NewExecutorId));

            var app = await _dbContext.Applications.FirstOrDefaultAsync(a => a.ID == request.ApplicationId, ct)
                ?? throw new KeyNotFoundException($"{nameof(ChangeExecutorHandler)} Заявка с ID {request.ApplicationId} не найдена.");

            app.ChangeExecutor(request.NewExecutorId);

            await _dbContext.SaveChangeAsync(ct);
        }
    }
}
