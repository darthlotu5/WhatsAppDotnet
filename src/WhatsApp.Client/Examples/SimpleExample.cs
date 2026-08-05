using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WhatsAppDotnet;
using WhatsAppDotnet.Events;
using WhatsAppDotnet.Structures;
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
                else if (key.KeyChar == 'b' || key.KeyChar == 'B')
                {
                    await SendButtonsTest(client);
                }
                else if (key.KeyChar == 'l' || key.KeyChar == 'L')
                {
                    await SendListTest(client);
                }
                else if (key.KeyChar == 'i' || key.KeyChar == 'I')
                {
                    await SendImageTest(client);
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

    private static async Task<string?> PromptChatIdAsync()
    {
        Console.WriteLine("Enter chat ID (e.g., 1234567890@c.us for individual or 1234567890@g.us for group):");
        var chatId = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(chatId))
        {
            Console.WriteLine("❌ Invalid chat ID");
            return null;
        }
        return chatId;
    }

    private static async Task SendButtonsTest(WhatsAppClient client)
    {
        var chatId = await PromptChatIdAsync();
        if (chatId == null) return;

        // ⚠️ Buttons are an unofficial, unsupported mechanism (see the
        // warning on WhatsAppClient.SendButtonsAsync's XML doc comment) —
        // WPPConnect themselves say they can stop working at any time.
        try
        {
            var message = await client.SendButtonsAsync(
                chatId,
                "Choose an option below:",
                new List<ButtonOption>
                {
                    ButtonOption.QuickReply("opt_yes", "Yes"),
                    ButtonOption.QuickReply("opt_no", "No"),
                    ButtonOption.Link("https://github.com/darthlotu5/WhatsAppDotnet", "View on GitHub"),
                },
                title: "Quick question",
                footer: "Sent via WhatsAppDotnet");

            Console.WriteLine(message != null ? "✅ Buttons message sent!" : "❌ Failed to send buttons message");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error sending buttons message: {ex.Message}");
        }
    }

    private static async Task SendListTest(WhatsAppClient client)
    {
        var chatId = await PromptChatIdAsync();
        if (chatId == null) return;

        try
        {
            var message = await client.SendListAsync(
                chatId,
                buttonText: "View options",
                description: "Pick the option that fits you best",
                sections: new List<ListSection>
                {
                    new ListSection
                    {
                        Title = "Plans",
                        Rows = new List<ListMessageRow>
                        {
                            new() { Title = "Basic", Description = "Free tier", RowId = "plan_basic" },
                            new() { Title = "Pro", Description = "$10/month", RowId = "plan_pro" },
                        }
                    }
                },
                title: "Choose a plan",
                footer: "Sent via WhatsAppDotnet");

            Console.WriteLine(message != null ? "✅ List message sent!" : "❌ Failed to send list message");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error sending list message: {ex.Message}");
        }
    }

    private static async Task SendImageTest(WhatsAppClient client)
    {
        var chatId = await PromptChatIdAsync();
        if (chatId == null) return;

        Console.WriteLine("Enter the local path to an image file (jpg/png/webp):");
        var imagePath = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
        {
            Console.WriteLine("❌ Invalid or missing image file");
            return;
        }

        try
        {
            var message = await client.SendImageAsync(chatId, imagePath, caption: "Sent via WhatsAppDotnet");
            Console.WriteLine(message != null ? "✅ Image sent!" : "❌ Failed to send image");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error sending image: {ex.Message}");
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
        Console.WriteLine("B - Send a buttons message (up to 3 native-flow buttons)");
        Console.WriteLine("L - Send a list message (sectioned picker)");
        Console.WriteLine("I - Send an image (with an optional caption + up to 2 buttons)");
        Console.WriteLine("H - Show this help");
        Console.WriteLine("Q - Quit");
        Console.WriteLine("\nPress any key...\n");
    }
}
