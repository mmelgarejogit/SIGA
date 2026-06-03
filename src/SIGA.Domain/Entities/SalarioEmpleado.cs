namespace SIGA.Domain.Entities;

public class SalarioEmpleado : Egreso
{
    public int EmpleadoId { get; set; }
    public Empleado Empleado { get; set; } = null!;
    public int PeriodoMes { get; set; }
    public int PeriodoAnio { get; set; }

    public SalarioEmpleado()
    {
        Tipo = TipoEgreso.Salario;
    }
}