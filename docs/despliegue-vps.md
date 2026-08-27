# Despliegue de SIGA en VPS (Contabo · Ubuntu 24.04)

Guía paso a paso para desplegar SIGA (API .NET + frontend Vue + PostgreSQL) en una
VPS de Contabo con **HTTPS automático** vía Caddy.

> Esta guía es operativa: dice qué correr. Si querés entender **qué hace cada pieza y por
> qué** — imágenes y contenedores, volúmenes, redes, healthchecks, proxy inverso, TLS
> automático y claves SSH — leé [conceptos-despliegue.md](conceptos-despliegue.md).

**Arquitectura:**

```
Internet ──443──► Caddy (TLS Let's Encrypt)
                   ├── /api/*, /uploads/*  ──► api  (.NET, :8080 interno)
                   └── resto               ──► web  (nginx + SPA, interno)
                                                 api ──► db (postgres, interno + volumen)
```

La API y la base de datos **no se exponen** a internet: solo se alcanzan por la red
interna de Docker, a través de Caddy.

---

## 0. Requisitos previos

- VPS de Contabo con **Ubuntu 24.04** y su IP pública (ej. `203.0.113.10`).
- Un **dominio** (ej. `siga.tudominio.com`).
- Acceso SSH como `root` (Contabo manda la contraseña por email al crear la VPS).

---

## 1. Apuntar el dominio a la VPS (DNS)

En el panel de tu proveedor de dominio, creá un registro **A**:

| Tipo | Nombre              | Valor (IP)     | TTL  |
|------|---------------------|----------------|------|
| A    | `siga` (o `@`)      | `203.0.113.10` | 3600 |

Verificá la propagación (puede tardar minutos/horas):

```bash
nslookup siga.tudominio.com
```

> Caddy **no podrá emitir el certificado** hasta que el dominio resuelva a la IP de la VPS.

---

## 2. Conectarse y asegurar el servidor

Desde tu PC (PowerShell en Windows ya trae `ssh`):

```bash
ssh root@203.0.113.10
```

Actualizar el sistema y crear un usuario no-root con sudo:

```bash
apt update && apt upgrade -y

adduser deploy                 # elegí una contraseña
usermod -aG sudo deploy
```

Configurar el firewall (UFW) — solo SSH y web:

```bash
ufw allow OpenSSH
ufw allow 80/tcp
ufw allow 443/tcp
ufw --force enable
ufw status
```

### Acceso por clave (no es opcional)

Una IP pública recibe intentos de login automatizados a las pocas horas de
existir. Mientras SSH acepte contraseñas, la seguridad del servidor es la de la
contraseña de `deploy`. Con clave, el ataque por fuerza bruta deja de aplicar.

Desde tu PC, generar la clave (una sola vez en la vida) y copiarla:

```bash
ssh-keygen -t ed25519          # si todavía no tenés ~/.ssh/id_ed25519.pub
ssh-copy-id deploy@203.0.113.10
```

**Antes de seguir, comprobá que la clave funciona.** Abrí una segunda terminal y
entrá sin que te pida contraseña:

```bash
ssh deploy@203.0.113.10
```

> ⚠️ No cierres la sesión que ya tenés abierta hasta terminar este paso. Si algo
> sale mal más abajo, esa sesión es tu única forma de volver a entrar.

Recién ahora, ya dentro como `deploy`, desactivar contraseñas y login de root:

```bash
sudo tee /etc/ssh/sshd_config.d/99-hardening.conf > /dev/null <<'CONF'
PasswordAuthentication no
KbdInteractiveAuthentication no
PermitRootLogin no
CONF

sudo sshd -t && sudo systemctl restart ssh
```

`sshd -t` valida la configuración antes de reiniciar: si hay un error de
sintaxis, el servicio no se reinicia y no te quedás afuera.

Verificá desde una **tercera** terminal que seguís entrando, y que la contraseña
efectivamente ya no se acepta:

```bash
ssh deploy@203.0.113.10                                    # entra
ssh -o PreferredAuthentications=password root@203.0.113.10 # debe rechazar
```

Dos extras baratos, que valen para cualquier servidor expuesto:

```bash
sudo apt install -y fail2ban unattended-upgrades   # bloqueo de IPs y parches automáticos
sudo dpkg-reconfigure -plow unattended-upgrades
```

El resto de la guía se hace como `deploy`.

---

## 3. Instalar Docker + Docker Compose

```bash
# Dependencias
sudo apt update
sudo apt install -y ca-certificates curl git

# Repositorio oficial de Docker
sudo install -m 0755 -d /etc/apt/keyrings
sudo curl -fsSL https://download.docker.com/linux/ubuntu/gpg -o /etc/apt/keyrings/docker.asc
sudo chmod a+r /etc/apt/keyrings/docker.asc
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] \
  https://download.docker.com/linux/ubuntu $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | \
  sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

sudo apt update
sudo apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

# Usar docker sin sudo (cerrá y reabrí la sesión SSH después de esto)
sudo usermod -aG docker $USER
```

Cerrá la sesión SSH y volvé a entrar para que tome el grupo `docker`. Verificá:

```bash
docker --version
docker compose version
docker run --rm hello-world
```

---

## 4. Clonar los repositorios

El `docker-compose.prod.yml` espera **ambos repos como hermanos** dentro de una carpeta:

```bash
mkdir -p ~/siga && cd ~/siga
git clone https://github.com/mmelgarejogit/SIGA.git
git clone https://github.com/mmelgarejogit/SIGA-Web.git
```

