using MediatR;
using Microsoft.EntityFrameworkCore;
using PTMK_Test.Application.Common;
using PTMK_Test.Application.Interface;

namespace PTMK_Test.Application.Implementation.Application.Commands
{
    public readonly record struct CreateApplicationCommand(
        string Number,
        Guid AuthorId,
        Guid ExecutorId,
        DateTime Deadline,
        string Description = "") : IRequest<RequestResult<Guid>>;

    public sealed class CreateApplicationHandler(IDbContext dbContext)
        : IRequestHandler<CreateApplicationCommand, RequestResult<Guid>>
    {
        private readonly IDbContext _dbContext = dbContext;

        public async Task<RequestResult<Guid>> Handle(CreateApplicationCommand request, CancellationToken ct)
        {
             if (await _dbContext.Applications.AnyAsync(a => a.Number == request.Number, ct))
                return RequestResult<Guid>.Failure("Заявка с таким номером уже существует.");

            var application = new Core.Implementation.Models.Application(
                request.Number,
                DateTime.UtcNow,
                request.AuthorId,
                request.ExecutorId,
                request.Deadline,
                request.Description
            );

            await _dbContext.Applications.AddAsync(application, ct);
            await _dbContext.SaveChangeAsync(ct);

            return RequestResult<Guid>.Success(application.ID);
        }
    }
}
