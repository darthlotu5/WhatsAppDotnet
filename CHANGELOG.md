# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] - 2025-12-01

### Added
- Initial release of WhatsApp.Client library
- Core WhatsApp Web API functionality using Microsoft Playwright
- Event-driven architecture with C# events for real-time interactions
- Strong typing throughout the library with comprehensive IntelliSense support
- Modern async/await patterns for better performance
- Dependency injection ready with Microsoft.Extensions.* integration
- Cross-platform support (Windows, macOS, Linux)

### Features
- **Browser Automation**
  - Microsoft Playwright integration for reliable WhatsApp Web interaction
  - Support for Chromium, Firefox, and WebKit browsers
  - Automatic browser installation and management
  
- **Authentication & Sessions**
  - QR code authentication flow
  - Session persistence across application restarts
  - Configurable session management
  
- **Message Management**
  - Send and receive text messages
  - Message replies and forwards
  - Message operations (delete, star, unstar)
  - Media message support (images, videos, documents)
  - Message acknowledgment tracking
  
- **Contact & Chat Management**
  - Retrieve all contacts and chats
  - Private chat and group chat support
  - Contact operations (block, unblock)
  - Chat operations (archive, pin, mute)
  
- **Group Features**
  - Group participant management
  - Admin operations (promote, demote participants)
  - Group settings (description, subject, invite links)
  
- **Configuration & Options**
  - Comprehensive configuration system
  - Auto-download settings for media
  - WhatsApp Web beta participation
  - Custom user agent and browser options
  
- **Developer Experience**
  - Extensive XML documentation
  - Comprehensive logging integration
  - Error handling and diagnostics
  - Example applications and usage patterns

### Dependencies
- Microsoft.Playwright (1.40.0) - Browser automation framework
- Microsoft.Extensions.Hosting (8.0.0) - Application hosting
- Microsoft.Extensions.Logging (8.0.0) - Logging infrastructure
- Microsoft.Extensions.Options (8.0.0) - Configuration system
- Microsoft.Extensions.DependencyInjection (8.0.0) - Dependency injection
- System.Text.Json (8.0.5) - JSON serialization
- FFMpegCore (5.0.2) - Media processing

### Target Framework
- .NET 8.0

### Notes
- This is a port of the popular JavaScript library [wajs](https://github.com/DikaArdnt/wajs)
- Maintains API compatibility while leveraging C#'s strong typing
- Licensed under Apache License 2.0
- Comprehensive examples and documentation included
