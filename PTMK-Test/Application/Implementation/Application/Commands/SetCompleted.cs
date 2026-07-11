using MediatR;
using Microsoft.EntityFrameworkCore;
using PTMK_Test.Application.Interface;

namespace PTMK_Test.Application.Implementation.Application.Commands
{
    public readonly record struct TrySetCompletedCommand(Guid ApplicationId) : IRequest<bool>;

    public sealed class TrySetCompletedHandler(IDbContext dbContext) : IRequestHandler<TrySetCompletedCommand, bool>
    {
        private readonly IDbContext _dbContext = dbContext;

        public async Task<bool> Handle(TrySetCompletedCommand request, CancellationToken ct)
        {
            var app = await _dbContext.Applications.FirstOrDefaultAsync(a => a.ID == request.ApplicationId, ct);
            if (app == null) return false;

            bool isTransitionValid = app.TrySetCompleted();

            if (isTransitionValid)
                await _dbContext.SaveChangeAsync(ct);

            return isTransitionValid;
        }
    }
}
