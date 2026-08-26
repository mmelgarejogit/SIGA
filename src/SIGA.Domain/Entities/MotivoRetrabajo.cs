namespace SIGA.Domain.Entities;

public enum MotivoRetrabajo
{
    // El laboratorio devolvió el trabajo defectuoso (graduación equivocada, rayado, tratamiento mal aplicado).
    DefectoLaboratorio  = 1,
    // Error de la óptica al tomar/transcribir la medida (DNP, graduación, montaje).
    ErrorOptica         = 2,
    // La graduación estaba bien pero el cliente no logra adaptarse (típico en progresivos).
    NoAdaptacionCliente = 3,
    // Falla dentro del período de garantía (antirreflejo que se pela, capa que se despega).
    RoturaGarantia      = 4,
    Otro                = 5,
}
