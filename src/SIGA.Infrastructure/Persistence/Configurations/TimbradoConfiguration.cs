using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class TimbradoConfiguration : IEntityTypeConfiguration<Timbrado>
{
    public void Configure(EntityTypeBuilder<Timbrado> builder)
    {
        builder.ToTable("timbrados");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NumeroTimbrado).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Establecimiento).IsRequired().HasMaxLength(3);
        builder.Property(x => x.PuntoExpedicion).IsRequired().HasMaxLength(3);
        builder.Property(x => x.UltimoNumero).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.NumeroDesde).IsRequired().HasDefaultValue(1);
        builder.Property(x => x.NumeroHasta);
        builder.Property(x => x.FechaInicioVigencia).IsRequired();
        builder.Property(x => x.FechaFinVigencia).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => new { x.NumeroTimbrado, x.Establecimiento, x.PuntoExpedicion }).IsUnique();
    }
}