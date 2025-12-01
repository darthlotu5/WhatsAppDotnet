using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WhatsAppDotnet;
using WhatsAppDotnet.Events;
using WhatsAppDotnet.Utilities;

/// <summary>
/// Example console application demonstrating WhatsAppDotnet usage
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        // Create host builder
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Configure WhatsApp client options
                services.Configure<WhatsAppClientOptions>(options =>
                {
                    options.PlaywrightOptions.Headless = false; // Show browser for QR code scanning
                    options.QrMaxRetries = 3;
                    options.SessionName = "example_session";
                });

                // Register WhatsApp client
                services.AddSingleton<WhatsAppClient>();
                services.AddSingleton<WhatsAppService>();
            })
            .Build();

        // Get the WhatsApp service
        var whatsAppService = host.Services.GetRequiredService<WhatsAppService>();

        // Start the service
        await whatsAppService.StartAsync();

        // Keep the application running
        Console.WriteLine("Press any key to stop...");
        Console.ReadKey();

        // Stop the service
        await whatsAppService.StopAsync();
        await host.StopAsync();
    }
}

/// <summary>
/// Service that manages WhatsApp client lifecycle and handles events
/// </summary>
public class WhatsAppService
{
    private readonly WhatsAppClient _client;
    private readonly ILogger<WhatsAppService> _logger;

    public WhatsAppService(WhatsAppClient client, ILogger<WhatsAppService> logger)
    {
        _client = client;
        _logger = logger;

        // Subscribe to events
        _client.QrReceived += OnQrReceived;
        _client.Authenticated += OnAuthenticated;
        _client.Ready += OnReady;
        _client.MessageReceived += OnMessageReceived;
        _client.Disconnected += OnDisconnected;
    }

    /// <summary>
    /// Starts the WhatsApp service
    /// </summary>
    public async Task StartAsync()
    {
        try
        {
            _logger.LogInformation("Starting WhatsApp service...");

            // Initialize the client
            await _client.InitializeAsync();

            // Start authentication
            await _client.AuthenticateAsync();

            _logger.LogInformation("WhatsApp service started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start WhatsApp service");
            throw;
        }
    }

    /// <summary>
    /// Stops the WhatsApp service
    /// </summary>
    public async Task StopAsync()
    {
        try
        {
            _logger.LogInformation("Stopping WhatsApp service...");
            await _client.DestroyAsync();
            _logger.LogInformation("WhatsApp service stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping WhatsApp service");
        }
    }

    private void OnQrReceived(object? sender, QrEventArgs e)
    {
        _logger.LogInformation("QR Code received. Please scan with your WhatsApp mobile app.");
        Console.WriteLine("\n=== QR CODE ===");
        Console.WriteLine("Please scan this QR code with your WhatsApp mobile app:");
        Console.WriteLine(e.QrCode);
        Console.WriteLine("===============\n");
    }

    private void OnAuthenticated(object? sender, AuthenticationEventArgs e)
    {
        if (e.IsSuccess)
        {
            _logger.LogInformation("Successfully authenticated with WhatsApp");
            Console.WriteLine("✅ Authenticated successfully!");
        }
        else
        {
            _logger.LogError("Authentication failed: {Error}", e.ErrorMessage);
            Console.WriteLine($"❌ Authentication failed: {e.ErrorMessage}");
        }
    }

    private async void OnReady(object? sender, EventArgs e)
    {
        _logger.LogInformation("WhatsApp client is ready");
        Console.WriteLine("🚀 WhatsApp client is ready!");

        try
        {
            // Example: Get all chats
            var chats = await _client.GetChatsAsync();
            _logger.LogInformation("Found {ChatCount} chats", chats.Count);
            Console.WriteLine($"Found {chats.Count} chats");

            // Example: Get all contacts
            var contacts = await _client.GetContactsAsync();
            _logger.LogInformation("Found {ContactCount} contacts", contacts.Count);
            Console.WriteLine($"Found {contacts.Count} contacts");

            // Example: Send a message (replace with actual chat ID)
            // await _client.SendMessageAsync("1234567890@c.us", "Hello from WhatsAppDotnet!");

            Console.WriteLine("\n📱 WhatsApp client is now active. You can:");
            Console.WriteLine("- Receive messages automatically");
            Console.WriteLine("- Send messages programmatically");
            Console.WriteLine("- Manage chats and contacts");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during ready operations");
        }
    }

    private async void OnMessageReceived(object? sender, MessageEventArgs e)
    {
        var message = e.Message;
        _logger.LogInformation("Message received from {From}: {Body}", message.From, message.Body);
        
        Console.WriteLine($"\n💬 New message:");
        Console.WriteLine($"   From: {message.From}");
        Console.WriteLine($"   Body: {message.Body}");
        Console.WriteLine($"   Time: {DateTimeOffset.FromUnixTimeSeconds(message.Timestamp):yyyy-MM-dd HH:mm:ss}");

        // Example: Auto-reply to messages (uncomment to enable)
        /*
        if (!message.FromMe && message.Body.ToLower().Contains("hello"))
        {
            try
            {
                await message.ReplyAsync("Hello! This is an automated reply from WhatsAppDotnet.");
                _logger.LogInformation("Sent auto-reply to {From}", message.From);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send auto-reply");
            }
        }
        */
    }

    private void OnDisconnected(object? sender, DisconnectionEventArgs e)
    {
        _logger.LogWarning("WhatsApp client disconnected: {Reason}", e.Reason);
        Console.WriteLine($"⚠️ Disconnected: {e.Reason}");
    }
}
