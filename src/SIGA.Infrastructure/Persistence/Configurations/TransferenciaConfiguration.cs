using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class TransferenciaConfiguration : IEntityTypeConfiguration<Transferencia>
{
    public void Configure(EntityTypeBuilder<Transferencia> b)
    {
        b.ToTable("transferencias");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        b.Property(x => x.Estado).HasConversion<string>();
        b.Property(x => x.Observacion).HasMaxLength(500);
        b.Property(x => x.MotivoRechazo).HasMaxLength(500);

        b.HasOne(x => x.SucursalOrigen)
            .WithMany(s => s.TransferenciasOrigen)
            .HasForeignKey(x => x.SucursalOrigenId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.SucursalDestino)
            .WithMany(s => s.TransferenciasDestino)
            .HasForeignKey(x => x.SucursalDestinoId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.SolicitadoPor)
            .WithMany()
            .HasForeignKey(x => x.SolicitadoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.AprobadoPor)
            .WithMany()
            .HasForeignKey(x => x.AprobadoPorId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}

public class TransferenciaLineaConfiguration : IEntityTypeConfiguration<TransferenciaLinea>
{
    public void Configure(EntityTypeBuilder<TransferenciaLinea> b)
    {
        b.ToTable("transferencia_lineas");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");

        b.HasOne(x => x.Transferencia)
            .WithMany(t => t.Lineas)
            .HasForeignKey(x => x.TransferenciaId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.ProductoVariante)
            .WithMany(p => p.TransferenciaLineas)
            .HasForeignKey(x => x.ProductoVarianteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
