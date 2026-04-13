using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Patients;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Domain.Security;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class PatientService : IPatientService
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public PatientService(AppDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<IEnumerable<PatientResponse>>> GetAllAsync()
    {
        var patients = await _dbContext.Patients
            .Include(p => p.User).ThenInclude(u => u.Person)
            .ToListAsync();

        return Result<IEnumerable<PatientResponse>>.Success(patients.Select(ToResponse));
    }

    public async Task<Result<PatientResponse>> GetByIdAsync(int id)
    {
        var patient = await _dbContext.Patients
            .Include(p => p.User).ThenInclude(u => u.Person)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patient is null)
            return Result<PatientResponse>.Failure("Patient not found.", ErrorType.NotFound);

        return Result<PatientResponse>.Success(ToResponse(patient));
    }

    public async Task<Result<PatientResponse>> CreateAsync(CreatePatientRequest request)
    {
        if (await _dbContext.Persons.AnyAsync(p => p.DNI == request.DNI))
            return Result<PatientResponse>.Failure("DNI already in use.", ErrorType.Conflict);

        if (await _dbContext.Persons.AnyAsync(p => p.Email == request.Email.Trim().ToLower()))
            return Result<PatientResponse>.Failure("Email already in use.", ErrorType.Conflict);

        var role = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Patient")
                   ?? new Role { Name = "Patient" };

        var now = DateTime.UtcNow;

        var person = new Person
        {
            DNI = request.DNI.Trim(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            BirthDate = request.BirthDate,
            PhoneNumber = request.PhoneNumber?.Trim(),
            Email = request.Email.Trim().ToLower(),
            CreatedAt = now,
            UpdatedAt = now
        };

        var user = new User
        {
            Person = person,
            PasswordHash = _passwordHasher.Hash(request.Password),
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        user.UserRoles.Add(new UserRole { User = user, Role = role });

        var patient = new Patient
        {
            User = user,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.Patients.Add(patient);
        await _dbContext.SaveChangesAsync();

        return Result<PatientResponse>.Success(ToResponse(patient));
    }

    public async Task<Result<PatientResponse>> UpdateAsync(int id, UpdatePatientRequest request)
    {
        var patient = await _dbContext.Patients
            .Include(p => p.User).ThenInclude(u => u.Person)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patient is null)
            return Result<PatientResponse>.Failure("Patient not found.", ErrorType.NotFound);

        var now = DateTime.UtcNow;

        patient.User.Person.FirstName = request.FirstName.Trim();
        patient.User.Person.LastName = request.LastName.Trim();
        patient.User.Person.PhoneNumber = request.PhoneNumber?.Trim();
        patient.User.Person.UpdatedAt = now;

        patient.User.IsActive = request.IsActive;
        patient.User.UpdatedAt = now;

        patient.UpdatedAt = now;

        await _dbContext.SaveChangesAsync();

        return Result<PatientResponse>.Success(ToResponse(patient));
    }

    public async Task<Result<bool>> DeleteAsync(int id)
    {
        var patient = await _dbContext.Patients
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patient is null)
            return Result<bool>.Failure("Patient not found.", ErrorType.NotFound);

        patient.User.IsActive = false;
        patient.User.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    private static PatientResponse ToResponse(Patient p) => new()
    {
        Id = p.Id,
        UserId = p.UserId,
        DNI = p.User.Person.DNI,
        FirstName = p.User.Person.FirstName,
        LastName = p.User.Person.LastName,
        BirthDate = p.User.Person.BirthDate,
        PhoneNumber = p.User.Person.PhoneNumber,
        Email = p.User.Person.Email,
        IsActive = p.User.IsActive,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };
}
