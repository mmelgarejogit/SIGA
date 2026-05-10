namespace SIGA.Infrastructure.Options;

public class ResendOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    /// <summary>
    /// Si está configurado, redirige todos los emails a esta dirección (útil en desarrollo
    /// cuando Resend no tiene dominio verificado y solo acepta el email del propietario).
    /// </summary>
    public string? DevRedirectEmail { get; set; }
}
