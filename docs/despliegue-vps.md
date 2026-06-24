# Despliegue de SIGA en VPS (Contabo · Ubuntu 24.04)

Guía paso a paso para desplegar SIGA (API .NET + frontend Vue + PostgreSQL) en una
VPS de Contabo con **HTTPS automático** vía Caddy.

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

(Opcional pero recomendado) copiar tu clave SSH para no usar contraseña:

```bash
# En tu PC (una vez):  ssh-keygen -t ed25519
ssh-copy-id deploy@203.0.113.10
```

Reconectarse como `deploy` para el resto de la guía:

```bash
ssh deploy@203.0.113.10
```

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

> El backend está en la rama `matias-gaona` por ahora. Si querés esa rama:
> `cd SIGA && git checkout matias-gaona && cd ..`

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
