using Microsoft.EntityFrameworkCore;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence;


public static class DbSeeder
{
    private static readonly string[] AllPermissions =
    [
        "ver_pacientes",     "crear_paciente",      "editar_paciente",    "desactivar_paciente",
        "ver_profesionales", "crear_profesional",   "editar_profesional",
        "ver_especialidades", "gestionar_especialidades",
        "ver_agenda",         "gestionar_agenda",
        "ver_usuarios",      "editar_usuario",
        "ver_roles",         "crear_rol",           "editar_rol",         "eliminar_rol",
        "ver_calendario",
        "ver_historia_clinica",
        "ver_consultas",
        "registrar_consulta",
        "editar_consulta",
        "eliminar_consulta",
        "ver_recetas",
        "ver_inventario",    "gestionar_inventario", "gestionar_pedidos",
        "ver_ventas",        "registrar_venta",
        "ver_reportes",
        "ver_dashboard",
        "ver_notificaciones",
        "gestionar_configuracion",
        "ver_mis_turnos",
        "ver_egresos",       "gestionar_egresos",
    ];

    private static readonly string[] AdminPermissions =
        AllPermissions.Where(p => p != "ver_mis_turnos").ToArray();

    private static readonly (string Type, string Name, string[] Permissions)[] Roles =
    [
        ("admin",        "Administrador", AdminPermissions),
        ("professional", "Profesional",   []),
        ("patient",      "Paciente",      ["ver_dashboard", "ver_mis_turnos"]),
    ];

    private static readonly (string Entidad, string Nombre, string Color, string CodigoInterno, bool EsProtegido, int Orden)[] EstadosIniciales =
    [
        ("Turno",    "Pendiente",  "#F59E0B", "Pendiente",  true,  1),
        ("Turno",    "Confirmado", "#3B82F6", "Confirmado", true,  2),
        ("Turno",    "Presente",   "#6366F1", "Presente",   true,  3),
        ("Turno",    "Completado", "#10B981", "Completado", true,  4),
        ("Turno",    "Cancelado",  "#EF4444", "Cancelado",  true,  5),
        ("Pedido",   "Pendiente",  "#F59E0B", "Pendiente",  true,  1),
        ("Pedido",   "Enviado",    "#3B82F6", "Enviado",    true,  2),
        ("Pedido",   "Recibido",   "#10B981", "Recibido",   true,  3),
        ("Pedido",   "Cancelado",  "#EF4444", "Cancelado",  true,  4),
        ("Consulta", "Pendiente",  "#F59E0B", "Pendiente",  false, 1),
        ("Consulta", "Abierta",    "#3B82F6", "Abierta",    false, 2),
        ("Consulta", "Cerrada",    "#10B981", "Cerrada",    false, 3),
        ("Consulta", "Cancelada",  "#EF4444", "Cancelada",  false, 4),
    ];

    private static readonly string[] EspecialidadesIniciales =
    [
        "Optometría",
        "Oftalmología",
        "Contactología",
        "Baja Visión",
        "Ortoqueratología",
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

            var expectedPermIds = rolePerms
                .Where(p => permissionMap.ContainsKey(p))
                .Select(p => permissionMap[p])
                .ToHashSet();

            var assignedPermIds = role.RolePermissions.Select(rp => rp.PermissionId).ToHashSet();

            var toAdd = expectedPermIds
                .Except(assignedPermIds)
                .Select(id => new RolePermission { RoleId = role.Id, PermissionId = id })
                .ToList();

            var toRemove = role.RolePermissions
                .Where(rp => !expectedPermIds.Contains(rp.PermissionId))
                .ToList();

            if (toAdd.Count > 0) db.RolePermissions.AddRange(toAdd);
            if (toRemove.Count > 0) db.RolePermissions.RemoveRange(toRemove);
            if (toAdd.Count > 0 || toRemove.Count > 0) await db.SaveChangesAsync();
        }

        // 4. Estados Config iniciales
        var existingEstados = await db.EstadosConfig
            .Select(e => new { e.Entidad, e.CodigoInterno })
            .ToListAsync();
        var existingEstadosSet = existingEstados
            .Select(e => $"{e.Entidad}:{e.CodigoInterno}")
            .ToHashSet();

        var nuevosEstados = EstadosIniciales
            .Where(s => !existingEstadosSet.Contains($"{s.Entidad}:{s.CodigoInterno}"))
            .Select(s => new EstadoConfig
            {
                Entidad       = s.Entidad,
                Nombre        = s.Nombre,
                Color         = s.Color,
                CodigoInterno = s.CodigoInterno,
                EsProtegido   = s.EsProtegido,
                Orden         = s.Orden,
            })
            .ToList();

        if (nuevosEstados.Count > 0)
        {
            db.EstadosConfig.AddRange(nuevosEstados);
            await db.SaveChangesAsync();
        }

        // 5. Especialidades iniciales (was 4)
        var existingEspecialidades = await db.Especialidades.Select(e => e.Nombre).ToHashSetAsync();
        var nuevasEspecialidades = EspecialidadesIniciales
            .Where(n => !existingEspecialidades.Contains(n))
            .Select(n => new Especialidad { Nombre = n })
            .ToList();

        if (nuevasEspecialidades.Count > 0)
        {
            db.Especialidades.AddRange(nuevasEspecialidades);
            await db.SaveChangesAsync();
        }
    }
}
