Arquitectura del Servidor
El proyecto `MemoryGame.Server` está organizado siguiendo los principios de Clean Architecture y dividido en cuatro capas independientes, con dependencias que siempre apuntan hacia adentro (la capa de dominio no depende de ninguna otra, mientras que la capa de infraestructura depende de aplicación y de dominio).

Capas
1. **MemoryGame.Domain**: contiene las entidades del negocio (User, Match, Card, Friendship, Penalty), los value objects (Email, Score), las interfaces de repositorios y las excepciones de dominio. No depende de ninguna otra capa ni de frameworks externos. Es la única que sobreviviría sin cambios si mañana decidimos cambiar de base de datos, framework web o protocolo de transporte.
2. **MemoryGame.Application**: orquesta los casos de uso siguiendo CQRS con MediatR. Cada caso de uso es un `Command` o `Query` con su `Handler`, su `Validator` (FluentValidation), y sus DTOs. Define interfaces propias de servicios externos (`IEmailService`, `IJwtService`, `ILobbyManager`) que la infraestructura implementa.
3. **MemoryGame.Infrastructure**: contiene las implementaciones concretas: `MemoryGameDbContext` (EF Core), los repositorios de cada entidad, `JwtService`, `EmailService` (SMTP), `LobbyManager` (en memoria con `ConcurrentDictionary`), `PresenceTracker` y migraciones.
4. **MemoryGame.API**: la capa de presentación HTTP/WebSocket. Controladores REST delgados (`AuthController`, `ProfileController`, `SocialController`, `MatchesController`, `ModerationController`) y un hub SignalR (`GameLobbyHub`). El `Program.cs` cablea todo con DI, configura JWT, CORS, Swagger, y los pipelines de MediatR.

Pipeline de MediatR
Los handlers se ejecutan dentro de un pipeline configurado en `MemoryGame.Application/DependencyInjection.cs`. Antes de llegar al handler, cada request pasa por:
1. `ValidationBehavior`: corre todos los `IValidator<TRequest>` registrados. Si alguno falla, lanza `ValidationException` (capturada por el middleware HTTP global y traducida a `400 Bad Request` con detalle por campo).
2. (Pendiente) `LoggingBehavior`: registrará en logs la duración y el resultado de cada comando, útil para diagnóstico.

Middleware
Bajo `MemoryGame.API/Middleware/` se concentran:
- `ExceptionHandlingMiddleware`: convierte excepciones de dominio (`DomainException`, `ValidationException`, `NotFoundException`) a respuestas HTTP estandarizadas con `{ "error": "..." }`.
- `JwtRefreshMiddleware`: detecta access tokens cercanos a expirar y emite un header `X-Token-Refresh-Suggested` para que el cliente solicite renovación con su refresh token.

Persistencia
EF Core con provider PostgreSQL. La cadena de conexión se lee del archivo `appsettings.json` o, en su defecto, de la variable de entorno `DATABASE_CONNECTION_STRING`. Las migraciones se generan con `dotnet ef migrations add <Name>` y se aplican automáticamente en arranque cuando `RunMigrationsOnStartup=true`.

Tests
El proyecto de tests está dividido en tres ensamblados (`MemoryGame.Domain.Tests`, `MemoryGame.Application.Tests`, `MemoryGame.Infrastructure.Tests`) usando MSTest. La cobertura actual está concentrada en `Domain` (validaciones de invariantes de entidades y value objects) y en los handlers de `Application` que tocan la lógica de lobbies. Los tests de infraestructura usan SQLite en memoria para verificar mappings de EF Core sin depender de PostgreSQL.

Cliente WPF
El cliente está desacoplado del servidor por dos protocolos: HTTP REST para operaciones puntuales (autenticación, perfil, social, historial de partidas) y SignalR para todo lo de tiempo real (lobby, chat, gameplay). La capa `Services/Network/` del cliente encapsula ambos transportes para que ViewModels solo dependan de interfaces (`ILobbyService`, `IGameService`, etc.).
