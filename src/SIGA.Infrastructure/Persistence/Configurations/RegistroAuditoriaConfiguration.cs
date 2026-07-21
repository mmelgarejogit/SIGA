using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class RegistroAuditoriaConfiguration : IEntityTypeConfiguration<RegistroAuditoria>
{
    public void Configure(EntityTypeBuilder<RegistroAuditoria> builder)
    {
        builder.ToTable("registros_auditoria");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FechaHora).IsRequired();
        builder.Property(x => x.Categoria).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Accion).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(x => x.UserId);
        builder.Property(x => x.UsuarioNombre).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Entidad).HasMaxLength(50);
        builder.Property(x => x.EntidadId);
        builder.Property(x => x.Descripcion).IsRequired().HasMaxLength(500);
        builder.Property(x => x.SucursalId);
        builder.Property(x => x.IpAddress).HasMaxLength(45);

        // Índices para los filtros de la vista.
        builder.HasIndex(x => x.FechaHora);
        builder.HasIndex(x => x.Categoria);
        builder.HasIndex(x => x.Accion);
        builder.HasIndex(x => x.UserId);
    }
}
