namespace SIGA.Domain.Entities;

public class Departamento
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Ciudad> Ciudades { get; set; } = [];
}
