using Microsoft.Playwright;

namespace WhatsAppDotnet.Utilities;

/// <summary>
/// Configuration options for the WhatsApp client
/// </summary>
public class WhatsAppClientOptions
{
    /// <summary>
    /// Playwright browser launch options
    /// </summary>
    public BrowserTypeLaunchOptions PlaywrightOptions { get; set; } = new()
    {
        Headless = true
    };

    /// <summary>
    /// How many times should the QR code be refreshed before giving up
    /// </summary>
    public int QrMaxRetries { get; set; } = 0;

    /// <summary>
    /// User agent to use in the browser
    /// </summary>
    public string UserAgent { get; set; } = Constants.DefaultUserAgent;

    /// <summary>
    /// FFmpeg path to use when formatting videos to webp while sending stickers
    /// </summary>
    public string FfmpegPath { get; set; } = Constants.DefaultFfmpegPath;

    /// <summary>
    /// Set your browser and WhatsApp session name
    /// </summary>
    public string SessionName { get; set; } = ".whatsapp_dotnet_auth";

    /// <summary>
    /// Folder name of your session
    /// </summary>
    public string SessionPath { get; set; } = "session";

    /// <summary>
    /// If another WhatsApp web session is detected, take over the session in the current browser
    /// </summary>
    public bool TakeoverOnConflict { get; set; } = false;

    /// <summary>
    /// How much time to wait before taking over the session (in milliseconds)
    /// </summary>
    public int TakeoverTimeoutMs { get; set; } = 30000;

    /// <summary>
    /// Browser type to use (Chromium, Firefox, or WebKit)
    /// </summary>
    public string Browser { get; set; } = "chromium";

    /// <summary>
    /// Device name for the session
    /// </summary>
    public string DeviceName { get; set; } = Constants.DefaultDeviceName;

    /// <summary>
    /// Version of WhatsApp Web to use
    /// </summary>
    public string? Version { get; set; }
}
