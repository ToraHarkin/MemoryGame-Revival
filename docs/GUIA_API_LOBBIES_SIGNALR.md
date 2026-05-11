Integración del Hub SignalR para Lobbies
El servidor expone un hub de SignalR (`GameLobbyHub`, ubicado en `MemoryGame.API/Hubs/`) que reemplaza el antiguo `IGameLobbyCallback` del proyecto WCF original. El hub centraliza el ciclo de vida de los lobbies, el chat de partida, la mecánica de juego (turnos, volteo de cartas, marcador), las votaciones para expulsar jugadores, y las invitaciones a amigos.

Autenticación
El hub está decorado con `[Authorize]`. La conexión del cliente debe incluir el `accessToken` JWT en el query string (parámetro `access_token`) o en el header `Authorization` durante el handshake. Los claims relevantes (`NameIdentifier`, `username`, `isGuest`) se extraen del contexto del usuario para construir el `LobbyPlayer` asociado a cada conexión.

URL del Hub
- `ws://<host>:5000/hubs/gamelobby` (sin TLS, entorno de desarrollo)
- `wss://<host>/hubs/gamelobby` (producción)

Métodos del Servidor (invocados por el cliente)
- `CreateLobby(gameCode, isPublic)`: Crea un lobby nuevo y registra al llamador como host. Si el código ya existe, retorna `Error: LOBBY_CODE_TAKEN`.
- `JoinLobby(gameCode)`: Une al jugador a un lobby existente. Posibles errores: `LOBBY_NOT_FOUND`, `LOBBY_GAME_IN_PROGRESS`, `LOBBY_FULL`.
- `LeaveLobby()`: Saca al jugador del lobby actual. Si el lobby queda vacío, se destruye.
- `SendChatMessage(message)`: Difunde un mensaje de chat al resto del grupo. Mensajes vacíos o de más de 500 caracteres se descartan silenciosamente.
- `StartGame(settings)`: Solo el host puede invocarlo. Valida que haya al menos 2 jugadores, que `CardCount` sea par y esté entre 4 y 36, y que `TurnTimeSeconds` esté entre 5 y 120.
- `FlipCard(cardIndex)`: Voltea una carta del tablero. La detección de pareja se maneja en el servidor con un retraso visual de 800 ms para dar tiempo al jugador de ver ambas cartas.
- `VoteToKick(targetUsername)`: Vota para expulsar a un jugador. Si se alcanza la mayoría, el objetivo es removido del lobby.
- `GetPublicLobbies()`: Devuelve los lobbies públicos no llenos que no están en partida.
- `InviteFriend(targetUserId)`: Si el amigo está en línea, se le envía una notificación en tiempo real. En caso contrario, se manda un correo de invitación.

Eventos del Cliente (callbacks que el cliente debe registrar)
- `UpdatePlayerList(players)`: Lista actualizada de jugadores en el lobby.
- `PlayerJoined(username, isGuest)`: Notifica el ingreso de un jugador nuevo.
- `PlayerLeft(username)`: Notifica la salida de un jugador.
- `LobbyCreated(gameCode)`: Confirma que el lobby fue creado exitosamente.
- `GameStarted(board)`: Inicia la partida y entrega el tablero (cartas sin revelar).
- `UpdateTurn(currentPlayer, turnTimeSeconds)`: Indica de quién es el turno y cuánto tiempo tiene.
- `ShowCard(index, imageIdentifier)`: Revela la carta volteada por algún jugador.
- `SetCardsAsMatched(indexA, indexB)`: Confirma que las dos cartas formaron pareja.
- `HideCards(indexA, indexB)`: Vuelve a esconder las cartas si no formaron pareja.
- `UpdateScore(username, score)`: Actualiza el marcador de un jugador.
- `GameFinished(winnerUsername)`: La partida terminó.
- `Kicked`: El llamador fue expulsado del lobby.
- `ReceiveChatMessage(sender, message, isSystem)`: Mensaje de chat recibido.
- `LobbyInviteReceived(fromUsername, gameCode)`: Llegó una invitación de un amigo.
- `LobbyInviteSent(targetUsername, deliveredInRealTime)`: Confirmación del envío de la invitación.
- `PublicLobbiesList(lobbies)`: Respuesta a `GetPublicLobbies`.
- `Error(code)`: Código de error legible en mayúsculas con prefijo `LOBBY_` o `USER_`.

Presencia y Desconexiones
El hub mantiene un `IPresenceTracker` que asocia `userId` ↔ `connectionId` mientras la conexión está abierta. Esto permite que `InviteFriend` resuelva si el destinatario está en línea y enrutar la invitación en tiempo real. En el evento `OnDisconnectedAsync`, si el jugador estaba en un lobby se remueve, se notifica al grupo, y si el lobby queda vacío se libera.
