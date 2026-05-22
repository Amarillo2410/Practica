# Desarrollo local (API)

## Requisitos

- .NET 10 SDK
- PostgreSQL en `localhost:5432`

## Arrancar la API

La contraseña de Postgres en `appsettings.json` puede no coincidir con tu instalación.
Sobrescribela con la variable de entorno `POSTGRES_PASSWORD`:

```bash
# Bash (Git Bash / WSL)
export POSTGRES_PASSWORD="tu_password_de_postgres"
cd Api
dotnet run
```

```powershell
# PowerShell
$env:POSTGRES_PASSWORD = "tu_password_de_postgres"
cd Api
dotnet run
```

La API queda en **http://localhost:5152**.

## Verificación por email (código de 6 dígitos)

Sin SMTP configurado, el código se guarda en `Api/logs/verification-emails.log` y en desarrollo también se muestra en pantalla.

Para enviar al correo real con **Gmail**:

1. Activa verificación en 2 pasos en tu cuenta Google.
2. Crea una **contraseña de aplicación** para “Correo”.
3. Define variables antes de `dotnet run`:

```bash
export Email__Smtp__UserName="tom.pradamd@gmail.com"
export Email__Smtp__Password="contraseña_de_aplicacion"
export Email__Smtp__FromEmail="tom.pradamd@gmail.com"
```

En PowerShell:

```powershell
$env:Email__Smtp__UserName = "tom.pradamd@gmail.com"
$env:Email__Smtp__Password = "contraseña_de_aplicacion"
$env:Email__Smtp__FromEmail = "tom.pradamd@gmail.com"
```

La cuenta remitente por defecto en desarrollo es **tom.pradamd@gmail.com** (`appsettings.Development.json`). Solo falta la contraseña de aplicación de Gmail en `Email__Smtp__Password`.

`Email:Provider` puede ser `Auto` (usa Resend si está configurado, si no SMTP, si no log local).

## Google OAuth

El `ClientId` debe ser el mismo en:

- `Api/appsettings.Development.json` → `Authentication:Google:ClientId`
- `FrontendAuthenticationLinkedin/.env` → `VITE_GOOGLE_CLIENT_ID`

En [Google Cloud Console](https://console.cloud.google.com/) agrega en **Authorized JavaScript origins**:

- `http://localhost:3000`
- `http://localhost:4173`
