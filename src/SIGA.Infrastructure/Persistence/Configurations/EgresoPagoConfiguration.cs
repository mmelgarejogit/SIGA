using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class EgresoPagoConfiguration : IEntityTypeConfiguration<EgresoPago>
{
    public void Configure(EntityTypeBuilder<EgresoPago> builder)
    {
        builder.ToTable("egresos_pagos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EgresoId).IsRequired();
        builder.Property(x => x.FechaPago).IsRequired();
        builder.Property(x => x.MetodoPago).HasConversion<int>().IsRequired();
        builder.Property(x => x.NumeroComprobante).HasMaxLength(100);
        builder.Property(x => x.Observaciones).HasMaxLength(1000);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne(x => x.Egreso)
            .WithOne(e => e.Pago)
            .HasForeignKey<EgresoPago>(x => x.EgresoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.RegistradoPorUser)
            .WithMany()
            .HasForeignKey(x => x.RegistradoPorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}