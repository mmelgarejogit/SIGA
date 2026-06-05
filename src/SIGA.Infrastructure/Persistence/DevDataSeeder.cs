using Microsoft.EntityFrameworkCore;
using SIGA.Domain.Entities;
using SIGA.Domain.Security;

namespace SIGA.Infrastructure.Persistence;

public static class DevDataSeeder
{
    private static readonly Random Rng = new(42);

    private static readonly string[] Nombres =
    [
        "Mateo", "Santiago", "Valentina", "Lucía", "Tomás", "Sofía", "Lucas", "Martina",
        "Nicolás", "Isabella", "Joaquín", "Camila", "Franco", "Agustina", "Facundo",
        "Florencia", "Diego", "Natalia", "Ignacio", "Valeria", "Emilio", "Carolina",
        "Rodrigo", "Gabriela", "Maximiliano", "Antonella", "Sebastián", "Paula",
        "Gonzalo", "Micaela", "Andrés", "Romina", "Leandro", "Vanesa", "Hernán",
        "Lorena", "Federico", "Daniela", "Gustavo", "Cecilia", "Ramiro", "Claudia",
        "Cristian", "Verónica", "Sergio", "Paola", "Roberto", "Silvana", "Alberto",
        "Mariana", "Eduardo", "Patricia", "Mario", "Graciela", "Jorge", "Elena",
        "Ricardo", "Amanda", "Hugo", "Beatriz", "Oscar", "Norma", "Rubén", "Alicia",
    ];

    private static readonly string[] Apellidos =
    [
        "González", "Martínez", "García", "López", "Rodríguez", "Fernández", "Sánchez",
        "Pérez", "Gómez", "Álvarez", "Díaz", "Torres", "Ruiz", "Ramírez", "Flores",
        "Acosta", "Medina", "Romero", "Herrera", "Castro", "Suárez", "Morales",
        "Ríos", "Vega", "Ortiz", "Gutiérrez", "Molina", "Ramos", "Mendoza", "Silva",
        "Delgado", "Pereyra", "Cabrera", "Muñoz", "Vargas", "Blanco", "Rojas",
        "Aguilar", "Benítez", "Vera", "Pinto", "Lara", "Quiroga", "Ibáñez",
        "Figueroa", "Guerrero", "Giménez", "Bravo", "Salinas", "Miranda",
    ];

    private static readonly string[] Motivs =
    [
        "Control anual", "Adaptación de lentes de contacto", "Revisión de graduación",
        "Dolor ocular", "Vista cansada", "Cambio de marcos", "Seguimiento post-cirugía",
        "Primera consulta", "Queratometría", "Control de presión ocular",
        "Síndrome de ojo seco", "Miopía progresiva", "Baja visión",
    ];

    private static readonly (string Nombre, string Apellido, string Matricula, int[] EspIdx)[]
        ProfesionalesSeed =
        [
            ("Ana",     "Pérez",      "MP-12345", [0, 2]),
            ("Carlos",  "Rodríguez",  "MP-23456", [1, 3]),
            ("María",   "García",     "MP-34567", [0, 4]),
            ("Jorge",   "Martínez",   "MP-45678", [1, 2]),
            ("Laura",   "González",   "MP-56789", [0, 3]),
        ];

