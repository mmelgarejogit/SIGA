namespace SIGA.Application.DTOs.Patients;

public class UpsertDatosFacturacionRequest
{
    public string? RucCiFiscal { get; set; }
    public string? RazonSocial { get; set; }
    public string? Direccion { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
}
