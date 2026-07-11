using Microsoft.EntityFrameworkCore;

namespace PTMK_Test.Application.Interface
{
    public interface IDbContext
    {
        DbSet<Core.Implementation.Models.Application> Applications { get; set; }
        DbSet<Core.Implementation.Models.Employee> Employees { get; set; }
        Task<int> SaveChangeAsync(CancellationToken cancellationToken);
    }
}
