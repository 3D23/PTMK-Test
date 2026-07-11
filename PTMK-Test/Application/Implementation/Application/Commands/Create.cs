using MediatR;
using PTMK_Test.Application.Interface;

namespace PTMK_Test.Application.Implementation.Application.Commands
{
    public readonly record struct CreateApplicationCommand(
        string Number,
        Guid AuthorId,
        Guid ExecutorId,
        DateTime Deadline,
        string Description = "") : IRequest<Guid>;

    public sealed class CreateApplicationHandler(IDbContext dbContext)
        : IRequestHandler<CreateApplicationCommand, Guid>
    {
        private readonly IDbContext _dbContext = dbContext;

        public async Task<Guid> Handle(CreateApplicationCommand request, CancellationToken ct)
        {
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

            return application.ID;
        }

    }
}
