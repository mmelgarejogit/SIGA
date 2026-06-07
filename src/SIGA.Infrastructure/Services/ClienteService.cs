using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Clientes;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class ClienteService : IClienteService
{
    private readonly AppDbContext _dbContext;

    private static readonly Regex OnlyLetters = new(@"^[\p{L}\s]+$", RegexOptions.Compiled);
    private static readonly Regex EmailFormat = new(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled);

    private static readonly HashSet<string> SexoValidos = new(StringComparer.OrdinalIgnoreCase)
        { "Masculino", "Femenino", "Otro" };

    public ClienteService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PagedResult<ClienteResponse>>> GetAllAsync(int page, int pageSize, string? search, string? status, string? tipo)
    {
        var query = _dbContext.Clientes
            .Include(c => c.Person)
            .AsQueryable();

        if (status == "active")   query = query.Where(c => c.IsActive);
        if (status == "inactive") query = query.Where(c => !c.IsActive);

        if (tipo == "fisica")   query = query.Where(c => c.TipoFacturacion == TipoFacturacion.Fisica);
        if (tipo == "juridica") query = query.Where(c => c.TipoFacturacion == TipoFacturacion.Juridica);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim().ToLower();
            query = query.Where(c =>
                c.Person.FirstName.ToLower().Contains(q) ||
                c.Person.LastName.ToLower().Contains(q)  ||
                c.Person.CI.ToLower().Contains(q)        ||
                (c.RazonSocial != null && c.RazonSocial.ToLower().Contains(q)) ||
                (c.RucCiFiscal != null && c.RucCiFiscal.ToLower().Contains(q))
            );
        }

        var totalCount  = await query.CountAsync();
        var totalActive = await _dbContext.Clientes.CountAsync(c => c.IsActive);
        var totalPages  = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Result<PagedResult<ClienteResponse>>.Success(new PagedResult<ClienteResponse>
        {
            Items       = items.Select(ToResponse),
            TotalCount  = totalCount,
            TotalActive = totalActive,
            Page        = page,
            PageSize    = pageSize,
            TotalPages  = totalPages,
        });
    }

    public async Task<Result<ClienteResponse>> GetByIdAsync(int id)
    {
        var cliente = await _dbContext.Clientes
            .Include(c => c.Person)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cliente is null)
            return Result<ClienteResponse>.Failure("Cliente no encontrado.", ErrorType.NotFound);

        return Result<ClienteResponse>.Success(ToResponse(cliente));
    }

    public async Task<Result<PersonLookupResponse?>> BuscarPersonaPorCiAsync(string ci)
    {
        if (string.IsNullOrWhiteSpace(ci))
            return Result<PersonLookupResponse?>.Success(null);

        var person = await _dbContext.Persons
            .FirstOrDefaultAsync(p => p.CI == ci.Trim());

        if (person is null)
            return Result<PersonLookupResponse?>.Success(null);

        var yaEsCliente = await _dbContext.Clientes.AnyAsync(c => c.PersonId == person.Id);

        return Result<PersonLookupResponse?>.Success(new PersonLookupResponse
        {
            PersonId    = person.Id,
            Ci          = person.CI,
            FirstName   = person.FirstName,
            LastName    = person.LastName,
            BirthDate   = person.BirthDate,
            Sexo        = person.Sexo,
            PhoneNumber = person.PhoneNumber,
            Email       = person.Email,
            YaEsCliente = yaEsCliente,
        });
    }

    public async Task<Result<ClienteResponse>> CreateAsync(CreateClienteRequest request)
    {
        if (!TryParseTipo(request.TipoFacturacion, out var tipo))
            return Result<ClienteResponse>.Failure("El tipo de facturación no es válido.", ErrorType.Validation);

        var facturacionError = ValidarFacturacion(tipo, request.RazonSocial, request.RucCiFiscal, request.Email);
        if (facturacionError is not null)
            return Result<ClienteResponse>.Failure(facturacionError, ErrorType.Validation);

        var now = DateTime.UtcNow;
        Person person;

        if (request.PersonId is int personId)
        {
            var existing = await _dbContext.Persons.FirstOrDefaultAsync(p => p.Id == personId);
            if (existing is null)
                return Result<ClienteResponse>.Failure("La persona seleccionada no existe.", ErrorType.NotFound);

            if (await _dbContext.Clientes.AnyAsync(c => c.PersonId == personId))
                return Result<ClienteResponse>.Failure("Esta persona ya está registrada como cliente.", ErrorType.Conflict);

            person = existing;
        }
        else
        {
            var personError = ValidarPersona(request.Ci, request.FirstName, request.LastName, request.Sexo, request.PersonEmail);
            if (personError is not null)
                return Result<ClienteResponse>.Failure(personError, ErrorType.Validation);

            if (await _dbContext.Persons.AnyAsync(p => p.CI == request.Ci!.Trim()))
                return Result<ClienteResponse>.Failure("El CI ya está registrado. Buscá la persona existente para registrarla como cliente.", ErrorType.Conflict);

            if (!string.IsNullOrWhiteSpace(request.PersonEmail) &&
                await _dbContext.Persons.AnyAsync(p => p.Email == request.PersonEmail!.Trim().ToLower()))
                return Result<ClienteResponse>.Failure("El email ya está registrado.", ErrorType.Conflict);

            person = new Person
            {
                CI          = request.Ci!.Trim(),
                FirstName   = request.FirstName!.Trim(),
                LastName    = request.LastName!.Trim(),
                BirthDate   = request.BirthDate ?? default,
                Sexo        = string.IsNullOrWhiteSpace(request.Sexo) ? null : request.Sexo.Trim(),
                PhoneNumber = request.PhoneNumber?.Trim(),
                Email       = string.IsNullOrWhiteSpace(request.PersonEmail) ? null : request.PersonEmail.Trim().ToLower(),
                CreatedAt   = now,
                UpdatedAt   = now,
            };
            _dbContext.Persons.Add(person);
        }

        var cliente = new Cliente
        {
            Person          = person,
            TipoFacturacion = tipo,
            RazonSocial     = request.RazonSocial?.Trim(),
            RucCiFiscal     = request.RucCiFiscal?.Trim(),
            Direccion       = request.Direccion?.Trim(),
            Email           = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLower(),
            Telefono        = request.Telefono?.Trim(),
            IsActive        = true,
            CreatedAt       = now,
            UpdatedAt       = now,
        };

        _dbContext.Clientes.Add(cliente);
        await _dbContext.SaveChangesAsync();

        return Result<ClienteResponse>.Success(ToResponse(cliente));
    }

    public async Task<Result<ClienteResponse>> UpdateAsync(int id, UpdateClienteRequest request)
    {
        if (!TryParseTipo(request.TipoFacturacion, out var tipo))
            return Result<ClienteResponse>.Failure("El tipo de facturación no es válido.", ErrorType.Validation);

        var facturacionError = ValidarFacturacion(tipo, request.RazonSocial, request.RucCiFiscal, request.Email);
        if (facturacionError is not null)
            return Result<ClienteResponse>.Failure(facturacionError, ErrorType.Validation);

        var personError = ValidarPersonaEdicion(request.FirstName, request.LastName, request.Sexo, request.PersonEmail);
        if (personError is not null)
            return Result<ClienteResponse>.Failure(personError, ErrorType.Validation);

        var cliente = await _dbContext.Clientes
            .Include(c => c.Person)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cliente is null)
            return Result<ClienteResponse>.Failure("Cliente no encontrado.", ErrorType.NotFound);

        if (!string.IsNullOrWhiteSpace(request.PersonEmail) &&
            await _dbContext.Persons.AnyAsync(p => p.Email == request.PersonEmail!.Trim().ToLower() && p.Id != cliente.PersonId))
            return Result<ClienteResponse>.Failure("El email ya está registrado.", ErrorType.Conflict);

        var now = DateTime.UtcNow;

        // CI no es modificable: se preserva el valor original
        cliente.Person.FirstName   = request.FirstName.Trim();
        cliente.Person.LastName    = request.LastName.Trim();
        cliente.Person.BirthDate   = request.BirthDate;
        cliente.Person.Sexo        = string.IsNullOrWhiteSpace(request.Sexo) ? null : request.Sexo.Trim();
        cliente.Person.PhoneNumber = request.PhoneNumber?.Trim();
        cliente.Person.Email       = string.IsNullOrWhiteSpace(request.PersonEmail) ? null : request.PersonEmail.Trim().ToLower();
        cliente.Person.UpdatedAt   = now;

        cliente.TipoFacturacion = tipo;
        cliente.RazonSocial     = request.RazonSocial?.Trim();
        cliente.RucCiFiscal     = request.RucCiFiscal?.Trim();
        cliente.Direccion       = request.Direccion?.Trim();
        cliente.Email           = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLower();
        cliente.Telefono        = request.Telefono?.Trim();
        cliente.UpdatedAt       = now;

        await _dbContext.SaveChangesAsync();

        return Result<ClienteResponse>.Success(ToResponse(cliente));
    }

    public async Task<Result<bool>> DesactivarAsync(int id)
    {
        var cliente = await _dbContext.Clientes.FirstOrDefaultAsync(c => c.Id == id);
        if (cliente is null)
            return Result<bool>.Failure("Cliente no encontrado.", ErrorType.NotFound);

        cliente.IsActive  = false;
        cliente.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> ActivarAsync(int id)
    {
        var cliente = await _dbContext.Clientes.FirstOrDefaultAsync(c => c.Id == id);
        if (cliente is null)
            return Result<bool>.Failure("Cliente no encontrado.", ErrorType.NotFound);

        cliente.IsActive  = true;
        cliente.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    private static bool TryParseTipo(string? value, out TipoFacturacion tipo)
    {
        tipo = TipoFacturacion.Fisica;
        if (string.IsNullOrWhiteSpace(value)) return true;
        return Enum.TryParse(value.Trim(), ignoreCase: true, out tipo);
    }

    private static string? ValidarFacturacion(TipoFacturacion tipo, string? razonSocial, string? rucCiFiscal, string? email)
    {
        if (tipo == TipoFacturacion.Juridica)
        {
            if (string.IsNullOrWhiteSpace(razonSocial))
                return "La razón social es obligatoria para facturación jurídica.";
            if (string.IsNullOrWhiteSpace(rucCiFiscal))
                return "El RUC es obligatorio para facturación jurídica.";
        }

        if (!string.IsNullOrWhiteSpace(email) && !EmailFormat.IsMatch(email.Trim()))
            return "El formato del email de facturación no es válido.";

        return null;
    }

    private static string? ValidarPersona(string? ci, string? firstName, string? lastName, string? sexo, string? email)
    {
        if (string.IsNullOrWhiteSpace(ci))
            return "El documento es obligatorio.";

        return ValidarPersonaEdicion(firstName, lastName, sexo, email);
    }

    private static string? ValidarPersonaEdicion(string? firstName, string? lastName, string? sexo, string? email)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return "El nombre es obligatorio.";
        if (!OnlyLetters.IsMatch(firstName.Trim()))
            return "El nombre solo puede contener letras y espacios.";

        if (string.IsNullOrWhiteSpace(lastName))
            return "El apellido es obligatorio.";
        if (!OnlyLetters.IsMatch(lastName.Trim()))
            return "El apellido solo puede contener letras y espacios.";

        if (!string.IsNullOrWhiteSpace(sexo) && !SexoValidos.Contains(sexo))
            return "El valor de sexo no es válido.";

        if (!string.IsNullOrWhiteSpace(email) && !EmailFormat.IsMatch(email.Trim()))
            return "El formato del email no es válido.";

        return null;
    }

    private static ClienteResponse ToResponse(Cliente c) => new()
    {
        Id              = c.Id,
        PersonId        = c.PersonId,
        Ci              = c.Person.CI,
        FirstName       = c.Person.FirstName,
        LastName        = c.Person.LastName,
        BirthDate       = c.Person.BirthDate,
        Sexo            = c.Person.Sexo,
        PhoneNumber     = c.Person.PhoneNumber,
        PersonEmail     = c.Person.Email,
        TipoFacturacion = c.TipoFacturacion.ToString(),
        RazonSocial     = c.RazonSocial,
        RucCiFiscal     = c.RucCiFiscal,
        Direccion       = c.Direccion,
        Email           = c.Email,
        Telefono        = c.Telefono,
        IsActive        = c.IsActive,
        CreatedAt       = c.CreatedAt,
        UpdatedAt       = c.UpdatedAt,
    };
}
