<div align="center">
  <img src="../../assets/logo.png" alt="WhatsApp.Client Logo" width="150" height="150">
  
  # WhatsApp.Client

  [![NuGet Version](https://img.shields.io/nuget/v/WhatsApp.Client.svg)](https://www.nuget.org/packages/WhatsApp.Client/)
  [![NuGet Downloads](https://img.shields.io/nuget/dt/WhatsApp.Client.svg)](https://www.nuget.org/packages/WhatsApp.Client/)
  [![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](../../LICENSE)
  [![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
</div>

A comprehensive C# .NET library for interacting with the WhatsApp Web API using browser automation. This library is a port of the popular JavaScript WhatsApp library (wajs) to .NET, providing developers with a powerful and type-safe way to build WhatsApp bots and automation tools.

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

## Quick Start

### Installation

```bash
dotnet add package WhatsApp.Client
```

### Basic Usage

```csharp
using WhatsApp.Client;
using WhatsApp.Client.Utilities;

var options = new WhatsAppClientOptions
{
    PlaywrightOptions = { Headless = false },
    QrMaxRetries = 3,
    SessionName = "my_whatsapp_session"
};

using var client = new WhatsAppClient(options);

client.QrReceived += (sender, e) => 
{
    Console.WriteLine("Scan this QR code with WhatsApp:");
    Console.WriteLine(e.QrCode);
};

client.Ready += async (sender, e) => 
{
    Console.WriteLine("WhatsApp client is ready!");
    await client.SendMessageAsync("1234567890@c.us", "Hello from WhatsApp.Client!");
};

await client.InitializeAsync();
await client.AuthenticateAsync();
```

## Documentation

- [Complete Documentation](../../README.md) - Full API documentation and examples
- [Contributing Guidelines](../../CONTRIBUTING.md) - How to contribute to the project
- [Publishing Guide](../../PUBLISHING.md) - Instructions for publishing to NuGet
- [Changelog](../../CHANGELOG.md) - Version history and release notes
- [License](../../LICENSE) - Apache 2.0 License

## Project Structure

```
src/WhatsApp.Client/
├── WhatsAppClient.cs          # Main client class
├── Events/                    # Event argument classes
├── Examples/                  # Usage examples
├── Factories/                 # Object factories
├── Structures/                # Data models
├── Utilities/                 # Utility classes and configuration
└── WhatsApp.Client.csproj     # Project file
```

## Requirements

- .NET 8.0 or later
- One of the supported browsers (automatically installed via Playwright)

## License

This project is licensed under the Apache License 2.0 - see the [LICENSE](../../LICENSE) file for details.

## Support

- [Issue Tracker](https://github.com/your-username/WhatsApp.Client/issues)
- [Discussions](https://github.com/your-username/WhatsApp.Client/discussions)

## Disclaimer

This library is not affiliated with, authorized, maintained, sponsored or endorsed by WhatsApp or any of its affiliates or subsidiaries. This is an independent and unofficial library. Use at your own risk.
