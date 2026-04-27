# Arquitectura del Sistema — SIGA Óptica

## Visión general

SIGA Óptica es un sistema de gestión clínica para ópticas. Centraliza la atención al paciente, la agenda médica, el historial clínico, el inventario de productos y las ventas en una sola plataforma, con control de acceso basado en roles.

El sistema está construido sobre un modelo de datos que parte de una entidad central (`persons`) que representa a cualquier persona física del sistema, ya sea un paciente, un profesional o un usuario administrativo.

---

## Módulos

---

### 1. Identidad y Acceso ✅

**Qué hace:**
Gestiona quién puede entrar al sistema y qué puede hacer dentro de él. Es la base sobre la que se construyen todos los demás módulos.

**Tablas involucradas:**
- `persons` — datos personales de cualquier individuo del sistema (CI, nombre, email, teléfono, fecha de nacimiento)
- `users` — credenciales de acceso (contraseña hasheada, estado activo/inactivo); siempre vinculado a una `person`
- `roles` — roles configurables (Admin, Recepcionista, Profesional, etc.) con un array de permisos granulares
- `user_roles` — tabla pivote que asigna uno o varios roles a cada usuario

**Cómo se relacionan:**
Toda persona que necesite acceder al sistema tiene un registro en `persons` y uno en `users`. Los permisos no se asignan directamente al usuario sino a través de roles: un usuario puede tener múltiples roles y cada rol define qué acciones puede realizar (por ejemplo, `ver_pacientes`, `crear_venta`, `editar_rol`). El token JWT que se genera al hacer login incluye los roles y permisos, que el frontend y el backend validan en cada operación.

**Flujo típico:**
Un administrador crea un usuario desde el panel, le asigna el rol "Recepcionista" y ese usuario puede iniciar sesión y acceder únicamente a las secciones y acciones que ese rol permite.

---

### 2. Pacientes ✅

**Qué hace:**
Registra y administra la base de datos de pacientes de la óptica. Permite buscar, filtrar, crear, editar y desactivar pacientes.

**Tablas involucradas:**
- `persons` — datos personales del paciente (CI, nombre, contacto, fecha de nacimiento)
- `patients` — marca que esa persona es paciente del sistema; incluye `is_active` para el borrado lógico
- `users` *(opcional)* — si el paciente tiene acceso al sistema, `patients.user_id` apunta a su cuenta; si no, ese campo es `null`

**Cómo se relacionan:**
Un paciente siempre tiene una entrada en `persons`. El registro en `patients` referencia esa persona a través de `person_id`. Esto permite que un paciente exista en el sistema sin necesidad de tener credenciales de acceso — puede ser registrado por recepción simplemente con su CI y datos de contacto. Si en el futuro se le quiere dar acceso al portal, se crea un `user` y se vincula vía `user_id`.

**Flujo típico:**
Recepción registra a un paciente nuevo con nombre, CI, teléfono y/o email. Ese paciente queda disponible para ser asignado a citas, tener historial clínico y figurar en ventas.

---

### 3. Profesionales ✅

**Qué hace:**
Registra a los profesionales de salud (optómetras, oftalmólogos) que trabajan en la óptica. A diferencia de los pacientes, un profesional siempre tiene acceso al sistema.

**Tablas involucradas:**
- `persons` — datos personales del profesional
- `users` — cuenta de acceso al sistema (obligatoria para profesionales)
- `professionals` — datos profesionales específicos: especialidad y número de matrícula

**Cómo se relacionan:**
Un profesional es siempre un `user` con un registro adicional en `professionals`. La relación es `users` → `professionals` (1:1). El profesional hereda los datos personales de `persons` a través de `users.person_id`. Su especialidad (guardada en `professionals.specialty`) se muestra en la interfaz y en el token JWT para identificar su perfil.

**Flujo típico:**
El administrador crea un profesional desde el panel, lo que genera simultáneamente un registro en `persons`, `users` y `professionals`, y le asigna el rol "Profesional" con los permisos correspondientes.

---

### 4. Agenda 🔜

**Qué hace:**
Gestiona el calendario de citas entre pacientes y profesionales. Permite programar, confirmar, completar o cancelar turnos.

**Tablas involucradas:**
- `appointments` — cada cita: paciente, profesional, fecha/hora, duración, estado y notas
- `patients` — quién asiste
- `professionals` — quién atiende

**Cómo se relacionan:**
Cada cita conecta un `patient_id` con un `professional_id` y una fecha/hora. El estado de la cita sigue un ciclo de vida: `pendiente → confirmado → completado | cancelado | no_asistio`. Cuando una cita se completa, puede dar origen a un registro en `clinical_records` vinculado opcionalmente a esa cita mediante `appointment_id`.

**Flujo típico:**
Recepción busca un paciente existente, selecciona un profesional disponible y asigna un horario. El profesional ve su agenda del día y al finalizar la consulta registra la historia clínica desde esa misma cita.

