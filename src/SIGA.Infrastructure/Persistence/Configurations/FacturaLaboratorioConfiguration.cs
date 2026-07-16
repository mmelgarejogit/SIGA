using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class FacturaLaboratorioConfiguration : IEntityTypeConfiguration<FacturaLaboratorio>
{
    public void Configure(EntityTypeBuilder<FacturaLaboratorio> builder)
    {
        builder.ToTable("facturas_laboratorio");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NumeroFactura).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Timbrado).HasMaxLength(20);
        builder.Property(x => x.Monto).IsRequired().HasColumnType("numeric(18,0)");
        builder.Property(x => x.Observaciones).HasMaxLength(500);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.TrabajoPedidoId).IsUnique();

        builder.HasOne(x => x.EmitidoPor)
            .WithMany()
            .HasForeignKey(x => x.EmitidoPorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
