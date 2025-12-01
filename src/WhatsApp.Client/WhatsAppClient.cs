using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using System.Text.Json;
using WhatsAppDotnet.Events;
using WhatsAppDotnet.Structures;
using WhatsAppDotnet.Utilities;
using static WhatsAppDotnet.Utilities.Events;

namespace WhatsAppDotnet;

/// <summary>
/// Main WhatsApp client for interacting with WhatsApp Web API
/// </summary>
public class WhatsAppClient : IDisposable
{
    private readonly ILogger<WhatsAppClient> _logger;
    private readonly WhatsAppClientOptions _options;
    private IBrowser? _browser;
    private IPage? _page;
    private ClientStatus _status = ClientStatus.Initializing;
    private int _qrRetries = 0;
    private bool _disposed;

    #region Events

    /// <summary>
    /// Fired when a QR code is received for authentication
    /// </summary>
    public event EventHandler<QrEventArgs>? QrReceived;

    /// <summary>
    /// Fired when the client is authenticated
    /// </summary>
    public event EventHandler<AuthenticationEventArgs>? Authenticated;

    /// <summary>
    /// Fired when authentication fails
    /// </summary>
    public event EventHandler<AuthenticationEventArgs>? AuthenticationFailed;

    /// <summary>
    /// Fired when the client is ready to use
    /// </summary>
    public event EventHandler? Ready;

    /// <summary>
    /// Fired when a message is received
    /// </summary>
    public event EventHandler<MessageEventArgs>? MessageReceived;

    /// <summary>
    /// Fired when a message is created
    /// </summary>
    public event EventHandler<MessageEventArgs>? MessageCreated;

    /// <summary>
    /// Fired when the client state changes
    /// </summary>
    public event EventHandler<StateChangeEventArgs>? StateChanged;

    /// <summary>
    /// Fired when the client is disconnected
    /// </summary>
    public event EventHandler<DisconnectionEventArgs>? Disconnected;

    #endregion

    /// <summary>
    /// Initializes a new instance of the WhatsAppClient class
    /// </summary>
    /// <param name="options">Client configuration options</param>
    /// <param name="logger">Logger instance</param>
    public WhatsAppClient(WhatsAppClientOptions? options = null, ILogger<WhatsAppClient>? logger = null)
    {
        _options = options ?? new WhatsAppClientOptions();
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<WhatsAppClient>.Instance;
    }

    /// <summary>
    /// Gets the current client status
    /// </summary>
    public ClientStatus Status => _status;

    /// <summary>
    /// Gets the browser instance
    /// </summary>
    public IBrowser? Browser => _browser;

    /// <summary>
    /// Gets the page instance
    /// </summary>
    public IPage? Page => _page;

    /// <summary>
    /// Initializes the WhatsApp client
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Initializing WhatsApp client...");
        
