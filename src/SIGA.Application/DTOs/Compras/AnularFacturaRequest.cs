using System.ComponentModel.DataAnnotations;

namespace SIGA.Application.DTOs.Compras;

public class AnularFacturaRequest
{
    [Required(ErrorMessage = "El motivo de anulación es obligatorio.")]
    [MaxLength(500, ErrorMessage = "El motivo no puede superar los 500 caracteres.")]
    public string Motivo { get; set; } = "";
}
