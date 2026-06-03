using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class AjusteManualConfiguration : IEntityTypeConfiguration<AjusteManual>
{
    public void Configure(EntityTypeBuilder<AjusteManual> b)
    {
        b.ToTable("ajustes_manual");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Observacion).IsRequired();
        b.Property(x => x.Estado).HasConversion<string>();

        b.HasOne(x => x.Sucursal)
            .WithMany(s => s.AjustesManual)
            .HasForeignKey(x => x.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.TipoAjuste)
            .WithMany(t => t.AjustesManual)
            .HasForeignKey(x => x.TipoAjusteId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.ProductoVariante)
            .WithMany(p => p.AjustesManual)
            .HasForeignKey(x => x.ProductoVarianteId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.CreadoPor)
            .WithMany()
            .HasForeignKey(x => x.CreadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.AprobadoPor)
            .WithMany()
            .HasForeignKey(x => x.AprobadoPorId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
