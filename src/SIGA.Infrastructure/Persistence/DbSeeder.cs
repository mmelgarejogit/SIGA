using Microsoft.EntityFrameworkCore;
using SIGA.Domain.Entities;
using SIGA.Domain.Security;

namespace SIGA.Infrastructure.Persistence;


public static class DbSeeder
{
    private static readonly string[] AllPermissions =
    [
        "ver_pacientes",     "crear_paciente",      "editar_paciente",    "desactivar_paciente",
        "ver_clientes",      "crear_cliente",       "editar_cliente",     "desactivar_cliente",
        "ver_profesionales", "crear_profesional",   "editar_profesional",
        "ver_especialidades", "gestionar_especialidades",
        "ver_agenda",         "gestionar_agenda",   "ver_recepcion",
        "ver_usuarios",      "editar_usuario",
        "ver_roles",         "crear_rol",           "editar_rol",         "eliminar_rol",
        "ver_calendario",
        "ver_historia_clinica",
        "ver_consultas",
        "registrar_consulta",
        "editar_consulta",
        "eliminar_consulta",
        "ver_recetas",
        "ver_inventario",    "gestionar_inventario", "gestionar_pedidos",  "aprobar_pedidos",
        "ver_ventas",        "registrar_venta",   "gestionar_ventas",
        "ver_laboratorio",   "gestionar_laboratorio",
        "gestionar_caja",    "aprobar_cierres_caja",
        "ver_reportes",
        "ver_dashboard",
        "ver_notificaciones",
        "gestionar_configuracion",
        "ver_mis_turnos",
        "ver_egresos",       "gestionar_egresos",  "aprobar_egresos",  "pagar_egresos",
        "ver_empleados",     "gestionar_empleados",
    ];

    private static readonly string[] AdminPermissions =
        AllPermissions.Where(p => p != "ver_mis_turnos").ToArray();

