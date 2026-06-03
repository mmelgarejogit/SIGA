using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class CargoEmpleadoConfiguration : IEntityTypeConfiguration<CargoEmpleado>
{
    public void Configure(EntityTypeBuilder<CargoEmpleado> builder)
    {
        builder.ToTable("cargos_empleado");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Descripcion).HasMaxLength(500);
        builder.Property(x => x.Activo).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
