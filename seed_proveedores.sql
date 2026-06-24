-- ============================================================
-- Seed: 20 Proveedores de ejemplo con 2+ contactos cada uno
-- Ejecutar contra la base de datos SIGA (PostgreSQL)
-- ============================================================

DO $$
DECLARE
  pid INTEGER;
BEGIN

-- 1. Óptica Alemana S.A.
INSERT INTO "Proveedores" ("Nombre","RazonSocial","Ruc","Direccion","Ciudad","SitioWeb","Facebook","Instagram","WhatsApp","IsActive","CreatedAt","UpdatedAt")
VALUES ('Óptica Alemana', 'Óptica Alemana S.A.', '80012345-6', 'Av. Mariscal López 1234', 'Asunción', 'www.opticaalemana.com.py', 'fb.com/opticaalemana', '@opticaalemanapy', '+595 971 100 200', true, NOW(), NOW())
RETURNING "Id" INTO pid;
INSERT INTO proveedor_contactos ("ProveedorId","Nombre","Cargo","Telefono","Email") VALUES
  (pid, 'Carlos Weiss', 'Gerente Comercial', '+595 971 100 201', 'c.weiss@opticaalemana.com.py'),
  (pid, 'Monika Braun', 'Coordinadora de Ventas', '+595 971 100 202', 'm.braun@opticaalemana.com.py');

-- 2. Laboratorios Visión Plus
INSERT INTO "Proveedores" ("Nombre","RazonSocial","Ruc","Direccion","Ciudad","SitioWeb","Facebook","Instagram","WhatsApp","IsActive","CreatedAt","UpdatedAt")
VALUES ('Laboratorios Visión Plus', 'Laboratorios Visión Plus S.R.L.', '4523678-9', 'Calle Palma 567', 'Asunción', 'www.visionplus.com.py', 'fb.com/visionpluspy', '@visionpluspy', '+595 981 200 300', true, NOW(), NOW())
RETURNING "Id" INTO pid;
INSERT INTO proveedor_contactos ("ProveedorId","Nombre","Cargo","Telefono","Email") VALUES
  (pid, 'Andrea Rodríguez', 'Directora de Laboratorio', '+595 981 200 301', 'a.rodriguez@visionplus.com.py'),
  (pid, 'Miguel Ávalos', 'Representante Técnico', '+595 981 200 302', 'm.avalos@visionplus.com.py'),
  (pid, 'Rosa Benítez', 'Administración', '+595 981 200 303', 'r.benitez@visionplus.com.py');

-- 3. Indo Paraguay
INSERT INTO "Proveedores" ("Nombre","RazonSocial","Ruc","Direccion","Ciudad","SitioWeb","Facebook","Instagram","WhatsApp","IsActive","CreatedAt","UpdatedAt")
VALUES ('Indo Paraguay', 'Indo Paraguay S.R.L.', '3301245-0', 'Mcal. Estigarribia 890', 'Asunción', 'www.indo.com.py', 'fb.com/indoparaguay', '@indoparaguay', '+595 991 300 400', true, NOW(), NOW())
RETURNING "Id" INTO pid;
INSERT INTO proveedor_contactos ("ProveedorId","Nombre","Cargo","Telefono","Email") VALUES
  (pid, 'Jorge Villalba', 'Gerente de Ventas', '+595 991 300 401', 'j.villalba@indo.com.py'),
  (pid, 'Claudia Sosa', 'Atención al Cliente', '+595 991 300 402', 'c.sosa@indo.com.py');

-- 4. Distribuidora Zeiss Paraguay
INSERT INTO "Proveedores" ("Nombre","RazonSocial","Ruc","Direccion","Ciudad","SitioWeb","Facebook","Instagram","WhatsApp","IsActive","CreatedAt","UpdatedAt")
VALUES ('Zeiss Paraguay', 'Distribuidora Zeiss Paraguay S.A.', '80198765-4', 'Av. España 2345', 'Asunción', 'www.zeiss.com.py', 'fb.com/zeissparaguay', '@zeissparaguay', '+595 972 400 500', true, NOW(), NOW())
RETURNING "Id" INTO pid;
INSERT INTO proveedor_contactos ("ProveedorId","Nombre","Cargo","Telefono","Email") VALUES
  (pid, 'Klaus Hoffmann', 'Director Regional', '+595 972 400 501', 'k.hoffmann@zeiss.com.py'),
  (pid, 'Laura Domínguez', 'Soporte Técnico', '+595 972 400 502', 'l.dominguez@zeiss.com.py'),
  (pid, 'Pablo Gaona', 'Ventas Corporativas', '+595 972 400 503', 'p.gaona@zeiss.com.py');

