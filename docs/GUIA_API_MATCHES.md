Endpoints del Módulo de Partidas
El controlador `MatchesController` (en `MemoryGame.API/Controllers/`) expone consultas relacionadas con el historial de partidas del usuario autenticado. Como el resto del API, sigue el patrón CQRS apoyado en MediatR: el controlador no contiene lógica de negocio, simplemente despacha una `Query` hacia la capa `MemoryGame.Application`.

Rutas
Todas las rutas viven bajo el prefijo `api/matches` y requieren un access token válido. El identificador del usuario se obtiene del claim `NameIdentifier` del JWT, nunca del body o el query string.

GET /api/matches/history
- **Descripción**: Recupera el historial de partidas del usuario autenticado.
- **Autorización**: Requiere `[Authorize]`.
- **Parámetros**: Ninguno. El `userId` se infiere del token.
- **Respuesta exitosa (200)**: Lista de objetos `MatchHistoryEntry` ordenada por fecha descendente.
  ```json
  [
    {
      "matchId": 42,
      "playedAt": "2026-05-01T19:07:32Z",
      "result": "Win",
      "score": 18,
      "opponents": ["alice", "bob"],
      "duration": "00:04:32"
    }
  ]
  ```
- **Errores**:
  - `401 Unauthorized`: Token ausente, inválido o expirado.

Capa de Aplicación
El comando se despacha como `GetMatchHistoryQuery(userId)` y es resuelto por `GetMatchHistoryQueryHandler` en `MemoryGame.Application/Matches/Queries/`. El handler consulta el `IMatchRepository` (implementado por `MatchRepository` en `MemoryGame.Infrastructure/Repositories/`), agrega información de los participantes y los resultados, y proyecta el resultado a un DTO plano para que el controlador lo devuelva.

Persistencia
Las partidas se almacenan en las tablas `Matches` y `MatchParticipations` (relación uno-a-muchos). Cada participación incluye el `userId`, el `score` final y un flag de victoria. La query usa `Include` para traer la relación de participaciones y joins por `userId` para resolver los nombres de usuario.

Consideraciones de Diseño
- La paginación está pendiente. En esta versión se devuelve el historial completo del usuario; conforme crezca el volumen de partidas se planea agregar parámetros `take` y `skip` (o cursor) sin romper el contrato actual.
- El endpoint sólo expone partidas del usuario autenticado. Para ver el perfil público de otro jugador habrá un endpoint separado bajo `api/profile/{username}/matches`.
