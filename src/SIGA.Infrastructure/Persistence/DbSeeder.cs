using Microsoft.EntityFrameworkCore;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence;

public static class DbSeeder
{
    private static readonly string[] AllPermissions =
    [
        "ver_pacientes",     "crear_paciente",      "editar_paciente",    "desactivar_paciente",
        "ver_profesionales", "crear_profesional",   "editar_profesional",
        "ver_usuarios",      "editar_usuario",
        "ver_roles",         "crear_rol",           "editar_rol",         "eliminar_rol",
        "ver_calendario",
        "ver_historia_clinica",
        "ver_inventario",
        "ver_ventas",
        "ver_reportes",
    ];

    private static readonly (string Type, string Name, string[] Permissions)[] Roles =
    [
        ("admin",        "Administrador", AllPermissions),
        ("professional", "Profesional",   []),
        ("patient",      "Paciente",      []),
    ];

    public static async Task SeedAsync(AppDbContext db)
    {
        // 1. Permisos — insertar los que no existen
        var existingPermissions = await db.Permissions.ToListAsync();
        var existingNames       = existingPermissions.Select(p => p.Name).ToHashSet();

        var newPermissions = AllPermissions
            .Where(name => !existingNames.Contains(name))
            .Select(name => new Permission { Name = name })
            .ToList();

        if (newPermissions.Count > 0)
        {
            db.Permissions.AddRange(newPermissions);
            await db.SaveChangesAsync();
        }

        // 2. Recargar todos los permisos para tener sus IDs
        var allPermissions = await db.Permissions.ToListAsync();
        var permissionMap  = allPermissions.ToDictionary(p => p.Name, p => p.Id);

        // 3. Roles — idempotencia por Type; actualizar Name si cambió
        foreach (var (roleType, roleName, rolePerms) in Roles)
        {
            var role = await db.Roles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Type == roleType);

            if (role is null)
            {
                role = new Role { Name = roleName, Type = roleType };
                db.Roles.Add(role);
                await db.SaveChangesAsync();
            }
            else if (role.Name != roleName)
            {
                role.Name = roleName;
                await db.SaveChangesAsync();
            }

            var assignedPermIds = role.RolePermissions.Select(rp => rp.PermissionId).ToHashSet();

            var missing = rolePerms
                .Where(p => permissionMap.ContainsKey(p) && !assignedPermIds.Contains(permissionMap[p]))
                .Select(p => new RolePermission { RoleId = role.Id, PermissionId = permissionMap[p] })
                .ToList();

            if (missing.Count > 0)
            {
                db.RolePermissions.AddRange(missing);
                await db.SaveChangesAsync();
            }
        }
    }
}
