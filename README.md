# Taller: Autenticación de Usuarios con JWT (Simulación LinkedIn)

## Objetivo

El objetivo de este taller es comprender cómo funciona la autenticación de usuarios mediante proveedores externos (como LinkedIn) y el uso de tokens JWT dentro de una arquitectura en capas (N-Layer).

En este caso, trabajarás sobre una **simulación de autenticación externa**, donde el backend recibe información de un proveedor (como si fuera LinkedIn), valida al usuario y genera un token que le permite acceder al sistema.

## Contexto 

En aplicacione modernas, es común permitir que los usuarios inicien sesión con servicios externos como Google, LinkedIn o Facebook. Este proceso implica:

\- Recibir datos del proveedor externo
\- Validar o crear el usuario en la base de datos
\- Generar un token JWT para mantener la sesión
\- Permitir la comunicación entre frontend y backend

En este proyecto, parte de esa lógica ha sido eliminada intencionalmente.

## Dinámica del Taller

Tu misión será **reconstruir la lógica faltante del sistema** completando ciertos archivos clave.

Estos archivos son importantes porque:

\- Controlan la autenticación del usuario
\- Gestionan la persistencia en base de datos
\- Generan tokens de seguridad (JWT)
\- Permiten la comunicación con el frontend

## Requisitos Previos

Antes de comenzar:

1. Clonar el repositorio:

```bash
git clone <url-del-repo>
```



## PASO 1 — Modelo de Usuario

Ruta:
Domain/Entities/Auth/User.cs

Agregar:

````
public AuthProvider AuthProvider { get; private set; }
public string? ProviderId { get; private set; }

Este código permite identificar proveedor y guardar ID externo.

Agregar:

  public User(

​    string email,

​    AuthProvider authProvider,

​    string? providerId,

​    bool isEmailVerified,

​    OnboardingStep onboardingStep)

  {

​    Id = Guid.NewGuid();

​    Email = NormalizeEmail(email);

​    AuthProvider = authProvider;

​    ProviderId = NormalizeOptional(providerId);

​    IsEmailVerified = isEmailVerified;

​    CurrentOnboardingStep = onboardingStep;

​    OnboardingComplete = onboardingStep == OnboardingStep.Completed;

​    Status = UserStatus.Active;

  }
````

se encarga de crear un nuevo usuario inicializando todos sus datos principales, como el correo, el proveedor de autenticación, el identificador externo del proveedor, el estado de verificación del email y el progreso dentro del sistema (onboarding). asignando automáticamente un ID único al usuario, dejando todo para que pueda utilizar la aplicación correctamente desde el momento en que se registra

---

##  PASO 2 — Login Externo

Ruta:
Application/UseCase/Auth/ExternalLogin/ExternalLoginHandler.cs

Copiar:

    private async Task<User> CreateExternalUserAsync(ExternalUserInfo externalUser, CancellationToken ct)
        {
            var user = new User(
                externalUser.Email,
                externalUser.Provider,
                externalUser.ProviderUserId,
                isEmailVerified: false,
                ResolveInitialOnboardingStep(externalUser));
    
            var profile = new UserProfile(
                user.Id,
                externalUser.FirstName,
                externalUser.LastName,
                externalUser.ProfilePictureUrl);
            profile.SetPublicProfileUrl(
                await BuildUniquePublicProfileUrlAsync(profile.PublicProfileUrl, user.Id, ct));
    
            user.SetProfile(profile);
            user.SetProfessionalInfo(new ProfessionalInfo(user.Id));
            user.SetJobPreferences(new JobPreferences(user.Id));
            user.SetSecurity(new UserSecurity(user.Id));
    
            await unitOfWork.Users.AddAsync(user, ct);
            return user;
        }
}

Este código valida, crea usuario y genera token.automatiza todo el proceso de registro de un usuario que inicia sesión mediante autenticación externa.

---

##  PASO 3 — JWT

Ruta:
Infrastructure/Services/Auth/JwtTokenService.cs

Copiar:

    public string GenerateAccessToken(User user)
        {
            var firstName = user.Profile?.FirstName ?? string.Empty;
            var lastName = user.Profile?.LastName ?? string.Empty;
    
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(JwtRegisteredClaimNames.GivenName, firstName),
                new(JwtRegisteredClaimNames.FamilyName, lastName),
                new("onboarding_step", user.CurrentOnboardingStep.ToString())
            };
    
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    
            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_options.DurationInMinutes),
                signingCredentials: creds);
    
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
}

Este método se encarga de generar el token JWT que identifica al usuario dentro del sistema. Toma los datos del usuario, como ID, correo y nombre, los convierte en “claims” (información incluida dentro del token), y luego crea un token firmado con una clave secreta y una fecha de expiración.

 Este token es el que permite que el usuario se mantenga autenticado y pueda acceder a los recursos protegidos de la aplicación.

---

##  PASO 4 — Repositorio

Ruta:
Infrastructure/Repositories/Auth/UserRepository.cs

Agregar:

````
 public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)

  {

​    var normalizedEmail = email.Trim().ToLowerInvariant();

​    return dbContext.Users

​      .AsTracking()

​      .Include(x => x.Profile)

​      .Include(x => x.ProfessionalInfo)

​      .Include(x => x.JobPreferences)

​      .Include(x => x.Security)

​      .FirstOrDefaultAsync(x => x.Email == normalizedEmail, ct);

}
````

Este método sirve para verificar si un usuario ya existe en el sistema antes de crearlo o permitirle iniciar sesión.



Cuando alguien intenta autenticarse (Google o LinkedIn), el sistema usa este método para buscar el usuario en la base de datos mediante su correo electrónico. Si el usuario existe, se reutiliza; si no, se crea uno nuevo

---

##  PASO 5 — Configuración

Ruta:
Api/Program.cs

Agregar:

````
builder.Services.AddCors(options =>

{

  var allowedOrigins = builder.Configuration

​    .GetSection("Cors:AllowedOrigins")

​    .Get<string[]>()

​    ?? [];

  options.AddPolicy(frontendCorsPolicy, policy =>

  {

​    policy

​      .WithOrigins(allowedOrigins)

​      .AllowAnyHeader()

​      .AllowAnyMethod();

  });

});

Activa autenticación JWT.


````



Activar:

```
app.UseCors(frontendCorsPolicy);
```



## Conclusion

Este taller permitió entender el flujo de autenticación en una aplicación moderna, reconstruyendo partes clave como la creación de usuarios, la generación de tokens JWT y la comunicación entre frontend y backend.
