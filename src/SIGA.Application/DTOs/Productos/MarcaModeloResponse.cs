namespace SIGA.Application.DTOs.Productos;

public class MarcaResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public bool IsActive { get; set; }
    public int TotalModelos { get; set; }
}

public class CreateMarcaRequest
{
    public string Nombre { get; set; } = "";
}

public class UpdateMarcaRequest
{
    public string Nombre { get; set; } = "";
    public bool IsActive { get; set; } = true;
}

public class ModeloResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public int MarcaId { get; set; }
    public string MarcaNombre { get; set; } = "";
    public bool IsActive { get; set; }
}

public class CreateModeloRequest
{
    public string Nombre { get; set; } = "";
    public int MarcaId { get; set; }
}

public class UpdateModeloRequest
{
    public string Nombre { get; set; } = "";
    public int MarcaId { get; set; }
    public bool IsActive { get; set; } = true;
}
