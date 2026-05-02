using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MemoryGame.Client.Localization;
using MemoryGame.Client.Models.Lobby;
using MemoryGame.Client.Services.Interfaces;
using MemoryGame.Client.Services.Network;

namespace MemoryGame.Client.ViewModels.Lobby;

/// <summary>
/// ViewModel for the host lobby screen.
/// The host can configure game settings (card count / turn timer),
/// see the player list, use the chat, invite friends, and start the game.
/// </summary>
public partial class HostLobbyViewModel : ObservableObject
{
    private readonly INavigationService _navigation;
    private readonly ISessionService _session;
    private readonly ILobbyService _lobbyService;
    private readonly IChatService _chatService;
    private readonly IGameService _gameService;
    private readonly IDialogService _dialog;
    private readonly HubService _hub;

    private bool _isGameStarting;
    private bool _disposed;

    // ── Observable Properties ──────────────────────────────────────────────

    [ObservableProperty]
    private string _gameCode = string.Empty;

    [ObservableProperty]
    private string _chatMessage = string.Empty;

    [ObservableProperty]
    private int _selectedCardCount = 16;

    [ObservableProperty]
    private int _turnTimeSeconds = 30;

    [ObservableProperty]
    private bool _isLoading;

    public ObservableCollection<LobbyPlayerDto> Players { get; } = new();
    public ObservableCollection<string> ChatMessages { get; } = new();

    public bool IsGuest => _session.Current?.IsGuest == true;
    public string CurrentUsername => _session.Current?.Username ?? "Player";

    /// <summary>Available card count options for the host to select.</summary>
    public int[] CardCountOptions { get; } = [16, 24, 30, 36];

    /// <summary>
    /// Fired when the chat needs to scroll to the newest message.
    /// The View subscribes to this to scroll its ScrollViewer.
    /// </summary>
    public event Action? ScrollChatToBottom;

    // ── Constructor ────────────────────────────────────────────────────────

    public HostLobbyViewModel(
        INavigationService navigation,
        ISessionService session,
        ILobbyService lobbyService,
        IChatService chatService,
        IGameService gameService,
        IDialogService dialog,
        HubService hub)
    {
        _navigation = navigation;
        _session = session;
        _lobbyService = lobbyService;
        _chatService = chatService;
        _gameService = gameService;
        _dialog = dialog;
        _hub = hub;

        // Initialize with currently known players so the list isn't empty on load
        foreach (var p in _lobbyService.CurrentPlayers)
            Players.Add(p);

        SubscribeEvents();
    }

    // ── Event Wiring ───────────────────────────────────────────────────────

    private void SubscribeEvents()
    {
        _lobbyService.PlayerListUpdated += OnPlayerListUpdated;
        _lobbyService.PlayerJoined += OnPlayerJoined;
        _lobbyService.PlayerLeft += OnPlayerLeft;
        _lobbyService.ErrorReceived += OnErrorReceived;
        _chatService.MessageReceived += OnChatMessageReceived;
        _gameService.GameStarted += OnGameStarted;
        _lobbyService.Kicked += OnKicked;
        _lobbyService.LobbyInviteReceived += OnLobbyInviteReceived;
        _lobbyService.LobbyInviteSent += OnLobbyInviteSent;
    }

    private void UnsubscribeEvents()
    {
        if (_disposed) return;
        _disposed = true;

        _lobbyService.PlayerListUpdated -= OnPlayerListUpdated;
        _lobbyService.PlayerJoined -= OnPlayerJoined;
        _lobbyService.PlayerLeft -= OnPlayerLeft;
        _lobbyService.ErrorReceived -= OnErrorReceived;
        _chatService.MessageReceived -= OnChatMessageReceived;
        _gameService.GameStarted -= OnGameStarted;
        _lobbyService.Kicked -= OnKicked;
        _lobbyService.LobbyInviteReceived -= OnLobbyInviteReceived;
        _lobbyService.LobbyInviteSent -= OnLobbyInviteSent;
    }

    // ── Server Event Handlers ──────────────────────────────────────────────

    private void OnPlayerListUpdated(List<LobbyPlayerDto> players)
    {
        if (_isGameStarting || _disposed) return;

        App.Current.Dispatcher.Invoke(() =>
        {
            Players.Clear();
            foreach (var player in players)
                Players.Add(player);
        });
    }

    private void OnPlayerJoined(string username, bool isGuest)
    {
        if (_isGameStarting || _disposed) return;

        App.Current.Dispatcher.Invoke(() =>
        {
            string message = LocalizationManager.Instance.Format("Lobby_Notification_PlayerJoined", username);
            AddSystemMessage(message);
        });
    }

    private void OnPlayerLeft(string username)
    {
        if (_isGameStarting || _disposed) return;

        App.Current.Dispatcher.Invoke(() =>
        {
            string message = LocalizationManager.Instance.Format("Lobby_Notification_PlayerLeft", username);
            AddSystemMessage(message);
        });
    }

    private void OnChatMessageReceived(string sender, string message, bool isSystem)
    {
        if (_isGameStarting || _disposed) return;

        App.Current.Dispatcher.Invoke(() =>
        {
            string formatted = isSystem ? $"⸻ {message} ⸻" : $"{sender}: {message}";
            ChatMessages.Add(formatted);
            ScrollChatToBottom?.Invoke();
        });
    }