-- 5. Essilor Distribuciones
INSERT INTO "Proveedores" ("Nombre","RazonSocial","Ruc","Direccion","Ciudad","SitioWeb","Facebook","Instagram","WhatsApp","IsActive","CreatedAt","UpdatedAt")
VALUES ('Essilor Paraguay', 'Essilor Distribuciones Paraguay', '7234561-2', 'Av. San Martín 456', 'Luque', 'www.essilor.com.py', 'fb.com/essilorpy', '@essilorparaguay', '+595 982 500 600', true, NOW(), NOW())
RETURNING "Id" INTO pid;
INSERT INTO proveedor_contactos ("ProveedorId","Nombre","Cargo","Telefono","Email") VALUES
  (pid, 'Natalia Peralta', 'Key Account Manager', '+595 982 500 601', 'n.peralta@essilor.com.py'),
  (pid, 'Roberto Cáceres', 'Logística', '+595 982 500 602', 'r.caceres@essilor.com.py');

-- 6. Hoya Lenses Paraguay
INSERT INTO "Proveedores" ("Nombre","RazonSocial","Ruc","Direccion","Ciudad","SitioWeb","Facebook","Instagram","WhatsApp","IsActive","CreatedAt","UpdatedAt")
VALUES ('Hoya Lenses PY', 'Hoya Lenses Paraguay S.R.L.', '5678901-3', 'Ruta 2 Km 18 Local 3', 'Capiatá', 'www.hoyaoptics.com.py', 'fb.com/hoyapy', '@hoyalensespy', '+595 983 600 700', true, NOW(), NOW())
RETURNING "Id" INTO pid;
INSERT INTO proveedor_contactos ("ProveedorId","Nombre","Cargo","Telefono","Email") VALUES
  (pid, 'Kenji Matsuda', 'Representante Técnico', '+595 983 600 701', 'k.matsuda@hoyaoptics.com.py'),
  (pid, 'Sandra González', 'Atención Comercial', '+595 983 600 702', 's.gonzalez@hoyaoptics.com.py');

-- 7. Rodenstock Paraguay
INSERT INTO "Proveedores" ("Nombre","RazonSocial","Ruc","Direccion","Ciudad","SitioWeb","Facebook","Instagram","WhatsApp","IsActive","CreatedAt","UpdatedAt")
VALUES ('Rodenstock Paraguay', 'Rodenstock Paraguay S.A.', '9012345-7', 'Gral. Santos 1100', 'Asunción', 'www.rodenstock.com.py', 'fb.com/rodenstockpy', '@rodenstockparaguay', '+595 984 700 800', true, NOW(), NOW())
RETURNING "Id" INTO pid;
INSERT INTO proveedor_contactos ("ProveedorId","Nombre","Cargo","Telefono","Email") VALUES
  (pid, 'Hans Kramer', 'Gerente País', '+595 984 700 801', 'h.kramer@rodenstock.com.py'),
  (pid, 'Valeria Torres', 'Ventas', '+595 984 700 802', 'v.torres@rodenstock.com.py'),
  (pid, 'Diego Leiva', 'Postventa', '+595 984 700 803', 'd.leiva@rodenstock.com.py');

-- 8. Transitions Paraguay
INSERT INTO "Proveedores" ("Nombre","RazonSocial","Ruc","Direccion","Ciudad","SitioWeb","Facebook","Instagram","WhatsApp","IsActive","CreatedAt","UpdatedAt")
VALUES ('Transitions Paraguay', 'Transitions Optical Paraguay', '1234509-8', 'Av. Artigas 789', 'Asunción', 'www.transitions.com.py', 'fb.com/transitionsparaguay', '@transitionsparaguay', '+595 985 800 900', true, NOW(), NOW())
RETURNING "Id" INTO pid;
INSERT INTO proveedor_contactos ("ProveedorId","Nombre","Cargo","Telefono","Email") VALUES
  (pid, 'Carolina Medina', 'Brand Manager', '+595 985 800 901', 'c.medina@transitions.com.py'),
  (pid, 'Luis Ferreira', 'Ejecutivo de Cuentas', '+595 985 800 902', 'l.ferreira@transitions.com.py');

