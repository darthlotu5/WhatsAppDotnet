![WhatsApp.Client Logo](assets/logo.png)

# WhatsApp.Client - .NET WhatsApp Web Automation Library

[![NuGet Version](https://img.shields.io/nuget/v/WhatsApp.Client.svg)](https://www.nuget.org/packages/WhatsApp.Client/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/WhatsApp.Client.svg)](https://www.nuget.org/packages/WhatsApp.Client/)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)

> **WhatsApp Bot & Automation SDK for .NET** | Alternative to WhatsApp Business API | C# WhatsApp Web Wrapper

A comprehensive **C# .NET library** for **WhatsApp Web automation** using browser automation. Perfect for building **WhatsApp bots**, **business messaging solutions**, and **WhatsApp integrations** in .NET applications. 

This library is a **C# port of the popular wajs/whatsapp-web.js** JavaScript library, bringing full WhatsApp Web functionality to the .NET ecosystem with **strong typing**, **async/await patterns**, and **modern .NET architecture**.

## 🚀 Why Choose WhatsApp.Client?

- **🆓 Free Alternative** to WhatsApp Business API - No API costs or approval process
- **🔧 Full WhatsApp Web Access** - Everything you can do in WhatsApp Web, you can automate
- **⚡ .NET Native** - Built specifically for C#/.NET developers with proper async patterns
- **🎯 Type-Safe** - Strong typing throughout with comprehensive IntelliSense support
- **🔄 Event-Driven** - Real-time events for messages, status changes, and more
- **📱 Cross-Platform** - Works on Windows, macOS, and Linux
- **🏗️ Production Ready** - Built with enterprise patterns (DI, logging, configuration)

## Features

- **Browser Automation**: Uses Microsoft Playwright for reliable WhatsApp Web interaction
- **Event-Driven Architecture**: Real-time events for messages, authentication, and state changes
- **Strong Typing**: Full C# type safety with comprehensive IntelliSense support
- **Async/Await Support**: Modern .NET async patterns for better performance
- **Dependency Injection Ready**: Built with modern .NET hosting and DI patterns
- **Cross-Platform**: Works on Windows, macOS, and Linux
- **Media Support**: Send and receive images, videos, documents, and stickers
- **Group Management**: Create, manage, and interact with WhatsApp groups
- **Contact Management**: Access and manage WhatsApp contacts
- **Message Operations**: Send, reply, forward, delete, and star messages

## 📋 Common Use Cases

**Business & Marketing:**
- **WhatsApp Chatbots** for customer support and lead generation
- **Bulk messaging** and marketing automation (within WhatsApp's terms)
- **Customer notifications** and appointment reminders
- **Order status updates** and delivery notifications

**Development & Integration:**
- **WhatsApp API alternative** for .NET applications
- **CRM integration** with WhatsApp messaging
- **Webhook replacements** for WhatsApp Business API
- **Desktop WhatsApp automation** tools

**Personal & Productivity:**
- **Message scheduling** and auto-replies
- **Group management** and moderation bots
- **Media backup** and organization tools
- **Cross-platform WhatsApp** desktop applications

## 🔄 Migrating from JavaScript?

Coming from **whatsapp-web.js**, **wajs**, or other JavaScript WhatsApp libraries? This library provides a familiar API structure while leveraging .NET's strong typing and performance benefits.

```javascript
// JavaScript (whatsapp-web.js)
client.on('message', msg => { ... });
client.sendMessage(chatId, 'Hello');
```

```csharp
// C# (WhatsApp.Client)
client.MessageReceived += (sender, e) => { ... };
await client.SendMessageAsync(chatId, "Hello");
```

## Prerequisites

- .NET 8.0 or later
- One of the supported browsers (Chromium, Firefox, or WebKit) - automatically installed via Playwright

## Installation

### NuGet Package Manager
```bash
Install-Package WhatsApp.Client
```

### .NET CLI
```bash
dotnet add package WhatsApp.Client
```

### PackageReference
```xml
<PackageReference Include="WhatsApp.Client" Version="1.0.0" />
```

## Quick Start

### Basic Usage

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WhatsApp.Client;
using WhatsApp.Client.Utilities;

// Create and configure the WhatsApp client
var options = new WhatsAppClientOptions
{
    PlaywrightOptions = { Headless = false }, // Show browser for QR scanning
    QrMaxRetries = 3,
    SessionName = "my_whatsapp_session"
};

using var client = new WhatsAppClient(options);

// Subscribe to events
client.QrReceived += (sender, e) => 
{
    Console.WriteLine("Scan this QR code with WhatsApp:");
    Console.WriteLine(e.QrCode);
};

client.Ready += async (sender, e) => 
{
    Console.WriteLine("WhatsApp client is ready!");
    
    // Send a message
    await client.SendMessageAsync("1234567890@c.us", "Hello from WhatsApp.Client!");
};

client.MessageReceived += (sender, e) => 
{
    Console.WriteLine($"New message from {e.Message.From}: {e.Message.Body}");
};

// Initialize and authenticate
await client.InitializeAsync();
await client.AuthenticateAsync();

// Keep running
Console.ReadLine();
```

### Using with Dependency Injection

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<WhatsAppClientOptions>(options =>
        {
            options.PlaywrightOptions.Headless = false;
            options.SessionName = "bot_session";
        });
        
        services.AddSingleton<WhatsAppClient>();
        services.AddHostedService<WhatsAppBot>();
    })
    .Build();

await host.RunAsync();
```

## Core Concepts

### Client Initialization

The `WhatsAppClient` is the main entry point for all WhatsApp operations:

```csharp
var options = new WhatsAppClientOptions
{
    PlaywrightOptions = new()
    {
        Headless = true,          // Run browser in background
        SlowMo = 100             // Slow down operations for stability
    },
    QrMaxRetries = 5,            // Max QR code refresh attempts
    SessionName = "my_session",   // Session persistence name
    UserAgent = "Custom-Agent",   // Custom user agent
    Browser = "chromium"          // Browser type (chromium/firefox/webkit)
};

var client = new WhatsAppClient(options);
```

### Event Handling

Subscribe to events for real-time WhatsApp interactions:

```csharp
// Authentication events
client.QrReceived += OnQrReceived;
client.Authenticated += OnAuthenticated;
client.AuthenticationFailed += OnAuthFailed;
client.Ready += OnReady;

// Message events
client.MessageReceived += OnMessageReceived;
client.MessageCreated += OnMessageCreated;

// State events
client.StateChanged += OnStateChanged;
client.Disconnected += OnDisconnected;
```

### Sending Messages

```csharp
// Simple text message
await client.SendMessageAsync("1234567890@c.us", "Hello World!");

// Message with options
var options = new MessageOptions
{
    LinkPreview = false,
    Mentions = new[] { "0987654321@c.us" }
};
await client.SendMessageAsync("group@g.us", "Hello @user!", options);

// Reply to a message
await message.ReplyAsync("Thanks for your message!");

// Forward a message
await message.ForwardAsync("friend@c.us");
```

### Working with Chats

```csharp
// Get all chats
var chats = await client.GetChatsAsync();

// Find a specific chat
var chat = chats.FirstOrDefault(c => c.Name.Contains("Family"));

// Send message to chat
await chat.SendMessageAsync("Hello family!");

// Mark as read
await chat.MarkAsReadAsync();

// Archive/unarchive
await chat.ArchiveAsync();
await chat.UnarchiveAsync();

// Pin/unpin
await chat.PinAsync();
await chat.UnpinAsync();
```

### Group Management

```csharp
// Cast to group chat
if (chat is GroupChat groupChat)
{
    // Get participants
    var participants = await groupChat.GetParticipantsAsync();
    
    // Add participants
    await groupChat.AddParticipantsAsync("newuser@c.us");
    
    // Remove participants
    await groupChat.RemoveParticipantsAsync("olduser@c.us");
    
    // Promote to admin
    await groupChat.PromoteParticipantsAsync("user@c.us");
    
    // Change group info
    await groupChat.SetSubjectAsync("New Group Name");
    await groupChat.SetDescriptionAsync("Updated description");
    
    // Get invite link
    var inviteLink = await groupChat.GetInviteLinkAsync();
}
```

### Contact Management

```csharp
// Get all contacts
var contacts = await client.GetContactsAsync();

// Filter business contacts
var businessContacts = contacts.OfType<BusinessContact>().ToList();

// Get contact details
foreach (var contact in contacts)
{
    var profilePic = await contact.GetProfilePictureAsync();
    var about = await contact.GetAboutAsync();
    
    Console.WriteLine($"{contact.Name}: {about}");
}

// Block/unblock contacts
await contact.BlockAsync();
await contact.UnblockAsync();
```

## Configuration Options

### WhatsAppClientOptions

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `PlaywrightOptions` | `BrowserTypeLaunchOptions` | `{ Headless = true }` | Playwright browser launch options |
| `QrMaxRetries` | `int` | `0` | Maximum QR code refresh attempts |
| `UserAgent` | `string` | Chrome UA | Browser user agent string |
| `SessionName` | `string` | `.whatsapp_dotnet_auth` | Session persistence name |
| `SessionPath` | `string` | `session` | Session storage directory |
| `Browser` | `string` | `chromium` | Browser type (chromium/firefox/webkit) |
| `TakeoverOnConflict` | `bool` | `false` | Take over existing WhatsApp sessions |

### Playwright Options

```csharp
PlaywrightOptions = new()
{
    Headless = false,           // Show browser window
    SlowMo = 50,               // Slow down operations (ms)
    Timeout = 30000,           // Default timeout (ms)
    Args = new[]               // Custom browser arguments
    {
        "--no-sandbox",
        "--disable-setuid-sandbox"
    }
}
```

## Advanced Usage

### Custom Message Handler

```csharp
public class MessageHandler
{
    private readonly WhatsAppClient _client;
    
    public MessageHandler(WhatsAppClient client)
    {
        _client = client;
        _client.MessageReceived += HandleMessage;
    }
    
    private async void HandleMessage(object sender, MessageEventArgs e)
    {
        var message = e.Message;
        
        // Skip own messages
        if (message.FromMe) return;
        
        // Command handling
        if (message.Body.StartsWith("/"))
        {
            await HandleCommand(message);
            return;
        }
        
        // Auto-reply to specific keywords
        if (message.Body.ToLower().Contains("help"))
        {
            await message.ReplyAsync("How can I assist you?");
        }
    }
    
    private async Task HandleCommand(Message message)
    {
        var command = message.Body.Split(' ')[0].ToLower();
        
        switch (command)
        {
            case "/ping":
                await message.ReplyAsync("Pong!");
                break;
                
            case "/time":
                await message.ReplyAsync($"Current time: {DateTime.Now:HH:mm:ss}");
                break;
                
            case "/joke":
                await message.ReplyAsync("Why don't scientists trust atoms? Because they make up everything!");
                break;
                
            default:
                await message.ReplyAsync("Unknown command. Try /help for available commands.");
                break;
        }
    }
}
```

### Media Handling

```csharp
client.MessageReceived += async (sender, e) =>
{
    var message = e.Message;
    
    if (message.HasMedia)
    {
        // Download media
        var mediaData = await message.DownloadMediaAsync();
        
        if (mediaData != null)
        {
            // Save to file
            var filename = $"media_{message.Id.Id}.bin";
            await File.WriteAllBytesAsync(filename, mediaData);
            
            // Process media (resize, convert, etc.)
            // await ProcessMediaFile(filename);
        }
    }
};

// Send media
var media = new MessageMedia
{
    MimeType = "image/jpeg",
    Data = await File.ReadAllBytesAsync("photo.jpg"),
    Filename = "photo.jpg",
    Caption = "Check out this photo!"
};

await chat.SendMediaAsync(media);
```

### Session Persistence

```csharp
var options = new WhatsAppClientOptions
{
    SessionName = "persistent_bot",
    SessionPath = "./sessions"
};

// Session data is automatically saved and restored
// No need to scan QR code again after first authentication
```

## Error Handling

```csharp
try
{
    await client.InitializeAsync();
    await client.AuthenticateAsync();
}
catch (TimeoutException)
{
    Console.WriteLine("Authentication timed out");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Client error: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");
}
finally
{
    await client.DestroyAsync();
}
```

## Logging

WhatsApp.Client uses Microsoft.Extensions.Logging:

```csharp
var services = new ServiceCollection()
    .AddLogging(builder =>
    {
        builder.AddConsole()
               .AddFile("logs/whatsapp-{Date}.txt")
               .SetMinimumLevel(LogLevel.Information);
    })
    .AddSingleton<WhatsAppClient>();

var serviceProvider = services.BuildServiceProvider();
var client = serviceProvider.GetService<WhatsAppClient>();
```

## CI/CD and Publishing

This project uses GitHub Actions for continuous integration and automated NuGet publishing.

### Automated Builds

Every push to the `main` branch and pull requests trigger:
- .NET 8 build and compilation
- Automated testing (when tests are available)
- NuGet package creation
- Artifact upload for review

### Automated Publishing to NuGet

Publishing to NuGet.org is triggered automatically when you create a version tag:

1. **Update the version** in `src/WhatsApp.Client/WhatsApp.Client.csproj` using [semantic versioning](https://semver.org/):
   ```xml
   <Version>1.0.1</Version>  <!-- MAJOR.MINOR.PATCH -->
   ```
   
   **Semantic Versioning Guidelines:**
   - **PATCH** (1.0.X): Bug fixes and backwards-compatible changes
   - **MINOR** (1.X.0): New features that are backwards-compatible
   - **MAJOR** (X.0.0): Breaking changes that are not backwards-compatible
   - **Pre-release** (1.0.0-alpha, 1.0.0-beta.1): Development versions

2. **Validate everything is ready** (recommended):
   ```bash
   ./validate-publish.sh
   ```

3. **Create and push a semantic version tag**:
   ```bash
   git tag v1.0.1        # Must match project version exactly
   git push origin v1.0.1
   ```

4. **GitHub Actions will automatically**:
   - Build the project
   - Run tests
   - Create NuGet package
   - Publish to NuGet.org (requires `NUGET_API_KEY` secret)
   - Create a GitHub release with package artifacts

### Required GitHub Secrets

To enable automated NuGet publishing, add this secret to your GitHub repository:

- `NUGET_API_KEY`: Your NuGet.org API key
  1. Go to [NuGet.org](https://www.nuget.org/account/apikeys)
  2. Create a new API key with "Push new packages and package versions" scope
  3. Add it as a repository secret in GitHub Settings > Secrets and variables > Actions

### Manual Publishing

For manual publishing, see the detailed [Publishing Guide](PUBLISHING.md).

## Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/new-feature`
3. Make your changes and add tests
4. Commit your changes: `git commit -m 'Add new feature'`
5. Push to the branch: `git push origin feature/new-feature`
6. Submit a pull request

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE](LICENSE) file for details.

## Disclaimer

This library is not affiliated with, authorized, maintained, sponsored or endorsed by WhatsApp or any of its affiliates or subsidiaries. This is an independent and unofficial library. Use at your own risk.

## Support

- [Documentation](https://github.com/your-username/WhatsApp.Client/wiki)
- [Issue Tracker](https://github.com/your-username/WhatsApp.Client/issues)
- [Discussions](https://github.com/darthlotu5/WhatsApp.Client/discussions)

## Acknowledgments

- Original [whatsapp-web.js](https://github.com/pedroslopez/whatsapp-web.js) library
- [wajs](https://github.com/DikaArdnt/wajs) JavaScript library that inspired this port
- [Microsoft Playwright](https://playwright.dev/) for browser automation
- The WhatsApp community for their continuous support and feedback
