using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class DatosFacturacionConfiguration : IEntityTypeConfiguration<DatosFacturacion>
{
    public void Configure(EntityTypeBuilder<DatosFacturacion> builder)
    {
        builder.ToTable("datos_facturacion");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RucCiFiscal).HasMaxLength(30);
        builder.Property(x => x.RazonSocial).HasMaxLength(200);
        builder.Property(x => x.Direccion).HasMaxLength(300);
        builder.Property(x => x.Email).HasMaxLength(320);
        builder.Property(x => x.Telefono).HasMaxLength(30);

        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOne(x => x.Patient)
            .WithOne(x => x.DatosFacturacion)
            .HasForeignKey<DatosFacturacion>(x => x.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.PatientId).IsUnique();
    }
}
