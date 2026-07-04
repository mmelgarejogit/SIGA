using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Empleados;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Domain.Security;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class EmpleadoService(AppDbContext db, IPasswordHasher passwordHasher) : IEmpleadoService
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static EmpleadoResponse Map(Empleado e) => new()
    {
        Id           = e.Id,
        UserId       = e.UserId,
        FirstName    = e.User.Person.FirstName,
        LastName     = e.User.Person.LastName,
        Email        = e.User.Person.Email,
        PhoneNumber  = e.User.Person.PhoneNumber,
        CI           = e.User.Person.CI,
        CargoId      = e.CargoId,
        CargoNombre  = e.Cargo.Nombre,
        FechaIngreso = e.FechaIngreso.ToString("yyyy-MM-dd"),
        FechaEgreso  = e.FechaEgreso?.ToString("yyyy-MM-dd"),
        SalarioBase  = e.SalarioBase,
        SucursalId     = e.User.SucursalId,
        SucursalNombre = e.User.Sucursal?.Nombre,
        IsActive     = e.IsActive,
        CreatedAt    = e.CreatedAt,
    };

    private static CargoEmpleadoResponse MapCargo(CargoEmpleado c) => new()
    {
        Id          = c.Id,
        Nombre      = c.Nombre,
        Descripcion = c.Descripcion,
        Activo      = c.Activo,
    };

    private IQueryable<Empleado> BaseQuery() =>
        db.Empleados
            .Include(e => e.User).ThenInclude(u => u.Person)
            .Include(e => e.User).ThenInclude(u => u.Sucursal)
            .Include(e => e.Cargo);

    // ── Cargos ────────────────────────────────────────────────────────────────

    public async Task<Result<IEnumerable<CargoEmpleadoResponse>>> GetCargosAsync()
    {
        var cargos = await db.CargosEmpleado.OrderBy(c => c.Nombre).ToListAsync();
        return Result<IEnumerable<CargoEmpleadoResponse>>.Success(cargos.Select(MapCargo));
    }

    public async Task<Result<CargoEmpleadoResponse>> CrearCargoAsync(CrearCargoEmpleadoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result<CargoEmpleadoResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        var cargo = new CargoEmpleado
        {
            Nombre      = request.Nombre.Trim(),
            Descripcion = request.Descripcion?.Trim(),
        };
        db.CargosEmpleado.Add(cargo);
        await db.SaveChangesAsync();
        return Result<CargoEmpleadoResponse>.Success(MapCargo(cargo));
    }

    public async Task<Result<CargoEmpleadoResponse>> ActualizarCargoAsync(int id, ActualizarCargoEmpleadoRequest request)
    {
        var cargo = await db.CargosEmpleado.FindAsync(id);
        if (cargo is null)
            return Result<CargoEmpleadoResponse>.Failure("Cargo no encontrado.", ErrorType.NotFound);

        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result<CargoEmpleadoResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        cargo.Nombre      = request.Nombre.Trim();
        cargo.Descripcion = request.Descripcion?.Trim();
        cargo.Activo      = request.Activo;
        await db.SaveChangesAsync();
        return Result<CargoEmpleadoResponse>.Success(MapCargo(cargo));
    }

    // ── Empleados ─────────────────────────────────────────────────────────────

    public async Task<Result<IEnumerable<EmpleadoResponse>>> GetAllAsync(bool? soloActivos)
    {
        var query = BaseQuery();
        if (soloActivos.HasValue)
            query = query.Where(e => e.IsActive == soloActivos.Value);

        var items = await query.OrderBy(e => e.User.Person.LastName).ThenBy(e => e.User.Person.FirstName).ToListAsync();
        return Result<IEnumerable<EmpleadoResponse>>.Success(items.Select(Map));
    }

    public async Task<Result<EmpleadoResponse>> GetByIdAsync(int id)
    {
        var empleado = await BaseQuery().FirstOrDefaultAsync(e => e.Id == id);
        if (empleado is null)
            return Result<EmpleadoResponse>.Failure("Empleado no encontrado.", ErrorType.NotFound);

        return Result<EmpleadoResponse>.Success(Map(empleado));
    }

    public async Task<Result<EmpleadoResponse>> CrearAsync(CrearEmpleadoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            return Result<EmpleadoResponse>.Failure("Nombre y apellido son obligatorios.", ErrorType.Validation);

        if (string.IsNullOrWhiteSpace(request.CI))
            return Result<EmpleadoResponse>.Failure("La CI es obligatoria.", ErrorType.Validation);

        if (await db.Persons.AnyAsync(p => p.CI == request.CI.Trim()))
            return Result<EmpleadoResponse>.Failure("La CI ya está registrada.", ErrorType.Conflict);

        if (await db.Persons.AnyAsync(p => p.Email == request.Email.Trim().ToLower()))
            return Result<EmpleadoResponse>.Failure("El email ya está en uso.", ErrorType.Conflict);

        var cargo = await db.CargosEmpleado.FindAsync(request.CargoId);
        if (cargo is null)
            return Result<EmpleadoResponse>.Failure("Cargo no encontrado.", ErrorType.NotFound);

        if (!DateOnly.TryParse(request.FechaIngreso, out var fechaIngreso))
            return Result<EmpleadoResponse>.Failure("Fecha de ingreso inválida.", ErrorType.Validation);

        if (request.SucursalId.HasValue && !await db.Sucursales.AnyAsync(s => s.Id == request.SucursalId.Value))
            return Result<EmpleadoResponse>.Failure("La sucursal indicada no existe.", ErrorType.Validation);

        var now = DateTime.UtcNow;

        var person = new Person
        {
            CI          = request.CI.Trim(),
            FirstName   = request.FirstName.Trim(),
            LastName    = request.LastName.Trim(),
            PhoneNumber = request.PhoneNumber?.Trim(),
            Email       = request.Email.Trim().ToLower(),
            CreatedAt   = now,
            UpdatedAt   = now,
        };

        var user = new User
        {
            Person       = person,
            SucursalId   = request.SucursalId,
            PasswordHash = passwordHasher.Hash(request.Password),
            IsActive     = true,
            IsEmailVerified = true,
            MustChangePassword = true,
            CreatedAt    = now,
            UpdatedAt    = now,
        };

        var empleado = new Empleado
        {
            User         = user,
            CargoId      = request.CargoId,
            FechaIngreso = fechaIngreso,
            SalarioBase  = request.SalarioBase,
            IsActive     = true,
            CreatedAt    = now,
            UpdatedAt    = now,
        };

        db.Empleados.Add(empleado);
        await db.SaveChangesAsync();

        empleado.Cargo = cargo;
        return Result<EmpleadoResponse>.Success(Map(empleado));
    }

    public async Task<Result<EmpleadoResponse>> ActualizarAsync(int id, ActualizarEmpleadoRequest request)
    {
        var empleado = await BaseQuery().FirstOrDefaultAsync(e => e.Id == id);
        if (empleado is null)
            return Result<EmpleadoResponse>.Failure("Empleado no encontrado.", ErrorType.NotFound);

        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            return Result<EmpleadoResponse>.Failure("Nombre y apellido son obligatorios.", ErrorType.Validation);

        var cargo = await db.CargosEmpleado.FindAsync(request.CargoId);
        if (cargo is null)
            return Result<EmpleadoResponse>.Failure("Cargo no encontrado.", ErrorType.NotFound);

        if (!DateOnly.TryParse(request.FechaIngreso, out var fechaIngreso))
            return Result<EmpleadoResponse>.Failure("Fecha de ingreso inválida.", ErrorType.Validation);

        DateOnly? fechaEgreso = null;
        if (!string.IsNullOrWhiteSpace(request.FechaEgreso))
        {
            if (!DateOnly.TryParse(request.FechaEgreso, out var fe))
                return Result<EmpleadoResponse>.Failure("Fecha de egreso inválida.", ErrorType.Validation);
            fechaEgreso = fe;
        }

        if (request.SucursalId.HasValue && !await db.Sucursales.AnyAsync(s => s.Id == request.SucursalId.Value))
            return Result<EmpleadoResponse>.Failure("La sucursal indicada no existe.", ErrorType.Validation);

        var person = empleado.User.Person;
        person.FirstName   = request.FirstName.Trim();
        person.LastName    = request.LastName.Trim();
        person.PhoneNumber = request.PhoneNumber?.Trim();
        person.UpdatedAt   = DateTime.UtcNow;
        empleado.User.SucursalId = request.SucursalId;

        empleado.CargoId      = request.CargoId;
        empleado.FechaIngreso = fechaIngreso;
        empleado.FechaEgreso  = fechaEgreso;
        empleado.SalarioBase  = request.SalarioBase;
        empleado.UpdatedAt    = DateTime.UtcNow;
        empleado.Cargo        = cargo;

        await db.SaveChangesAsync();
        return Result<EmpleadoResponse>.Success(Map(empleado));
    }

    public async Task<Result<EmpleadoResponse>> DesactivarAsync(int id)
    {
        var empleado = await BaseQuery().FirstOrDefaultAsync(e => e.Id == id);
        if (empleado is null)
            return Result<EmpleadoResponse>.Failure("Empleado no encontrado.", ErrorType.NotFound);

        empleado.IsActive        = false;
        empleado.User.IsActive   = false;
        empleado.UpdatedAt       = DateTime.UtcNow;
        empleado.User.UpdatedAt  = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Result<EmpleadoResponse>.Success(Map(empleado));
    }
}