-- 9. Marco Visión
INSERT INTO "Proveedores" ("Nombre","RazonSocial","Ruc","Direccion","Ciudad","SitioWeb","Facebook","Instagram","WhatsApp","IsActive","CreatedAt","UpdatedAt")
VALUES ('Marco Visión', 'Marco Visión S.R.L.', '6789012-4', 'Mcal. López 345', 'San Lorenzo', NULL, 'fb.com/marcovisionpy', '@marcovisionpy', '+595 986 900 001', true, NOW(), NOW())
RETURNING "Id" INTO pid;
INSERT INTO proveedor_contactos ("ProveedorId","Nombre","Cargo","Telefono","Email") VALUES
  (pid, 'Gustavo Meza', 'Propietario', '+595 986 900 002', 'g.meza@marcovision.com.py'),
  (pid, 'Patricia Almada', 'Ventas', '+595 986 900 003', 'p.almada@marcovision.com.py');

-- 10. Armazones del Paraguay
INSERT INTO "Proveedores" ("Nombre","RazonSocial","Ruc","Direccion","Ciudad","SitioWeb","Facebook","Instagram","WhatsApp","IsActive","CreatedAt","UpdatedAt")
VALUES ('Armazones del Paraguay', 'Armazones del Paraguay S.A.', '2345670-1', 'Tte. Fariña 1200', 'Asunción', 'www.armazonespy.com', 'fb.com/armazonespy', '@armazonespy', '+595 987 001 100', true, NOW(), NOW())
RETURNING "Id" INTO pid;
INSERT INTO proveedor_contactos ("ProveedorId","Nombre","Cargo","Telefono","Email") VALUES
  (pid, 'Ramón Fleitas', 'Gerente Comercial', '+595 987 001 101', 'r.fleitas@armazonespy.com'),
  (pid, 'Ingrid Ríos', 'Diseño y Catálogo', '+595 987 001 102', 'i.rios@armazonespy.com'),
  (pid, 'Tomás Jara', 'Depósito y Envíos', '+595 987 001 103', 't.jara@armazonespy.com');

-- 11. Accesorios Ópticos JM
INSERT INTO "Proveedores" ("Nombre","RazonSocial","Ruc","Direccion","Ciudad","SitioWeb","Facebook","Instagram","WhatsApp","IsActive","CreatedAt","UpdatedAt")
VALUES ('Accesorios Ópticos JM', 'Accesorios Ópticos JM S.R.L.', '3456781-5', 'Calle Colón 678', 'Asunción', NULL, 'fb.com/accesoriosjm', '@accesoriosjm', '+595 981 101 200', true, NOW(), NOW())
RETURNING "Id" INTO pid;
INSERT INTO proveedor_contactos ("ProveedorId","Nombre","Cargo","Telefono","Email") VALUES
  (pid, 'Juan Monges', 'Dueño', '+595 981 101 201', 'j.monges@accesoriosjm.com.py'),
  (pid, 'María Monges', 'Administración', '+595 981 101 202', 'm.monges@accesoriosjm.com.py');

-- 12. Cristales y Monturas PY
INSERT INTO "Proveedores" ("Nombre","RazonSocial","Ruc","Direccion","Ciudad","SitioWeb","Facebook","Instagram","WhatsApp","IsActive","CreatedAt","UpdatedAt")
VALUES ('Cristales y Monturas PY', 'Cristales y Monturas del Paraguay', '4567892-6', 'Av. Boggiani 890', 'Asunción', 'www.cristalesymonturas.com.py', 'fb.com/cristalesymonturas', '@cristalesymonturas', '+595 982 201 300', true, NOW(), NOW())
RETURNING "Id" INTO pid;
INSERT INTO proveedor_contactos ("ProveedorId","Nombre","Cargo","Telefono","Email") VALUES
  (pid, 'Hugo Duarte', 'Jefe de Ventas', '+595 982 201 301', 'h.duarte@cristalesymonturas.com.py'),
  (pid, 'Elena Zárate', 'Soporte al Cliente', '+595 982 201 302', 'e.zarate@cristalesymonturas.com.py');

