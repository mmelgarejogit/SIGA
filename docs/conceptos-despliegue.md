# Conceptos de despliegue — Docker, Caddy y SSH

`despliegue-vps.md` te dice **qué comandos correr**. Este documento explica **qué hace
cada uno y por qué**, usando los archivos reales de este repo como ejemplo.

Leelo una vez antes de desplegar y otra vez cuando algo falle: casi todos los problemas
de despliegue son un concepto de acá que no encajaba donde uno creía.

---

## El mapa: qué pasa cuando alguien abre la URL

```
Navegador
   │  https://siga.tudominio.com
   ▼
DNS  ─────────────► "ese nombre es la IP 203.0.113.10"
   │
   ▼
VPS :443
   │
   ▼
┌─────────────────────────────────────────────┐
│ Caddy          (único expuesto a internet)  │
│   ├── /api/*, /uploads/*  ──► api  :8080    │
│   └── todo lo demás       ──► web  :80      │
│                                 api ──► db  │
└─────────────────────────────────────────────┘
        red interna de Docker (siga_net)
```

Los cuatro contenedores están en la misma red privada. **Solo Caddy tiene puertos
publicados**; `db`, `api` y `web` son inalcanzables desde internet. Eso no es una opción
de configuración que activamos: es la consecuencia de que en `docker-compose.prod.yml`
únicamente el servicio `caddy` tenga la clave `ports`.

---

## Parte 1 — Docker

### Imagen y contenedor no son lo mismo

- **Imagen**: un sistema de archivos congelado, con la app ya compilada adentro. Es una
  plantilla de solo lectura. Se construye una vez.
- **Contenedor**: una instancia viva de esa imagen, con una capa de escritura encima.

La analogía útil: la imagen es la clase, el contenedor es el objeto. De una imagen podés
levantar diez contenedores idénticos.

**Consecuencia práctica, y la fuente de sustos más común:** todo lo que un contenedor
escribe en su capa propia **desaparece cuando el contenedor se destruye**. Por eso los
datos que tienen que sobrevivir van en volúmenes (más abajo).

### Por qué el `Dockerfile` tiene dos `FROM`

Se llama *multi-stage build*. Mirá el de este repo:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build     # etapa 1: compilar
...
RUN dotnet publish ... -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0           # etapa 2: ejecutar
COPY --from=build /app/publish .
```

La etapa 1 usa el **SDK**, que trae el compilador, NuGet y todas las herramientas:
cientos de megabytes que solo hacen falta para *construir*. La etapa 2 arranca de cero
con la imagen de **runtime**, que solo sabe ejecutar, y se trae con `COPY --from=build`
únicamente el resultado compilado.

La imagen final no contiene el compilador ni el código fuente. Menos peso, y menos
superficie de ataque en un servidor público.

Lo mismo hace `SIGA-Web/Dockerfile`: compila con `node:20-alpine` y sirve el resultado
con `nginx:alpine`. Node no viaja al servidor.

### Capas y caché: por qué el orden de las líneas importa

Cada instrucción del `Dockerfile` crea una capa, y Docker las cachea. Si una capa no
cambió, reusa la anterior y sigue. **Pero si una capa cambia, todas las siguientes se
reconstruyen.**

Por eso el Dockerfile del front hace esto, y no al revés:

```dockerfile
COPY package*.json ./
RUN npm ci          # ← capa cara, se cachea
COPY . .            # ← el código cambia todo el tiempo
RUN npm run build
```

Si copiara todo el código primero, cualquier cambio en un `.vue` invalidaría la capa del
`npm ci` y reinstalaría todas las dependencias en cada deploy.

> El backend de SIGA hace `COPY . .` antes del `restore`, así que hoy no aprovecha ese
> caché: cada build vuelve a bajar los paquetes NuGet. Funciona bien, solo es más lento.
> Es una optimización pendiente, no un error.

### Volúmenes: lo único que sobrevive

Un volumen es almacenamiento gestionado por Docker que vive **fuera** del ciclo de vida
del contenedor. En este stack hay cuatro:

| Volumen | Qué guarda | Si lo perdés |
|---|---|---|
| `siga_db_data` | La base de datos entera | Perdiste todo |
| `siga_uploads` | Imágenes de productos | Fichas sin foto |
| `caddy_data` | Los certificados TLS | Se reemiten solos |
| `caddy_config` | Estado interno de Caddy | Irrelevante |

```yaml
volumes:
  - siga_db_data:/var/lib/postgresql/data