> **Si los repos son privados**, git va a pedir credenciales. Usá un **Personal Access
> Token** de GitHub como contraseña (Settings → Developer settings → Tokens, scope `repo`),
> o configurá una *deploy key* SSH. Para clonar por HTTPS con token:
> `git clone https://<TOKEN>@github.com/mmelgarejogit/SIGA.git`

> Ambos repos despliegan desde `master`, que es la rama por defecto: el trabajo
> que vivía en `matias-gaona` ya está mergeado ahí. No hace falta cambiar de rama.

Estructura resultante:

```
~/siga/
├── SIGA/        ← compose, Caddyfile, .env
└── SIGA-Web/
```

---

## 5. Configurar las variables de entorno

```bash
cd ~/siga/SIGA
cp .env.example .env
nano .env
```

Completá **todos** los valores. Para generar secretos fuertes:

```bash
openssl rand -base64 32   # JWT_SECRET
openssl rand -base64 24   # POSTGRES_PASSWORD
```

Imprescindibles: `DOMAIN`, `ACME_EMAIL`, `POSTGRES_PASSWORD`, `JWT_SECRET`,
`SEED_ADMIN_EMAIL`, `SEED_ADMIN_PASSWORD`.

> El admin se crea solo en el primer arranque con `SEED_ADMIN_EMAIL` /
> `SEED_ADMIN_PASSWORD`. **Cambiá la contraseña desde la app tras el primer login.**

---

## 6. Construir y levantar el stack

```bash
cd ~/siga/SIGA
docker compose -f docker-compose.prod.yml up -d --build
```

La primera vez tarda varios minutos (compila .NET y el front). Seguir el progreso:

```bash
docker compose -f docker-compose.prod.yml logs -f
```

Qué pasa al arrancar:
1. `db` levanta Postgres (volumen persistente).
2. `api` corre las **migraciones** automáticamente y siembra roles, permisos,
   catálogo base y el **usuario admin**.
3. `web` sirve la SPA.
4. `caddy` pide el certificado TLS a Let's Encrypt para tu dominio.

---

## 7. Verificar

```bash
docker compose -f docker-compose.prod.yml ps          # todos "running"/"healthy"
docker compose -f docker-compose.prod.yml logs caddy  # buscar "certificate obtained"
```

La `api` arranca en `health: starting` y pasa a `healthy` recién cuando termina
de correr las migraciones y el seed — en el primer arranque eso puede tardar un
par de minutos. Caddy espera ese estado antes de levantar, así que **un 502 en
ese rato es lo esperado, no un error**. Si después de unos minutos sigue en
`starting`, ahí sí mirá `logs api`.

En el navegador: `https://siga.tudominio.com` → candado verde y pantalla de login.
Entrá con el `SEED_ADMIN_EMAIL` / `SEED_ADMIN_PASSWORD` que pusiste.

Chequeo rápido de la API desde la VPS:

```bash
curl -s -o /dev/null -w "%{http_code}\n" https://siga.tudominio.com/api/auth/login
# 400/405 está OK (responde); 502 = la API no está arriba
```

---

## 8. Operación día a día

**Actualizar (deploy de cambios nuevos):**

```bash
cd ~/siga/SIGA      && git pull
cd ~/siga/SIGA-Web  && git pull
cd ~/siga/SIGA      && docker compose -f docker-compose.prod.yml up -d --build
```

(Las migraciones nuevas se aplican solas al reiniciar la `api`.)

**Logs / estado:**

```bash
docker compose -f docker-compose.prod.yml logs -f api
docker compose -f docker-compose.prod.yml restart api
docker compose -f docker-compose.prod.yml ps
```

**Backup de la base de datos:**

```bash
cd ~/siga/SIGA
docker compose -f docker-compose.prod.yml exec -T db \
  pg_dump -U "$(grep POSTGRES_USER .env | cut -d= -f2)" \
          "$(grep POSTGRES_DB .env | cut -d= -f2)" \
  > ~/siga/backup-$(date +%F).sql
```

> Recomendado: automatizar con `cron` y subir el dump a un bucket/almacenamiento externo.

**Restaurar un backup:**

```bash
cat ~/siga/backup-AAAA-MM-DD.sql | docker compose -f docker-compose.prod.yml exec -T db \
  psql -U "$(grep POSTGRES_USER .env | cut -d= -f2)" \
       "$(grep POSTGRES_DB .env | cut -d= -f2)"
```

**Backup de imágenes subidas** (volumen `siga_uploads`):

```bash
docker run --rm -v siga_siga_uploads:/data -v ~/siga:/backup alpine \
  tar czf /backup/uploads-$(date +%F).tar.gz -C /data .
```

> El nombre real del volumen suele prefijarse con el del proyecto (carpeta `SIGA` →
> `siga_siga_uploads`). Confirmá con `docker volume ls`.

---

## 9. Notas y pendientes

- **Secretos de dev**: el `JWT_SECRET`, password de Neon y API key de Resend que están
  en `appsettings.Development.json` (local, no versionado) NO se usan en prod. Conviene
  **rotarlos** si alguna vez se compartieron.
- **Emails (Resend)**: con `onboarding@resend.dev` los correos solo llegan a tu propia
  casilla. Para producción real, verificá un dominio en Resend y actualizá
  `RESEND_FROM_EMAIL`.
- **hCaptcha**: las claves por defecto son de prueba (siempre validan). Solo afecta al
  registro público de pacientes, no al login. Para captcha real, creá un sitio en
  hCaptcha y reemplazá `VITE_HCAPTCHA_SITE_KEY` (front) y `HCAPTCHA_SECRET` (back).
- **`www` / dominio raíz**: si querés cubrir también `www.` o el apex, agregá esos
  nombres al bloque del `Caddyfile` (Caddy gestiona los certificados de cada uno).
