Guía de Despliegue Local con Docker
El servidor se despliega como un par de contenedores con `docker compose`: la API en .NET y una base de datos PostgreSQL. El cliente WPF se ejecuta nativo en Windows y se conecta al servidor por HTTP/WebSocket.

Prerequisitos
- Docker Desktop 4.x o superior con WSL2 habilitado.
- .NET SDK 8.x para compilar el cliente (no es estrictamente necesario para correr el servidor).
- Visual Studio 2022 17.10+ o Rider para el cliente WPF.

Levantar el Servidor
Desde la raíz del repositorio:
```
docker compose up --build
```
Esto construye la imagen de la API a partir de `MemoryGame.Server/src/MemoryGame.API/Dockerfile`, arranca PostgreSQL con un volumen persistente para los datos, y aplica las migraciones de EF Core en el primer arranque.

Variables de Entorno
Configurar en un archivo `.env` en la raíz (hay un `.env.example` como plantilla). Variables relevantes:
- `DATABASE_CONNECTION_STRING`: cadena de conexión completa a Postgres. En el contexto de docker-compose, el host es `db` (no `localhost`).
- `JWT_SECRET`: llave simétrica (mínimo 32 caracteres) para firmar tokens. Si no se define, el servidor falla en arranque por seguridad.
- `JWT_ISSUER`, `JWT_AUDIENCE`: valores arbitrarios pero estables; deben coincidir entre emisor y verificador.
- `JWT_ACCESS_TOKEN_MINUTES`: duración del access token (default 15).
- `JWT_REFRESH_TOKEN_DAYS`: duración del refresh token (default 7).
- `SMTP_HOST`, `SMTP_PORT`, `SMTP_USER`, `SMTP_PASSWORD`, `SMTP_FROM`: configuración del servicio de correo para PINs de registro e invitaciones. En desarrollo se puede usar MailHog en un contenedor adicional.
- `CORS__AllowedOrigins__0`: primer origen permitido. Para Godot/cliente WPF en local: `http://localhost`.

Puertos
- API: `5000` (HTTP) y `5001` (HTTPS, opcional).
- Postgres: `5432` (no expuesto a host por default; usar `docker compose exec db psql ...` para acceso directo).

Swagger
Disponible en `http://localhost:5000/swagger` cuando el entorno es `Development`. Permite probar todos los endpoints REST sin necesidad de un cliente externo. Para invocar endpoints con `[Authorize]` usar el botón "Authorize" en la barra superior y pegar el access token devuelto por `POST /api/auth/login`.

Migraciones de Base de Datos
Para crear una migración nueva (desde la raíz):
```
dotnet ef migrations add NombreDeMigracion --project MemoryGame.Server/src/MemoryGame.Infrastructure --startup-project MemoryGame.Server/src/MemoryGame.API
```
Aplicarla manualmente:
```
dotnet ef database update --project MemoryGame.Server/src/MemoryGame.Infrastructure --startup-project MemoryGame.Server/src/MemoryGame.API
```
En docker, las migraciones se aplican al arranque del contenedor de API si `RunMigrationsOnStartup=true`.

Cliente WPF
1. Abrir `MemoryGame.Client/MemoryGame.Client.sln` en Visual Studio.
2. Compilar en modo `Debug` o `Release` para `net8.0-windows`.
3. En `appsettings.json` del cliente (o variables de entorno equivalentes) verificar que `ApiBaseUrl` apunte al servidor (`http://localhost:5000` en local).
4. Ejecutar con F5. El primer arranque muestra la pantalla de selección de idioma; las preferencias quedan persistidas en `%AppData%/MemoryGame/settings.json`.

Solución de Problemas
- **CORS bloqueado**: revisar que la variable `CORS__AllowedOrigins` incluya el origen del cliente.
- **Token rechazado**: verificar que `JWT_SECRET` sea idéntico entre el emisor y el verificador; si se cambia, todos los tokens previos quedan invalidados.
- **PIN no llega**: en desarrollo el PIN se devuelve directamente en el body de `POST /api/auth/register` para evitar depender de SMTP.
- **Migración falla en arranque**: ver logs de `docker compose logs api`. Lo más común es una `DATABASE_CONNECTION_STRING` mal formada o el contenedor de DB todavía inicializando (esperar 10-15s y reintentar).
