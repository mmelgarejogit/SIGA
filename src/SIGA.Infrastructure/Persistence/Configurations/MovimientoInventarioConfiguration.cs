using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class MovimientoInventarioConfiguration : IEntityTypeConfiguration<MovimientoInventario>
{
    public void Configure(EntityTypeBuilder<MovimientoInventario> b)
    {
        b.ToTable("movimientos_inventario");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Tipo).HasConversion<string>();
        b.Property(x => x.OrigenTipo).HasConversion<string>();

        b.HasOne(x => x.ProductoVariante)
            .WithMany(p => p.Movimientos)
            .HasForeignKey(x => x.ProductoVarianteId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Sucursal)
            .WithMany(s => s.Movimientos)
            .HasForeignKey(x => x.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Usuario)
            .WithMany()
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.TipoAjuste)
            .WithMany()
            .HasForeignKey(x => x.TipoAjusteId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        b.HasIndex(x => new { x.ProductoVarianteId, x.SucursalId, x.Fecha });
    }
}
