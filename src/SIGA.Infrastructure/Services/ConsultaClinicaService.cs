using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Clinica;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class ConsultaClinicaService : IConsultaClinicaService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserContext _current;

    public ConsultaClinicaService(AppDbContext db, ICurrentUserContext current)
    {
        _db = db;
        _current = current;
    }

    public async Task<Result<PagedResult<ConsultaClinicaResponse>>> GetAllAsync(
        int page, int pageSize, string? search, int? patientId, int? professionalId)
    {
        var query = _db.ConsultasClinicas
            .Where(c => c.IsActive)
            .Include(c => c.Patient).ThenInclude(p => p.Person)
            .Include(c => c.Professional).ThenInclude(pr => pr.User).ThenInclude(u => u.Person)
            .Include(c => c.Receta)
            .Include(c => c.EstadoConfig)
            .AsQueryable();

        if (_current.SucursalId is int suc)
            query = query.Where(c => c.SucursalId == suc);

        if (patientId.HasValue)
            query = query.Where(c => c.PatientId == patientId.Value);

        if (professionalId.HasValue)
            query = query.Where(c => c.ProfessionalId == professionalId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim().ToLower();
            query = query.Where(c =>
                c.Patient.Person.FirstName.ToLower().Contains(q) ||
                c.Patient.Person.LastName.ToLower().Contains(q) ||
                c.Patient.Person.CI.ToLower().Contains(q) ||
                c.DiagnosticoPrincipal.ToLower().Contains(q) ||
                c.Motivo.ToLower().Contains(q));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .OrderByDescending(c => c.FechaConsulta)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Result<PagedResult<ConsultaClinicaResponse>>.Success(new PagedResult<ConsultaClinicaResponse>
        {
            Items = items.Select(ToResponse),
            TotalCount = totalCount,
            TotalActive = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
        });
    }

    public async Task<Result<IEnumerable<ConsultaClinicaResponse>>> GetByPatientAsync(int patientId)
    {
        var patientExists = await _db.Patients.AnyAsync(p => p.Id == patientId);
        if (!patientExists)
            return Result<IEnumerable<ConsultaClinicaResponse>>.Failure("Paciente no encontrado.", ErrorType.NotFound);

        var consultas = await _db.ConsultasClinicas
            .Where(c => c.IsActive && c.PatientId == patientId)
            .Include(c => c.Patient).ThenInclude(p => p.Person)
            .Include(c => c.Professional).ThenInclude(pr => pr.User).ThenInclude(u => u.Person)
            .Include(c => c.Receta)
            .Include(c => c.EstadoConfig)
            .OrderByDescending(c => c.FechaConsulta)
            .ToListAsync();

        return Result<IEnumerable<ConsultaClinicaResponse>>.Success(consultas.Select(ToResponse));
    }

    private async Task<int?> ResolvePatientIdAsync(int userId)
    {
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
        return patient?.Id;
    }

    public async Task<Result<IEnumerable<ConsultaClinicaResponse>>> GetMisConsultasAsync(int userId)
    {
        var patientId = await ResolvePatientIdAsync(userId);
        if (!patientId.HasValue)
            return Result<IEnumerable<ConsultaClinicaResponse>>.Failure("No se encontró perfil de paciente asociado a tu cuenta.", ErrorType.NotFound);

        return await GetByPatientAsync(patientId.Value);
    }

    public async Task<Result<ConsultaClinicaResponse>> GetMiConsultaAsync(int userId, int consultaId)
    {
        var patientId = await ResolvePatientIdAsync(userId);
        if (!patientId.HasValue)
            return Result<ConsultaClinicaResponse>.Failure("No se encontró perfil de paciente asociado a tu cuenta.", ErrorType.NotFound);

        var consulta = await _db.ConsultasClinicas
            .Where(c => c.IsActive)
            .Include(c => c.Patient).ThenInclude(p => p.Person)
            .Include(c => c.Professional).ThenInclude(pr => pr.User).ThenInclude(u => u.Person)
            .Include(c => c.Receta)
            .Include(c => c.EstadoConfig)
            .FirstOrDefaultAsync(c => c.Id == consultaId && c.PatientId == patientId.Value);

        // Devolver NotFound aunque exista pero no sea del paciente (no filtrar existencia).
        if (consulta is null)
            return Result<ConsultaClinicaResponse>.Failure("Consulta no encontrada.", ErrorType.NotFound);

        return Result<ConsultaClinicaResponse>.Success(ToResponse(consulta));
    }

    public async Task<Result<ConsultaClinicaResponse>> GetByIdAsync(int id)
    {
        var consulta = await _db.ConsultasClinicas
            .Include(c => c.Patient).ThenInclude(p => p.Person)
            .Include(c => c.Professional).ThenInclude(pr => pr.User).ThenInclude(u => u.Person)
            .Include(c => c.Receta)
            .Include(c => c.EstadoConfig)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (consulta is null)
            return Result<ConsultaClinicaResponse>.Failure("Consulta no encontrada.", ErrorType.NotFound);

        return Result<ConsultaClinicaResponse>.Success(ToResponse(consulta));
    }

    public async Task<Result<ConsultaClinicaResponse>> CreateAsync(CreateConsultaClinicaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Motivo))
            return Result<ConsultaClinicaResponse>.Failure("El motivo es obligatorio.", ErrorType.Validation);

        if (!await _db.Patients.AnyAsync(p => p.Id == request.PatientId))
            return Result<ConsultaClinicaResponse>.Failure("Paciente no encontrado.", ErrorType.NotFound);

        if (!await _db.Professionals.AnyAsync(p => p.Id == request.ProfessionalId))
            return Result<ConsultaClinicaResponse>.Failure("Profesional no encontrado.", ErrorType.NotFound);

        var now = DateTime.UtcNow;

        int? estadoFinal;
        if (request.EstadoConfigId.HasValue)
        {
            estadoFinal = request.EstadoConfigId.Value;
        }
        else
        {
            var estadoAbierta = await _db.EstadosConfig
                .FirstOrDefaultAsync(e => e.Entidad == "Consulta" && e.CodigoInterno == "Abierta");
            estadoFinal = estadoAbierta?.Id;
        }

        var consulta = new ConsultaClinica
        {
            SucursalId = await SucursalResolver.WriteBranchAsync(_db, _current),
            PatientId = request.PatientId,
            ProfessionalId = request.ProfessionalId,
            CitaId = request.CitaId,
            EstadoConfigId = estadoFinal,
            FechaConsulta = DateTime.SpecifyKind(request.FechaConsulta, DateTimeKind.Utc),
            Motivo = request.Motivo.Trim(),
            Anamnesis = request.Anamnesis?.Trim(),
            ExamenFisico = request.ExamenFisico?.Trim(),
            DiagnosticoPrincipal = request.DiagnosticoPrincipal?.Trim() ?? string.Empty,
            DiagnosticoSecundario = request.DiagnosticoSecundario?.Trim(),
            PlanTratamiento = request.PlanTratamiento?.Trim(),
            Observaciones = request.Observaciones?.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
        };

        if (request.Receta is not null)
        {
            var personIdReceta = await _db.Patients
                .Where(p => p.Id == request.PatientId)
                .Select(p => (int?)p.PersonId)
                .FirstOrDefaultAsync();

            consulta.Receta = new Receta
            {
                PersonId = personIdReceta,
                FechaEmision = request.Receta.FechaEmision,
                OdEsferico = request.Receta.OdEsferico,
                OdCilindro = request.Receta.OdCilindro,
                OdEje = request.Receta.OdEje,
                OdAdicion = request.Receta.OdAdicion,
                OiEsferico = request.Receta.OiEsferico,
                OiCilindro = request.Receta.OiCilindro,
                OiEje = request.Receta.OiEje,
                OiAdicion = request.Receta.OiAdicion,
                DistanciaInterpupilar = request.Receta.DistanciaInterpupilar,
                AvSinCorreccion = request.Receta.AvSinCorreccion?.Trim(),
                AvConCorreccion = request.Receta.AvConCorreccion?.Trim(),
                Observaciones = request.Receta.Observaciones?.Trim(),
                CreatedAt = now,
                UpdatedAt = now,
            };
        }

        _db.ConsultasClinicas.Add(consulta);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(consulta.Id);
    }

    public async Task<Result<ConsultaClinicaResponse>> UpdateAsync(int id, UpdateConsultaClinicaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Motivo))
            return Result<ConsultaClinicaResponse>.Failure("El motivo es obligatorio.", ErrorType.Validation);
        if (string.IsNullOrWhiteSpace(request.DiagnosticoPrincipal))
            return Result<ConsultaClinicaResponse>.Failure("El diagnóstico principal es obligatorio.", ErrorType.Validation);

        if (!await _db.Professionals.AnyAsync(p => p.Id == request.ProfessionalId))
            return Result<ConsultaClinicaResponse>.Failure("Profesional no encontrado.", ErrorType.NotFound);

        var consulta = await _db.ConsultasClinicas
            .Include(c => c.Patient).ThenInclude(p => p.Person)
            .Include(c => c.Professional).ThenInclude(pr => pr.User).ThenInclude(u => u.Person)
            .Include(c => c.Receta)
            .Include(c => c.EstadoConfig)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (consulta is null)
            return Result<ConsultaClinicaResponse>.Failure("Consulta no encontrada.", ErrorType.NotFound);

        consulta.ProfessionalId = request.ProfessionalId;
        consulta.FechaConsulta = DateTime.SpecifyKind(request.FechaConsulta, DateTimeKind.Utc);
        consulta.Motivo = request.Motivo.Trim();
        consulta.Anamnesis = request.Anamnesis?.Trim();
        consulta.ExamenFisico = request.ExamenFisico?.Trim();
        consulta.DiagnosticoPrincipal = request.DiagnosticoPrincipal.Trim();
        consulta.DiagnosticoSecundario = request.DiagnosticoSecundario?.Trim();
        consulta.PlanTratamiento = request.PlanTratamiento?.Trim();
        consulta.Observaciones = request.Observaciones?.Trim();
        consulta.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Result<ConsultaClinicaResponse>.Success(ToResponse(consulta));
    }

    public async Task<Result<bool>> DeleteAsync(int id)
    {
        var consulta = await _db.ConsultasClinicas.FirstOrDefaultAsync(c => c.Id == id);

        if (consulta is null)
            return Result<bool>.Failure("Consulta no encontrada.", ErrorType.NotFound);

        consulta.IsActive = false;
        consulta.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    public async Task<Result<RecetaResponse>> CreateOrUpdateRecetaAsync(int consultaId, CreateRecetaRequest request)
    {
        var consulta = await _db.ConsultasClinicas
            .Include(c => c.Receta)
            .FirstOrDefaultAsync(c => c.Id == consultaId);

        if (consulta is null)
            return Result<RecetaResponse>.Failure("Consulta no encontrada.", ErrorType.NotFound);

        var now = DateTime.UtcNow;

        if (consulta.Receta is null)
        {
            var personIdReceta = await _db.Patients
                .Where(p => p.Id == consulta.PatientId)
                .Select(p => (int?)p.PersonId)
                .FirstOrDefaultAsync();

            consulta.Receta = new Receta
            {
                ConsultaClinicaId = consultaId,
                PersonId = personIdReceta,
                FechaEmision = request.FechaEmision,
                OdEsferico = request.OdEsferico,
                OdCilindro = request.OdCilindro,
                OdEje = request.OdEje,
                OdAdicion = request.OdAdicion,
                OiEsferico = request.OiEsferico,
                OiCilindro = request.OiCilindro,
                OiEje = request.OiEje,
                OiAdicion = request.OiAdicion,
                DistanciaInterpupilar = request.DistanciaInterpupilar,
                AvSinCorreccion = request.AvSinCorreccion?.Trim(),
                AvConCorreccion = request.AvConCorreccion?.Trim(),
                Observaciones = request.Observaciones?.Trim(),
                CreatedAt = now,
                UpdatedAt = now,
            };
        }
        else
        {
            var r = consulta.Receta;
            r.FechaEmision = request.FechaEmision;
            r.OdEsferico = request.OdEsferico;
            r.OdCilindro = request.OdCilindro;
            r.OdEje = request.OdEje;
            r.OdAdicion = request.OdAdicion;
            r.OiEsferico = request.OiEsferico;
            r.OiCilindro = request.OiCilindro;
            r.OiEje = request.OiEje;
            r.OiAdicion = request.OiAdicion;
            r.DistanciaInterpupilar = request.DistanciaInterpupilar;
            r.AvSinCorreccion = request.AvSinCorreccion?.Trim();
            r.AvConCorreccion = request.AvConCorreccion?.Trim();
            r.Observaciones = request.Observaciones?.Trim();
            r.UpdatedAt = now;
        }

        await _db.SaveChangesAsync();

        return Result<RecetaResponse>.Success(ToRecetaResponse(consulta.Receta));
    }

    public async Task<Result<ConsultaClinicaResponse>> CambiarEstadoAsync(int id, int estadoConfigId)
    {
        var consulta = await _db.ConsultasClinicas
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

        if (consulta is null)
            return Result<ConsultaClinicaResponse>.Failure("Consulta no encontrada.", ErrorType.NotFound);

        var estado = await _db.EstadosConfig
            .FirstOrDefaultAsync(e => e.Id == estadoConfigId && e.Entidad == "Consulta");

        if (estado is null)
            return Result<ConsultaClinicaResponse>.Failure("Estado no válido para consultas.", ErrorType.Validation);

        consulta.EstadoConfigId = estadoConfigId;
        consulta.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    private static ConsultaClinicaResponse ToResponse(ConsultaClinica c) => new()
    {
        Id = c.Id,
        PatientId = c.PatientId,
        PatientFirstName = c.Patient.Person.FirstName,
        PatientLastName = c.Patient.Person.LastName,
        PatientCI = c.Patient.Person.CI,
        ProfessionalId = c.ProfessionalId,
        ProfessionalFirstName = c.Professional.User.Person.FirstName,
        ProfessionalLastName = c.Professional.User.Person.LastName,
        CitaId = c.CitaId,
        FechaConsulta = c.FechaConsulta,
        Motivo = c.Motivo,
        Anamnesis = c.Anamnesis,
        ExamenFisico = c.ExamenFisico,
        DiagnosticoPrincipal = c.DiagnosticoPrincipal,
        DiagnosticoSecundario = c.DiagnosticoSecundario,
        PlanTratamiento = c.PlanTratamiento,
        Observaciones = c.Observaciones,
        EstadoId = c.EstadoConfigId,
        EstadoNombre = c.EstadoConfig?.Nombre,
        EstadoColor = c.EstadoConfig?.Color,
        Receta = c.Receta is null ? null : ToRecetaResponse(c.Receta),
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt,
    };

    public async Task<Result<ProfessionalDashboardStatsResponse>> GetProfessionalStatsAsync(int professionalId)
    {
        var now      = DateTime.UtcNow;
        var today    = now.Date;
        var weekStart = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var base_ = _db.ConsultasClinicas
            .Where(c => c.IsActive && c.ProfessionalId == professionalId);

        var consultasHoy        = await base_.CountAsync(c => c.FechaConsulta.Date == today);
        var consultasEstaSemana = await base_.CountAsync(c => c.FechaConsulta.Date >= weekStart);
        var consultasEsteMes    = await base_.CountAsync(c => c.FechaConsulta >= monthStart);

        var pacientesUnicos = await base_
            .Where(c => c.FechaConsulta >= monthStart)
            .Select(c => c.PatientId)
            .Distinct()
            .CountAsync();

        var recetasEmitidas = await base_
            .Where(c => c.FechaConsulta >= monthStart && c.Receta != null)
            .CountAsync();

        var ultimas = await _db.ConsultasClinicas
            .Where(c => c.IsActive && c.ProfessionalId == professionalId)
            .Include(c => c.Patient).ThenInclude(p => p.Person)
            .Include(c => c.Professional).ThenInclude(p => p.User).ThenInclude(u => u.Person)
            .Include(c => c.Receta)
            .OrderByDescending(c => c.FechaConsulta)
            .Take(5)
            .ToListAsync();

        return Result<ProfessionalDashboardStatsResponse>.Success(new ProfessionalDashboardStatsResponse
        {
            ConsultasHoy            = consultasHoy,
            ConsultasEstaSemana     = consultasEstaSemana,
            ConsultasEsteMes        = consultasEsteMes,
            PacientesUnicosEsteMes  = pacientesUnicos,
            RecetasEmitidasEsteMes  = recetasEmitidas,
            UltimasConsultas        = ultimas.Select(ToResponse).ToList(),
        });
    }

    private static RecetaResponse ToRecetaResponse(Receta r) => new()
    {
        Id = r.Id,
        ConsultaClinicaId = r.ConsultaClinicaId,
        PersonId = r.PersonId,
        EsExterna = r.ConsultaClinicaId == null,
        FechaEmision = r.FechaEmision,
        OdEsferico = r.OdEsferico,
        OdCilindro = r.OdCilindro,
        OdEje = r.OdEje,
        OdAdicion = r.OdAdicion,
        OiEsferico = r.OiEsferico,
        OiCilindro = r.OiCilindro,
        OiEje = r.OiEje,
        OiAdicion = r.OiAdicion,
        DistanciaInterpupilar = r.DistanciaInterpupilar,
        AvSinCorreccion = r.AvSinCorreccion,
        AvConCorreccion = r.AvConCorreccion,
        Observaciones = r.Observaciones,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
    };
}
