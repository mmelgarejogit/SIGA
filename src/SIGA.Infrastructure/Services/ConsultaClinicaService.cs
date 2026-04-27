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

    public ConsultaClinicaService(AppDbContext db) => _db = db;

    public async Task<Result<PagedResult<ConsultaClinicaResponse>>> GetAllAsync(
        int page, int pageSize, string? search, int? patientId, int? professionalId)
    {
        var query = _db.ConsultasClinicas
            .Include(c => c.Patient).ThenInclude(p => p.Person)
            .Include(c => c.Professional).ThenInclude(pr => pr.User).ThenInclude(u => u.Person)
            .Include(c => c.Receta)
            .AsQueryable();

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
            .Include(c => c.Patient).ThenInclude(p => p.Person)
            .Include(c => c.Professional).ThenInclude(pr => pr.User).ThenInclude(u => u.Person)
            .Include(c => c.Receta)
            .Where(c => c.PatientId == patientId)
            .OrderByDescending(c => c.FechaConsulta)
            .ToListAsync();

        return Result<IEnumerable<ConsultaClinicaResponse>>.Success(consultas.Select(ToResponse));
    }

    public async Task<Result<ConsultaClinicaResponse>> GetByIdAsync(int id)
    {
        var consulta = await _db.ConsultasClinicas
            .Include(c => c.Patient).ThenInclude(p => p.Person)
            .Include(c => c.Professional).ThenInclude(pr => pr.User).ThenInclude(u => u.Person)
            .Include(c => c.Receta)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (consulta is null)
            return Result<ConsultaClinicaResponse>.Failure("Consulta no encontrada.", ErrorType.NotFound);

        return Result<ConsultaClinicaResponse>.Success(ToResponse(consulta));
    }

    public async Task<Result<ConsultaClinicaResponse>> CreateAsync(CreateConsultaClinicaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Motivo))
            return Result<ConsultaClinicaResponse>.Failure("El motivo es obligatorio.", ErrorType.Validation);
        if (string.IsNullOrWhiteSpace(request.DiagnosticoPrincipal))
            return Result<ConsultaClinicaResponse>.Failure("El diagnóstico principal es obligatorio.", ErrorType.Validation);

        if (!await _db.Patients.AnyAsync(p => p.Id == request.PatientId))
            return Result<ConsultaClinicaResponse>.Failure("Paciente no encontrado.", ErrorType.NotFound);

        if (!await _db.Professionals.AnyAsync(p => p.Id == request.ProfessionalId))
            return Result<ConsultaClinicaResponse>.Failure("Profesional no encontrado.", ErrorType.NotFound);

        var now = DateTime.UtcNow;

        var consulta = new ConsultaClinica
        {
            PatientId = request.PatientId,
            ProfessionalId = request.ProfessionalId,
            CitaId = request.CitaId,
            FechaConsulta = request.FechaConsulta,
            Motivo = request.Motivo.Trim(),
            Anamnesis = request.Anamnesis?.Trim(),
            ExamenFisico = request.ExamenFisico?.Trim(),
            DiagnosticoPrincipal = request.DiagnosticoPrincipal.Trim(),
            DiagnosticoSecundario = request.DiagnosticoSecundario?.Trim(),
            PlanTratamiento = request.PlanTratamiento?.Trim(),
            Observaciones = request.Observaciones?.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
        };

        if (request.Receta is not null)
        {
            consulta.Receta = new Receta
            {
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
            .FirstOrDefaultAsync(c => c.Id == id);

        if (consulta is null)
            return Result<ConsultaClinicaResponse>.Failure("Consulta no encontrada.", ErrorType.NotFound);

        consulta.ProfessionalId = request.ProfessionalId;
        consulta.FechaConsulta = request.FechaConsulta;
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

        _db.ConsultasClinicas.Remove(consulta);
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
            consulta.Receta = new Receta
            {
                ConsultaClinicaId = consultaId,
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
        Receta = c.Receta is null ? null : ToRecetaResponse(c.Receta),
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt,
    };

    private static RecetaResponse ToRecetaResponse(Receta r) => new()
    {
        Id = r.Id,
        ConsultaClinicaId = r.ConsultaClinicaId,
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
