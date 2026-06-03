using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class HonorarioConfiguration : IEntityTypeConfiguration<Honorario>
{
    public void Configure(EntityTypeBuilder<Honorario> builder)
    {
        builder.Property(x => x.PeriodoMes).IsRequired();
        builder.Property(x => x.PeriodoAnio).IsRequired();

        builder.HasOne(x => x.Professional)
            .WithMany()
            .HasForeignKey(x => x.ProfessionalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}