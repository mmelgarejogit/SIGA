using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class EgresoConfiguration : IEntityTypeConfiguration<Egreso>
{
    public void Configure(EntityTypeBuilder<Egreso> builder)
    {
        builder.ToTable("egresos");
        builder.HasKey(x => x.Id);

        builder.HasDiscriminator(x => x.Tipo)
            .HasValue<FacturaCompra>(TipoEgreso.FacturaCompra)
            .HasValue<Honorario>(TipoEgreso.Honorario)
            .HasValue<GastoGeneral>(TipoEgreso.GastoGeneral)
            .HasValue<SalarioEmpleado>(TipoEgreso.Salario);

        builder.Property(x => x.SucursalId).IsRequired();
        builder.Property(x => x.CreadoPorUserId).IsRequired();
        builder.Property(x => x.FechaCreacion).IsRequired();
        builder.Property(x => x.Tipo).HasConversion<int>().IsRequired();
        builder.Property(x => x.Estado).HasConversion<int>().IsRequired();
        builder.Property(x => x.Monto).IsRequired().HasColumnType("numeric(18,0)");
        builder.Property(x => x.Concepto).IsRequired().HasMaxLength(300);
        builder.Property(x => x.Observaciones).HasMaxLength(1000);
        builder.Property(x => x.FechaEmision).IsRequired();
        builder.Property(x => x.FechaVencimiento);
        builder.Property(x => x.AprobadoPorUserId);
        builder.Property(x => x.FechaAprobacion);
        builder.Property(x => x.MotivoRechazo).HasMaxLength(1000);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOne(x => x.Sucursal)
            .WithMany()
            .HasForeignKey(x => x.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CreadoPorUser)
            .WithMany()
            .HasForeignKey(x => x.CreadoPorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AprobadoPorUser)
            .WithMany()
            .HasForeignKey(x => x.AprobadoPorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}