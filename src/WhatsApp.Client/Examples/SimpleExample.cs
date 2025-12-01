using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WhatsAppDotnet;
using WhatsAppDotnet.Events;
using WhatsAppDotnet.Utilities;

namespace WhatsAppDotnet.Examples;

/// <summary>
/// Simple console example for WhatsAppDotnet
/// </summary>
public class SimpleExample
{
    /// <summary>
    /// Example entry point
    /// </summary>
    /// <param name="args">Command line arguments</param>
    public static async Task Main(string[] args)
    {
        Console.WriteLine("🚀 WhatsAppDotnet Simple Example");
        Console.WriteLine("================================");

        // Configure options
        var options = new WhatsAppClientOptions
        {
            PlaywrightOptions = { Headless = false }, // Show browser for QR scanning
            QrMaxRetries = 3,
            SessionName = "simple_example_session"
        };

        // Create logger factory
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole().SetMinimumLevel(LogLevel.Information);
        });
        var logger = loggerFactory.CreateLogger<WhatsAppClient>();

        // Create WhatsApp client
        using var client = new WhatsAppClient(options, logger);

        // Set up event handlers
        SetupEventHandlers(client);

        try
        {
            Console.WriteLine("📱 Initializing WhatsApp client...");
            await client.InitializeAsync();

            Console.WriteLine("🔐 Starting authentication...");
            await client.AuthenticateAsync();

            Console.WriteLine("✅ Client is ready! Press 'q' to quit or any other key to send a test message.");
            
            // Main loop
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.KeyChar == 'q' || key.KeyChar == 'Q')
                {
                    break;
                }
                else if (key.KeyChar == 't' || key.KeyChar == 'T')
                {
                    await SendTestMessage(client);
                }
                else if (key.KeyChar == 'c' || key.KeyChar == 'C')
                {
                    await ShowChats(client);
                }
                else if (key.KeyChar == 'h' || key.KeyChar == 'H')
                {
                    ShowHelp();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("🛑 Shutting down...");
            await client.DestroyAsync();
        }
    }

    private static void SetupEventHandlers(WhatsAppClient client)
    {
        client.QrReceived += (sender, e) =>
        {
            Console.WriteLine("\n📱 QR Code received!");
            Console.WriteLine("Please scan the following QR code with your WhatsApp mobile app:");
            Console.WriteLine("=======================================");
            // In a real implementation, you might want to generate a QR code image
            // or use a QR code library to display it properly in the console
            Console.WriteLine(e.QrCode);
            Console.WriteLine("=======================================");
        };

        client.Authenticated += (sender, e) =>
        {
            if (e.IsSuccess)
            {
                Console.WriteLine("✅ Authentication successful!");
            }
            else
            {
                Console.WriteLine($"❌ Authentication failed: {e.ErrorMessage}");
            }
        };

        client.Ready += (sender, e) =>
        {
            Console.WriteLine("🎉 WhatsApp client is ready!");
            ShowHelp();
        };

        client.MessageReceived += (sender, e) =>
        {
            var msg = e.Message;
            var time = DateTimeOffset.FromUnixTimeSeconds(msg.Timestamp).ToString("HH:mm:ss");
            Console.WriteLine($"\n💬 [{time}] New message from {msg.From}: {msg.Body}");
            
            // Auto-reply example (commented out to avoid spam)
            /*
            if (!msg.FromMe && msg.Body.ToLower().Contains("ping"))
            {
                _ = Task.Run(async () =>
                {
                    await msg.ReplyAsync("Pong! 🏓");
                });
            }
            */
        };

        client.StateChanged += (sender, e) =>
        {
            Console.WriteLine($"📊 State changed: {e.PreviousState} → {e.NewState}");
        };

        client.Disconnected += (sender, e) =>
        {
            Console.WriteLine($"⚠️ Disconnected: {e.Reason}");
        };
    }

    private static async Task SendTestMessage(WhatsAppClient client)
    {
        Console.WriteLine("Enter chat ID (e.g., 1234567890@c.us for individual or 1234567890@g.us for group):");
        var chatId = Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(chatId))
        {
            Console.WriteLine("❌ Invalid chat ID");
            return;
        }

        Console.WriteLine("Enter message text:");
        var messageText = Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(messageText))
        {
            Console.WriteLine("❌ Invalid message text");
            return;
        }

        try
        {
            var message = await client.SendMessageAsync(chatId, messageText);
            if (message != null)
            {
                Console.WriteLine("✅ Message sent successfully!");
            }
            else
            {
                Console.WriteLine("❌ Failed to send message");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error sending message: {ex.Message}");
        }
    }

    private static async Task ShowChats(WhatsAppClient client)
    {
        try
        {
            Console.WriteLine("📋 Loading chats...");
            var chats = await client.GetChatsAsync();
            
            Console.WriteLine($"\n📊 Found {chats.Count} chats:");
            Console.WriteLine("=================================");
            
            for (int i = 0; i < Math.Min(chats.Count, 10); i++) // Show first 10 chats
            {
                var chat = chats[i];
                var type = chat.IsGroup ? "👥" : "👤";
                var unread = chat.UnreadCount > 0 ? $" ({chat.UnreadCount} unread)" : "";
                Console.WriteLine($"{i + 1:D2}. {type} {chat.Name}{unread}");
                Console.WriteLine($"    ID: {chat.Id}");
            }
            
            if (chats.Count > 10)
            {
                Console.WriteLine($"... and {chats.Count - 10} more chats");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error loading chats: {ex.Message}");
        }
    }

    private static void ShowHelp()
    {
        Console.WriteLine("\n📖 Available commands:");
        Console.WriteLine("======================");
        Console.WriteLine("T - Send a test message");
        Console.WriteLine("C - Show chats");
        Console.WriteLine("H - Show this help");
        Console.WriteLine("Q - Quit");
        Console.WriteLine("\nPress any key...\n");
    }
}
