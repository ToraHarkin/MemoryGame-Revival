Guía del Cliente WPF (`MemoryGame.Client`)
El cliente está desarrollado en .NET con WPF y sigue el patrón MVVM. La estructura sigue una convención por carpeta, donde cada feature tiene su propia `View`, `ViewModel` y, si aplica, sus servicios o modelos de soporte.

Estructura de Carpetas
- `App.xaml` / `App.xaml.cs`: punto de entrada, configura el contenedor DI y los temas globales.
- `MainWindow.xaml`: ventana principal con un `ContentControl` que aloja la vista actual según `MainWindowViewModel.CurrentView`.
- `Models/`: DTOs que se intercambian con el servidor (REST + SignalR). Incluye `Models/Lobby/` para las entidades específicas del módulo de lobbies.
- `ViewModels/`: lógica de presentación. Hereda de `BaseViewModel` que implementa `INotifyPropertyChanged` y expone helpers para comandos asíncronos.
- `Views/`: archivos `.xaml` con su code-behind. El code-behind se mantiene mínimo; toda la lógica vive en el ViewModel asociado.
- `Services/`: capa de servicios partida en cuatro subcarpetas:
  - `Core/`: estado y configuración global (`SessionService`, `ClientSettings`).
  - `Network/`: comunicación con el servidor (`ApiClient`, `LobbyService`, `HubService`, `GameService`, `ChatService`, `ProfileService`).
  - `UI/`: utilidades de presentación (`NavigationService`, `ThemeService`, `DialogService`, `WindowService`).
  - `Media/`: audio (`MusicService`).
  - `Interfaces/`: interfaces de todos los servicios anteriores para favorecer testabilidad.
- `Helpers/`: utilidades transversales (`ViewModelLocator`, `ProfileLoader`).
- `Converters/`: convertidores de WPF para bindings (`BoolToVisibilityConverter`, `ByteArrayToImageConverter`, etc.).
- `Behaviors/`: behaviors XAML reutilizables (`ZoomPanBehavior`).
- `Localization/`: gestor de idiomas (`LocalizationManager`, `ErrorResolver`).
- `Properties/Langs/`: archivos `.resx` con las traducciones (`Lang.resx` base, más es-MX, ja-JP, ko-KR, zh-CN).
- `Resources/`: assets (fuentes, imágenes, música, temas).
- `Messages/`: mensajes que se envían por el `Messenger` de CommunityToolkit.Mvvm para comunicar ViewModels sin acoplarlos.

Navegación
La navegación entre vistas se hace a través de `INavigationService` con un único punto de entrada: `NavigateTo<TViewModel>()`. El servicio resuelve la View asociada al ViewModel por convención de nombres (`LobbyViewModel` → `LobbyView`) y la asigna a `MainWindowViewModel.CurrentView`. Para diálogos modales se usa `IDialogService.ShowDialog<TViewModel>()` que internamente abre una `DialogWindow` con el `ContentControl` apuntando al ViewModel.

Capa de Red
- `ApiClient`: wrapper de `HttpClient` con manejo automático de tokens (inyecta `Authorization: Bearer <accessToken>` y, si la respuesta es `401`, dispara la renovación con el refresh token antes de reintentar).
- `HubService`: gestiona la conexión SignalR. Expone eventos `.NET` tradicionales (`PlayerJoined`, `UpdatePlayerList`, etc.) que los ViewModels suscriben. La reconexión automática está activada con backoff de 0s, 2s, 10s, 30s.
- `LobbyService`, `GameService`, `ChatService`: fachadas sobre `HubService` que exponen métodos tipados por feature.
- `ProfileService`: usa `ApiClient` para el módulo de perfil (REST).

Temas
`ThemeService` permite alternar entre `BaseTheme` (acabado limpio, paleta principal) y `SketchTheme` (estilo dibujado a mano, recursos alternativos). Los recursos están definidos como `DynamicResource` para que el cambio sea instantáneo sin recargar vistas.

Idiomas
El usuario puede cambiar de idioma desde la vista de configuración. `LocalizationManager` notifica el cambio y todos los bindings que apuntan a `LocalizationManager.Instance[Key]` se refrescan automáticamente. Para nuevas cadenas, agregar la clave en `Lang.resx` y traducirla en cada archivo regional.

Patrón MVVM Estricto
- Code-behind solo para wiring de XAML específicos del framework (`InitializeComponent`, navegación de teclas, animaciones del code-behind que no se pueden expresar en XAML).
- Nada de lógica de negocio en code-behind. Todo via bindings y comandos.
- ViewModels jamás referencian tipos de `System.Windows` o `System.Windows.Controls`; abstraen cualquier acercamiento al UI a través de `IDialogService`, `INavigationService`, `IWindowService`.
