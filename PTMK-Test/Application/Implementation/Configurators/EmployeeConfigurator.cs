using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PTMK_Test.Application.Implementation.Configurators
{
    public class EmployeeConfigurator : IEntityTypeConfiguration<Core.Implementation.Models.Employee>
    {
        public void Configure(EntityTypeBuilder<Core.Implementation.Models.Employee> builder)
        {
            builder.ToTable("Employees");

            builder.HasKey(e => e.ID);
            builder.Property(e => e.ID)
                .HasField("_id")
                .ValueGeneratedNever();

            builder.ComplexProperty(e => e.FullName, nameBuilder =>
            {
                nameBuilder.HasField("_fullName");
                nameBuilder.UsePropertyAccessMode(PropertyAccessMode.Field);

                nameBuilder.Property(n => n.FirstName)
                    .HasColumnName("FirstName")
                    .HasMaxLength(50)
                    .IsRequired();

                nameBuilder.Property(n => n.Surname)
                    .HasColumnName("Surname")
                    .HasMaxLength(50)
                    .IsRequired();

                nameBuilder.Property(n => n.MiddleName)
                    .HasColumnName("MiddleName")
                    .HasMaxLength(50);
            });

            builder.Property(e => e.Department)
                .HasField("_department")
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(e => e.Position)
                .HasField("_position")
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            builder.HasIndex(e => e.Department)
                .HasDatabaseName("IX_Employees_Department");

            builder.HasIndex(e => e.Position)
                .HasDatabaseName("IX_Employees_Position");
        }
    }
}
