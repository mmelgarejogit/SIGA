using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class EgresoConfiguration : IEntityTypeConfiguration<Egreso>
{
    public void Configure(EntityTypeBuilder<Egreso> builder)
    {
        builder.ToTable("egresos");
        builder.HasKey(x => x.Id);

        builder.HasDiscriminator(x => x.Tipo)
            .HasValue<FacturaCompra>(TipoEgreso.FacturaCompra)
            .HasValue<Honorario>(TipoEgreso.Honorario)
            .HasValue<GastoGeneral>(TipoEgreso.GastoGeneral)
            .HasValue<SalarioEmpleado>(TipoEgreso.Salario);

        builder.Property(x => x.Tipo).HasConversion<int>().IsRequired();
        builder.Property(x => x.Estado).HasConversion<int>().IsRequired();
        builder.Property(x => x.Monto).IsRequired().HasColumnType("numeric(18,0)");
        builder.Property(x => x.Concepto).IsRequired().HasMaxLength(300);
        builder.Property(x => x.Observaciones).HasMaxLength(1000);
        builder.Property(x => x.FechaEmision).IsRequired();
        builder.Property(x => x.FechaVencimiento);
        builder.Property(x => x.FechaPago);
        builder.Property(x => x.MetodoPago).HasConversion<int>();
        builder.Property(x => x.MotivoRechazo).HasMaxLength(1000);
        builder.Property(x => x.FechaAprobacion);
        builder.Property(x => x.NroComprobante).HasMaxLength(100);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
    }
}
