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
    private static readonly HttpClient s_httpClient = new();

    private readonly ILogger<WhatsAppClient> _logger;
    private readonly WhatsAppClientOptions _options;
    private IBrowser? _browser;
    private IPage? _page;
    private ClientStatus _status = ClientStatus.Initializing;
    private int _qrRetries = 0;
    private bool _wppInjected = false;
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

            // Register WPPConnect's wa-js library as an init script BEFORE
            // navigating. This hooks into WhatsApp Web's internal webpack
            // modules, unlocking store-level APIs (list messages,
            // native-flow buttons on text/media messages) that are not
            // reachable through the public page UI that the rest of this
            // client automates.
            //
            // Order matters: AddInitScriptAsync only applies to page loads
            // that happen *after* registration, not retroactively — it must
            // be called before GotoAsync.
            //
            // It must also be injected via AddInitScriptAsync (CDP-level,
            // runs before the page's own scripts) with the script *content*
            // inlined, not AddScriptTagAsync with a URL: WhatsApp Web's own
            // Content-Security-Policy (script-src) blocks loading external
            // scripts entirely — confirmed by testing both approaches
            // directly against web.whatsapp.com. AddInitScriptAsync bypasses
            // this because it operates below the page's CSP enforcement.
            await InjectWppAsync();

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
    /// Sends a text message with up to 3 native-flow buttons attached
    /// (quick-reply, open-link, call, or copy-code — see
    /// <see cref="ButtonOption"/>). Requires wa-js (injected automatically
    /// during <see cref="InitializeAsync"/>).
    ///
    /// ⚠️ WPPConnect's own docs are explicit about this: "The buttons are an
    /// alternative solution we found to make it work. There is no guarantee
    /// that they will continue functioning, or when they might stop: The
    /// only certainty is: They will stop, so use them responsibly." This
    /// works by driving the real WhatsApp Web client's internal store, so it
    /// tends to be more reliable than raw-protocol libraries (e.g. Baileys/
    /// whatsmeow), but it is still an unsupported, unofficial mechanism.
    /// </summary>
    /// <param name="chatId">The chat ID to send the message to</param>
    /// <param name="content">The message body text</param>
    /// <param name="buttons">1 to 3 buttons</param>
    /// <param name="title">Optional title shown above the message body</param>
    /// <param name="footer">Optional footer text below the buttons</param>
    /// <returns>The sent message, or null on failure</returns>
    public async Task<Message?> SendButtonsAsync(
        string chatId,
        string content,
        List<ButtonOption> buttons,
        string? title = null,
        string? footer = null)
    {
        if (_page == null || _status != ClientStatus.Ready)
            throw new InvalidOperationException("Client is not ready");
        if (buttons == null || buttons.Count == 0 || buttons.Count > 3)
            throw new ArgumentException("Provide between 1 and 3 buttons", nameof(buttons));

        await WaitForWppReadyAsync();

        _logger.LogDebug("Sending {Count} button(s) to chat {ChatId}", buttons.Count, chatId);

        var buttonPayload = buttons.Select(BuildButtonPayload).ToArray();

        try
        {
            var result = await _page.EvaluateAsync<dynamic>(@"
                async ({ chatId, content, buttonPayload, title, footer }) => {
                    try {
                        return await window.WPP.chat.sendTextMessage(chatId, content, {
                            useInteractiveMessage: true,
                            buttons: buttonPayload,
                            title: title || undefined,
                            footer: footer || undefined
                        });
                    } catch (error) {
                        return { error: error.message };
                    }
                }
            ", new { chatId, content, buttonPayload, title, footer });

            if (result?.error != null)
            {
                _logger.LogError("Failed to send buttons message: {Error}", (string)result.error);
                return null;
            }

            return new Message(this, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending buttons message to chat {ChatId}", chatId);
            return null;
        }
    }

    /// <summary>
    /// Sends a single-select list message: a button that opens a sectioned
    /// picker of rows. Requires wa-js (injected automatically during
    /// <see cref="InitializeAsync"/>).
    /// </summary>
    /// <param name="chatId">The chat ID to send the message to</param>
    /// <param name="buttonText">Label on the button that opens the list</param>
    /// <param name="description">Body text shown above the button</param>
    /// <param name="sections">1 to 10 sections, each with at least one row</param>
    /// <param name="title">Optional title shown above the description</param>
    /// <param name="footer">Optional footer text</param>
    /// <returns>The sent message, or null on failure</returns>
    public async Task<Message?> SendListAsync(
        string chatId,
        string buttonText,
        string description,
        List<ListSection> sections,
        string? title = null,
        string? footer = null)
    {
        if (_page == null || _status != ClientStatus.Ready)
            throw new InvalidOperationException("Client is not ready");
        if (sections == null || sections.Count == 0 || sections.Count > 10)
            throw new ArgumentException("Provide between 1 and 10 sections", nameof(sections));
        foreach (var section in sections)
        {
            if (section.Rows == null || section.Rows.Count == 0)
                throw new ArgumentException($"Section '{section.Title}' must have at least one row", nameof(sections));
        }

        await WaitForWppReadyAsync();

        _logger.LogDebug("Sending list message ({Sections} section(s)) to chat {ChatId}", sections.Count, chatId);

        var sectionsPayload = sections.Select(s => new
        {
            title = s.Title,
            rows = s.Rows.Select(r => new { rowId = r.RowId, title = r.Title, description = r.Description }).ToArray()
        }).ToArray();

        try
        {
            var result = await _page.EvaluateAsync<dynamic>(@"
                async ({ chatId, buttonText, description, sectionsPayload, title, footer }) => {
                    try {
                        return await window.WPP.chat.sendListMessage(chatId, {
                            buttonText,
                            description,
                            title: title || undefined,
                            footer: footer || undefined,
                            sections: sectionsPayload
                        });
                    } catch (error) {
                        return { error: error.message };
                    }
                }
            ", new { chatId, buttonText, description, sectionsPayload, title, footer });

            if (result?.error != null)
            {
                _logger.LogError("Failed to send list message: {Error}", (string)result.error);
                return null;
            }

            return new Message(this, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending list message to chat {ChatId}", chatId);
            return null;
        }
    }

    /// <summary>
    /// Sends an image from a local file path. Thin wrapper over
    /// <see cref="SendImageBytesAsync"/> — reads the file and infers the
    /// mime type from its extension.
    /// </summary>
    /// <param name="chatId">The chat ID to send the message to</param>
    /// <param name="imagePath">Local file path to the image (jpg/png/webp)</param>
    /// <param name="caption">Optional caption</param>
    /// <param name="buttons">0 to 2 buttons to attach</param>
    /// <param name="isViewOnce">Send as a view-once image</param>
    /// <returns>The sent message, or null on failure</returns>
    public async Task<Message?> SendImageAsync(
        string chatId,
        string imagePath,
        string? caption = null,
        List<ButtonOption>? buttons = null,
        bool isViewOnce = false)
    {
        if (!File.Exists(imagePath))
            throw new FileNotFoundException("Image file not found", imagePath);

        var mimeType = Path.GetExtension(imagePath).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "image/jpeg",
        };
        var bytes = await File.ReadAllBytesAsync(imagePath);
        return await SendImageBytesAsync(chatId, bytes, mimeType, caption, buttons, isViewOnce);
    }

    /// <summary>
    /// Sends an image from raw bytes already in memory (e.g. loaded from a
    /// database, downloaded from another API, or provided by
    /// <see cref="Structures.Chat.SendMediaAsync"/>), optionally with a
    /// caption and up to 2 buttons (file-message buttons are capped at 2,
    /// vs. 3 for plain text — a wa-js/WhatsApp protocol limit, not a
    /// limitation of this client).
    /// </summary>
    /// <param name="chatId">The chat ID to send the message to</param>
    /// <param name="imageBytes">Raw image bytes</param>
    /// <param name="mimeType">e.g. "image/jpeg", "image/png"</param>
    /// <param name="caption">Optional caption</param>
    /// <param name="buttons">0 to 2 buttons to attach</param>
    /// <param name="isViewOnce">Send as a view-once image</param>
    /// <returns>The sent message, or null on failure</returns>
    public async Task<Message?> SendImageBytesAsync(
        string chatId,
        byte[] imageBytes,
        string mimeType,
        string? caption = null,
        List<ButtonOption>? buttons = null,
        bool isViewOnce = false)
    {
        if (_page == null || _status != ClientStatus.Ready)
            throw new InvalidOperationException("Client is not ready");
        if (buttons != null && buttons.Count > 2)
            throw new ArgumentException("File messages support at most 2 buttons", nameof(buttons));

        await WaitForWppReadyAsync();

        var base64 = Convert.ToBase64String(imageBytes);
        var dataUri = $"data:{mimeType};base64,{base64}";
        var buttonPayload = buttons?.Select(BuildButtonPayload).ToArray();

        _logger.LogDebug("Sending image ({Size} bytes) to chat {ChatId}", imageBytes.Length, chatId);

        try
        {
            var result = await _page.EvaluateAsync<dynamic>(@"
                async ({ chatId, dataUri, caption, isViewOnce, buttonPayload }) => {
                    try {
                        const options = {
                            type: 'image',
                            caption: caption || undefined,
                            isViewOnce
                        };
                        if (buttonPayload && buttonPayload.length > 0) {
                            options.buttons = buttonPayload;
                        }
                        return await window.WPP.chat.sendFileMessage(chatId, dataUri, options);
                    } catch (error) {
                        return { error: error.message };
                    }
                }
            ", new { chatId, dataUri, caption, isViewOnce, buttonPayload });

            if (result?.error != null)
            {
                _logger.LogError("Failed to send image: {Error}", (string)result.error);
                return null;
            }

            return new Message(this, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending image to chat {ChatId}", chatId);
            return null;
        }
    }

    /// <summary>
    /// Converts a <see cref="ButtonOption"/> into the exact shape wa-js's
    /// prepareMessageButtons expects: {id,text} | {url,text} |
    /// {phoneNumber,text} | {code,text}.
    /// </summary>
    private static object BuildButtonPayload(ButtonOption button)
    {
        if (!string.IsNullOrEmpty(button.Url))
            return new { url = button.Url, text = button.Text };
        if (!string.IsNullOrEmpty(button.PhoneNumber))
            return new { phoneNumber = button.PhoneNumber, text = button.Text };
        if (!string.IsNullOrEmpty(button.Code))
            return new { code = button.Code, text = button.Text };
        return new { id = button.Id ?? Guid.NewGuid().ToString("N"), text = button.Text };
    }

    /// <summary>
    /// Injects WPPConnect's wa-js bundle into the current page. Idempotent —
    /// safe to call even if already injected (checks window.WPP first).
    /// </summary>
    private async Task InjectWppAsync()
    {
        if (_page == null || _wppInjected) return;

        string waJsCode;
        try
        {
            // A plain HTTP fetch here is not subject to WhatsApp Web's page
            // CSP at all — that policy only restricts what the page itself
            // is allowed to load. Fetching the bundle out-of-band and
            // handing Playwright the raw text (via AddInitScriptAsync) is
            // what actually gets it past the block; see the ordering/CSP
            // notes on the call site in InitializeAsync.
            waJsCode = await s_httpClient.GetStringAsync(Constants.WaJsScriptUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download wa-js from {Url} — button/list/image sending will not work", Constants.WaJsScriptUrl);
            return;
        }

        _logger.LogInformation("Registering wa-js (WPPConnect) init script ({Bytes} bytes) from {Url}", waJsCode.Length, Constants.WaJsScriptUrl);
        await _page.AddInitScriptAsync(script: waJsCode);
        _wppInjected = true;
    }

    /// <summary>
    /// Diagnostic helper: reports whether wa-js (window.WPP) is present on
    /// the current page and, if so, its reported version and whether the
    /// button/list/file-message functions this client relies on exist.
    /// Useful when troubleshooting environments where the CDN fetch or
    /// script injection might be blocked (corporate proxies, offline CI,
    /// etc.) — call this right after <see cref="InitializeAsync"/> to
    /// confirm wa-js loaded before waiting on QR/authentication.
    /// </summary>
    public async Task<WppInjectionStatus> GetWppInjectionStatusAsync()
    {
        if (_page == null)
            return new WppInjectionStatus(false, null, false, false, false);

        var raw = await _page.EvaluateAsync<System.Text.Json.JsonElement>(@"
            () => ({
                present: typeof window.WPP !== 'undefined',
                version: (window.WPP && window.WPP.version) || null,
                hasSendListMessage: !!(window.WPP && window.WPP.chat && window.WPP.chat.sendListMessage),
                hasSendTextMessage: !!(window.WPP && window.WPP.chat && window.WPP.chat.sendTextMessage),
                hasSendFileMessage: !!(window.WPP && window.WPP.chat && window.WPP.chat.sendFileMessage)
            })
        ");

        return new WppInjectionStatus(
            Present: raw.GetProperty("present").GetBoolean(),
            Version: raw.GetProperty("version").ValueKind == System.Text.Json.JsonValueKind.String
                ? raw.GetProperty("version").GetString()
                : null,
            HasSendListMessage: raw.GetProperty("hasSendListMessage").GetBoolean(),
            HasSendTextMessage: raw.GetProperty("hasSendTextMessage").GetBoolean(),
            HasSendFileMessage: raw.GetProperty("hasSendFileMessage").GetBoolean());
    }

    /// <summary>
    /// Waits until wa-js has finished attaching to WhatsApp Web's internal
    /// store (window.WPP.isFullReady). Required before calling any
    /// WPP.chat.* function — calling too early throws inside the page.
    /// </summary>
    private async Task WaitForWppReadyAsync(int timeoutMs = 30000)
    {
        if (_page == null) return;

        await _page.WaitForFunctionAsync(
            "() => window.WPP && window.WPP.isFullReady === true",
            new PageWaitForFunctionOptions { Timeout = timeoutMs });
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

/// <summary>
/// Result of <see cref="WhatsAppClient.GetWppInjectionStatusAsync"/> — reports
/// whether wa-js successfully loaded and which of the APIs this client
/// depends on (button/list/file messages) are actually present.
/// </summary>
/// <param name="Present">Whether window.WPP exists on the page at all.</param>
/// <param name="Version">wa-js's reported version string, if present.</param>
/// <param name="HasSendListMessage">Whether WPP.chat.sendListMessage exists.</param>
/// <param name="HasSendTextMessage">Whether WPP.chat.sendTextMessage exists.</param>
/// <param name="HasSendFileMessage">Whether WPP.chat.sendFileMessage exists.</param>
public record WppInjectionStatus(
    bool Present,
    string? Version,
    bool HasSendListMessage,
    bool HasSendTextMessage,
    bool HasSendFileMessage);