-- 13. Importadora Óptica Central
INSERT INTO "Proveedores" ("Nombre","RazonSocial","Ruc","Direccion","Ciudad","SitioWeb","Facebook","Instagram","WhatsApp","IsActive","CreatedAt","UpdatedAt")
VALUES ('Importadora Óptica Central', 'Importadora Óptica Central S.A.', '5678903-7', 'Estrella 1456', 'Asunción', 'www.opticacentral.com.py', 'fb.com/opticacentral', '@opticacentralpy', '+595 983 301 400', true, NOW(), NOW())
RETURNING "Id" INTO pid;
INSERT INTO proveedor_contactos ("ProveedorId","Nombre","Cargo","Telefono","Email") VALUES
  (pid, 'Osvaldo Romero', 'Gerente General', '+595 983 301 401', 'o.romero@opticacentral.com.py'),
  (pid, 'Graciela Núñez', 'Encargada de Compras', '+595 983 301 402', 'g.nunez@opticacentral.com.py'),
  (pid, 'Sebastián Vega', 'Ventas Regionales', '+595 983 301 403', 's.vega@opticacentral.com.py');

-- 14. Gran Visión Laboratorio
INSERT INTO "Proveedores" ("Nombre","RazonSocial","Ruc","Direccion","Ciudad","SitioWeb","Facebook","Instagram","WhatsApp","IsActive","CreatedAt","UpdatedAt")
VALUES ('Gran Visión Laboratorio', 'Gran Visión Laboratorio S.R.L.', '6789014-8', 'Ruta 1 Km 5', 'Fernando de la Mora', NULL, 'fb.com/granvisionlab', '@granvisionlab', '+595 984 401 500', true, NOW(), NOW())
RETURNING "Id" INTO pid;
INSERT INTO proveedor_contactos ("ProveedorId","Nombre","Cargo","Telefono","Email") VALUES
  (pid, 'Alberto Cano', 'Director Técnico', '+595 984 401 501', 'a.cano@granvision.com.py'),
  (pid, 'Silvia Ortiz', 'Recepción y Despacho', '+595 984 401 502', 's.ortiz@granvision.com.py');

-- 15. Distribuidora Visión Total
INSERT INTO "Proveedores" ("Nombre","RazonSocial","Ruc","Direccion","Ciudad","SitioWeb","Facebook","Instagram","WhatsApp","IsActive","CreatedAt","UpdatedAt")
VALUES ('Distribuidora Visión Total', 'Distribuidora Visión Total S.A.', '7890125-9', 'Av. Artigas 2300', 'Asunción', 'www.visiontotal.com.py', 'fb.com/visiontotalpy', '@visiontotalpy', '+595 985 501 600', true, NOW(), NOW())
RETURNING "Id" INTO pid;
INSERT INTO proveedor_contactos ("ProveedorId","Nombre","Cargo","Telefono","Email") VALUES
  (pid, 'Fernando Acosta', 'Gerente de Distribución', '+595 985 501 601', 'f.acosta@visiontotal.com.py'),
  (pid, 'Adriana Benítez', 'Ejecutiva de Cuentas', '+595 985 501 602', 'a.benitez@visiontotal.com.py'),
  (pid, 'Marcelo Paniagua', 'Logística', '+595 985 501 603', 'm.paniagua@visiontotal.com.py');

-- 16. Óptica Moderna Importaciones
INSERT INTO "Proveedores" ("Nombre","RazonSocial","Ruc","Direccion","Ciudad","SitioWeb","Facebook","Instagram","WhatsApp","IsActive","CreatedAt","UpdatedAt")
VALUES ('Óptica Moderna Import.', 'Óptica Moderna Importaciones S.R.L.', '8901236-0', 'Cerro Corá 789', 'Asunción', NULL, 'fb.com/opticamoderna', '@opticamoderna', '+595 986 601 700', true, NOW(), NOW())
RETURNING "Id" INTO pid;
INSERT INTO proveedor_contactos ("ProveedorId","Nombre","Cargo","Telefono","Email") VALUES
  (pid, 'Nicolás Ibarra', 'Socio Gerente', '+595 986 601 701', 'n.ibarra@opticamoderna.com.py'),
  (pid, 'Teresa Bogado', 'Facturación', '+595 986 601 702', 't.bogado@opticamoderna.com.py');

