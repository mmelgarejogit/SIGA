using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Professionals;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Domain.Security;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class ProfessionalService : IProfessionalService
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;

    public ProfessionalService(AppDbContext dbContext, IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<IEnumerable<ProfessionalResponse>>> GetAllAsync()
    {
        var professionals = await _dbContext.Professionals
            .Include(p => p.User).ThenInclude(u => u.Person)
            .ToListAsync();

        return Result<IEnumerable<ProfessionalResponse>>.Success(professionals.Select(ToResponse));
    }

    public async Task<Result<ProfessionalResponse>> GetByIdAsync(int id)
    {
        var professional = await _dbContext.Professionals
            .Include(p => p.User).ThenInclude(u => u.Person)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (professional is null)
            return Result<ProfessionalResponse>.Failure("Professional not found.", ErrorType.NotFound);

        return Result<ProfessionalResponse>.Success(ToResponse(professional));
    }

    public async Task<Result<ProfessionalResponse>> CreateAsync(CreateProfessionalRequest request)
    {
        if (await _dbContext.Persons.AnyAsync(p => p.CI == request.CI))
            return Result<ProfessionalResponse>.Failure("CI already in use.", ErrorType.Conflict);

        if (await _dbContext.Persons.AnyAsync(p => p.Email == request.Email.Trim().ToLower()))
            return Result<ProfessionalResponse>.Failure("Email already in use.", ErrorType.Conflict);

        if (await _dbContext.Professionals.AnyAsync(p => p.LicenseNumber == request.LicenseNumber))
            return Result<ProfessionalResponse>.Failure("License number already in use.", ErrorType.Conflict);

        var role = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Professional")
                   ?? new Role { Name = "Professional" };

        var now = DateTime.UtcNow;

        var person = new Person
        {
            CI = request.CI.Trim(),
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

        var professional = new Professional
        {
            User = user,
            Specialty = request.Specialty.Trim(),
            LicenseNumber = request.LicenseNumber.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.Professionals.Add(professional);
        await _dbContext.SaveChangesAsync();

        return Result<ProfessionalResponse>.Success(ToResponse(professional));
    }

    public async Task<Result<ProfessionalResponse>> UpdateAsync(int id, UpdateProfessionalRequest request)
    {
        var professional = await _dbContext.Professionals
            .Include(p => p.User).ThenInclude(u => u.Person)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (professional is null)
            return Result<ProfessionalResponse>.Failure("Professional not found.", ErrorType.NotFound);

        if (await _dbContext.Professionals.AnyAsync(p => p.LicenseNumber == request.LicenseNumber && p.Id != id))
            return Result<ProfessionalResponse>.Failure("License number already in use.", ErrorType.Conflict);

        var now = DateTime.UtcNow;

        professional.User.Person.FirstName = request.FirstName.Trim();
        professional.User.Person.LastName = request.LastName.Trim();
        professional.User.Person.PhoneNumber = request.PhoneNumber?.Trim();
        professional.User.Person.UpdatedAt = now;

        professional.User.IsActive = request.IsActive;
        professional.User.UpdatedAt = now;

        professional.Specialty = request.Specialty.Trim();
        professional.LicenseNumber = request.LicenseNumber.Trim();
        professional.UpdatedAt = now;

        await _dbContext.SaveChangesAsync();

        return Result<ProfessionalResponse>.Success(ToResponse(professional));
    }

    public async Task<Result<bool>> DeleteAsync(int id)
    {
        var professional = await _dbContext.Professionals
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (professional is null)
            return Result<bool>.Failure("Professional not found.", ErrorType.NotFound);

        professional.User.IsActive = false;
        professional.User.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    private static ProfessionalResponse ToResponse(Professional p) => new()
    {
        Id = p.Id,
        UserId = p.UserId,
        CI = p.User.Person.CI,
        FirstName = p.User.Person.FirstName,
        LastName = p.User.Person.LastName,
        BirthDate = p.User.Person.BirthDate,
        PhoneNumber = p.User.Person.PhoneNumber,
        Email = p.User.Person.Email,
        Specialty = p.Specialty,
        LicenseNumber = p.LicenseNumber,
        IsActive = p.User.IsActive,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };
}
