Integración de la API REST
El proyecto de servidor está construido utilizando ASP.NET Core Web API y sigue principios de diseño muy limpios (Clean Architecture) combinados con el patrón CQRS (Command Query Responsibility Segregation) a través de la librería MediatR.

Controladores "Delgados" (Thin Controllers): El controlador AuthController (en MemoryGame.API/Controllers) actúa netamente como un enrutador. No contiene lógica de negocio; en su lugar, delega las tareas inyectando ISender y despachando comandos (Command) hacia la capa de aplicación (MemoryGame.Application).
Endpoints RESTful: Se exponen claramente los siguientes endpoints bajo el prefijo api/auth:
POST /register: Para iniciar un registro y generar un PIN de verificación.
POST /verify-registration: Para validar el PIN ingresado.
POST /finalize-registration: Para crear definitivamente la cuenta tras confirmar el registro.
POST /resend-verification: Para reintentar el envío de un código.
POST /login: Retorna los tokens de acceso tras interactuar con el comando de inicio de sesión.
POST /logout: Usa el atributo [Authorize] para asegurar la sesión, extrayendo el ID desde el claim (token JWT) para invalidarlo.
Gestión CORS: En Program.cs, CORS está bien configurado para permitir que clientes del lado web/juego consuman la API en base a Cors:AllowedOrigins definido en las variables de entorno.
Implementación y Configuración de Tokens JWT
La autenticación y autorización se apoyan exitosamente en validaciones estrictas y simétricas a través de JwtBearer.

Configuración del Middleware: En Program.cs, se inyecta el esquema de JWT con validación rigurosa de IssuerSigningKey, Audience, y Lifetime (Tiempo de caducidad). También se quita el desfase de tiempo (ClockSkew = TimeSpan.Zero), por lo que las expiraciones son exactas.
Generación de Tokens (JwtService):
Al crear un Access Token, se inyectan claims seguros: sub (Identificador del Usuario), email, username, un indicador de si es invitado (isGuest), y un identificador único para el token (jti).
Utiliza un algoritmo fuerte (HmacSha256) firmando la llave combinada por la variable de entorno JWT_SECRET.
Refresh Tokens: Se evidencia el uso de un modelo de Token Refreshment. GenerateRefreshToken emite una cadena segura en Base64 utilizando 64 bits de entropía pseudoaleatoria pura (RandomNumberGenerator), lo cual protege contra ataques de fuerza bruta en los tokens de refresco en la base de datos de sesiones (UserSession).
Recuperación Segura: La función GetUserIdFromToken extrae claims de id omitiendo la validación de la fecha de caducidad temporalmente, un patrón ideal para cuando se necesita regenerar credenciales a partir de un Access Token vencido y un Refresh Token válido.

