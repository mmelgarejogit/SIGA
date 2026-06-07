using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("clientes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TipoFacturacion).HasConversion<int>().IsRequired();
        builder.Property(x => x.RazonSocial).HasMaxLength(200);
        builder.Property(x => x.RucCiFiscal).HasMaxLength(30);
        builder.Property(x => x.Direccion).HasMaxLength(300);
        builder.Property(x => x.Email).HasMaxLength(320);
        builder.Property(x => x.Telefono).HasMaxLength(30);

        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        // El cliente siempre es una persona; la identidad jurídica (razón social / RUC)
        // es solo el dato de facturación, no una entidad aparte.
        builder.HasOne(x => x.Person)
            .WithMany()
            .HasForeignKey(x => x.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.PersonId).IsUnique();
    }
}