        try
        {
            SetStatus(ClientStatus.Initializing);

            // Install Playwright browsers if needed
            Microsoft.Playwright.Program.Main(new[] { "install", _options.Browser });

            // Launch browser
            var playwright = await Playwright.CreateAsync();
            var browserType = _options.Browser.ToLower() switch
            {
                "firefox" => playwright.Firefox,
                "webkit" => playwright.Webkit,
                _ => playwright.Chromium
            };

            _browser = await browserType.LaunchAsync(_options.PlaywrightOptions);
            _page = await _browser.NewPageAsync();

            // Set user agent
            await _page.SetExtraHTTPHeadersAsync(new Dictionary<string, string>
            {
                ["User-Agent"] = _options.UserAgent
            });

            // Expose functions for JavaScript callbacks
            await ExposeCallbackFunctionsAsync();

            // Navigate to WhatsApp Web
            await _page.GotoAsync(Constants.WhatsWebUrl);

            _logger.LogInformation("WhatsApp client initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize WhatsApp client");
            throw;
        }
    }

    /// <summary>
    /// Starts the authentication process
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    public async Task AuthenticateAsync(CancellationToken cancellationToken = default)
    {
        if (_page == null)
            throw new InvalidOperationException("Client must be initialized before authentication");

        _logger.LogInformation("Starting authentication process...");
        SetStatus(ClientStatus.Authenticating);

        try
        {
            // Wait for WhatsApp Web to load and check if already authenticated
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Check if already authenticated
            var isAuthenticated = await CheckIfAuthenticatedAsync();
            if (isAuthenticated)
            {
                await OnAuthenticatedAsync();
                return;
            }

            // Wait for QR code or authentication
            await WaitForAuthenticationAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication failed");
            AuthenticationFailed?.Invoke(this, new AuthenticationEventArgs(false, ex.Message));
            throw;
        }
    }

    /// <summary>
    /// Sends a text message to a chat
    /// </summary>
    /// <param name="chatId">The chat ID to send the message to</param>
    /// <param name="message">The message content</param>
    /// <param name="options">Optional message options</param>
    /// <returns>The sent message</returns>
    public async Task<Message?> SendMessageAsync(string chatId, string message, MessageOptions? options = null)
    {
        if (_page == null || _status != ClientStatus.Ready)
            throw new InvalidOperationException("Client is not ready");

        _logger.LogDebug("Sending message to chat {ChatId}", chatId);

        try
        {
            var result = await _page.EvaluateAsync<dynamic>(@"
                async ({ chatId, message, options }) => {
                    try {
                        const chat = await window.WPP.chat.get(chatId);
                        if (!chat) return null;
                        
                        const messageOptions = {
                            type: 'text',
                            body: message,
                            ...options
                        };
                        
                        const result = await window.WPP.chat.sendTextMessage(chatId, message, messageOptions);
                        return result;
                    } catch (error) {
                        return { error: error.message };
                    }
                }
            ", new { chatId, message, options });

            if (result?.error != null)
            {
                _logger.LogError("Failed to send message: {Error}", (string)result.error);
                return null;
            }

            // Convert result to Message object
            var sentMessage = new Message(this, result);
            return sentMessage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to chat {ChatId}", chatId);
            return null;
        }
    }

    /// <summary>
    /// Gets all chats
    /// </summary>
    /// <returns>List of chats</returns>
    public async Task<List<Chat>> GetChatsAsync()
    {
        if (_page == null || _status != ClientStatus.Ready)
            throw new InvalidOperationException("Client is not ready");

        try
        {
            var chats = await _page.EvaluateAsync<dynamic[]>(@"
                async () => {
                    try {
                        const chats = await window.WPP.chat.list();
                        return chats.map(chat => ({
                            id: chat.id._serialized,
                            name: chat.name,
                            isGroup: chat.isGroup,
                            unreadCount: chat.unreadCount,
                            timestamp: chat.t,
                            archived: chat.archived,
                            pinned: chat.pin
                        }));
                    } catch (error) {
                        return [];
                    }
                }
            ");

            return chats?.Select(chatData => new Chat(this, chatData)).ToList() ?? new List<Chat>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chats");
            return new List<Chat>();
        }
    }

    /// <summary>
    /// Gets all contacts
    /// </summary>
    /// <returns>List of contacts</returns>
    public async Task<List<Contact>> GetContactsAsync()
    {
        if (_page == null || _status != ClientStatus.Ready)
            throw new InvalidOperationException("Client is not ready");

        try
        {
            var contacts = await _page.EvaluateAsync<dynamic[]>(@"
                async () => {
                    try {
                        const contacts = await window.WPP.contact.list();
                        return contacts.map(contact => ({
                            id: contact.id._serialized,
                            number: contact.number,
                            name: contact.name,
                            pushName: contact.pushname,
                            shortName: contact.shortName,
                            isUser: contact.isUser,
                            isGroup: contact.isGroup,
                            isMe: contact.isMe,
                            isBusiness: contact.isBusiness,
                            profilePicUrl: contact.profilePicUrl
                        }));
                    } catch (error) {
                        return [];
                    }
                }
            ");

            return contacts?.Select(contactData => new Contact(this, contactData)).ToList() ?? new List<Contact>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contacts");
            return new List<Contact>();
        }
    }

    /// <summary>
    /// Sets auto download setting for photos
    /// </summary>
    /// <param name="flag">True to enable auto download</param>
    /// <returns>Task representing the async operation</returns>
    public async Task SetAutoDownloadPhotosAsync(bool flag)
    {
        if (_page == null || _status != ClientStatus.Ready)
            throw new InvalidOperationException("Client is not ready");

        await _page.EvaluateAsync(@"
            async (flag) => {
                const autoDownload = window.Store.Settings.getAutoDownloadPhotos();
                if (autoDownload === flag) {
                    return flag;
                }
                await window.Store.Settings.setAutoDownloadPhotos(flag);
                return flag;
            }
        ", flag);
    }

    /// <summary>
    /// Sets auto download setting for videos
    /// </summary>
    /// <param name="flag">True to enable auto download</param>
    /// <returns>Task representing the async operation</returns>
    public async Task SetAutoDownloadVideosAsync(bool flag)
    {
        if (_page == null || _status != ClientStatus.Ready)
            throw new InvalidOperationException("Client is not ready");

        await _page.EvaluateAsync(@"
            async (flag) => {
                const autoDownload = window.Store.Settings.getAutoDownloadVideos();
                if (autoDownload === flag) {
                    return flag;
                }
                await window.Store.Settings.setAutoDownloadVideos(flag);
                return flag;
            }
        ", flag);
    }

    /// <summary>
    /// Joins WhatsApp Web beta
    /// </summary>
    /// <param name="action">True to join beta, false to leave</param>
    /// <returns>Result of the operation</returns>
    public async Task<bool> JoinWebBetaAsync(bool action = true)
    {
        if (_page == null || _status != ClientStatus.Ready)
            throw new InvalidOperationException("Client is not ready");

        return await _page.EvaluateAsync<bool>(@"
            async (action) => {
                return await window.WPP.conn.joinWebBeta(action);
            }
        ", action);
    }

    /// <summary>
    /// Destroys the client and cleans up resources
    /// </summary>
    /// <returns>Task representing the async operation</returns>
    public async Task DestroyAsync()
    {
        _logger.LogInformation("Destroying WhatsApp client...");

        try
        {
            if (_page != null)
            {
                await _page.CloseAsync();
                _page = null;
            }

            if (_browser != null)
            {
                await _browser.CloseAsync();
                _browser = null;
            }

            SetStatus(ClientStatus.Initializing);
            _logger.LogInformation("WhatsApp client destroyed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error destroying WhatsApp client");
        }
    }

    #region Private Methods

    private async Task ExposeCallbackFunctionsAsync()
    {
        if (_page == null) return;

        await _page.ExposeFunctionAsync("qrChanged", (string qr) =>
        {
            _qrRetries++;
            QrReceived?.Invoke(this, new QrEventArgs(qr));

            if (_options.QrMaxRetries > 0 && _qrRetries > _options.QrMaxRetries)
            {
                Disconnected?.Invoke(this, new DisconnectionEventArgs("Max QR code retries reached"));
                _ = Task.Run(DestroyAsync);
            }
            return Task.CompletedTask;
        });

        await _page.ExposeFunctionAsync("EmitEvent", (string eventName, object[] data) =>
        {
            HandleWhatsAppEvent(eventName, data);
            return Task.CompletedTask;
        });

        await _page.ExposeFunctionAsync("onAddMessageEvent", (object msgData) =>
        {
            var message = new Message(this, msgData);
            MessageReceived?.Invoke(this, new MessageEventArgs(message));
            return Task.CompletedTask;
        });
    }

    private void HandleWhatsAppEvent(string eventName, object[] data)
    {
        _logger.LogDebug("Received WhatsApp event: {EventName}", eventName);

        switch (eventName)
        {
            case "ready":
                _ = Task.Run(OnReadyAsync);
                break;
            case "disconnected":
                var reason = data.Length > 0 ? data[0]?.ToString() : "Unknown reason";
                Disconnected?.Invoke(this, new DisconnectionEventArgs(reason ?? "Unknown"));
                break;
        }
    }

    private async Task<bool> CheckIfAuthenticatedAsync()
    {
        if (_page == null) return false;

        try
        {
            var isAuthenticated = await _page.EvaluateAsync<bool>(@"
                () => {
                    return window.Store && window.Store.State && window.Store.State.default.state === 'CONNECTED';
                }
            ");

            return isAuthenticated;
        }
        catch
        {
            return false;
        }
    }

    private async Task WaitForAuthenticationAsync(CancellationToken cancellationToken)
    {
        if (_page == null) return;

        // Wait for either authentication success or QR code
        await _page.WaitForFunctionAsync(@"
            () => {
                return (window.Store && window.Store.State && window.Store.State.default.state === 'CONNECTED') ||
                       document.querySelector('[data-testid=""qr-code""]');
            }
        ", new PageWaitForFunctionOptions { Timeout = 60000 });

        var isAuthenticated = await CheckIfAuthenticatedAsync();
        if (isAuthenticated)
        {
            await OnAuthenticatedAsync();
        }
    }

    private async Task OnAuthenticatedAsync()
    {
        _logger.LogInformation("WhatsApp client authenticated successfully");
        Authenticated?.Invoke(this, new AuthenticationEventArgs(true));
        await OnReadyAsync();
    }

    private async Task OnReadyAsync()
    {
        SetStatus(ClientStatus.Ready);
        _logger.LogInformation("WhatsApp client is ready");
        Ready?.Invoke(this, EventArgs.Empty);

        // Inject WhatsApp Web enhancements
        await InjectScriptsAsync();
    }

    private async Task InjectScriptsAsync()
    {
        if (_page == null) return;

        try
        {
            // Inject WPP (WhatsApp Web Plus) scripts for enhanced functionality
            await _page.EvaluateAsync(@"
                () => {
                    if (!window.WPP) {
                        // Basic WPP-like functionality
                        window.WPP = {
                            chat: {
                                get: async (chatId) => window.Store.Chat.get(chatId),
                                list: async () => window.Store.Chat.getModelsArray(),
                                sendTextMessage: async (chatId, message, options) => {
                                    const chat = await window.Store.Chat.get(chatId);
                                    return await window.Store.SendMessage(chat, message, options);
                                }
                            },
                            contact: {
                                list: async () => window.Store.Contact.getModelsArray()
                            },
                            conn: {
                                joinWebBeta: async (action) => {
                                    return await window.Store.BetaFlags.joinBeta(action);
                                }
                            }
                        };
                    }
                }
            ");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to inject enhancement scripts");
        }
    }

    private void SetStatus(ClientStatus newStatus)
    {
        var previousStatus = _status;
        _status = newStatus;
        StateChanged?.Invoke(this, new StateChangeEventArgs(previousStatus, newStatus));
    }

    #endregion

    #region IDisposable

    /// <summary>
    /// Disposes the WhatsApp client
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _ = Task.Run(DestroyAsync);
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    #endregion
}
