using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class ComprobanteConfiguration : IEntityTypeConfiguration<Comprobante>
{
    public void Configure(EntityTypeBuilder<Comprobante> builder)
    {
        builder.ToTable("comprobantes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Tipo).HasConversion<int>().IsRequired();
        builder.Property(x => x.Estado).HasConversion<int>().IsRequired();
        builder.Property(x => x.MotivoAnulacion).HasMaxLength(500);
        builder.Property(x => x.FechaEmision).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.VentaId).IsUnique();

        builder.HasOne(x => x.EmitidoPor)
            .WithMany()
            .HasForeignKey(x => x.EmitidoPorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AnuladoPor)
            .WithMany()
            .HasForeignKey(x => x.AnuladoPorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
