using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class MovimientoCajaConfiguration : IEntityTypeConfiguration<MovimientoCaja>
{
    public void Configure(EntityTypeBuilder<MovimientoCaja> builder)
    {
        builder.ToTable("movimientos_caja");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Tipo).HasConversion<int>().IsRequired();
        builder.Property(x => x.Monto).IsRequired().HasColumnType("numeric(18,2)");
        builder.Property(x => x.Concepto).IsRequired().HasMaxLength(300);
        builder.Property(x => x.MetodoPago).HasConversion<int>().IsRequired();
        builder.Property(x => x.Fecha).IsRequired();
        builder.Property(x => x.Referencia).HasMaxLength(200);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne(x => x.Venta).WithMany().HasForeignKey(x => x.VentaId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.Egreso).WithMany().HasForeignKey(x => x.EgresoId).OnDelete(DeleteBehavior.SetNull);
    }
}
