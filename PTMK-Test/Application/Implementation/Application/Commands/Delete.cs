using MediatR;
using Microsoft.EntityFrameworkCore;
using PTMK_Test.Application.Common;
using PTMK_Test.Application.Interface;

namespace PTMK_Test.Application.Implementation.Application.Commands
{
    public readonly record struct DeleteApplicationCommand(Guid Id) : IRequest<RequestResult>;

    public sealed class DeleteApplicationHandler(IDbContext dbContext)
    : IRequestHandler<DeleteApplicationCommand, RequestResult>
    {
        private readonly IDbContext _dbContext = dbContext;

        public async Task<RequestResult> Handle(DeleteApplicationCommand request, CancellationToken ct)
        {
            var application = await _dbContext.Applications
                .FirstOrDefaultAsync(a => a.ID == request.Id, ct);

            if (application == null)
                return RequestResult.Failure("Заявка не найдена.");

            _dbContext.Applications.Remove(application);
            await _dbContext.SaveChangeAsync(ct);

            return RequestResult.Success();
        }
    }
}
