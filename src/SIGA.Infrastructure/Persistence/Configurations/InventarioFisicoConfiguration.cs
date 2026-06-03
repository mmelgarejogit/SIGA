using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class InventarioFisicoConfiguration : IEntityTypeConfiguration<InventarioFisico>
{
    public void Configure(EntityTypeBuilder<InventarioFisico> b)
    {
        b.ToTable("inventarios_fisicos");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Estado).HasConversion<string>();
        b.Property(x => x.Alcance).HasConversion<string>();
        b.Property(x => x.Observacion).HasMaxLength(500);

        b.HasOne(x => x.Sucursal).WithMany().HasForeignKey(x => x.SucursalId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.FiltroCategoria).WithMany().HasForeignKey(x => x.FiltroCategoriaId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
        b.HasOne(x => x.IniciadoPor).WithMany().HasForeignKey(x => x.IniciadoPorId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.EjecutadoPor).WithMany().HasForeignKey(x => x.EjecutadoPorId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);
        b.HasOne(x => x.AprobadoPor).WithMany().HasForeignKey(x => x.AprobadoPorId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);

        b.HasIndex(x => new { x.SucursalId, x.Estado });
    }
}

public class InventarioFisicoLineaConfiguration : IEntityTypeConfiguration<InventarioFisicoLinea>
{
    public void Configure(EntityTypeBuilder<InventarioFisicoLinea> b)
    {
        b.ToTable("inventario_fisico_lineas");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");

        b.HasOne(x => x.InventarioFisico).WithMany(i => i.Lineas).HasForeignKey(x => x.InventarioFisicoId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.ProductoVariante).WithMany().HasForeignKey(x => x.ProductoVarianteId).OnDelete(DeleteBehavior.Restrict);
    }
}
