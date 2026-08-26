using Microsoft.EntityFrameworkCore;
using SIGA.Domain.Entities;
using SIGA.Domain.Security;

namespace SIGA.Infrastructure.Persistence;

/// <summary>
/// Datos operativos de demo: proveedores y laboratorios, empleados, clientes, recetas,
/// agenda vigente, compras, ventas, trabajos de laboratorio, egresos y caja.
///
/// Complementa a <see cref="DevDataSeeder"/> (que carga el maestro: personas, profesionales,
/// productos). Solo corre en Development. Cada bloque es idempotente: si ya hay datos de ese
/// tipo no vuelve a insertar, así que se puede reiniciar el backend sin duplicar nada.
/// </summary>
public static class DemoDataSeeder
{
    private static readonly Random Rng = new(2026);

    /// <summary>Paraguay es UTC−3 fijo; las fechas de negocio se calculan en hora local.</summary>
    private static DateOnly Hoy => DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-3));

    public static async Task SeedAsync(AppDbContext db, IPasswordHasher hasher)
    {
        var sucursal = await db.Sucursales.OrderBy(s => s.Id).FirstOrDefaultAsync();
        if (sucursal is null) return;

        var admin = await db.Users
            .Include(u => u.Person)
            .FirstOrDefaultAsync(u => u.Person.CI == "99999999");
        if (admin is null) return;

        await NormalizarPreciosProductosAsync(db);
        await SeedProveedoresAsync(db);
        await SeedCategoriasGastoAsync(db);
        var empleados = await SeedCargosYEmpleadosAsync(db, hasher, sucursal);
        await SeedTimbradosAsync(db, sucursal);
        var clientes = await SeedClientesAsync(db);
        await SeedTurnosVigentesAsync(db, sucursal);
        await SeedConsultasYRecetasAsync(db, sucursal);

        // La caja se abre antes que ventas y egresos: los movimientos se cuelgan de la
        // sesión del día, igual que en el flujo real. El cierre se calcula al final.
        var (sesiones, nuevasSesiones, dejarUnaAbierta) =
            await SeedSesionesCajaAsync(db, sucursal, admin, empleados);

        await SeedComprasAsync(db, sucursal, admin);
        await SeedVentasAsync(db, sucursal, admin, empleados, clientes, sesiones);
        await SeedEgresosVariosAsync(db, sucursal, admin, empleados, sesiones);
        await CerrarSesionesCajaAsync(db, admin, nuevasSesiones, dejarUnaAbierta);
    }

    // ── Precios en guaraníes ──────────────────────────────────────────────────
    // El catálogo de DevDataSeeder se cargó con una escala de precios que no es la de
    // Guaraníes (un armazón a 15.900). Se reescriben SOLO esos productos y SOLO mientras
    // sigan por debajo de 100.000 — un producto ya corregido a mano no se toca.

    private static readonly (string Nombre, decimal Costo, decimal Venta)[] PreciosGuaranies =
    [
        ("Marcos Ray-Ban RB5154",         620_000m, 1_290_000m),
        ("Marcos Oakley OX8046",          780_000m, 1_590_000m),
        ("Marcos Vogue VO5230",           390_000m,   850_000m),
        ("Marcos Prada PR 07WV",        1_150_000m, 2_390_000m),
        ("Marcos Armani EA3187",          890_000m, 1_790_000m),
        ("Montura Infantil Disney",       210_000m,   450_000m),
        ("Montura Infantil Flexible",     260_000m,   560_000m),
        ("Lentes de Sol Ray-Ban Aviator", 690_000m, 1_390_000m),
        ("Lentes de Sol Oakley Holbrook", 820_000m, 1_650_000m),
        ("Lentes de Sol Vulk Polarizado", 320_000m,   690_000m),
        ("Acuvue Oasys 6u",               195_000m,   390_000m),
        ("Dailies Total1 30u",            290_000m,   580_000m),
        ("Biofinity 6u",                  175_000m,   350_000m),
        ("Air Optix Plus 6u",             170_000m,   340_000m),
        ("FreshLook Colorblends 2u",      110_000m,   230_000m),
        ("Solución ReNu 360ml",            52_000m,   105_000m),
        ("Solución salina B&L 360ml",      38_000m,    78_000m),
        ("Clear Care 360ml",               72_000m,   145_000m),
        ("Gotas Optase 10ml",              59_000m,   120_000m),
        ("Systane Ultra 10ml",             66_000m,   135_000m),
        ("Estuche rígido estándar",        21_000m,    45_000m),
        ("Estuche blando premium",         15_000m,    33_000m),
        ("Paño de microfibra",              7_000m,    15_000m),
        ("Spray limpiador antirreflejo",   27_000m,    58_000m),
        ("Kit destornilladores ópticos",   48_000m,    98_000m),
        ("Correa deportiva para lentes",   18_000m,    39_000m),
        ("Lupa de lectura +2.5",           72_000m,   145_000m),
        ("Clip solar polarizado UV400",    39_000m,    79_000m),
        ("Kit reparación patillas",        11_000m,    24_000m),
        ("Colgante porta-lentes",          13_000m,    29_000m),
    ];

    private static async Task NormalizarPreciosProductosAsync(AppDbContext db)
    {
        var nombres  = PreciosGuaranies.Select(p => p.Nombre).ToList();
        var afectados = await db.Productos
            .Where(p => nombres.Contains(p.Nombre) && p.PrecioVenta < 100_000m)
            .ToListAsync();
        if (afectados.Count == 0) return;

        var precios = PreciosGuaranies.ToDictionary(p => p.Nombre);
        var now     = DateTime.UtcNow;

        foreach (var producto in afectados)
        {
            var (_, costo, venta) = precios[producto.Nombre];
            producto.PrecioCosto = costo;
            producto.PrecioVenta = venta;
            producto.UpdatedAt   = now;
        }

        await db.SaveChangesAsync();
    }

    // ── Proveedores y laboratorios ────────────────────────────────────────────

    private static readonly (string Nombre, string RazonSocial, string Ruc, bool EsLab, string Direccion, string Contacto, string Cargo)[] ProveedoresSeed =
    [
        ("Laboratorio Óptico Asunción", "Laboratorio Óptico Asunción S.A.",  "80012345-6", true,  "Av. Mcal. López 2340, Asunción",       "Rubén Ayala",     "Jefe de producción"),
        ("OptiLab Paraguay",            "OptiLab Paraguay S.R.L.",           "80023456-1", true,  "Tte. Fariña 1120, Asunción",           "Marta Cabrera",   "Atención al cliente"),
        ("Cristal Sur Laboratorio",     "Cristal Sur S.A.",                  "80034567-9", true,  "Ruta 1 km 12, San Lorenzo",            "Diego Fretes",    "Coordinador de pedidos"),
        ("Luxottica Paraguay",          "Luxottica Paraguay S.A.",           "80045678-3", false, "Av. España 1450, Asunción",            "Silvia Duarte",   "Ejecutiva de cuentas"),
        ("Distribuidora Óptica del Este","Distribuidora Óptica del Este S.R.L.","80056789-7", false, "Av. Monday 890, Ciudad del Este",   "Óscar Villalba",  "Vendedor mayorista"),
        ("Alcon Paraguay",              "Alcon Paraguay S.A.",               "80067890-2", false, "Av. Aviadores del Chaco 2050, Asunción","Carmen Ortega",  "Representante comercial"),
        ("Importadora Visión Total",    "Visión Total Import S.R.L.",        "80078901-8", false, "Palma 456, Asunción",                  "Julio Benítez",   "Gerente comercial"),
        ("Insumos Ópticos S.R.L.",      "Insumos Ópticos S.R.L.",            "80089012-4", false, "Eusebio Ayala 3300, Asunción",         "Nilda Ramírez",   "Administración"),
    ];

    private static async Task SeedProveedoresAsync(AppDbContext db)
    {
        if (await db.Proveedores.AnyAsync()) return;

        var ciudadId = await db.Ciudades
            .Where(c => c.Nombre == "Asunción")
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync();

        var now = DateTime.UtcNow;

        foreach (var (nombre, razon, ruc, esLab, direccion, contacto, cargo) in ProveedoresSeed)
        {
            db.Proveedores.Add(new Proveedor
            {
                Nombre        = nombre,
                RazonSocial   = razon,
                Ruc           = ruc,
                Direccion     = direccion,
                CiudadId      = ciudadId,
                EsLaboratorio = esLab,
                WhatsApp      = GenTelefono(),
                SitioWeb      = $"https://www.{NormStr(nombre.Split(' ')[0])}.com.py",
                IsActive      = true,
                CreatedAt     = now.AddDays(-Rng.Next(200, 700)),
                UpdatedAt     = now,
                Contactos     =
                [
                    new ProveedorContacto
                    {
                        Nombre   = contacto,
                        Cargo    = cargo,
                        Telefono = GenTelefono(),
                        Email    = $"{NormStr(contacto.Split(' ')[0])}@{NormStr(nombre.Split(' ')[0])}.com.py",
                    },
                ],
            });
        }

        await db.SaveChangesAsync();
    }

    // ── Categorías de gasto ───────────────────────────────────────────────────

    private static readonly (string Nombre, string Descripcion)[] CategoriasGastoSeed =
    [
        ("Alquiler",                  "Alquiler del local comercial"),
        ("Servicios básicos",         "ANDE, ESSAP y recolección de residuos"),
        ("Internet y telefonía",      "Conexión de internet y líneas telefónicas"),
        ("Marketing y publicidad",    "Campañas, redes sociales y material gráfico"),
        ("Limpieza e insumos",        "Productos de limpieza y consumibles de oficina"),
        ("Mantenimiento",             "Reparaciones del local y de los equipos"),
        ("Impuestos y tasas",         "Patente municipal, IVA y tasas varias"),
        ("Seguros",                   "Pólizas del local y de los equipos"),
        ("Fletes y envíos",           "Envíos al laboratorio y traslados de mercadería"),
        ("Honorarios profesionales",  "Contador, asesoría legal y servicios tercerizados"),
    ];

    private static async Task SeedCategoriasGastoAsync(AppDbContext db)
    {
        var existentes = await db.CategoriasGasto.Select(c => c.Nombre).ToHashSetAsync();
        var nuevas = CategoriasGastoSeed
            .Where(c => !existentes.Contains(c.Nombre))
            .Select(c => new CategoriaGasto
            {
                Nombre      = c.Nombre,
                Descripcion = c.Descripcion,
                Activo      = true,
                CreatedAt   = DateTime.UtcNow,
            })
            .ToList();

        if (nuevas.Count == 0) return;

        db.CategoriasGasto.AddRange(nuevas);
        await db.SaveChangesAsync();
    }

    // ── Cargos y empleados ────────────────────────────────────────────────────

    private static readonly (string Nombre, string Descripcion)[] CargosSeed =
    [
        ("Encargado de sucursal", "Responsable de la operación diaria del local"),
        ("Vendedor",              "Atención al cliente y venta de mostrador"),
        ("Cajero",                "Manejo de caja y cobros"),
        ("Óptico de taller",      "Montaje, biselado y control de calidad de lentes"),
        ("Recepcionista",         "Recepción y gestión de la agenda"),
        ("Administrativo",        "Compras, egresos y tareas administrativas"),
    ];

    private static readonly (string Nombre, string Apellido, string Cargo, decimal Salario)[] EmpleadosSeed =
    [
        ("Liliana", "Espínola", "Encargado de sucursal", 5_200_000m),
        ("Óscar",   "Barrios",  "Vendedor",              3_400_000m),
        ("Rocío",   "Melgarejo","Vendedor",              3_400_000m),
        ("Nelson",  "Duarte",   "Cajero",                3_100_000m),
        ("Fabiola", "Ocampos",  "Óptico de taller",      4_100_000m),
        ("Marcelo", "Riveros",  "Administrativo",        3_800_000m),
    ];

    private static async Task<List<Empleado>> SeedCargosYEmpleadosAsync(
        AppDbContext db, IPasswordHasher hasher, Sucursal sucursal)
    {
        var now = DateTime.UtcNow;

        var cargosExistentes = await db.CargosEmpleado.Select(c => c.Nombre).ToHashSetAsync();
        var cargosNuevos = CargosSeed
            .Where(c => !cargosExistentes.Contains(c.Nombre))
            .Select(c => new CargoEmpleado
            {
                Nombre      = c.Nombre,
                Descripcion = c.Descripcion,
                Activo      = true,
                CreatedAt   = now,
            })
            .ToList();

        if (cargosNuevos.Count > 0)
        {
            db.CargosEmpleado.AddRange(cargosNuevos);
            await db.SaveChangesAsync();
        }

        if (await db.Empleados.AnyAsync())
            return await db.Empleados.Include(e => e.User).ThenInclude(u => u.Person).ToListAsync();

        var cargoPorNombre = await db.CargosEmpleado.ToDictionaryAsync(c => c.Nombre, c => c.Id);
        var pwHash         = hasher.Hash("12345678");
        var empleados      = new List<Empleado>();
        var ci             = 8_000_001;

        foreach (var (nombre, apellido, cargo, salario) in EmpleadosSeed)
        {
            if (!cargoPorNombre.TryGetValue(cargo, out var cargoId)) continue;

            // Sin rol asignado: son legajos de personal, no cuentas operativas. El admin
            // puede darles rol desde la UI cuando quiera que operen el sistema.
            var user = new User
            {
                Person = new Person
                {
                    CI          = (ci++).ToString(),
                    FirstName   = nombre,
                    LastName    = apellido,
                    BirthDate   = new DateOnly(1980 + Rng.Next(18), Rng.Next(1, 13), Rng.Next(1, 28)),
                    PhoneNumber = GenTelefono(),
                    Email       = $"{NormStr(nombre)}.{NormStr(apellido)}@siga-optica.com",
                    CreatedAt   = now,
                    UpdatedAt   = now,
                },
                SucursalId      = sucursal.Id,
                PasswordHash    = pwHash,
                IsActive        = true,
                IsEmailVerified = true,
                CreatedAt       = now,
                UpdatedAt       = now,
            };

            var empleado = new Empleado
            {
                User         = user,
                CargoId      = cargoId,
                FechaIngreso = Hoy.AddDays(-Rng.Next(200, 1500)),
                SalarioBase  = salario,
                IsActive     = true,
                CreatedAt    = now,
                UpdatedAt    = now,
            };

            db.Empleados.Add(empleado);
            empleados.Add(empleado);
        }

        await db.SaveChangesAsync();
        return empleados;
    }

    // ── Timbrados ─────────────────────────────────────────────────────────────

    private static async Task SeedTimbradosAsync(AppDbContext db, Sucursal sucursal)
    {
        if (await db.Timbrados.AnyAsync()) return;

        var now    = DateTime.UtcNow;
        var inicio = Hoy.AddDays(-180);
        var fin    = Hoy.AddDays(185);

        db.Timbrados.AddRange(
            new Timbrado
            {
                SucursalId          = sucursal.Id,
                Tipo                = TipoDocumentoFiscal.Factura,
                NumeroTimbrado      = "17845621",
                Establecimiento     = "001",
                PuntoExpedicion     = "001",
                NumeroDesde         = 1,
                NumeroHasta         = 5000,
                UltimoNumero        = 0,
                FechaInicioVigencia = inicio,
                FechaFinVigencia    = fin,
                IsActive            = true,
                CreatedAt           = now,
            },
            new Timbrado
            {
                SucursalId          = sucursal.Id,
                Tipo                = TipoDocumentoFiscal.NotaCredito,
                NumeroTimbrado      = "17845621",
                Establecimiento     = "001",
                PuntoExpedicion     = "002",
                NumeroDesde         = 1,
                NumeroHasta         = 1000,
                UltimoNumero        = 0,
                FechaInicioVigencia = inicio,
                FechaFinVigencia    = fin,
                IsActive            = true,
                CreatedAt           = now,
            });

        await db.SaveChangesAsync();
    }

    // ── Clientes ──────────────────────────────────────────────────────────────

    private static readonly string[] RazonesSociales =
    [
        "Farmacia San Roque S.A.", "Colegio Cristo Rey", "Transporte Guaraní S.R.L.",
        "Estudio Contable Ayala & Asoc.", "Supermercado El Pueblo S.A.", "Constructora Itá S.R.L.",
    ];

    private const int ObjetivoClientes = 90;

    private static async Task<List<Cliente>> SeedClientesAsync(AppDbContext db)
    {
        // Se completa hasta el objetivo en vez de abortar si ya hay clientes: la base de
        // desarrollo suele tener un puñado cargado a mano y aun así necesita volumen.
        var existentes = await db.Clientes.Include(c => c.Person).ToListAsync();
        var faltantes  = ObjetivoClientes - existentes.Count;
        if (faltantes <= 0) return existentes;

        var yaClientes = existentes.Select(c => c.PersonId).ToHashSet();
        var personas = await db.Patients
            .Include(p => p.Person)
            .Where(p => !yaClientes.Contains(p.PersonId))
            .OrderBy(p => p.Id)
            .Take(faltantes)
            .Select(p => p.Person)
            .ToListAsync();

        var now      = DateTime.UtcNow;
        var clientes = new List<Cliente>();

        for (int i = 0; i < personas.Count; i++)
        {
            var persona  = personas[i];
            var juridica = existentes.Count == 0 && i < RazonesSociales.Length;

            clientes.Add(new Cliente
            {
                PersonId        = persona.Id,
                Person          = persona,
                TipoFacturacion = juridica ? TipoFacturacion.Juridica : TipoFacturacion.Fisica,
                RazonSocial     = juridica ? RazonesSociales[i] : $"{persona.FirstName} {persona.LastName}",
                RucCiFiscal     = juridica ? $"80{Rng.Next(100000, 999999)}-{Rng.Next(0, 10)}" : persona.CI,
                Direccion       = GenDireccion(),
                Email           = persona.Email,
                Telefono        = persona.PhoneNumber,
                IsActive        = true,
                CreatedAt       = now.AddDays(-Rng.Next(30, 500)),
                UpdatedAt       = now,
            });
        }

        db.Clientes.AddRange(clientes);
        await db.SaveChangesAsync();
        return existentes.Concat(clientes).ToList();
    }

    // ── Agenda vigente ────────────────────────────────────────────────────────

    private static readonly string[] MotivosTurno =
    [
        "Control anual", "Adaptación de lentes de contacto", "Revisión de graduación",
        "Vista cansada", "Cambio de armazón", "Primera consulta", "Control de presión ocular",
        "Ojo seco", "Miopía progresiva", "Control post-cirugía", "Molestia con los lentes nuevos",
    ];

    /// <summary>
    /// Rellena la agenda alrededor de hoy. El seeder original genera turnos relativos a la fecha
    /// en que se sembró la base, así que con el tiempo la agenda queda vacía; esto la revive.
    /// </summary>
    private static async Task SeedTurnosVigentesAsync(AppDbContext db, Sucursal sucursal)
    {
        var desde = Hoy.AddDays(-45);
        var hasta = Hoy.AddDays(45);
        var from  = DateTime.SpecifyKind(desde.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var to    = DateTime.SpecifyKind(hasta.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);

        // Si ya hay agenda razonablemente poblada en la ventana, no se toca.
        if (await db.Turnos.CountAsync(t => t.FechaHora >= from && t.FechaHora <= to) > 80) return;

        var profesionales = await db.Professionals.OrderBy(p => p.Id).ToListAsync();
        var pacientes     = await db.Patients.OrderBy(p => p.Id).Select(p => p.Id).ToListAsync();
        if (profesionales.Count == 0 || pacientes.Count == 0) return;

        var ocupados = await db.Turnos
            .Where(t => t.FechaHora >= from && t.FechaHora <= to)
            .Select(t => new { t.ProfessionalId, t.FechaHora })
            .ToListAsync();
        var claves = ocupados.Select(o => $"{o.ProfessionalId}|{o.FechaHora:O}").ToHashSet();

        var ahora  = DateTime.UtcNow.AddHours(-3);
        var turnos = new List<Turno>();

        for (var dia = desde; dia <= hasta; dia = dia.AddDays(1))
        {
            if (dia.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) continue;

            foreach (var prof in profesionales)
            {
                // Ocupación del día: entre 3 y 8 turnos por profesional (jornada 08:00–17:00).
                var cantidad = Rng.Next(3, 9);
                for (int i = 0; i < cantidad; i++)
                {
                    var hora  = Rng.Next(8, 17);
                    var min   = Rng.Next(2) == 0 ? 0 : 30;
                    var fecha = new DateTime(dia.Year, dia.Month, dia.Day, hora, min, 0, DateTimeKind.Utc);

                    if (!claves.Add($"{prof.Id}|{fecha:O}")) continue;

                    var estado = fecha < ahora
                        ? Rng.Next(10) switch
                        {
                            0    => TurnoEstado.Cancelado,
                            1    => TurnoEstado.Cancelado,
                            _    => TurnoEstado.Completado,
                        }
                        : Rng.Next(3) == 0 ? TurnoEstado.Confirmado : TurnoEstado.Pendiente;

                    var creado = fecha.AddDays(-Rng.Next(2, 21));

                    turnos.Add(new Turno
                    {
                        SucursalId     = sucursal.Id,
                        ProfessionalId = prof.Id,
                        PatientId      = pacientes[Rng.Next(pacientes.Count)],
                        FechaHora      = fecha,
                        Estado         = estado,
                        Motivo         = MotivosTurno[Rng.Next(MotivosTurno.Length)],
                        CreatedAt      = creado,
                        UpdatedAt      = creado,
                    });
                }
            }
        }

        db.Turnos.AddRange(turnos);
        await db.SaveChangesAsync();
    }

    // ── Consultas clínicas y recetas ──────────────────────────────────────────

    private static readonly string[] Diagnosticos =
    [
        "Miopía simple", "Astigmatismo miópico compuesto", "Hipermetropía leve",
        "Presbicia", "Astigmatismo hipermetrópico", "Ojo seco evaporativo",
        "Miopía con astigmatismo", "Ametropía mixta",
    ];

    private static readonly string[] Planes =
    [
        "Corrección óptica con lentes monofocales. Control en 12 meses.",
        "Se indica lente progresivo. Control en 6 meses.",
        "Lágrimas artificiales 4 veces al día por 30 días.",
        "Adaptación de lentes de contacto blandos. Control en 15 días.",
        "Actualización de graduación. Control anual.",
    ];

    private static async Task SeedConsultasYRecetasAsync(AppDbContext db, Sucursal sucursal)
    {
        if (await db.ConsultasClinicas.CountAsync() >= 100) return;

        // Solo turnos que todavía no tienen consulta cargada, para no duplicar historia clínica.
        var conConsulta = await db.ConsultasClinicas
            .Where(c => c.CitaId != null)
            .Select(c => c.CitaId!.Value)
            .ToHashSetAsync();

        var estadoCerrada = await db.EstadosConfig
            .Where(e => e.Entidad == "Consulta" && e.CodigoInterno == "Cerrada")
            .Select(e => (int?)e.Id)
            .FirstOrDefaultAsync();

        var completados = await db.Turnos
            .Include(t => t.Patient).ThenInclude(p => p.Person)
            .Where(t => t.Estado == TurnoEstado.Completado && !conConsulta.Contains(t.Id))
            .OrderByDescending(t => t.FechaHora)
            .Take(120)
            .ToListAsync();

        var now = DateTime.UtcNow;

        foreach (var turno in completados)
        {
            var consulta = new ConsultaClinica
            {
                SucursalId           = sucursal.Id,
                PatientId            = turno.PatientId,
                ProfessionalId       = turno.ProfessionalId,
                CitaId               = turno.Id,
                FechaConsulta        = turno.FechaHora,
                Motivo               = turno.Motivo ?? "Control",
                Anamnesis            = "Refiere visión borrosa de lejos y cansancio ocular al final del día.",
                ExamenFisico         = "Segmento anterior sin alteraciones. Fondo de ojo normal.",
                DiagnosticoPrincipal = Diagnosticos[Rng.Next(Diagnosticos.Length)],
                PlanTratamiento      = Planes[Rng.Next(Planes.Length)],
                EstadoConfigId       = estadoCerrada,
                IsActive             = true,
                CreatedAt            = turno.FechaHora,
                UpdatedAt            = turno.FechaHora,
            };

            // ~70% de las consultas terminan en receta.
            if (Rng.Next(10) < 7)
                consulta.Receta = BuildReceta(sucursal.Id, turno.Patient.PersonId,
                    DateOnly.FromDateTime(turno.FechaHora), now);

            db.ConsultasClinicas.Add(consulta);
        }

        await db.SaveChangesAsync();
    }

    private static Receta BuildReceta(int sucursalId, int personId, DateOnly fecha, DateTime now)
    {
        // Esféricos y cilindros en pasos de 0,25 dioptrías, como en una receta real.
        decimal Paso(int min, int max) => Rng.Next(min, max + 1) * 0.25m;

        var adicion = Rng.Next(3) == 0 ? Paso(4, 12) : (decimal?)null;

        return new Receta
        {
            SucursalId            = sucursalId,
            PersonId              = personId,
            FechaEmision          = fecha,
            OdEsferico            = Paso(-24, 12),
            OdCilindro            = Rng.Next(3) == 0 ? null : -Paso(1, 10),
            OdEje                 = Rng.Next(0, 181),
            OdAdicion             = adicion,
            OiEsferico            = Paso(-24, 12),
            OiCilindro            = Rng.Next(3) == 0 ? null : -Paso(1, 10),
            OiEje                 = Rng.Next(0, 181),
            OiAdicion             = adicion,
            DistanciaInterpupilar = 56 + Rng.Next(0, 12),
            AvSinCorreccion       = "20/60",
            AvConCorreccion       = "20/20",
            CreatedAt             = now,
            UpdatedAt             = now,
        };
    }

    // ── Caja: apertura de sesiones ────────────────────────────────────────────

    private static async Task<(Dictionary<DateOnly, SesionCaja> Todas, List<SesionCaja> Nuevas, bool DejarUnaAbierta)>
        SeedSesionesCajaAsync(AppDbContext db, Sucursal sucursal, User admin, List<Empleado> empleados)
    {
        var existentes = await db.SesionesCaja
            .Where(s => s.SucursalId == sucursal.Id)
            .ToListAsync();

        var sesiones = existentes
            .GroupBy(s => DateOnly.FromDateTime(s.FechaApertura))
            .ToDictionary(g => g.Key, g => g.First());

        // Nunca puede haber dos cajas abiertas en la misma sucursal. Si el usuario ya dejó
        // una abierta se respeta y todas las sesiones generadas acá se cierran.
        var yaHayAbierta = existentes.Any(s =>
            s.Estado is EstadoSesionCaja.Abierta or EstadoSesionCaja.PendienteAprobacion);

        var cajeroId = empleados.FirstOrDefault()?.UserId ?? admin.Id;
        var nuevas   = new List<SesionCaja>();

        for (var dia = Hoy.AddDays(-60); dia <= Hoy; dia = dia.AddDays(1))
        {
            if (dia.DayOfWeek == DayOfWeek.Sunday) continue;
            if (sesiones.ContainsKey(dia)) continue;

            // 08:00 local = 11:00 UTC.
            var apertura = DateTime.SpecifyKind(dia.ToDateTime(new TimeOnly(11, 0)), DateTimeKind.Utc);

            var sesion = new SesionCaja
            {
                SucursalId    = sucursal.Id,
                Estado        = EstadoSesionCaja.Abierta,
                MontoInicial  = 500_000m,
                AbiertaPorId  = cajeroId,
                FechaApertura = apertura,
            };

            db.SesionesCaja.Add(sesion);
            sesiones[dia] = sesion;
            nuevas.Add(sesion);
        }

        await db.SaveChangesAsync();
        return (sesiones, nuevas, !yaHayAbierta);
    }

    // ── Compras a proveedores ─────────────────────────────────────────────────

    private static async Task SeedComprasAsync(AppDbContext db, Sucursal sucursal, User admin)
    {
        if (await db.PedidosProveedor.CountAsync() >= 16) return;

        var proveedores = await db.Proveedores.Where(p => !p.EsLaboratorio && p.IsActive).ToListAsync();
        var productos   = await db.Productos.Where(p => p.IsActive).ToListAsync();
        if (proveedores.Count == 0 || productos.Count == 0) return;

        var now = DateTime.UtcNow;

        for (int i = 0; i < 16; i++)
        {
            var proveedor  = proveedores[Rng.Next(proveedores.Count)];
            var fechaOrden = Hoy.AddDays(-Rng.Next(5, 240));
            var creado     = DateTime.SpecifyKind(fechaOrden.ToDateTime(new TimeOnly(13, 0)), DateTimeKind.Utc);

            var items = productos
                .OrderBy(_ => Rng.Next())
                .Take(Rng.Next(2, 6))
                .Select(p => new PedidoProveedorItem
                {
                    ProductoId     = p.Id,
                    Cantidad       = Rng.Next(3, 21),
                    PrecioUnitario = p.PrecioCosto,
                })
                .ToList();

            // Mezcla de estados: borradores, órdenes en tránsito y compras ya recibidas.
            var recibido = i >= 4;

            var pedido = new PedidoProveedor
            {
                SucursalId    = sucursal.Id,
                ProveedorId   = proveedor.Id,
                Estado        = i switch
                {
                    < 2 => EstadoPedido.Borrador,
                    < 4 => EstadoPedido.Confirmada,
                    _   => EstadoPedido.Facturada,
                },
                FechaOrden    = i < 2 ? null : fechaOrden,
                Observaciones = i % 4 == 0 ? "Reposición de temporada." : null,
                Items         = items,
                CreatedAt     = creado,
                UpdatedAt     = creado,
            };

            db.PedidosProveedor.Add(pedido);
            if (!recibido) continue;

            var total   = items.Sum(it => it.Cantidad * it.PrecioUnitario);
            var pagada  = fechaOrden < Hoy.AddDays(-35);
            var factura = new FacturaCompra
            {
                SucursalId       = sucursal.Id,
                RegistradoPorId  = admin.Id,
                PedidoProveedor  = pedido,
                ProveedorId      = proveedor.Id,
                NroFactura       = $"001-001-{Rng.Next(1, 999999):D7}",
                MontoGravado10   = total,
                CondicionVenta   = CondicionVenta.Credito,
                Monto            = total,
                Concepto         = $"Compra de mercadería — {proveedor.Nombre}",
                Estado           = pagada ? EstadoEgreso.Pagado : EstadoEgreso.Pendiente,
                FechaEmision     = fechaOrden,
                FechaVencimiento = fechaOrden.AddDays(30),
                FechaPago        = pagada ? fechaOrden.AddDays(Rng.Next(10, 31)) : null,
                MetodoPago       = pagada ? MetodoPago.Transferencia : null,
                CreatedAt        = creado,
                UpdatedAt        = creado,
                Items            = items.Select(it => new FacturaCompraItem
                {
                    ProductoId     = it.ProductoId,
                    Descripcion    = productos.First(p => p.Id == it.ProductoId).Nombre,
                    Cantidad       = it.Cantidad,
                    PrecioUnitario = it.PrecioUnitario,
                    TipoIva        = TipoIvaFactura.Iva10,
                }).ToList(),
            };
            db.Add(factura);

            var fechaRecepcion = fechaOrden.AddDays(Rng.Next(3, 15));
            var recepcion = new RecepcionMercaderia
            {
                SucursalId      = sucursal.Id,
                PedidoProveedor = pedido,
                FacturaCompra   = factura,
                FechaRecepcion  = fechaRecepcion,
                UserId          = admin.Id,
                CreatedAt       = DateTime.SpecifyKind(fechaRecepcion.ToDateTime(new TimeOnly(14, 0)), DateTimeKind.Utc),
            };

            foreach (var item in items)
            {
                item.CantidadRecibida = item.Cantidad;
                var producto = productos.First(p => p.Id == item.ProductoId);

                // Solo los consumibles se controlan por lote y vencimiento.
                var conLote = producto.Categoria is "Lentes de Contacto" or "Soluciones";

                var linea = new RecepcionMercaderiaItem
                {
                    PedidoItem       = item,
                    Cantidad         = item.Cantidad,
                    Lote             = conLote ? $"L{fechaRecepcion:yyMM}-{Rng.Next(100, 999)}" : null,
                    FechaVencimiento = conLote ? fechaRecepcion.AddMonths(Rng.Next(12, 36)) : null,
                };

                if (conLote)
                {
                    linea.StockLote = new StockLote
                    {
                        ProductoId       = producto.Id,
                        SucursalId       = sucursal.Id,
                        Lote             = linea.Lote!,
                        FechaVencimiento = linea.FechaVencimiento,
                        CantidadInicial  = item.Cantidad,
                        FechaIngreso     = fechaRecepcion,
                        CreatedAt        = now,
                    };
                }

                recepcion.Items.Add(linea);

                db.MovimientosStock.Add(new MovimientoStock
                {
                    ProductoId      = producto.Id,
                    SucursalId      = sucursal.Id,
                    Tipo            = TipoMovimientoStock.Entrada,
                    Cantidad        = item.Cantidad,
                    Motivo          = $"Recepción de mercadería — {proveedor.Nombre}",
                    FechaMovimiento = DateTime.SpecifyKind(fechaRecepcion.ToDateTime(new TimeOnly(14, 0)), DateTimeKind.Utc),
                    Estado          = EstadoMovimientoStock.Aprobado,
                    FechaAprobacion = DateTime.SpecifyKind(fechaRecepcion.ToDateTime(new TimeOnly(14, 0)), DateTimeKind.Utc),
                    CreatedAt       = now,
                });
            }

            db.RecepcionesMercaderia.Add(recepcion);
        }

        await db.SaveChangesAsync();
    }

    // ── Ventas, cobros y trabajos de laboratorio ──────────────────────────────

    private const int ObjetivoVentas = 150;

    private static async Task SeedVentasAsync(
        AppDbContext db, Sucursal sucursal, User admin, List<Empleado> empleados,
        List<Cliente> clientes, Dictionary<DateOnly, SesionCaja> sesiones)
    {
        var aCrear = ObjetivoVentas - await db.Ventas.CountAsync();
        if (aCrear <= 0 || clientes.Count == 0) return;

        var productos    = await db.Productos.Where(p => p.IsActive).ToListAsync();
        var armazones    = productos.Where(p => p.Categoria is "Marcos" or "Monturas Infantiles").ToList();
        var servicios    = await db.Servicios.Where(s => s.IsActive).ToListAsync();
        var tiposLente   = await db.TiposLente.Where(t => t.IsActive).ToListAsync();
        var tratamientos = await db.Tratamientos.Where(t => t.IsActive).ToListAsync();
        var laboratorios = await db.Proveedores.Where(p => p.EsLaboratorio && p.IsActive).ToListAsync();
        var timbrado     = await db.Timbrados.FirstOrDefaultAsync(t => t.Tipo == TipoDocumentoFiscal.Factura && t.IsActive);

        var recetasPorPersona = (await db.Recetas.ToListAsync())
            .Where(r => r.PersonId.HasValue)
            .GroupBy(r => r.PersonId!.Value)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(r => r.FechaEmision).First().Id);

        var vendedores = empleados.Select(e => e.UserId).DefaultIfEmpty(admin.Id).ToList();
        var ventas     = new List<Venta>();
        var now        = DateTime.UtcNow;

        for (int i = 0; i < aCrear; i++)
        {
            var cliente    = clientes[Rng.Next(clientes.Count)];
            var fechaVenta = Hoy.AddDays(-Rng.Next(0, 170));
            var creado     = DateTime.SpecifyKind(fechaVenta.ToDateTime(new TimeOnly(15, 0)), DateTimeKind.Utc);
            var aPedido    = Rng.Next(10) < 4 && tiposLente.Count > 0 && armazones.Count > 0;
            var antiguedad = Hoy.DayNumber - fechaVenta.DayNumber;

            var venta = new Venta
            {
                NumeroComprobante = "",
                SucursalId        = sucursal.Id,
                ClienteId         = cliente.Id,
                VendedorId        = vendedores[Rng.Next(vendedores.Count)],
                Tipo              = aPedido ? TipoVenta.TrabajoAPedido : TipoVenta.Directa,
                CondicionVenta    = CondicionVenta.Contado,
                FechaVenta        = fechaVenta,
                CreatedAt         = creado,
                UpdatedAt         = creado,
            };

            if (aPedido)
            {
                var tipoLente = tiposLente[Rng.Next(tiposLente.Count)];
                var armazon   = armazones[Rng.Next(armazones.Count)];
                var extras    = tratamientos.OrderBy(_ => Rng.Next()).Take(Rng.Next(0, 3)).ToList();
                var precio    = tipoLente.PrecioBase + extras.Sum(t => t.Precio);

                venta.Lineas.Add(new VentaLinea
                {
                    Tipo            = TipoLineaVenta.Lente,
                    Descripcion     = extras.Count > 0
                        ? $"Lente {tipoLente.Nombre} — {string.Join(", ", extras.Select(t => t.Nombre))}"
                        : $"Lente {tipoLente.Nombre}",
                    Cantidad        = 1,
                    PrecioUnitario  = precio,
                    CategoriaFiscal = CategoriaFiscal.Gravado10,
                });

                venta.Lineas.Add(new VentaLinea
                {
                    Tipo            = TipoLineaVenta.Producto,
                    ProductoId      = armazon.Id,
                    Descripcion     = armazon.Nombre,
                    Cantidad        = 1,
                    PrecioUnitario  = armazon.PrecioVenta,
                    CategoriaFiscal = CategoriaFiscal.Gravado10,
                });

                if (recetasPorPersona.TryGetValue(cliente.PersonId, out var recetaId))
                    venta.RecetaId = recetaId;

                // El estado del trabajo sigue la antigüedad de la venta: lo más viejo ya se
                // entregó, lo de esta semana sigue en el laboratorio.
                var estadoTrabajo = antiguedad switch
                {
                    > 40 => EstadoTrabajoPedido.Entregado,
                    > 20 => EstadoTrabajoPedido.Recibido,
                    > 8  => EstadoTrabajoPedido.Enviado,
                    > 2  => EstadoTrabajoPedido.PendienteEnvio,
                    _    => EstadoTrabajoPedido.Borrador,
                };

                venta.Estado = estadoTrabajo switch
                {
                    EstadoTrabajoPedido.Entregado      => EstadoVenta.ComprobanteEmitido,
                    EstadoTrabajoPedido.Recibido       => EstadoVenta.ListaParaCobrar,
                    EstadoTrabajoPedido.Enviado        => EstadoVenta.EnProceso,
                    EstadoTrabajoPedido.PendienteEnvio => EstadoVenta.Confirmada,
                    _                                  => EstadoVenta.Borrador,
                };

                if (venta.Estado != EstadoVenta.Borrador)
                    venta.FechaConfirmacion = fechaVenta;

                var trabajo = new TrabajoPedido
                {
                    RecetaId               = venta.RecetaId,
                    TipoLenteId            = tipoLente.Id,
                    ArmazonProductoId      = armazon.Id,
                    LaboratorioProveedorId = laboratorios.Count > 0
                        ? laboratorios[Rng.Next(laboratorios.Count)].Id
                        : null,
                    Estado                 = estadoTrabajo,
                    CreatedAt              = creado,
                    UpdatedAt              = creado,
                };

                foreach (var t in extras) trabajo.Tratamientos.Add(t);

                if (estadoTrabajo is EstadoTrabajoPedido.Enviado or EstadoTrabajoPedido.Recibido or EstadoTrabajoPedido.Entregado)
                {
                    trabajo.FechaEnvio           = fechaVenta.AddDays(1);
                    trabajo.FechaEstimadaEntrega = fechaVenta.AddDays(Rng.Next(7, 15));
                    trabajo.MedioEnvio           = (MedioEnvioLaboratorio)Rng.Next(0, 5);
                }

                if (estadoTrabajo is EstadoTrabajoPedido.Recibido or EstadoTrabajoPedido.Entregado)
                    trabajo.FechaRecepcion = fechaVenta.AddDays(Rng.Next(7, 16));

                if (estadoTrabajo == EstadoTrabajoPedido.Entregado)
                {
                    trabajo.FechaEntrega   = trabajo.FechaRecepcion!.Value.AddDays(Rng.Next(1, 8));
                    trabajo.EntregadoPorId = vendedores[Rng.Next(vendedores.Count)];
                    if (Rng.Next(5) == 0)
                        trabajo.RetiradoPor = "Retirado por un familiar";
                }

                venta.TrabajoPedido = trabajo;
            }
            else
            {
                var cantidadLineas = Rng.Next(1, 4);
                var elegidos = productos.OrderBy(_ => Rng.Next()).Take(cantidadLineas).ToList();

                foreach (var producto in elegidos)
                {
                    venta.Lineas.Add(new VentaLinea
                    {
                        Tipo            = TipoLineaVenta.Producto,
                        ProductoId      = producto.Id,
                        Descripcion     = producto.Nombre,
                        Cantidad        = Rng.Next(1, 3),
                        PrecioUnitario  = producto.PrecioVenta,
                        CategoriaFiscal = CategoriaFiscal.Gravado10,
                    });
                }

                if (servicios.Count > 0 && Rng.Next(4) == 0)
                {
                    var servicio = servicios[Rng.Next(servicios.Count)];
                    venta.Lineas.Add(new VentaLinea
                    {
                        Tipo            = TipoLineaVenta.Servicio,
                        ServicioId      = servicio.Id,
                        Descripcion     = servicio.Nombre,
                        Cantidad        = 1,
                        PrecioUnitario  = servicio.Precio,
                        CategoriaFiscal = CategoriaFiscal.Gravado10,
                    });
                }

                // Un puñado de ventas quedan en borrador (presupuesto) o canceladas.
                venta.Estado = Rng.Next(20) switch
                {
                    0 => EstadoVenta.Borrador,
                    1 => EstadoVenta.Cancelada,
                    2 => EstadoVenta.ListaParaCobrar,
                    _ => EstadoVenta.ComprobanteEmitido,
                };

                if (venta.Estado != EstadoVenta.Borrador)
                    venta.FechaConfirmacion = fechaVenta;
            }

            // Ventas a crédito con plan de cuotas: una de cada seis, sobre ventas ya cerradas.
            if (venta.Estado == EstadoVenta.ComprobanteEmitido && Rng.Next(6) == 0)
            {
                venta.CondicionVenta       = CondicionVenta.Credito;
                venta.CantidadCuotas       = Rng.Next(2, 5);
                venta.FrecuenciaCuotasDias = 30;
            }

            ventas.Add(venta);
            db.Ventas.Add(venta);
        }

        await db.SaveChangesAsync();

        // Numeración: el servicio la deriva del Id, así que se asigna después del insert.
        foreach (var venta in ventas)
            venta.NumeroComprobante = $"REC-{venta.Id:D7}";

        await db.SaveChangesAsync();

        await SeedCobrosYDocumentosAsync(db, sucursal, admin, empleados, ventas, sesiones, timbrado, laboratorios, now);
    }

    private static async Task SeedCobrosYDocumentosAsync(
        AppDbContext db, Sucursal sucursal, User admin, List<Empleado> empleados,
        List<Venta> ventas, Dictionary<DateOnly, SesionCaja> sesiones, Timbrado? timbrado,
        List<Proveedor> laboratorios, DateTime now)
    {
        var cobradores = empleados.Select(e => e.UserId).DefaultIfEmpty(admin.Id).ToList();

        void RegistrarCaja(Venta venta, decimal monto, MetodoPago metodo, DateOnly fecha, string concepto)
        {
            db.MovimientosCaja.Add(new MovimientoCaja
            {
                SucursalId      = sucursal.Id,
                Tipo            = TipoMovimientoCaja.Ingreso,
                Monto           = monto,
                Concepto        = concepto,
                MetodoPago      = metodo,
                Venta           = venta,
                SesionCaja      = sesiones.GetValueOrDefault(fecha),
                RegistradoPorId = cobradores[Rng.Next(cobradores.Count)],
                Fecha           = fecha,
                CreatedAt       = now,
            });
        }

        Cobro NuevoCobro(Venta venta, TipoCobro tipo, decimal monto, DateOnly fecha, MetodoPago metodo)
        {
            var cobro = new Cobro
            {
                Venta           = venta,
                Tipo            = tipo,
                MontoTotal      = monto,
                Fecha           = fecha,
                RegistradoPorId = cobradores[Rng.Next(cobradores.Count)],
                CreatedAt       = now,
                Lineas          =
                [
                    new CobroLinea
                    {
                        MetodoPago = metodo,
                        Monto      = monto,
                        Referencia = metodo == MetodoPago.Tarjeta ? $"AUTH{Rng.Next(100000, 999999)}" : null,
                    },
                ],
            };

            db.Cobros.Add(cobro);
            RegistrarCaja(venta, monto, metodo, fecha, $"Cobro {tipo} — venta {venta.NumeroComprobante}");
            return cobro;
        }

        MetodoPago MetodoAlAzar() => Rng.Next(10) switch
        {
            <= 4 => MetodoPago.Efectivo,
            <= 7 => MetodoPago.Tarjeta,
            8    => MetodoPago.Transferencia,
            _    => MetodoPago.Cheque,
        };

        foreach (var venta in ventas)
        {
            var total = venta.Lineas.Sum(l => l.Subtotal);
            if (total <= 0) continue;

            var esPedido = venta.Tipo == TipoVenta.TrabajoAPedido;

            // Seña del 50% al confirmar el trabajo a pedido.
            if (esPedido && venta.Estado is not (EstadoVenta.Borrador or EstadoVenta.Cancelada))
                NuevoCobro(venta, TipoCobro.Seña, Math.Round(total * 0.5m, 0), venta.FechaVenta, MetodoAlAzar());

            if (venta.Estado == EstadoVenta.ComprobanteEmitido)
            {
                var fechaDoc = venta.FechaVenta.AddDays(esPedido ? Rng.Next(8, 20) : 0);
                if (fechaDoc > Hoy) fechaDoc = Hoy;
                venta.FechaComprobante = fechaDoc;

                var cobrado = esPedido ? Math.Round(total * 0.5m, 0) : 0m;
                var saldo   = total - cobrado;

                if (venta.CantidadCuotas is int cuotas && cuotas > 0)
                {
                    // Crédito: se cobraron algunas cuotas, el resto sigue pendiente.
                    var montoCuota = Math.Round(saldo / cuotas, 0);
                    var pagadas    = Rng.Next(1, cuotas + 1);
                    for (int c = 0; c < pagadas; c++)
                    {
                        var fechaCuota = fechaDoc.AddDays(30 * (c + 1));
                        if (fechaCuota > Hoy) break;
                        NuevoCobro(venta, TipoCobro.Cuota, montoCuota, fechaCuota, MetodoAlAzar());
                    }
                }
                else if (saldo > 0)
                {
                    NuevoCobro(venta, TipoCobro.Cuota, saldo, fechaDoc, MetodoAlAzar());
                }

                // Un tercio de las ventas cerradas se factura; el resto sale con recibo simple.
                // Fuera de la vigencia del timbrado no se puede facturar: va recibo sí o sí.
                var puedeFacturar = timbrado is not null
                    && fechaDoc >= timbrado.FechaInicioVigencia
                    && fechaDoc <= timbrado.FechaFinVigencia
                    && (timbrado.NumeroHasta is null || timbrado.UltimoNumero < timbrado.NumeroHasta);

                if (puedeFacturar && Rng.Next(3) == 0)
                {
                    timbrado!.UltimoNumero++;
                    db.FacturasVenta.Add(new FacturaVenta
                    {
                        Venta           = venta,
                        NumeroFactura   = $"{timbrado.Establecimiento}-{timbrado.PuntoExpedicion}-{timbrado.UltimoNumero:D7}",
                        Timbrado        = timbrado.NumeroTimbrado,
                        Establecimiento = timbrado.Establecimiento,
                        MontoExento     = 0,
                        MontoGravado5   = 0,
                        MontoGravado10  = total,
                        FechaEmision    = fechaDoc,
                        TimbradoId      = timbrado.Id,
                        CreatedAt       = now,
                    });
                }
                else
                {
                    db.Comprobantes.Add(new Comprobante
                    {
                        Venta        = venta,
                        Tipo         = TipoComprobante.ReciboSimple,
                        Estado       = EstadoComprobante.Emitido,
                        EmitidoPorId = cobradores[Rng.Next(cobradores.Count)],
                        FechaEmision = DateTime.SpecifyKind(fechaDoc.ToDateTime(new TimeOnly(16, 0)), DateTimeKind.Utc),
                        CreatedAt    = now,
                    });
                }

                // Salida de stock por las líneas de producto, igual que al emitir el documento.
                foreach (var linea in venta.Lineas.Where(l => l.Tipo == TipoLineaVenta.Producto && l.ProductoId.HasValue))
                {
                    db.MovimientosStock.Add(new MovimientoStock
                    {
                        ProductoId      = linea.ProductoId!.Value,
                        SucursalId      = sucursal.Id,
                        Tipo            = TipoMovimientoStock.Salida,
                        Cantidad        = linea.Cantidad,
                        Motivo          = $"Comprobante venta {venta.NumeroComprobante}",
                        FechaMovimiento = DateTime.SpecifyKind(fechaDoc.ToDateTime(new TimeOnly(16, 0)), DateTimeKind.Utc),
                        Estado          = EstadoMovimientoStock.Aprobado,
                        FechaAprobacion = DateTime.SpecifyKind(fechaDoc.ToDateTime(new TimeOnly(16, 0)), DateTimeKind.Utc),
                        CreatedAt       = now,
                    });
                }
            }

            // Factura del laboratorio por el trabajo terminado → egreso a pagar.
            if (venta.TrabajoPedido is { } trabajo &&
                trabajo.Estado is EstadoTrabajoPedido.Recibido or EstadoTrabajoPedido.Entregado &&
                Rng.Next(10) < 8)
            {
                var costoLab   = Math.Round(total * 0.35m, 0);
                var fechaLab   = trabajo.FechaRecepcion ?? venta.FechaVenta.AddDays(10);
                var lab        = laboratorios.FirstOrDefault(l => l.Id == trabajo.LaboratorioProveedorId);
                var pagada     = fechaLab < Hoy.AddDays(-30);

                var facturaLab = new FacturaLaboratorio
                {
                    TrabajoPedido = trabajo,
                    NumeroFactura = $"002-001-{Rng.Next(1, 999999):D7}",
                    Timbrado      = "17233456",
                    FechaEmision  = fechaLab,
                    Monto         = costoLab,
                    EmitidoPorId  = admin.Id,
                    CreatedAt     = now,
                };
                db.FacturasLaboratorio.Add(facturaLab);

                db.Add(new EgresoFacturaLaboratorio
                {
                    SucursalId         = sucursal.Id,
                    RegistradoPorId    = admin.Id,
                    FacturaLaboratorio = facturaLab,
                    Monto              = costoLab,
                    Concepto           = $"Factura laboratorio {facturaLab.NumeroFactura} — {lab?.Nombre ?? "laboratorio"}",
                    Estado             = pagada ? EstadoEgreso.Pagado : EstadoEgreso.Pendiente,
                    FechaEmision       = fechaLab,
                    FechaVencimiento   = fechaLab.AddDays(30),
                    FechaPago          = pagada ? fechaLab.AddDays(Rng.Next(5, 30)) : null,
                    MetodoPago         = pagada ? MetodoPago.Transferencia : null,
                    NroComprobante     = pagada ? facturaLab.NumeroFactura : null,
                    CreatedAt          = now,
                    UpdatedAt          = now,
                });
            }
        }

        await db.SaveChangesAsync();
    }

    // ── Egresos recurrentes: gastos, salarios y honorarios ────────────────────

    private static readonly (string Categoria, string Concepto, decimal Monto, int Dia)[] GastosMensuales =
    [
        ("Alquiler",               "Alquiler del local",                     6_500_000m,  5),
        ("Servicios básicos",      "ANDE — energía eléctrica",                 980_000m, 12),
        ("Servicios básicos",      "ESSAP — agua corriente",                   185_000m, 12),
        ("Internet y telefonía",   "Internet fibra + línea telefónica",        420_000m,  8),
        ("Marketing y publicidad", "Campaña en redes sociales",              1_200_000m, 15),
        ("Limpieza e insumos",     "Insumos de limpieza y librería",           340_000m, 20),
        ("Impuestos y tasas",      "Patente municipal y tasas",                510_000m, 10),
        ("Honorarios profesionales", "Honorarios del contador",              1_500_000m,  7),
    ];

    private static async Task SeedEgresosVariosAsync(
        AppDbContext db, Sucursal sucursal, User admin, List<Empleado> empleados,
        Dictionary<DateOnly, SesionCaja> sesiones)
    {
        // Sentinel propio: se comprueba que no estén ya los gastos fijos de este seeder,
        // en vez de abortar porque la base tenga cualquier otro egreso cargado a mano.
        if (await db.Egresos.AnyAsync(e => e.Concepto.StartsWith("Alquiler del local —"))) return;

        var categorias = await db.CategoriasGasto.ToDictionaryAsync(c => c.Nombre, c => c.Id);
        var profesionales = await db.Professionals.ToListAsync();
        var now = DateTime.UtcNow;
        var pagados = new List<Egreso>();

        // Últimos 8 meses de gastos fijos.
        for (int m = 7; m >= 0; m--)
        {
            var mes = new DateOnly(Hoy.Year, Hoy.Month, 1).AddMonths(-m);
            var periodo = $"{mes:yyyy-MM}";

            foreach (var (categoria, concepto, monto, dia) in GastosMensuales)
            {
                if (!categorias.TryGetValue(categoria, out var categoriaId)) continue;

                var emision = new DateOnly(mes.Year, mes.Month, Math.Min(dia, DateTime.DaysInMonth(mes.Year, mes.Month)));
                if (emision > Hoy) continue;

                // Variación mensual de ±8% para que los gráficos no salgan planos.
                var importe = Math.Round(monto * (1 + (Rng.Next(-8, 9) / 100m)), 0);
                var pagado  = emision < Hoy.AddDays(-10);

                var gasto = new GastoGeneral
                {
                    SucursalId       = sucursal.Id,
                    RegistradoPorId  = admin.Id,
                    CategoriaGastoId = categoriaId,
                    Monto            = importe,
                    Concepto         = $"{concepto} — {periodo}",
                    Estado           = pagado ? EstadoEgreso.Pagado : EstadoEgreso.Pendiente,
                    FechaEmision     = emision,
                    FechaVencimiento = emision.AddDays(15),
                    FechaPago        = pagado ? emision.AddDays(Rng.Next(1, 12)) : null,
                    MetodoPago       = pagado ? (Rng.Next(2) == 0 ? MetodoPago.Efectivo : MetodoPago.Transferencia) : null,
                    CreatedAt        = now,
                    UpdatedAt        = now,
                };

                db.Add(gasto);
                if (pagado) pagados.Add(gasto);
            }

            // Salarios del mes.
            foreach (var empleado in empleados)
            {
                var emision = new DateOnly(mes.Year, mes.Month, Math.Min(30, DateTime.DaysInMonth(mes.Year, mes.Month)));
                if (emision > Hoy) continue;

                var pagado = emision < Hoy.AddDays(-3);
                var salario = new SalarioEmpleado
                {
                    SucursalId      = sucursal.Id,
                    RegistradoPorId = admin.Id,
                    EmpleadoId      = empleado.Id,
                    Periodo         = periodo,
                    Monto           = empleado.SalarioBase ?? 3_000_000m,
                    Concepto        = $"Salario {periodo} — {empleado.User.Person.FirstName} {empleado.User.Person.LastName}",
                    Estado          = pagado ? EstadoEgreso.Pagado : EstadoEgreso.Pendiente,
                    FechaEmision    = emision,
                    FechaPago       = pagado ? emision : null,
                    MetodoPago      = pagado ? MetodoPago.Transferencia : null,
                    CreatedAt       = now,
                    UpdatedAt       = now,
                };

                db.Add(salario);
                if (pagado) pagados.Add(salario);
            }

            // Honorarios de los profesionales.
            foreach (var profesional in profesionales)
            {
                var emision = new DateOnly(mes.Year, mes.Month, Math.Min(28, DateTime.DaysInMonth(mes.Year, mes.Month)));
                if (emision > Hoy) continue;

                var pagado = emision < Hoy.AddDays(-5);
                var honorario = new Honorario
                {
                    SucursalId      = sucursal.Id,
                    RegistradoPorId = admin.Id,
                    ProfessionalId  = profesional.Id,
                    Periodo         = periodo,
                    Monto           = Math.Round(2_500_000m + Rng.Next(0, 30) * 100_000m, 0),
                    Concepto        = $"Honorarios profesionales {periodo}",
                    Estado          = pagado ? EstadoEgreso.Pagado : EstadoEgreso.Pendiente,
                    FechaEmision    = emision,
                    FechaPago       = pagado ? emision.AddDays(2) : null,
                    MetodoPago      = pagado ? MetodoPago.Transferencia : null,
                    CreatedAt       = now,
                    UpdatedAt       = now,
                };

                db.Add(honorario);
                if (pagado) pagados.Add(honorario);
            }
        }

        await db.SaveChangesAsync();

        // Los egresos pagados en efectivo salen por caja; los bancarios no tocan la sesión.
        foreach (var egreso in pagados.Where(e => e.MetodoPago == MetodoPago.Efectivo && e.FechaPago.HasValue))
        {
            db.MovimientosCaja.Add(new MovimientoCaja
            {
                SucursalId      = sucursal.Id,
                Tipo            = TipoMovimientoCaja.Egreso,
                Monto           = egreso.Monto,
                Concepto        = $"Pago egreso — {egreso.Concepto}",
                MetodoPago      = MetodoPago.Efectivo,
                EgresoId        = egreso.Id,
                SesionCaja      = sesiones.GetValueOrDefault(egreso.FechaPago!.Value),
                RegistradoPorId = admin.Id,
                Fecha           = egreso.FechaPago.Value,
                CreatedAt       = now,
            });
        }

        await db.SaveChangesAsync();
    }

    // ── Caja: cierre de las sesiones pasadas ──────────────────────────────────

    private static async Task CerrarSesionesCajaAsync(
        AppDbContext db, User admin, List<SesionCaja> nuevas, bool dejarUnaAbierta)
    {
        if (nuevas.Count == 0) return;

        var ids = nuevas.Select(s => s.Id).ToList();

        // Solo se cierran las sesiones creadas por este seeder; las del usuario no se tocan.
        var sesiones = await db.SesionesCaja
            .Include(s => s.Movimientos)
            .Where(s => ids.Contains(s.Id) && s.Estado == EstadoSesionCaja.Abierta)
            .OrderBy(s => s.FechaApertura)
            .ToListAsync();

        // Si no había ninguna caja abierta previa, la última queda abierta para que el
        // sistema arranque con la caja del día en curso.
        var aCerrar = dejarUnaAbierta ? sesiones.Take(Math.Max(0, sesiones.Count - 1)) : sesiones;

        foreach (var sesion in aCerrar)
        {
            var ingresos = sesion.Movimientos
                .Where(m => m.Tipo == TipoMovimientoCaja.Ingreso && m.MetodoPago == MetodoPago.Efectivo)
                .Sum(m => m.Monto);
            var egresos = sesion.Movimientos
                .Where(m => m.Tipo == TipoMovimientoCaja.Egreso && m.MetodoPago == MetodoPago.Efectivo)
                .Sum(m => m.Monto);

            var esperado = sesion.MontoInicial + ingresos - egresos;

            // Una de cada diez cajas cierra con diferencia de arqueo y queda para aprobación.
            var diferencia = Rng.Next(10) == 0 ? Rng.Next(-3, 4) * 5_000m : 0m;
            var contado    = esperado + diferencia;

            sesion.EfectivoEsperado = esperado;
            sesion.EfectivoContado  = contado;
            sesion.Diferencia       = diferencia;
            sesion.CerradaPorId     = sesion.AbiertaPorId;
            sesion.FechaCierre      = sesion.FechaApertura.AddHours(11); // 08:00 → 19:00 local

            if (diferencia == 0)
            {
                sesion.Estado = EstadoSesionCaja.Cerrada;
            }
            else
            {
                sesion.ObservacionCierre = diferencia > 0
                    ? "Sobrante de arqueo, se revisa con el encargado."
                    : "Faltante de arqueo, pendiente de revisión.";

                // Las diferencias viejas ya fueron aprobadas; la más reciente queda pendiente.
                if (sesion.FechaApertura < DateTime.UtcNow.AddDays(-7))
                {
                    sesion.Estado          = EstadoSesionCaja.Cerrada;
                    sesion.AprobadoPorId   = admin.Id;
                    sesion.FechaAprobacion = sesion.FechaCierre;
                }
                else
                {
                    sesion.Estado = EstadoSesionCaja.PendienteAprobacion;
                }
            }
        }

        await db.SaveChangesAsync();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static readonly string[] Calles =
    [
        "Av. Mcal. López", "Av. España", "Palma", "Estrella", "Tte. Fariña",
        "Av. Eusebio Ayala", "Av. San Martín", "Denis Roa", "Rca. Argentina", "Sacramento",
    ];

    private static string GenDireccion() =>
        $"{Calles[Rng.Next(Calles.Length)]} {Rng.Next(100, 4000)} c/ {Calles[Rng.Next(Calles.Length)]}";

    private static string GenTelefono() => $"09{Rng.Next(61, 99)}{Rng.Next(100000, 999999)}";

    private static string NormStr(string s) =>
        s.ToLower()
         .Replace('á', 'a').Replace('é', 'e').Replace('í', 'i')
         .Replace('ó', 'o').Replace('ú', 'u').Replace('ñ', 'n')
         .Replace(".", "").Replace(",", "");
}
