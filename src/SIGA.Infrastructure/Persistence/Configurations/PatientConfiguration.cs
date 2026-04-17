using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("patients");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        // Un paciente siempre tiene una persona asociada (sus datos clínicos/personales)
        builder.HasOne(x => x.Person)
            .WithMany()
            .HasForeignKey(x => x.PersonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.PersonId).IsUnique();

        // El vínculo con User es opcional: solo existe si el paciente también tiene cuenta en el sistema
        builder.HasOne(x => x.User)
            .WithOne(x => x.Patient)
            .HasForeignKey<Patient>(x => x.UserId)
            .IsRequired(false);
    }
}
