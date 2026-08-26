namespace SIGA.Domain.Security;

/// <summary>
/// Reglas de contraseña centralizadas — antes cada servicio repetía (o directamente
/// omitía, en EmpleadoService/ProfessionalService) su propio chequeo de longitud mínima.
/// </summary>
public static class PasswordPolicy
{
    public const int MinLength = 8;

    /// <summary>Reglas de complejidad, sin comparar contra ninguna contraseña anterior
    /// (alta inicial de usuario, registro). Null = válida.</summary>
    public static string? Validate(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return "La contraseña es obligatoria.";

        if (password.Length < MinLength)
            return $"La contraseña debe tener al menos {MinLength} caracteres.";

        if (!password.Any(char.IsLetter))
            return "La contraseña debe tener al menos una letra.";

        if (!password.Any(char.IsDigit))
            return "La contraseña debe tener al menos un número.";

        return null;
    }

    /// <summary>Reglas de complejidad + que la nueva contraseña sea distinta de la
    /// actual (cambio de contraseña, reset propio o por admin). Null = válida.</summary>
    public static string? ValidateNew(string? newPassword, string currentHash, IPasswordHasher hasher)
    {
        var error = Validate(newPassword);
        if (error is not null)
            return error;

        if (hasher.Verify(newPassword!, currentHash))
            return "La nueva contraseña debe ser diferente a la actual.";

        return null;
    }
}
