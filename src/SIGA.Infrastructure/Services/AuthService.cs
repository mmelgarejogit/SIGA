using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Auth;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Domain.Security;
using SIGA.Infrastructure.Options;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IHCaptchaService _hCaptchaService;
    private readonly IEmailService _emailService;
    private readonly AppOptions _appOptions;
    private readonly ICurrentUserContext _current;
    private readonly IAuditService _audit;

    public AuthService(
        AppDbContext dbContext,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IHCaptchaService hCaptchaService,
        IEmailService emailService,
        IOptions<AppOptions> appOptions,
        ICurrentUserContext current,
        IAuditService audit)
    {
        _dbContext         = dbContext;
        _passwordHasher    = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _hCaptchaService   = hCaptchaService;
        _emailService      = emailService;
        _appOptions        = appOptions.Value;
        _current           = current;
        _audit             = audit;
    }

    public async Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            return Result<RegisterResponse>.Failure("First name and last name are required.", ErrorType.Validation);

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return Result<RegisterResponse>.Failure("Email and password are required.", ErrorType.Validation);

        var passwordError = PasswordPolicy.Validate(request.Password);
        if (passwordError is not null)
            return Result<RegisterResponse>.Failure(passwordError, ErrorType.Validation);

        if (string.IsNullOrWhiteSpace(request.CI))
            return Result<RegisterResponse>.Failure("CI is required.", ErrorType.Validation);

        var email = request.Email.Trim().ToLower();

        if (await _dbContext.Persons.AnyAsync(p => p.Email == email))
            return Result<RegisterResponse>.Failure("Email is already in use.", ErrorType.Conflict);

        if (await _dbContext.Persons.AnyAsync(p => p.CI == request.CI.Trim()))
            return Result<RegisterResponse>.Failure("CI is already in use.", ErrorType.Conflict);

        var now = DateTime.UtcNow;

        var person = new Person
        {
            CI          = request.CI.Trim(),
            FirstName   = request.FirstName.Trim(),
            LastName    = request.LastName.Trim(),
            BirthDate   = request.BirthDate,
            PhoneNumber = request.PhoneNumber?.Trim(),
            Email       = email,
            CreatedAt   = now,
            UpdatedAt   = now
        };

        var user = new User
        {
            Person            = person,
            PasswordHash      = _passwordHasher.Hash(request.Password),
            IsActive          = true,
            IsEmailVerified   = true,
            CreatedAt         = now,
            UpdatedAt         = now
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return Result<RegisterResponse>.Success(new RegisterResponse
        {
            Email     = person.Email,
            FirstName = person.FirstName,
            LastName  = person.LastName
        });
    }

    public async Task<Result<RegisterResponse>> RegisterPatientAsync(RegisterPatientRequest request)
    {
        if (!await _hCaptchaService.VerifyAsync(request.HCaptchaToken))
            return Result<RegisterResponse>.Failure("La verificación de seguridad falló. Intentá de nuevo.", ErrorType.Validation);

        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            return Result<RegisterResponse>.Failure("El nombre y apellido son obligatorios.", ErrorType.Validation);

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return Result<RegisterResponse>.Failure("El email y la contraseña son obligatorios.", ErrorType.Validation);

        var passwordError = PasswordPolicy.Validate(request.Password);
        if (passwordError is not null)
            return Result<RegisterResponse>.Failure(passwordError, ErrorType.Validation);

        if (string.IsNullOrWhiteSpace(request.CI))
            return Result<RegisterResponse>.Failure("El documento es obligatorio.", ErrorType.Validation);

        var email = request.Email.Trim().ToLower();

        if (await _dbContext.Persons.AnyAsync(p => p.Email == email))
            return Result<RegisterResponse>.Failure("El email ya está registrado.", ErrorType.Conflict);

        if (await _dbContext.Persons.AnyAsync(p => p.CI == request.CI.Trim()))
            return Result<RegisterResponse>.Failure("El CI ya está registrado.", ErrorType.Conflict);

        var now               = DateTime.UtcNow;
        var firstName         = request.FirstName.Trim();
        var verificationToken = Guid.NewGuid().ToString("N");

        // Enviar email ANTES de guardar en DB: si falla, el paciente no queda creado
        var verifyUrl = $"{_appOptions.FrontendUrl}/verificar-email?token={verificationToken}";
        await _emailService.SendAsync(email, "Verificá tu email — SIGA-Óptica",
            BuildVerificationEmail(firstName, verifyUrl));

        var person = new Person
        {
            CI          = request.CI.Trim(),
            FirstName   = firstName,
            LastName    = request.LastName.Trim(),
            BirthDate   = request.BirthDate,
            PhoneNumber = request.PhoneNumber?.Trim(),
            Email       = email,
            CreatedAt   = now,
            UpdatedAt   = now
        };

        var user = new User
        {
            Person                 = person,
            PasswordHash           = _passwordHasher.Hash(request.Password),
            IsActive               = true,
            IsEmailVerified        = false,
            EmailVerificationToken = verificationToken,
            CreatedAt              = now,
            UpdatedAt              = now
        };

        var patientRole = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Type == "patient");
        if (patientRole != null)
            user.UserRoles.Add(new UserRole { Role = patientRole });

        var patient = new Patient
        {
            Person    = person,
            User      = user,
            IsActive  = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.Users.Add(user);
        _dbContext.Patients.Add(patient);
        await _dbContext.SaveChangesAsync();

        return Result<RegisterResponse>.Success(new RegisterResponse
        {
            Email     = person.Email,
            FirstName = person.FirstName,
            LastName  = person.LastName
        });
    }

    public async Task<Result<bool>> VerifyEmailAsync(string token)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.EmailVerificationToken == token);

        if (user is null)
            return Result<bool>.Failure("El enlace de verificación no es válido o ya fue utilizado.", ErrorType.NotFound);

        user.IsEmailVerified        = true;
        user.EmailVerificationToken = null;
        user.UpdatedAt              = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLower();

        var user = await _dbContext.Users
            .Include(u => u.Person)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .Include(u => u.Professional)
                .ThenInclude(p => p!.Especialidades).ThenInclude(pe => pe.Especialidad)
            .Include(u => u.Sucursal)
            .FirstOrDefaultAsync(u => u.Person.Email == email);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            await _audit.LogAsync(AuditAccion.LoginFallido,
                $"Intento de inicio de sesión fallido para {email}",
                entidad: "User", entidadId: user?.Id,
                userIdOverride: user?.Id, usuarioNombreOverride: email);
            return Result<LoginResponse>.Failure("Credenciales incorrectas.", ErrorType.Unauthorized);
        }

        if (!user.IsActive)
            return Result<LoginResponse>.Failure("La cuenta está desactivada.", ErrorType.Unauthorized);

        if (!user.IsEmailVerified)
            return Result<LoginResponse>.Failure(
                "Debés verificar tu email antes de iniciar sesión. Revisá tu bandeja de entrada.",
                ErrorType.Unauthorized);

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission.Name))
            .Distinct()
            .ToList();
        var jwtToken = _jwtTokenGenerator.GenerateToken(user, roles, permissions, user.Professional?.Id, user.SucursalId);

        await _audit.LogAsync(AuditAccion.LoginExitoso, "Inició sesión",
            entidad: "User", entidadId: user.Id,
            userIdOverride: user.Id,
            usuarioNombreOverride: $"{user.Person.FirstName} {user.Person.LastName}".Trim());

        return Result<LoginResponse>.Success(new LoginResponse
        {
            JwtToken       = jwtToken,
            Email          = user.Person.Email,
            FirstName      = user.Person.FirstName,
            LastName       = user.Person.LastName,
            Specialty      = user.Professional?.Especialidades.FirstOrDefault()?.Especialidad.Nombre,
            ProfessionalId = user.Professional?.Id,
            SucursalId     = user.SucursalId,
            SucursalNombre = user.Sucursal?.Nombre,
            RoleClaims     = roles,
            Permissions    = permissions,
            MustChangePassword = user.MustChangePassword
        });
    }

    public async Task<Result<bool>> ChangePasswordAsync(ChangePasswordRequest request)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == _current.UserId);
        if (user is null)
            return Result<bool>.Failure("Usuario no encontrado.", ErrorType.NotFound);

        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            return Result<bool>.Failure("La contraseña actual es incorrecta.", ErrorType.Validation);

        var passwordError = PasswordPolicy.ValidateNew(request.NewPassword, user.PasswordHash, _passwordHasher);
        if (passwordError is not null)
            return Result<bool>.Failure(passwordError, ErrorType.Validation);

        user.PasswordHash       = _passwordHasher.Hash(request.NewPassword);
        user.MustChangePassword = false;
        user.UpdatedAt          = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        await _audit.LogAsync(AuditAccion.PasswordCambiado, "Cambió su contraseña",
            entidad: "User", entidadId: user.Id);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> RequestPasswordResetAsync(ForgotPasswordRequest request)
    {
        if (!await _hCaptchaService.VerifyAsync(request.HCaptchaToken))
            return Result<bool>.Failure("La verificación de seguridad falló. Intentá de nuevo.", ErrorType.Validation);

        var email = request.Email.Trim().ToLower();

        var user = await _dbContext.Users
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.Person.Email == email);

        // No revelamos si el email existe ni si el envío falló: siempre devolvemos éxito
        // genérico. Si el proveedor de email tira una excepción (dominio no verificado,
        // proveedor caído, etc.) la absorbemos acá para no filtrar esa diferencia al cliente
        // ni dejar un token generado sin que el usuario pueda llegar a usarlo.
        if (user is not null && user.IsActive)
        {
            var token = Guid.NewGuid().ToString("N");
            var resetUrl = $"{_appOptions.FrontendUrl}/restablecer-contrasena?token={token}";

            try
            {
                await _emailService.SendAsync(user.Person.Email, "Restablecé tu contraseña — SIGA-Óptica",
                    BuildPasswordResetEmail(user.Person.FirstName, resetUrl));

                user.PasswordResetToken          = token;
                user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);
                user.UpdatedAt                   = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }
            catch
            {
                // Ver comentario arriba: no propagamos el error del proveedor de email.
            }
        }

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> ResetPasswordWithTokenAsync(ResetPasswordWithTokenRequest request)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u =>
            u.PasswordResetToken == request.Token &&
            u.PasswordResetTokenExpiresAt != null &&
            u.PasswordResetTokenExpiresAt > DateTime.UtcNow);

        if (user is null)
            return Result<bool>.Failure("El enlace no es válido o expiró.", ErrorType.NotFound);

        var passwordError = PasswordPolicy.ValidateNew(request.NewPassword, user.PasswordHash, _passwordHasher);
        if (passwordError is not null)
            return Result<bool>.Failure(passwordError, ErrorType.Validation);

        user.PasswordHash                = _passwordHasher.Hash(request.NewPassword);
        user.PasswordResetToken          = null;
        user.PasswordResetTokenExpiresAt = null;
        user.MustChangePassword          = false;
        user.UpdatedAt                   = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        await _audit.LogAsync(AuditAccion.PasswordReseteado,
            "Restableció su contraseña desde el enlace de recuperación",
            entidad: "User", entidadId: user.Id, userIdOverride: user.Id);

        return Result<bool>.Success(true);
    }

    private static string BuildPasswordResetEmail(string firstName, string resetUrl) => $"""
        <!DOCTYPE html>
        <html lang="es">
        <head><meta charset="UTF-8"></head>
        <body style="margin:0;padding:0;background-color:#F7F9FE;font-family:Arial,sans-serif;">
          <table width="100%" cellpadding="0" cellspacing="0">
            <tr>
              <td align="center" style="padding:40px 20px;">
                <table width="560" cellpadding="0" cellspacing="0"
                  style="background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.06);">
                  <tr>
                    <td style="background:#00288E;padding:28px 40px;">
                      <p style="margin:0;color:#ffffff;font-size:22px;font-weight:900;">SIGA-Óptica</p>
                    </td>
                  </tr>
                  <tr>
                    <td style="padding:40px;">
                      <h2 style="margin:0 0 12px;color:#181C20;font-size:22px;">Restablecé tu contraseña</h2>
                      <p style="color:#444653;margin:0 0 8px;">Hola <strong>{firstName}</strong>,</p>
                      <p style="color:#444653;margin:0 0 32px;">
                        Recibimos un pedido para restablecer tu contraseña. Hacé clic en el botón para elegir una nueva
                        (el enlace vence en 1 hora):
                      </p>
                      <a href="{resetUrl}"
                        style="display:inline-block;background:#00288E;color:#ffffff;text-decoration:none;
                               padding:14px 32px;border-radius:9999px;font-weight:700;font-size:15px;">
                        Restablecer contraseña
                      </a>
                      <p style="color:#757684;font-size:12px;margin:32px 0 0;">
                        Si no pediste esto podés ignorar este mensaje: tu contraseña actual sigue siendo válida.
                      </p>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
          </table>
        </body>
        </html>
        """;

    private static string BuildVerificationEmail(string firstName, string verifyUrl) => $"""
        <!DOCTYPE html>
        <html lang="es">
        <head><meta charset="UTF-8"></head>
        <body style="margin:0;padding:0;background-color:#F7F9FE;font-family:Arial,sans-serif;">
          <table width="100%" cellpadding="0" cellspacing="0">
            <tr>
              <td align="center" style="padding:40px 20px;">
                <table width="560" cellpadding="0" cellspacing="0"
                  style="background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.06);">
                  <tr>
                    <td style="background:#00288E;padding:28px 40px;">
                      <p style="margin:0;color:#ffffff;font-size:22px;font-weight:900;">SIGA-Óptica</p>
                    </td>
                  </tr>
                  <tr>
                    <td style="padding:40px;">
                      <h2 style="margin:0 0 12px;color:#181C20;font-size:22px;">Verificá tu email</h2>
                      <p style="color:#444653;margin:0 0 8px;">Hola <strong>{firstName}</strong>,</p>
                      <p style="color:#444653;margin:0 0 32px;">
                        Gracias por registrarte en SIGA-Óptica. Para activar tu cuenta hacé clic en el botón:
                      </p>
                      <a href="{verifyUrl}"
                        style="display:inline-block;background:#00288E;color:#ffffff;text-decoration:none;
                               padding:14px 32px;border-radius:9999px;font-weight:700;font-size:15px;">
                        Verificar mi email
                      </a>
                      <p style="color:#757684;font-size:12px;margin:32px 0 0;">
                        Si no creaste esta cuenta podés ignorar este mensaje.
                      </p>
                    </td>
                  </tr>
                </table>
              </td>
            </tr>
          </table>
        </body>
        </html>
        """;
}