    // ManagePermissions = true  → el seeder sincroniza permisos (agrega y elimina)
    // ManagePermissions = false → el seeder solo agrega defaults en la creación inicial; nunca elimina permisos asignados manualmente
    private static readonly (string Type, string Name, string[] Permissions, bool ManagePermissions)[] Roles =
    [
        ("admin",        "Administrador", AdminPermissions,                    true),
        ("professional", "Profesional",   [],                                  false),
        ("patient",      "Paciente",      ["ver_dashboard", "ver_mis_turnos"], false),
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
            .Distinct()
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
        foreach (var (roleType, roleName, rolePerms, managePermissions) in Roles)
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

            // Solo eliminar permisos en roles completamente gestionados por el seeder (admin).
            // Para professional y patient, el admin puede asignar permisos manualmente y el seeder no los toca.
            var toRemove = managePermissions
                ? role.RolePermissions
                    .Where(rp => !expectedPermIds.Contains(rp.PermissionId))
                    .ToList()
                : [];

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

        // 6. Categorías de producto
        var existingCats = await db.CategoriasProducto.Select(c => c.Nombre).ToHashSetAsync();
        var nuevasCats = CategoriasProductoIniciales
            .Where(c => !existingCats.Contains(c.Nombre))
            .Select(c => new CategoriaProducto
            {
                Nombre      = c.Nombre,
                Descripcion = c.Descripcion,
                Tipo        = c.Tipo,
                IsActive    = true,
                CreatedAt   = DateTime.UtcNow,
            })
            .ToList();

        if (nuevasCats.Count > 0)
        {
            db.CategoriasProducto.AddRange(nuevasCats);
            await db.SaveChangesAsync();
        }

        // 7. Marcas
        var existingMarcas = await db.Marcas.Select(m => m.Nombre).ToHashSetAsync();
        var nuevasMarcas = MarcasIniciales
            .Where(n => !existingMarcas.Contains(n))
            .Select(n => new Marca { Nombre = n, IsActive = true, CreatedAt = DateTime.UtcNow })
            .ToList();

        if (nuevasMarcas.Count > 0)
        {
            db.Marcas.AddRange(nuevasMarcas);
            await db.SaveChangesAsync();
        }

        // 8. Modelos (requiere que las marcas estén persistidas)
        var marcaMap = await db.Marcas.ToDictionaryAsync(m => m.Nombre, m => m.Id);

        var existingModelos = await db.Modelos
            .Select(m => new { m.MarcaId, m.Nombre })
            .ToListAsync();
        var existingModelosSet = existingModelos
            .Select(m => $"{m.MarcaId}:{m.Nombre}")
            .ToHashSet();

        var nuevosModelos = ModelosIniciales
            .Where(x => marcaMap.ContainsKey(x.Marca))
            .Select(x => new { MarcaId = marcaMap[x.Marca], x.Nombre })
            .Where(x => !existingModelosSet.Contains($"{x.MarcaId}:{x.Nombre}"))
            .Select(x => new Modelo { Nombre = x.Nombre, MarcaId = x.MarcaId, IsActive = true, CreatedAt = DateTime.UtcNow })
            .ToList();

        if (nuevosModelos.Count > 0)
        {
            db.Modelos.AddRange(nuevosModelos);
            await db.SaveChangesAsync();
        }

        // 9. Tipos de lente (diseños del cristal a pedido, con precio sugerido)
        var existingTiposLente = await db.TiposLente.Select(t => t.Nombre).ToHashSetAsync();
        var nuevosTiposLente = TiposLenteIniciales
            .Where(t => !existingTiposLente.Contains(t.Nombre))
            .Select(t => new TipoLente { Nombre = t.Nombre, PrecioBase = t.PrecioBase, IsActive = true, CreatedAt = DateTime.UtcNow })
            .ToList();

        if (nuevosTiposLente.Count > 0)
        {
            db.TiposLente.AddRange(nuevosTiposLente);
            await db.SaveChangesAsync();
        }

        // 10. Tratamientos
        var existingTratamientos = await db.Tratamientos.Select(t => t.Nombre).ToHashSetAsync();
        var nuevosTratamientos = TratamientosIniciales
            .Where(n => !existingTratamientos.Contains(n))
            .Select(n => new Tratamiento { Nombre = n, IsActive = true, CreatedAt = DateTime.UtcNow })
            .ToList();

        if (nuevosTratamientos.Count > 0)
        {
            db.Tratamientos.AddRange(nuevosTratamientos);
            await db.SaveChangesAsync();
        }

        // 11. Servicios (catálogo para cobro de consultas y servicios sin stock).
        //     "Consulta" lleva tarifas por especialidad como ejemplo de precio variable;
        //     las tarifas por profesional puntual se cargan desde la UI (Ventas → Servicios).
        if (!await db.Servicios.AnyAsync())
        {
            var espPorNombre = await db.Especialidades.ToDictionaryAsync(e => e.Nombre, e => e.Id);
            int? Esp(string n) => espPorNombre.TryGetValue(n, out var id) ? id : (int?)null;
            var ahora = DateTime.UtcNow;

            ServicioTarifa TarifaEsp(string especialidad, decimal precio) => new()
            {
                EspecialidadId = Esp(especialidad),
                Precio         = precio,
                CreatedAt      = ahora,
                UpdatedAt      = ahora,
            };

            List<ServicioTarifa> SoloValidas(params ServicioTarifa[] tarifas) =>
                tarifas.Where(t => t.EspecialidadId != null).ToList();

            var servicios = new List<Servicio>
            {
                new()
                {
                    Nombre = "Consulta oftalmológica",
                    Descripcion = "Consulta clínica con profesional",
                    Precio = 150000m, IsActive = true, CreatedAt = ahora, UpdatedAt = ahora,
                    Tarifas = SoloValidas(
                        TarifaEsp("Oftalmología", 180000m),
                        TarifaEsp("Optometría",   120000m)),
                },
                new()
                {
                    Nombre = "Consulta de control / seguimiento",
                    Descripcion = "Control posterior a una consulta",
                    Precio = 80000m, IsActive = true, CreatedAt = ahora, UpdatedAt = ahora,
                },
                new()
                {
                    Nombre = "Adaptación de lentes de contacto",
                    Descripcion = "Prueba y adaptación de lentes de contacto",
                    Precio = 200000m, IsActive = true, CreatedAt = ahora, UpdatedAt = ahora,
                    Tarifas = SoloValidas(TarifaEsp("Contactología", 170000m)),
                },
                new() { Nombre = "Ajuste de armazón",      Descripcion = "Ajuste y calibrado de armazón",                       Precio = 30000m, IsActive = true, CreatedAt = ahora, UpdatedAt = ahora },
                new() { Nombre = "Reparación de armazón",   Descripcion = "Reparación de armazón (soldadura, cambio de tornillos)", Precio = 50000m, IsActive = true, CreatedAt = ahora, UpdatedAt = ahora },
                new() { Nombre = "Limpieza ultrasónica",    Descripcion = "Limpieza ultrasónica de anteojos",                     Precio = 20000m, IsActive = true, CreatedAt = ahora, UpdatedAt = ahora },
            };

            db.Servicios.AddRange(servicios);
            await db.SaveChangesAsync();
        }
    }

