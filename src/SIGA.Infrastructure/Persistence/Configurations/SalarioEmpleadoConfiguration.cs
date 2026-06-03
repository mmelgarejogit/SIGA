using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class SalarioEmpleadoConfiguration : IEntityTypeConfiguration<SalarioEmpleado>
{
    public void Configure(EntityTypeBuilder<SalarioEmpleado> builder)
    {
        builder.Property(x => x.PeriodoMes).IsRequired();
        builder.Property(x => x.PeriodoAnio).IsRequired();

        builder.HasOne(x => x.Empleado)
            .WithMany()
            .HasForeignKey(x => x.EmpleadoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}