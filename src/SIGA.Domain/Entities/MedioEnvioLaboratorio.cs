namespace SIGA.Domain.Entities;

/// <summary>Cómo se le comunicó el trabajo al laboratorio externo al registrar el envío.</summary>
public enum MedioEnvioLaboratorio
{
    WhatsApp  = 0,
    Email     = 1,
    Portal    = 2,
    Telefono  = 3,
    EnPersona = 4,
    Otro      = 5,
}
