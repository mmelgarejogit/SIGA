using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class MotivoMovimientoConfiguration : IEntityTypeConfiguration<MotivoMovimiento>
{
    public void Configure(EntityTypeBuilder<MotivoMovimiento> builder)
    {
        builder.ToTable("motivos_movimiento");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Tipo).HasConversion<int>().IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasMany(x => x.Movimientos)
               .WithOne(x => x.MotivoMovimiento)
               .HasForeignKey(x => x.MotivoMovimientoId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
