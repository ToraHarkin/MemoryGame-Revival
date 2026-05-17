Endpoints del Módulo de Moderación
El controlador `ModerationController` (en `MemoryGame.API/Controllers/`) provee operaciones para reportar conducta inapropiada de otros jugadores. La lógica de aplicación correspondiente vive en `MemoryGame.Application/Moderation/`, siguiendo el mismo patrón CQRS con MediatR que el resto del API.

Rutas
Todas las rutas viven bajo el prefijo `api/moderation` y requieren autenticación. El identificador del usuario que reporta se obtiene del claim `NameIdentifier`, no del cuerpo del request, para evitar suplantaciones.

POST /api/moderation/report
- **Descripción**: Reporta a un usuario por mala conducta durante una partida.
- **Autorización**: Requiere `[Authorize]`.
- **Body**:
  ```json
  {
    "TargetUserId": 17,
    "MatchId": 42
  }
  ```
- **Respuesta exitosa (204 NoContent)**: El reporte fue aceptado y persistido.
- **Errores comunes**:
  - `400 Bad Request`: Validación fallida (por ejemplo, `TargetUserId == ReporterId`, o `MatchId` no existe).
  - `409 Conflict`: El usuario ya había reportado al mismo jugador por la misma partida.
  - `401 Unauthorized`: Token ausente o inválido.

Capa de Aplicación
El comando `ReportUserCommand(reporterId, targetUserId, matchId)` se resuelve por `ReportUserCommandHandler` en `MemoryGame.Application/Moderation/Commands/ReportUser/`. El handler:
1. Valida que el reportante y el reportado sean usuarios reales y distintos.
2. Verifica que ambos hayan participado en la partida indicada.
3. Inserta un registro `UserReport` con timestamp y razón pendiente de revisión.

Penalizaciones
El módulo de moderación se integra con la entidad `Penalty` (en `MemoryGame.Domain/Penalties/`) y su repositorio `IPenaltyRepository`. Cuando un usuario acumula cierto número de reportes en una ventana de tiempo, una rutina administrativa puede aplicar una penalización (mute temporal, suspensión, etc.). La aplicación de penalizaciones automáticas no está expuesta en este endpoint; solo el registro de reportes.

Validación
El `ReportUserCommandValidator` (FluentValidation) impone:
- `TargetUserId > 0`
- `MatchId > 0`
- `TargetUserId != ReporterId` (no se puede uno auto-reportar)

Pendientes
- Endpoint para que un administrador consulte los reportes pendientes (`GET /api/moderation/reports`).
- Endpoint para aplicar/revertir penalizaciones desde un panel administrativo.
- Filtro de palabras en el chat de lobby para que ciertos abusos se detecten automáticamente sin requerir reporte manual.