    private void OnGameStarted(List<CardInfoDto> cards)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            _isGameStarting = true;
            UnsubscribeEvents();
            // TODO: Navigate to the multiplayer board view when implemented
        });
    }

    private void OnErrorReceived(string errorCode)
    {
        if (_disposed) return;

        App.Current.Dispatcher.Invoke(() =>
        {
            IsLoading = false;
            string message = LocalizationManager.Instance[$"Error_{errorCode}"]
                             ?? LocalizationManager.Instance["Error_UNKNOWN"];
            _dialog.ShowMessage(message,
                LocalizationManager.Instance["Global_Title_Error"],
                DialogButton.OK, DialogIcon.Error);
        });
    }

    private void OnKicked()
    {
        if (_disposed) return;

        App.Current.Dispatcher.Invoke(() =>
        {
            UnsubscribeEvents();
            _dialog.ShowMessage(
                LocalizationManager.Instance["Lobby_Message_Kicked"],
                LocalizationManager.Instance["Global_Title_Information"],
                DialogButton.OK, DialogIcon.Information);
            _navigation.GoBack();
        });
    }

    private void OnLobbyInviteReceived(string inviterUsername, string gameCode)
    {
        if (_disposed) return;
        App.Current.Dispatcher.Invoke(() =>
        {
            // Usually global, but just to notify here
            string message = LocalizationManager.Instance.Format("MainMenu_Message_LobbyInvite", inviterUsername, gameCode);
            AddSystemMessage(message);
        });
    }

    private void OnLobbyInviteSent(string targetUsername, bool isOnline)
    {
        if (_disposed) return;
        App.Current.Dispatcher.Invoke(() =>
        {
            string message = isOnline 
                ? LocalizationManager.Instance.Format("Lobby_Message_InviteSentRealTime", targetUsername)
                : LocalizationManager.Instance.Format("Lobby_Message_InviteSentEmail", targetUsername);
            AddSystemMessage(message);
        });
    }

    // ── Commands ───────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task SendChatMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(ChatMessage)) return;

        try
        {
            await _chatService.SendChatMessageAsync(ChatMessage);
            ChatMessage = string.Empty;
        }
        catch
        {
            // Silent — best-effort
        }
    }

    [RelayCommand]
    private async Task StartGameAsync()
    {
        if (Players.Count < 2)
        {
            _dialog.ShowMessage(
                LocalizationManager.Instance["Lobby_Label_NotEnoughPlayers"],
                LocalizationManager.Instance["Global_Title_Warning"],
                DialogButton.OK, DialogIcon.Warning);
            return;
        }

        IsLoading = true;

        try
        {
            var settings = new GameSettingsDto(SelectedCardCount, TurnTimeSeconds);
            await _gameService.StartGameAsync(settings);
        }
        catch
        {
            IsLoading = false;
            _dialog.ShowMessage(
                LocalizationManager.Instance["Error_UNKNOWN"],
                LocalizationManager.Instance["Global_Title_Error"],
                DialogButton.OK, DialogIcon.Error);
        }
    }

    [RelayCommand]
    private async Task LeaveAsync()
    {
        var result = _dialog.ShowMessage(
            LocalizationManager.Instance["Lobby_Message_LeaveLobby"],
            LocalizationManager.Instance["Global_Title_Confirm"],
            DialogButton.YesNo, DialogIcon.Question);

        if (result != Services.Interfaces.DialogResult.Yes) return;

        try
        {
            await _lobbyService.LeaveLobbyAsync();
        }
        catch
        {
            // Best-effort
        }

        UnsubscribeEvents();
        _navigation.GoBack();
    }

    [RelayCommand]
    private async Task VoteToKickAsync(LobbyPlayerDto player)
    {
        if (player.IsHost) return;

        var result = _dialog.ShowMessage(
            LocalizationManager.Instance.Format("KickVote_Message_VoteKickPlayer", player.Username),
            LocalizationManager.Instance["Global_Title_Confirm"],
            DialogButton.YesNo, DialogIcon.Question);

        if (result != Services.Interfaces.DialogResult.Yes) return;

        try
        {
            await _lobbyService.VoteToKickAsync(player.Username);
        }
        catch
        {
            // Best-effort
        }
    }

    [RelayCommand]
    private async Task InviteFriendAsync()
    {
        // Placeholder for user input prompt. Ideally dialog service would ask for a Friend ID.
        // As a quick implementation, we can simulate or we might have an InviteFriend UI.
        // For now, this is a placeholder implementation that fulfills the callback wiring requirement.
        try
        {
            // Example: wait for prompt implementation
            // await _lobbyService.InviteFriendAsync(targetUserId);
            AddSystemMessage("Friend invitation not yet implemented in UI.");
        }
        catch
        {
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        _ = SafeLeaveAsync();
        UnsubscribeEvents();
        _navigation.GoBack();
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private void AddSystemMessage(string message)
    {
        ChatMessages.Add($"⸻ {message} ⸻");
        ScrollChatToBottom?.Invoke();
    }

    private async Task SafeLeaveAsync()
    {
        try
        {
            await _lobbyService.LeaveLobbyAsync();
        }
        catch
        {
            // Ignore
        }
    }
}
