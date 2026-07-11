using MediatR;
using Microsoft.EntityFrameworkCore;
using PTMK_Test.Application.Common;
using PTMK_Test.Application.Interface;

namespace PTMK_Test.Application.Implementation.Application.Commands
{
    public readonly record struct TrySetCompletedCommand(Guid ApplicationId) 
        : IRequest<RequestResult>;

    public sealed class TrySetCompletedHandler(IDbContext dbContext)
        : IRequestHandler<TrySetCompletedCommand, RequestResult>
    {
        private readonly IDbContext _dbContext = dbContext;

        public async Task<RequestResult> Handle(TrySetCompletedCommand request, CancellationToken ct)
        {
            var app = await _dbContext.Applications.FirstOrDefaultAsync(a => a.ID == request.ApplicationId, ct);
            if (app == null)
                return RequestResult.NotFound($"Заявка с ID {request.ApplicationId} не найдена.");

            bool isTransitionValid = app.TrySetCompleted();
            if (!isTransitionValid)
                return RequestResult.Failure($"Невозможно перевести заявку {request.ApplicationId} в статус 'Completed' из текущего состояния.");

            await _dbContext.SaveChangeAsync(ct);

            return RequestResult.Success();
        }
    }
}