-- 17. Salud Visual Paraguay
INSERT INTO "Proveedores" ("Nombre","RazonSocial","Ruc","Direccion","Ciudad","SitioWeb","Facebook","Instagram","WhatsApp","IsActive","CreatedAt","UpdatedAt")
VALUES ('Salud Visual Paraguay', 'Proveedores de Salud Visual S.A.', '9012347-1', 'Gral. Díaz 1890', 'Asunción', 'www.saludvisualpy.com', 'fb.com/saludvisualpy', '@saludvisualpy', '+595 987 701 800', true, NOW(), NOW())
RETURNING "Id" INTO pid;
INSERT INTO proveedor_contactos ("ProveedorId","Nombre","Cargo","Telefono","Email") VALUES
  (pid, 'Rodrigo Espínola', 'Director Comercial', '+595 987 701 801', 'r.espinola@saludvisualpy.com'),
  (pid, 'Liliana Cabral', 'Asesora Técnica', '+595 987 701 802', 'l.cabral@saludvisualpy.com');

-- 18. Lentes & Co.
INSERT INTO "Proveedores" ("Nombre","RazonSocial","Ruc","Direccion","Ciudad","SitioWeb","Facebook","Instagram","WhatsApp","IsActive","CreatedAt","UpdatedAt")
VALUES ('Lentes & Co.', 'Lentes y Compañía S.R.L.', '1023458-2', 'Av. Mcal. López 3400', 'Lambaré', 'www.lentesco.com.py', 'fb.com/lentesco', '@lentescoparaguay', '+595 981 801 900', true, NOW(), NOW())
RETURNING "Id" INTO pid;
INSERT INTO proveedor_contactos ("ProveedorId","Nombre","Cargo","Telefono","Email") VALUES
  (pid, 'Fabián Aquino', 'Propietario', '+595 981 801 901', 'f.aquino@lentesco.com.py'),
  (pid, 'Nadia Paredes', 'Ventas', '+595 981 801 902', 'n.paredes@lentesco.com.py'),
  (pid, 'Ernesto Vera', 'Técnico Óptico', '+595 981 801 903', 'e.vera@lentesco.com.py');

-- 19. Sur Óptica Distribuciones (INACTIVO — ejemplo)
INSERT INTO "Proveedores" ("Nombre","RazonSocial","Ruc","Direccion","Ciudad","SitioWeb","Facebook","Instagram","WhatsApp","IsActive","CreatedAt","UpdatedAt")
VALUES ('Sur Óptica Distribuciones', 'Sur Óptica Distribuciones S.R.L.', '2134569-3', 'Av. Defensores del Chaco 567', 'Encarnación', NULL, 'fb.com/suroptica', '@suropticapy', '+595 985 901 000', false, NOW() - INTERVAL '6 months', NOW() - INTERVAL '2 months')
RETURNING "Id" INTO pid;
INSERT INTO proveedor_contactos ("ProveedorId","Nombre","Cargo","Telefono","Email") VALUES
  (pid, 'Carlos Ayala', 'Ex Gerente', '+595 985 901 001', 'c.ayala@suroptica.com.py'),
  (pid, 'Mirta Frutos', 'Administración', '+595 985 901 002', 'm.frutos@suroptica.com.py');

-- 20. PhotoChromic PY
INSERT INTO "Proveedores" ("Nombre","RazonSocial","Ruc","Direccion","Ciudad","SitioWeb","Facebook","Instagram","WhatsApp","IsActive","CreatedAt","UpdatedAt")
VALUES ('PhotoChromic PY', 'PhotoChromic Paraguay S.A.', '3245670-4', 'Av. Aviadores del Chaco 4500', 'Asunción', 'www.photochromicpy.com', 'fb.com/photochromicpy', '@photochromicpy', '+595 984 001 100', true, NOW(), NOW())
RETURNING "Id" INTO pid;
INSERT INTO proveedor_contactos ("ProveedorId","Nombre","Cargo","Telefono","Email") VALUES
  (pid, 'Alejandro Núñez', 'Gerente General', '+595 984 001 101', 'a.nunez@photochromicpy.com'),
  (pid, 'Verónica Cáceres', 'Directora de Marketing', '+595 984 001 102', 'v.caceres@photochromicpy.com'),
  (pid, 'Bruno Insaurralde', 'Soporte Técnico', '+595 984 001 103', 'b.insaurralde@photochromicpy.com');

END $$;
