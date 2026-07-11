using MediatR;
using Microsoft.EntityFrameworkCore;
using PTMK_Test.Application.Common;
using PTMK_Test.Application.Interface;

namespace PTMK_Test.Application.Implementation.Application.Commands
{
    public readonly record struct TrySetInProgressCommand(Guid ApplicationId) 
        : IRequest<RequestResult>;

    public sealed class TrySetInProgressHandler(IDbContext dbContext)
        : IRequestHandler<TrySetInProgressCommand, RequestResult>
    {
        private readonly IDbContext _dbContext = dbContext;

        public async Task<RequestResult> Handle(TrySetInProgressCommand request, CancellationToken ct)
        {
            var app = await _dbContext.Applications.FirstOrDefaultAsync(a => a.ID == request.ApplicationId, ct);
            if (app == null)
                return RequestResult.NotFound($"Заявка с ID {request.ApplicationId} не найдена.");

            bool isTransitionValid = app.TrySetInProgress();
            if (!isTransitionValid)
                return RequestResult.Failure($"Невозможно перевести заявку {request.ApplicationId} в статус 'In Progress' из текущего состояния.");

            await _dbContext.SaveChangeAsync(ct);

            return RequestResult.Success();
        }
    }
}