    private static readonly (string Nombre, string Cat, decimal Costo, decimal Venta, int Min, int Act)[]
        ProductosSeed =
        [
            ("Marcos Ray-Ban RB5154",            "Marcos",              8500m,  15900m,  5,  23),
            ("Marcos Oakley OX8046",              "Marcos",             12000m,  22500m,  3,  11),
            ("Marcos Vogue VO5230",               "Marcos",              5500m,   9800m,  5,  18),
            ("Marcos Prada PR 07WV",              "Marcos",             18000m,  34900m,  2,   7),
            ("Marcos Armani EA3187",              "Marcos",             14000m,  26500m,  3,   2),
            ("Lente monofocal CR39 +2.00",        "Lentes Oftálmicos",   2200m,   4500m, 10,  42),
            ("Lente monofocal CR39 -1.50",        "Lentes Oftálmicos",   2200m,   4500m, 10,  38),
            ("Lente progresivo Essilor",          "Lentes Oftálmicos",   9500m,  18900m,  5,  14),
            ("Lente progresivo Hoya",             "Lentes Oftálmicos",   8200m,  16500m,  5,   9),
            ("Lente fotocromático Transitions",   "Lentes Oftálmicos",   6800m,  13200m,  5,   3),
            ("Acuvue Oasys 6u",                   "Lentes de Contacto",  3200m,   5900m, 10,  45),
            ("Dailies Total1 30u",                "Lentes de Contacto",  4800m,   8900m,  8,  27),
            ("Biofinity 6u",                      "Lentes de Contacto",  2900m,   5400m, 10,  33),
            ("Air Optix Plus 6u",                 "Lentes de Contacto",  2800m,   5200m, 10,   2),
            ("FreshLook Colorblends 2u",          "Lentes de Contacto",  1800m,   3400m,  5,  16),
            ("Solución ReNu 360ml",               "Soluciones",           850m,   1600m, 15,  52),
            ("Solución salina B&L 360ml",         "Soluciones",           600m,   1200m, 10,  38),
            ("Clear Care 360ml",                  "Soluciones",          1200m,   2300m,  8,  19),
            ("Gotas Optase 10ml",                 "Soluciones",           980m,   1900m, 10,   7),
            ("Systane Ultra 10ml",                "Soluciones",          1100m,   2100m, 10,  29),
            ("Estuche rígido estándar",           "Accesorios",           350m,    750m, 10,  44),
            ("Estuche blando premium",            "Accesorios",           250m,    550m, 10,  31),
            ("Paño de microfibra",                "Accesorios",           120m,    280m, 20,  67),
            ("Spray limpiador antirreflejo",      "Accesorios",           450m,    950m, 10,  13),
            ("Kit destornilladores ópticos",      "Accesorios",           800m,   1600m,  5,   8),
            ("Correa deportiva para lentes",      "Accesorios",           300m,    650m,  8,  22),
            ("Lupa de lectura +2.5",              "Accesorios",          1200m,   2400m,  3,   6),
            ("Clip solar polarizado UV400",       "Accesorios",           650m,   1300m,  5,   2),
            ("Kit reparación patillas",           "Accesorios",           180m,    400m, 10,   4),
            ("Colgante porta-lentes",             "Accesorios",           220m,    480m,  8,  17),
        ];

    // ── Entry point ───────────────────────────────────────────────────────────

    public static async Task SeedAsync(AppDbContext db, IPasswordHasher hasher)
    {
        var pwHash           = hasher.Hash("12345678");
        var adminRole        = await db.Roles.FirstAsync(r => r.Type == "admin");
        var patientRole      = await db.Roles.FirstAsync(r => r.Type == "patient");
        var professionalRole = await db.Roles.FirstAsync(r => r.Type == "professional");
        var especialidades   = await db.Especialidades.ToListAsync();

        await SeedAdminAsync(db, pwHash, adminRole);
        await SyncHorariosAsync(db);

        if (!await db.Patients.AnyAsync())
        {
            var professionals = await SeedProfessionalsAsync(db, pwHash, professionalRole, especialidades);
            var patients      = await SeedPatientsAsync(db, pwHash, patientRole);
            await SeedTurnosAsync(db, professionals, patients);
            await SeedInventarioAsync(db);
        }
    }

    // ── Admin ─────────────────────────────────────────────────────────────────

    private static async Task SeedAdminAsync(AppDbContext db, string pwHash, Role adminRole)
    {
        if (await db.Persons.AnyAsync(p => p.CI == "99999999")) return;

        var now = DateTime.UtcNow;
        var person = new Person
        {
            CI          = "99999999",
            FirstName   = "Admin",
            LastName    = "SIGA",
            BirthDate   = new DateOnly(1990, 1, 1),
            Email       = "admin@siga.com",
            CreatedAt   = now,
            UpdatedAt   = now,
        };
        var user = new User
        {
            Person          = person,
            PasswordHash    = pwHash,
            IsActive        = true,
            IsEmailVerified = true,
            CreatedAt       = now,
            UpdatedAt       = now,
            UserRoles       = [new UserRole { RoleId = adminRole.Id }],
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }

    // ── Profesionales ─────────────────────────────────────────────────────────

    private static async Task<List<Professional>> SeedProfessionalsAsync(
        AppDbContext db, string pwHash, Role role, List<Especialidad> especialidades)
    {
        var now    = DateTime.UtcNow;
        var result = new List<Professional>();
        int ciBase = 9000001;

        foreach (var (nombre, apellido, matricula, espIdx) in ProfesionalesSeed)
        {
            var person = new Person
            {
                CI          = (ciBase++).ToString(),
                FirstName   = nombre,
                LastName    = apellido,
                BirthDate   = new DateOnly(1975 + Rng.Next(20), Rng.Next(1, 13), Rng.Next(1, 28)),
                PhoneNumber = GenPhone(),
                Email       = $"{NormStr(nombre)}.{NormStr(apellido)}@siga-optica.com",
                CreatedAt   = now,
                UpdatedAt   = now,
            };

            var user = new User
            {
                Person           = person,
                PasswordHash     = pwHash,
                IsActive         = true,
                IsEmailVerified  = true,
                CreatedAt        = now,
                UpdatedAt        = now,
                UserRoles        = [new UserRole { RoleId = role.Id }],
            };

            var professional = new Professional
            {
                User          = user,
                LicenseNumber = matricula,
                CreatedAt     = now,
                UpdatedAt     = now,
                Horarios      = BuildHorarios(),
                Especialidades = espIdx
                    .Where(i => i < especialidades.Count)
                    .Select(i => new ProfesionalEspecialidad { EspecialidadId = especialidades[i].Id })
                    .ToList(),
            };

            db.Professionals.Add(professional);
            result.Add(professional);
        }

        await db.SaveChangesAsync();
        return result;
    }

    private static readonly DayOfWeek[] DiasLaborales =
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
        DayOfWeek.Thursday, DayOfWeek.Friday,
    ];