---

### 5. Clínica / Historia Clínica 🔜

**Qué hace:**
Registra el historial médico-óptico de cada paciente: motivo de consulta, observaciones y, cuando corresponde, la prescripción óptica resultante.

**Tablas involucradas:**
- `clinical_records` — cada consulta: paciente, profesional, fecha, motivo y notas
- `prescriptions` — receta óptica vinculada a una consulta: esfera, cilindro, eje y adición para cada ojo, distancia pupilar y tipo de lente
- `appointments` *(opcional)* — la consulta puede o no haber surgido de una cita previa

**Cómo se relacionan:**
Una consulta (`clinical_record`) pertenece a un paciente y a un profesional. Si fue programada, referencia la cita original; si fue espontánea, `appointment_id` es `null`. Dentro de cada consulta puede existir una prescripción óptica (relación 1:1 con `prescriptions`). Esa prescripción puede ser referenciada luego desde una venta, permitiendo saber exactamente qué lentes se vendieron y con qué graduación.

**Flujo típico:**
El profesional atiende a un paciente, registra el motivo de consulta, sus observaciones y carga la receta con los datos de graduación. Recepción puede luego tomar esa receta y generar una venta de lentes vinculada a ella.

---

### 6. Inventario 🔜

**Qué hace:**
Administra el catálogo de productos de la óptica: marcos, lentes de contacto, soluciones y accesorios. Controla precios, stock y alertas de reposición.

**Tablas involucradas:**
- `product_categories` — categorías de productos (Marcos, Lentes de Contacto, Soluciones, Accesorios)
- `products` — cada producto con nombre, marca, SKU, precio, stock actual y stock mínimo

**Cómo se relacionan:**
Cada `product` pertenece a una `product_category`. El campo `stock_min` permite identificar productos con stock bajo para activar alertas. Los productos son consumidos por el módulo de ventas: cada vez que se registra una venta, el stock del producto vendido disminuye.

**Flujo típico:**
El administrador carga los productos del catálogo con su precio y stock inicial. Cuando el stock de un producto cae por debajo de `stock_min`, el sistema lo señala como prioritario para reposición. Los productos aparecen disponibles para ser seleccionados al registrar una venta.

---

### 7. Ventas 🔜

**Qué hace:**
Registra las ventas de productos de la óptica, asociándolas opcionalmente a un paciente y a una receta médica. Soporta distintos métodos de pago y lleva el historial de transacciones.

**Tablas involucradas:**
- `sales` — cabecera de la venta: paciente (opcional), usuario que registró, fecha, total, método de pago y estado
- `sale_items` — líneas de la venta: qué producto, cantidad, precio unitario al momento de la venta y subtotal
- `patients` *(opcional)* — a quién se le vendió
- `prescriptions` *(opcional)* — receta que originó la venta (ej.: venta de lentes graduados)
- `products` — qué se vendió
- `users` — quién registró la venta

**Cómo se relacionan:**
Una venta puede estar asociada a un paciente (si está registrado) o no (venta de mostrador). Si la venta incluye lentes graduados, puede vincularse a la prescripción correspondiente del paciente, creando trazabilidad clínica completa. Cada ítem de la venta registra el precio unitario en el momento de la transacción para preservar el historial aunque el precio del producto cambie después. Al confirmar la venta, el stock de cada producto vendido se descuenta en `products.stock`.

**Flujo típico:**
Recepción selecciona un paciente, agrega productos al carrito (opcionalmente vinculando una receta), ingresa el método de pago y confirma la venta. El stock se actualiza automáticamente y queda el registro completo para reportes.

---

## Relaciones entre módulos

```
Identidad/Acceso
    └── es la base de todos los módulos
    └── todo usuario que opera el sistema tiene Person + User + Roles

Pacientes / Profesionales
    └── ambos parten de Person
    └── Profesionales siempre tienen User; Pacientes opcionalmente

Agenda
    └── conecta Pacientes ↔ Profesionales en un horario específico
    └── puede dar origen a una consulta clínica

Clínica
    └── consume Pacientes y Profesionales
    └── puede originarse desde una cita de Agenda o de forma independiente
    └── genera Prescripciones que alimentan Ventas

Inventario
    └── provee los productos que se venden
    └── su stock es afectado por cada Venta confirmada

Ventas
    └── consume Inventario (descuenta stock)
    └── puede referenciar un Paciente y una Prescripción de Clínica
    └── registra qué usuario de Identidad/Acceso realizó la operación
```

---

## Flujo completo de un paciente

```
1. Recepción registra al paciente (persons + patients)
2. Se agenda una cita (appointments) con un profesional
3. El profesional atiende y registra la consulta (clinical_records)
4. Si corresponde, carga la receta óptica (prescriptions)
5. Recepción toma la receta y genera una venta (sales + sale_items)
6. El stock del inventario se actualiza (products.stock --)
7. El historial queda disponible para consultas futuras y reportes
```
