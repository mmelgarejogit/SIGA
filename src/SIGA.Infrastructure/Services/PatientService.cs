using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Patients;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class PatientService : IPatientService
{
    private readonly AppDbContext _dbContext;

    private static readonly Regex OnlyLetters = new(@"^[\p{L}\s]+$", RegexOptions.Compiled);
    private static readonly Regex EmailFormat  = new(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled);

    public PatientService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PagedResult<PatientResponse>>> GetAllAsync(int page, int pageSize, string? search, string? status)
    {
        var query = _dbContext.Patients
            .Include(p => p.Person)
            .AsQueryable();

        if (status == "active")   query = query.Where(p => p.IsActive);
        if (status == "inactive") query = query.Where(p => !p.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim().ToLower();
            query = query.Where(p =>
                p.Person.FirstName.ToLower().Contains(q) ||
                p.Person.LastName.ToLower().Contains(q)  ||
                p.Person.DNI.ToLower().Contains(q)       ||
                (p.Person.Email != null && p.Person.Email.ToLower().Contains(q)) ||
                (p.Person.PhoneNumber != null && p.Person.PhoneNumber.ToLower().Contains(q))
            );
        }

        var totalCount  = await query.CountAsync();
        var totalActive = await _dbContext.Patients.CountAsync(p => p.IsActive);
        var totalPages  = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Result<PagedResult<PatientResponse>>.Success(new PagedResult<PatientResponse>
        {
            Items       = items.Select(ToResponse),
            TotalCount  = totalCount,
            TotalActive = totalActive,
            Page        = page,
            PageSize    = pageSize,
            TotalPages  = totalPages,
        });
    }

    public async Task<Result<PatientResponse>> GetByIdAsync(int id)
    {
        var patient = await _dbContext.Patients
            .Include(p => p.Person)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patient is null)
            return Result<PatientResponse>.Failure("Paciente no encontrado.", ErrorType.NotFound);

        return Result<PatientResponse>.Success(ToResponse(patient));
    }

    public async Task<Result<PatientResponse>> CreateAsync(CreatePatientRequest request)
    {
        // Validaciones de formato
        if (string.IsNullOrWhiteSpace(request.FirstName))
            return Result<PatientResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);
        if (!OnlyLetters.IsMatch(request.FirstName.Trim()))
            return Result<PatientResponse>.Failure("El nombre solo puede contener letras y espacios.", ErrorType.Validation);

        if (string.IsNullOrWhiteSpace(request.LastName))
            return Result<PatientResponse>.Failure("El apellido es obligatorio.", ErrorType.Validation);
        if (!OnlyLetters.IsMatch(request.LastName.Trim()))
            return Result<PatientResponse>.Failure("El apellido solo puede contener letras y espacios.", ErrorType.Validation);

        if (string.IsNullOrWhiteSpace(request.DNI))
            return Result<PatientResponse>.Failure("El documento es obligatorio.", ErrorType.Validation);

        if (!string.IsNullOrWhiteSpace(request.Email) && !EmailFormat.IsMatch(request.Email.Trim()))
            return Result<PatientResponse>.Failure("El formato del email no es válido.", ErrorType.Validation);

        // RN-P03: al menos un dato de contacto
        if (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.PhoneNumber))
            return Result<PatientResponse>.Failure(
                "Se requiere al menos un dato de contacto: email o teléfono.",
                ErrorType.Validation);

        // RN-P01: documento único
        if (await _dbContext.Persons.AnyAsync(p => p.DNI == request.DNI.Trim()))
            return Result<PatientResponse>.Failure("El DNI ya está registrado.", ErrorType.Conflict);

        // RN-P02: email único si se provee
        if (!string.IsNullOrWhiteSpace(request.Email) &&
            await _dbContext.Persons.AnyAsync(p => p.Email == request.Email.Trim().ToLower()))
            return Result<PatientResponse>.Failure("El email ya está registrado.", ErrorType.Conflict);

        var now = DateTime.UtcNow;

        var person = new Person
        {
            DNI = request.DNI.Trim(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            BirthDate = request.BirthDate,
            PhoneNumber = request.PhoneNumber?.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLower(),
            CreatedAt = now,
            UpdatedAt = now
        };

        var patient = new Patient
        {
            Person = person,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.Patients.Add(patient);
        await _dbContext.SaveChangesAsync();

        return Result<PatientResponse>.Success(ToResponse(patient));
    }

    public async Task<Result<PatientResponse>> UpdateAsync(int id, UpdatePatientRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName))
            return Result<PatientResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);
        if (!OnlyLetters.IsMatch(request.FirstName.Trim()))
            return Result<PatientResponse>.Failure("El nombre solo puede contener letras y espacios.", ErrorType.Validation);

        if (string.IsNullOrWhiteSpace(request.LastName))
            return Result<PatientResponse>.Failure("El apellido es obligatorio.", ErrorType.Validation);
        if (!OnlyLetters.IsMatch(request.LastName.Trim()))
            return Result<PatientResponse>.Failure("El apellido solo puede contener letras y espacios.", ErrorType.Validation);

        if (string.IsNullOrWhiteSpace(request.DNI))
            return Result<PatientResponse>.Failure("El nro. de cédula es obligatorio.", ErrorType.Validation);

        if (!string.IsNullOrWhiteSpace(request.Email) && !EmailFormat.IsMatch(request.Email.Trim()))
            return Result<PatientResponse>.Failure("El formato del email no es válido.", ErrorType.Validation);

        var patient = await _dbContext.Patients
            .Include(p => p.Person)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patient is null)
            return Result<PatientResponse>.Failure("Paciente no encontrado.", ErrorType.NotFound);

        // RN-P01: DNI único excluyendo el propio registro
        if (await _dbContext.Persons.AnyAsync(p => p.DNI == request.DNI.Trim() && p.Id != patient.PersonId))
            return Result<PatientResponse>.Failure("El nro. de cédula ya está registrado.", ErrorType.Conflict);

        // RN-P02: email único si se provee, excluyendo el propio registro
        if (!string.IsNullOrWhiteSpace(request.Email) &&
            await _dbContext.Persons.AnyAsync(p => p.Email == request.Email.Trim().ToLower() && p.Id != patient.PersonId))
            return Result<PatientResponse>.Failure("El email ya está registrado.", ErrorType.Conflict);

        var now = DateTime.UtcNow;

        patient.Person.FirstName   = request.FirstName.Trim();
        patient.Person.LastName    = request.LastName.Trim();
        patient.Person.DNI         = request.DNI.Trim();
        patient.Person.BirthDate   = request.BirthDate;
        patient.Person.PhoneNumber = request.PhoneNumber?.Trim();
        patient.Person.Email       = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLower();
        patient.Person.UpdatedAt   = now;

        patient.IsActive   = request.IsActive;
        patient.UpdatedAt  = now;

        await _dbContext.SaveChangesAsync();

        return Result<PatientResponse>.Success(ToResponse(patient));
    }

    public async Task<Result<bool>> DeleteAsync(int id)
    {
        var patient = await _dbContext.Patients
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patient is null)
            return Result<bool>.Failure("Paciente no encontrado.", ErrorType.NotFound);

        patient.IsActive = false;
        patient.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    private static PatientResponse ToResponse(Patient p) => new()
    {
        Id = p.Id,
        UserId = p.UserId,
        DNI = p.Person.DNI,
        FirstName = p.Person.FirstName,
        LastName = p.Person.LastName,
        BirthDate = p.Person.BirthDate,
        PhoneNumber = p.Person.PhoneNumber,
        Email = p.Person.Email,
        IsActive = p.IsActive,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };
}
