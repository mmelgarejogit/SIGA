using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class SesionCajaConfiguration : IEntityTypeConfiguration<SesionCaja>
{
    public void Configure(EntityTypeBuilder<SesionCaja> builder)
    {
        builder.ToTable("sesiones_caja");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Estado).HasConversion<int>().IsRequired();
        builder.Property(x => x.MontoInicial).HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(x => x.FechaApertura).IsRequired();
        builder.Property(x => x.EfectivoContado).HasColumnType("numeric(18,2)");
        builder.Property(x => x.EfectivoEsperado).HasColumnType("numeric(18,2)");
        builder.Property(x => x.Diferencia).HasColumnType("numeric(18,2)");
        builder.Property(x => x.ObservacionCierre).HasMaxLength(500);

        builder.Property(x => x.MotivoRechazo).HasMaxLength(500);

        builder.HasOne(x => x.Sucursal).WithMany()
            .HasForeignKey(x => x.SucursalId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.AbiertaPor).WithMany()
            .HasForeignKey(x => x.AbiertaPorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CerradaPor).WithMany()
            .HasForeignKey(x => x.CerradaPorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.AprobadoPor).WithMany()
            .HasForeignKey(x => x.AprobadoPorId).OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Movimientos).WithOne(m => m.SesionCaja)
            .HasForeignKey(m => m.SesionCajaId).OnDelete(DeleteBehavior.SetNull);
    }
}