```

Se lee: "montá el volumen `siga_db_data` en esa ruta de adentro del contenedor". Postgres
escribe ahí creyendo que es un directorio normal.

Esto es lo que hace seguro el ciclo de actualización: `docker compose up -d --build`
**destruye y recrea contenedores**, y los datos igual siguen ahí.

> ⚠️ `docker compose down -v` sí borra los volúmenes. Esa `-v` es la diferencia entre
> reiniciar el stack y perder la base. En producción, nunca.

### Redes: por qué `db` no necesita contraseña fuerte... y sin embargo la tiene

Los servicios de la misma red de Docker **se encuentran por su nombre**. Por eso la
cadena de conexión de la API dice `Host=db`:

```yaml
ConnectionStrings__DefaultConnection: "Host=db;Port=5432;..."
```

No hay IPs escritas a mano: `db` es un nombre DNS que Docker resuelve dentro de
`siga_net`. Y `Port=5432` es el puerto *interno*, que existe solo dentro de esa red.

Contrastá con `docker-compose.yml` (el de desarrollo), que sí publica:

```yaml
ports:
  - "5433:5432"     # host:contenedor
```

Eso significa "el 5433 de mi máquina llega al 5432 del contenedor" — cómodo para
conectarte con un cliente SQL en local. **El de producción no tiene esa línea a
propósito.** Postgres escucha, pero solo lo alcanzan los contenedores vecinos.

La contraseña igual importa: protege contra cualquier *otro* contenedor de la misma red,
que es la defensa en profundidad de siempre.

### Healthcheck: la diferencia entre "arrancó" y "está listo"

Un contenedor puede estar corriendo y todavía no servir para nada. En SIGA eso es
literal: `Program.cs` corre las migraciones y el seed **después** de configurar las rutas
y **antes** de `app.Run()`. Durante ese rato el proceso vive, pero no escucha.

```yaml
healthcheck:
  test: ["CMD-SHELL", "curl -s -o /dev/null http://localhost:8080/ || exit 1"]
  start_period: 120s
```

Como el puerto no acepta conexiones hasta que el seed terminó, que conteste **cualquier
cosa** (incluido un 404) ya prueba que está lista. Por eso no hace falta un endpoint
`/health`: la pregunta no es "¿está sana la lógica?" sino "¿está escuchando?".

`start_period: 120s` es la ventana de gracia: los fallos ahí adentro no cuentan como
caídas, porque el primer arranque tarda.

Y esto es lo que lo vuelve útil de verdad:

```yaml
caddy:
  depends_on:
    api:
      condition: service_healthy
