using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PTMK_Test.Application.Implementation.Configurators
{
    public class ApplicationConfigurator : IEntityTypeConfiguration<Core.Implementation.Models.Application>
    {
        public void Configure(EntityTypeBuilder<Core.Implementation.Models.Application> builder)
        {
            builder.ToTable("Applications");

            builder.HasKey(a => a.ID);
            builder.Property(a => a.ID)
                .HasField("_id")
                .ValueGeneratedNever();

            builder.Property(a => a.Number)
                .HasField("_number")
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(a => a.CreatedAt)
                .HasField("_createdAt")
                .IsRequired();

            builder.Property(a => a.Description)
                .HasField("_description")
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(a => a.Deadline)
                .HasField("_deadline")
                .IsRequired();

            builder.ComplexProperty(a => a.Status, statusBuilder =>
            {
                statusBuilder.HasField("_status");
                statusBuilder.UsePropertyAccessMode(PropertyAccessMode.Field);
                statusBuilder.Property(s => s.StatusType)
                    .HasColumnName("Status")
                    .HasConversion<string>()
                    .HasMaxLength(30)
                    .IsRequired();
            });

            builder.HasOne<Core.Implementation.Models.Employee>()
                .WithMany()
                .HasForeignKey(a => a.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Core.Implementation.Models.Employee>()
                .WithMany()
                .HasForeignKey(a => a.ExecutorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(a => a.Number).HasDatabaseName("IX_Applications_Number");
            builder.HasIndex(a => a.AuthorId).HasDatabaseName("IX_Applications_AuthorId");
            builder.HasIndex(a => a.ExecutorId).HasDatabaseName("IX_Applications_ExecutorId");
        }
    }
}
