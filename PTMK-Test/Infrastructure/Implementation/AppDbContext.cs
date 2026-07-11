using Microsoft.EntityFrameworkCore;
using PTMK_Test.Application.Implementation.Configurators;
using PTMK_Test.Application.Interface;
using PTMK_Test.Core.Implementation.Models;

namespace PTMK_Test.Infrastructure.Implementation
{
    public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
        : DbContext(options), IDbContext
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Core.Implementation.Models.Application> Applications { get; set; }

        public async Task<int> SaveChangeAsync(CancellationToken ct = default) =>
            await SaveChangesAsync(ct);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new EmployeeConfigurator());
            modelBuilder.ApplyConfiguration(new ApplicationConfigurator());
        }
    }
}
