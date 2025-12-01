<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

# WhatsApp.Client Project Instructions

This is a C# .NET class library that provides WhatsApp Web API functionality using Playwright for browser automation. It's a port of the JavaScript WhatsApp library (wajs) to .NET.

## Architecture

- **WhatsAppClient**: Main client class for interacting with WhatsApp Web
- **Structures**: Data models for messages, contacts, chats, etc.
- **Events**: Event argument classes for WhatsApp events  
- **Utilities**: Helper classes, constants, and configuration options
- **Factories**: Factory classes for creating structured objects

## Key Features

- Browser automation using Microsoft.Playwright
- Event-driven architecture using C# events
- Strong typing throughout the library
- Async/await patterns for better performance
- Modern .NET patterns and dependency injection support

## Dependencies

- Microsoft.Playwright - Browser automation
- Microsoft.Extensions.Hosting - Application hosting
- Microsoft.Extensions.Logging - Logging infrastructure
- Microsoft.Extensions.Options - Configuration system
- System.Text.Json - JSON serialization
- FFMpegCore - Media processing

## Development Guidelines

1. Follow async/await patterns consistently
2. Use proper exception handling with logging
3. Maintain strong typing throughout
4. Follow C# naming conventions
5. Add comprehensive XML documentation
6. Use dependency injection where appropriate
7. Implement proper IDisposable patterns for resource cleanup

## Event System

The library uses C# events for real-time WhatsApp interactions:
- QrReceived - When QR code is generated for authentication
- Authenticated/AuthenticationFailed - Authentication status
- Ready - When client is ready to use
- MessageReceived/MessageCreated - Message events
- StateChanged - Client state changes
- Disconnected - Connection loss events

## Browser Integration

Uses Playwright to control WhatsApp Web:
- Supports Chromium, Firefox, and WebKit browsers
- Handles QR code authentication
- Injects JavaScript for WhatsApp Web interaction
- Manages browser sessions and state

When adding new functionality, ensure it follows the established patterns and maintains compatibility with the original JavaScript library's API design.