```

Sin la `condition`, `depends_on` solo garantiza **orden de arranque** — Caddy salía
primero y devolvía 502 mientras la API migraba. Con ella, espera.

### Docker Compose: el archivo es la infraestructura

`docker-compose.prod.yml` describe el estado deseado del sistema entero. `docker compose
up -d` lo hace realidad; `-d` (*detached*) lo deja corriendo en segundo plano.

Que esté versionado en git es el punto: el servidor no tiene configuración secreta que
solo vos conozcas. Se reconstruye desde el repo más el `.env`.

### Por qué el `.env` no se versiona (y el `.env.example` sí)

`.env` tiene los secretos reales: `JWT_SECRET`, la contraseña de Postgres, la del admin.
Está en `.gitignore`.

`.env.example` es la **plantilla sin valores**, y sí se versiona — con una excepción
explícita en `.gitignore:33` (`!.env.example`). Cumple dos funciones: te dice qué hay que
completar, y deja registrado qué variables exige el compose. Si algún día agregás una
variable nueva al compose y no la agregás acá, el próximo despliegue falla sin explicar
por qué.

---

## Parte 2 — Caddy, el proxy inverso

### Qué problema resuelve

Un servidor tiene un solo puerto 443. Vos tenés varias apps. El **proxy inverso** es el
único que escucha ahí afuera y reparte hacia adentro según lo que pida el navegador.

En el `Caddyfile` de este repo el reparto es por ruta:

```
@backend path /api/* /uploads/*
handle @backend {
    reverse_proxy api:8080
}
handle {
    reverse_proxy web:80
}
```

Todo lo que empiece con `/api/` o `/uploads/` va al .NET; el resto, a la SPA.

**Esto no es un detalle estético.** `SIGA-Web/src/api/http.ts` tiene `baseURL: ""`, o sea
que el front pide a **su mismo origen**. Como Caddy sirve front y API bajo el mismo
dominio, el navegador nunca ve una petición cruzada y **CORS no existe como problema**.
Si separaras el front y la API en dominios distintos, tendrías que tocar ese código y
configurar CORS en el backend.

Ese mismo Caddy puede servir después a GQG y a la API de divisas: se agrega un bloque por
dominio y se comparte el servidor.

### HTTPS automático

Caddy pide el certificado a Let's Encrypt solo, por el protocolo ACME, y lo renueva antes
de que venza. Lo guarda en el volumen `caddy_data`.

Para probar que el dominio es tuyo, Let's Encrypt te hace una petición **a ese dominio**.
De ahí sale la regla que la guía repite:

> **El registro DNS tiene que estar propagado antes de levantar el stack.** Si el dominio
> todavía no resuelve a la IP de la VPS, la validación falla y no hay certificado.

Y de ahí también el `ACME_EMAIL`: es donde Let's Encrypt avisa si una renovación viene
fallando.

### Los encabezados reenviados

Caddy termina el TLS: hacia adentro habla HTTP plano. La API, entonces, ve una petición
`http://` y —si nadie le avisa— podría redirigir a HTTPS en un bucle infinito.

Eso lo resuelve esta línea del compose:

```yaml
ASPNETCORE_FORWARDEDHEADERS_ENABLED: "true"
```

Con eso ASP.NET lee el encabezado `X-Forwarded-Proto: https` que Caddy manda, entiende
que el usuario **sí** vino por HTTPS, y `app.UseHttpsRedirection()` no redirige.

---

## Parte 3 — SSH

### Contraseña contra clave

Una contraseña es un secreto **compartido**: vos la sabés y el servidor también. Viaja en
cada login, se puede adivinar, y una IP pública recibe intentos automatizados a las pocas
horas de existir.

Un par de claves es asimétrico:

- **Clave privada** (`~/.ssh/id_ed25519`): vive en tu PC y no sale nunca de ahí.
- **Clave pública** (`~/.ssh/id_ed25519.pub`): se copia al servidor. Es pública, no
  importa quién la vea.

Al conectarte, el servidor manda un desafío que **solo** se puede responder con la
privada. La privada nunca viaja. No hay nada que interceptar y nada que adivinar por
fuerza bruta.

`ssh-copy-id` no hace magia: agrega una línea a `~/.ssh/authorized_keys` del servidor.

### El endurecimiento, y por qué en ese orden

Tener clave no alcanza mientras el servidor **siga aceptando** contraseñas: el atacante
elige el método más débil. Por eso:

```
PasswordAuthentication no
KbdInteractiveAuthentication no
PermitRootLogin no
```

La tercera es aparte: `root` existe en todos los servidores del mundo, así que es el
usuario que todo ataque prueba primero. Trabajar como `deploy` y escalar con `sudo` te da
además un registro de quién hizo qué.

**El orden de la guía es lo importante:** primero copiar la clave, después *comprobar en
otra terminal* que entrás con ella, y recién entonces desactivar contraseñas — sin cerrar
la sesión que ya tenías abierta. Si desactivás primero y la clave estaba mal copiada, te
quedaste afuera de tu propio servidor y hay que entrar por la consola de rescate del
proveedor.

Por lo mismo:

```bash
sudo sshd -t && sudo systemctl restart ssh
```

`sshd -t` valida la sintaxis. Si está rota, el `&&` corta y el servicio **no** se
reinicia: seguís adentro, con tiempo de arreglarlo.

### El firewall

```bash
ufw allow OpenSSH
ufw allow 80/tcp
ufw allow 443/tcp
ufw --force enable
```

UFW pasa a denegar todo lo que no esté en esa lista. Es la segunda capa: aunque un día
alguien agregue un `ports:` de más en el compose, el firewall lo sigue tapando.

El 80 hace falta aunque todo sea HTTPS: es donde Let's Encrypt valida el dominio, y donde
Caddy redirige a HTTPS a quien llega por HTTP.

---

## El ciclo de actualización, releído

Ahora estos comandos deberían explicarse solos:

```bash
cd ~/siga/SIGA     && git pull      # traer el código nuevo
cd ~/siga/SIGA-Web && git pull
cd ~/siga/SIGA     && docker compose -f docker-compose.prod.yml up -d --build
```

`--build` reconstruye las imágenes (reusando las capas que no cambiaron), Compose
reemplaza los contenedores cuya imagen cambió, **los volúmenes siguen intactos**, y la
API aplica las migraciones nuevas al arrancar mientras Caddy espera su healthcheck.

Lo único que no está en git es el `.env`. Guardalo aparte, en un gestor de contraseñas.
