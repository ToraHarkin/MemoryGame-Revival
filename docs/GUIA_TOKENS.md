 Guía de Tokens JWT (JSON Web Tokens)
Para la verificación de identidad del sistema se utiliza JWT (JSON Web Tokens) mediante el uso del MiddleWare nativo JwtBearerDefaults.

Emisores y Reglas de Seguridad
El marco base está configurado en el archivo Program.cs y los parámetros principales se alimentan de variables de entorno estables (IConfiguration):

JWT_SECRET: Llave simétrica fuerte mediante la cual se firman las huellas de autenticidad para afirmar que el servidor fue quien envió la llave.
JWT_ISSUER / AUDIENCE: Determinan al originador y a la audiencia permitida ("memorygame-api" y "memorygame-client") dificultando el envenenamiento o falsificación de tokens.
ClockSkew = TimeSpan.Zero: Corta inmediatamente el acceso tras la caducidad del token, evadiendo los minutos extra de margen (gracia) que ASP.NET añade por defecto.
Implementación del Gestor de Tokens (JwtService)
El servicio JwtService (MemoryGame.Infrastructure/Services/JwtService.cs) encapsula la creación de JWT con tres responsabilidades principales:

Creación de Access Tokens: Retorna un texto firmado en SHA256 (HmacSha256) cada que el usuario hace un "Login" o "Refresh" exitoso. El servicio automáticamente inyecta los siguientes reclamos (Claims) vitales dentro del Token, permitiéndole a la API saber el estado del dispositivo al desencriptarlo:
Sub (Subject): El identificador único del usuario (user.Id).
Email: El correo del usuario.
Username: El apodo del usuario.
isGuest: Distingue encriptadamente si el usuario juega como invitado.
Jti (JWT ID): Un identificador probabilísticamente único generado con Guid en caso de requerir revocaciones granulares o rastreo.
Creación de Refresh Tokens: Dado que los Access Tokens expiran rápidamente (normalmente para seguridad anti secuestros), se incluye un Refresh Token. JwtService crea un arreglo seguro usando RandomNumberGenerator con 64 bytes de autenticidad criptográfica codificados a base 64.
Extracción sin Validación Crítica: GetUserIdFromToken() es una función especial que obtiene el id del usuario (sub) ignorando momentáneamente si el Access Token se ha vencido (ValidateLifetime = false). Esto está modelado exclusivamente para que si se vencen los privilegios ordinarios del cliente, este retorne tanto su token viejo como el especial Refresh Token a cambio de unos completamente nuevos de forma silenciosa.