    // ── Admin bootstrap (corre en TODOS los entornos, incluido Producción) ──────
    // Sin esto, en Producción no existiría ningún usuario para el primer login
    // (el admin de demo vive en DevDataSeeder, que solo corre en Development).
    // Idempotente por el CI-sentinel "99999999": DevDataSeeder reutiliza el mismo
    // sentinel, por lo que en dev no se duplica el admin.
    public static async Task SeedAdminAsync(AppDbContext db, IPasswordHasher hasher, string email, string password)
    {
        if (await db.Persons.AnyAsync(p => p.CI == "99999999")) return;

        var adminRole = await db.Roles.FirstAsync(r => r.Type == "admin");
        var now = DateTime.UtcNow;

        var person = new Person
        {
            CI        = "99999999",
            FirstName = "Admin",
            LastName  = "SIGA",
            BirthDate = new DateOnly(1990, 1, 1),
            Email     = email,
            CreatedAt = now,
            UpdatedAt = now,
        };
        var user = new User
        {
            Person          = person,
            PasswordHash    = hasher.Hash(password),
            IsActive        = true,
            IsEmailVerified = true,
            CreatedAt       = now,
            UpdatedAt       = now,
            UserRoles       = [new UserRole { RoleId = adminRole.Id }],
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }

    // Diseños del cristal a pedido, con precio base sugerido (editable por venta).
    // Los lentes de contacto NO van acá: son productos de stock (categoría "Lentes de Contacto").
    private static readonly (string Nombre, decimal PrecioBase)[] TiposLenteIniciales =
    [
        ("Monofocal",             250000m),
        ("Bifocal",               450000m),
        ("Progresivo",            850000m),
        ("Ocupacional / Oficina", 600000m),
    ];

    private static readonly string[] TratamientosIniciales =
    [
        "Antirreflejo",
        "Fotocromático (Transitions)",
        "Polarizado",
        "Filtro Luz Azul",
        "UV 400",
        "Endurecimiento Superficial",
        "Hidrofóbico / Antihumedad",
        "Antirayaduras",
    ];

    // Marcas de armazones y lentes de contacto (productos con stock). Las marcas de
    // cristales (Essilor, Hoya, Zeiss, Transitions) se quitaron: el lente ya no es producto.
    private static readonly string[] MarcasIniciales =
    [
        "Ray-Ban", "Oakley", "Prada", "Gucci", "Versace", "Tom Ford", "Carrera",
        "Police", "Hugo Boss", "Silhouette", "Vogue", "Persol", "Michael Kors",
        "Emporio Armani", "Giorgio Armani", "Dolce & Gabbana", "Chanel", "Dior",
        "Bvlgari", "Burberry", "Lacoste", "Nautica", "Fossil", "Kate Spade",
        "CooperVision", "Bausch & Lomb", "Alcon", "Johnson & Johnson",
    ];

    private static readonly (string Marca, string Nombre)[] ModelosIniciales =
    [
        // Ray-Ban
        ("Ray-Ban", "Aviator Classic"),
        ("Ray-Ban", "Aviator Reverse"),
        ("Ray-Ban", "Clubmaster"),
        ("Ray-Ban", "Wayfarer"),
        ("Ray-Ban", "Wayfarer Ease"),
        ("Ray-Ban", "Round Metal"),
        ("Ray-Ban", "New Wayfarer"),
        ("Ray-Ban", "Erika"),
        ("Ray-Ban", "Justin"),
        ("Ray-Ban", "Predator"),

        // Oakley
        ("Oakley", "Holbrook"),
        ("Oakley", "Frogskins"),
        ("Oakley", "Sylas"),
        ("Oakley", "Crosshair"),
        ("Oakley", "Flak 2.0"),
        ("Oakley", "Jawbreaker"),
        ("Oakley", "Radar EV Path"),
        ("Oakley", "Sutro"),

        // Prada
        ("Prada", "PR 17WS"),
        ("Prada", "PR 53SS"),
        ("Prada", "PR 56MS"),
        ("Prada", "PR 60XS"),
        ("Prada", "PR 01XS"),

        // Gucci
        ("Gucci", "GG0061S"),
        ("Gucci", "GG0396S"),
        ("Gucci", "GG1244S"),
        ("Gucci", "GG0593SK"),

        // Tom Ford
        ("Tom Ford", "FT0237 Henry"),
        ("Tom Ford", "FT0711 Snowdon"),
        ("Tom Ford", "FT0775 Fausto"),
        ("Tom Ford", "FT0858 Jameson"),

        // Carrera
        ("Carrera", "Carrera 1011/S"),
        ("Carrera", "Carrera 1049/S"),
        ("Carrera", "Carrera 8837/S"),
        ("Carrera", "Hyperfit"),
        ("Carrera", "Ducati Cardboard"),

        // Silhouette
        ("Silhouette", "Momentum"),
        ("Silhouette", "Urban Fusion"),
        ("Silhouette", "Titan Minimal Art"),
        ("Silhouette", "Spx Art Comfort"),

        // CooperVision
        ("CooperVision", "Biofinity"),
        ("CooperVision", "Biofinity Toric"),
        ("CooperVision", "MyDay"),
        ("CooperVision", "Proclear"),
        ("CooperVision", "Avaira Vitality"),

        // Bausch & Lomb
        ("Bausch & Lomb", "Ultra"),
        ("Bausch & Lomb", "Ultra Toric"),
        ("Bausch & Lomb", "PureVision 2"),
        ("Bausch & Lomb", "SofLens 38"),
        ("Bausch & Lomb", "INFUSE"),

        // Alcon
        ("Alcon", "Air Optix Plus HydraGlyde"),
        ("Alcon", "Air Optix Night & Day"),
        ("Alcon", "Dailies Total1"),
        ("Alcon", "Dailies AquaComfort Plus"),
        ("Alcon", "Total30"),

        // Johnson & Johnson
        ("Johnson & Johnson", "Acuvue Oasys"),
        ("Johnson & Johnson", "Acuvue Vita"),
        ("Johnson & Johnson", "1-Day Acuvue Moist"),
        ("Johnson & Johnson", "Acuvue Oasys 1-Day"),
        ("Johnson & Johnson", "Acuvue Oasys Max 1-Day"),
    ];

    // Solo categorías de productos con stock real. Los lentes graduados (cristales) NO son
    // productos: se modelan como diseño (TipoLente) + tratamientos en el trabajo a pedido.
    // El tipo Armazón habilita el producto en el flujo de venta a pedido.
    private static readonly (string Nombre, string Descripcion, TipoCategoriaProducto Tipo)[] CategoriasProductoIniciales =
    [
        ("Marcos",                "Marcos ópticos para receta y sol",                       TipoCategoriaProducto.Armazon),
        ("Monturas Infantiles",   "Marcos y monturas diseñados para niños y adolescentes",   TipoCategoriaProducto.Armazon),
        ("Lentes de Sol",         "Lentes de sol con y sin prescripción",                   TipoCategoriaProducto.Generico),
        ("Lentes de Contacto",    "Lentes de contacto blandos, rígidos y de colores",       TipoCategoriaProducto.Generico),
        ("Soluciones",            "Soluciones multiusos, salinas y gotas oculares",         TipoCategoriaProducto.Generico),
        ("Accesorios",            "Estuches, paños, correas y herramientas ópticas",        TipoCategoriaProducto.Generico),
        ("Instrumentos",          "Equipos y herramientas de diagnóstico y adaptación",     TipoCategoriaProducto.Generico),
        ("Insumos",               "Materiales de consumo y laboratorio óptico",             TipoCategoriaProducto.Generico),
    ];
}