    private static readonly TimeOnly HoraInicioDev = new(8, 0);
    private static readonly TimeOnly HoraFinDev    = new(17, 0);

    private static List<HorarioProfesional> BuildHorarios() =>
        DiasLaborales.Select(d => new HorarioProfesional
        {
            DiaSemana  = d,
            HoraInicio = HoraInicioDev,
            HoraFin    = HoraFinDev,
            Activo     = true,
        }).ToList();

    private static async Task SyncHorariosAsync(AppDbContext db)
    {
        var horarios = await db.HorariosProfesional.ToListAsync();
        if (!horarios.Any()) return;

        var changed = false;
        foreach (var h in horarios.Where(h => DiasLaborales.Contains(h.DiaSemana)))
        {
            if (h.HoraInicio == HoraInicioDev && h.HoraFin == HoraFinDev) continue;
            h.HoraInicio = HoraInicioDev;
            h.HoraFin    = HoraFinDev;
            changed      = true;
        }

        if (changed) await db.SaveChangesAsync();
    }

    // ── Pacientes ─────────────────────────────────────────────────────────────

    private static async Task<List<Patient>> SeedPatientsAsync(
        AppDbContext db, string pwHash, Role role)
    {
        var now    = DateTime.UtcNow;
        var result = new List<Patient>();
        var usedCi = new HashSet<string>();
        int ciBase = 1000001;

        for (int i = 0; i < 200; i++)
        {
            var nombre   = Nombres[Rng.Next(Nombres.Length)];
            var apellido = Apellidos[Rng.Next(Apellidos.Length)];
            var ci       = NextCi(ref ciBase, usedCi);
            var email    = $"{NormStr(nombre)}.{NormStr(apellido)}{i:D3}@mail.com";
            var isUser   = i < 20; // primeros 20 tienen cuenta

            var person = new Person
            {
                CI          = ci,
                FirstName   = nombre,
                LastName    = apellido,
                BirthDate   = new DateOnly(1940 + Rng.Next(65), Rng.Next(1, 13), Rng.Next(1, 28)),
                PhoneNumber = GenPhone(),
                Email       = email,
                CreatedAt   = now,
                UpdatedAt   = now,
            };

            Patient patient;

            if (isUser)
            {
                var user = new User
                {
                    Person          = person,
                    PasswordHash    = pwHash,
                    IsActive        = true,
                    IsEmailVerified = true,
                    CreatedAt       = now,
                    UpdatedAt       = now,
                    UserRoles       = [new UserRole { RoleId = role.Id }],
                };
                patient = new Patient
                {
                    Person    = person,
                    User      = user,
                    IsActive  = true,
                    CreatedAt = now,
                    UpdatedAt = now,
                };
            }
            else
            {
                patient = new Patient
                {
                    Person    = person,
                    IsActive  = true,
                    CreatedAt = now,
                    UpdatedAt = now,
                };
            }

            db.Patients.Add(patient);
            result.Add(patient);
        }

        await db.SaveChangesAsync();
        return result;
    }

    // ── Turnos ────────────────────────────────────────────────────────────────

