using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class EgresoFacturaLaboratorioConfiguration : IEntityTypeConfiguration<EgresoFacturaLaboratorio>
{
    public void Configure(EntityTypeBuilder<EgresoFacturaLaboratorio> builder)
    {
        builder.Property(e => e.FacturaLaboratorioId).HasColumnName("factura_laboratorio_id");

        builder.HasOne(e => e.FacturaLaboratorio)
            .WithOne()
            .HasForeignKey<EgresoFacturaLaboratorio>(e => e.FacturaLaboratorioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
