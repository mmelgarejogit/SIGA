namespace SIGA.Domain.Entities;

public class SalarioEmpleado : Egreso
{
    public int EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;
    public string? Periodo { get; set; }

    public SalarioEmpleado()
    {
        Tipo = TipoEgreso.Salario;
    }
}