    private static async Task SeedTurnosAsync(
        AppDbContext db, List<Professional> professionals, List<Patient> patients)
    {
        var now  = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        // Slots disponibles (9:00–17:30, cada 30 min)
        var slots = new List<(int h, int m)>();
        for (int h = 9; h < 18; h++)
        {
            slots.Add((h, 0));
            if (h < 17) slots.Add((h, 30));
        }
        slots.Add((17, 30));

        var occupied = new HashSet<string>();
        var turnos   = new List<Turno>();

        foreach (var patient in patients)
        {
            if (turnos.Count >= 420) break;

            int qty = Rng.Next(1, 4);
            for (int t = 0; t < qty; t++)
            {
                if (turnos.Count >= 420) break;

                var daysOff = Rng.Next(-90, 45);
                var date    = today.AddDays(daysOff);

                // Mover al siguiente día hábil si cae en fin de semana
                while (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    date = date.AddDays(1);

                var prof = professionals[Rng.Next(professionals.Count)];
                var slot = slots[Rng.Next(slots.Count)];
                var key  = $"{prof.Id}_{date:yyyyMMdd}_{slot.h}:{slot.m:D2}";

                if (!occupied.Add(key)) continue;

                var fechaHora = new DateTime(date.Year, date.Month, date.Day,
                    slot.h, slot.m, 0, DateTimeKind.Utc);

                var estado = fechaHora < now
                    ? (Rng.Next(5) == 0 ? TurnoEstado.Cancelado : TurnoEstado.Completado)
                    : TurnoEstado.Pendiente;

                var createdAt = fechaHora.AddDays(-Rng.Next(1, 14));

                turnos.Add(new Turno
                {
                    ProfessionalId = prof.Id,
                    PatientId      = patient.Id,
                    FechaHora      = fechaHora,
                    Estado         = estado,
                    Motivo         = Motivs[Rng.Next(Motivs.Length)],
                    CreatedAt      = createdAt,
                    UpdatedAt      = createdAt,
                });
            }
        }

        db.Turnos.AddRange(turnos);
        await db.SaveChangesAsync();
    }

    // ── Inventario ────────────────────────────────────────────────────────────

    private static async Task SeedInventarioAsync(AppDbContext db)
    {
        var now      = DateTime.UtcNow;
        var productos = new List<Producto>();

        foreach (var (nombre, cat, costo, venta, min, act) in ProductosSeed)
        {
            productos.Add(new Producto
            {
                Nombre      = nombre,
                Categoria   = cat,
                Sku         = GenSku(cat, nombre),
                PrecioCosto = costo,
                PrecioVenta = venta,
                IsActive    = true,
                CreatedAt   = now.AddDays(-Rng.Next(30, 365)),
                UpdatedAt   = now,
                StockConfig = new ProductoStockConfig { StockMinimo = min, UpdatedAt = now },
            });
        }

        db.Productos.AddRange(productos);
        await db.SaveChangesAsync();

        // Stock inicial como movimientos aprobados
        var movimientosIniciales = ProductosSeed
            .Zip(productos, (seed, p) => (p, act: seed.Item6))
            .Where(x => x.act > 0)
            .Select(x => new MovimientoStock
            {
                ProductoId      = x.p.Id,
                Tipo            = "Entrada",
                Cantidad        = x.act,
                Motivo          = "Ingreso inicial",
                Estado          = "Aprobado",
                FechaMovimiento = x.p.CreatedAt,
                FechaAprobacion = x.p.CreatedAt,
            });
        db.MovimientosStock.AddRange(movimientosIniciales);
        await db.SaveChangesAsync();

        var tiposMovimiento = new[] { "Entrada", "Salida", "Salida", "Ajuste" };
        string[] motivosMov =
        [
            "Compra a proveedor", "Venta al cliente", "Pérdida / rotura",
            "Ajuste de inventario", "Devolución de cliente", "Ingreso inicial",
        ];

        var movimientos = new List<MovimientoStock>();
        foreach (var prod in productos)
        {
            int qty = Rng.Next(3, 8);
            for (int m = 0; m < qty; m++)
            {
                var tipo     = tiposMovimiento[Rng.Next(tiposMovimiento.Length)];
                var cantidad = tipo == "Ajuste" ? Rng.Next(5, 30) : Rng.Next(1, 12);

                movimientos.Add(new MovimientoStock
                {
                    ProductoId = prod.Id,
                    Tipo       = tipo,
                    Cantidad   = cantidad,
                    Motivo     = motivosMov[Rng.Next(motivosMov.Length)],
                    CreatedAt  = now.AddDays(-Rng.Next(1, 180)),
                });
            }
        }

        db.MovimientosStock.AddRange(movimientos);
        await db.SaveChangesAsync();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string GenPhone() =>
        $"09{Rng.Next(1, 9)}{Rng.Next(1000000, 9999999)}";

    private static string NormStr(string s) =>
        s.ToLower()
         .Replace('á', 'a').Replace('é', 'e').Replace('í', 'i')
         .Replace('ó', 'o').Replace('ú', 'u').Replace('ñ', 'n');

    private static string NextCi(ref int base_, HashSet<string> used)
    {
        var ci = (base_++).ToString();
        while (used.Contains(ci)) ci = (base_++).ToString();
        used.Add(ci);
        return ci;
    }

    private static string GenSku(string cat, string nombre)
    {
        var prefix = cat switch
        {
            "Marcos"              => "MAR",
            "Lentes Oftálmicos"   => "LOP",
            "Lentes de Contacto"  => "LCT",
            "Soluciones"          => "SOL",
            _                     => "ACC",
        };
        return $"{prefix}-{Math.Abs(nombre.GetHashCode() % 9000) + 1000}";
    }
}
