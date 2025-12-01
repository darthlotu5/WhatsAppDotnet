<div align="center">
  <img src="assets/logo.png" alt="WhatsApp.Client Logo" width="120" height="120">
</div>

# Contributing to WhatsApp.Client

Thank you for your interest in contributing to WhatsApp.Client! We welcome contributions from the community and are pleased to have you join us.

## How to Contribute

### Reporting Issues

1. **Check existing issues** first to avoid duplicates
2. **Use issue templates** when available
3. **Provide clear descriptions** with steps to reproduce
4. **Include system information** (.NET version, OS, etc.)

### Suggesting Features

1. **Open a feature request** issue first
2. **Describe the use case** and expected behavior
3. **Consider the scope** and impact on existing users
4. **Be open to discussion** and feedback

### Code Contributions

1. **Fork the repository** and create a feature branch
2. **Follow coding standards** (see below)
3. **Write or update tests** for new functionality
4. **Update documentation** as needed
5. **Submit a pull request** with clear description

## Development Setup

### Prerequisites

- .NET 8.0 SDK or later
- Git
- Your favorite IDE (Visual Studio, VS Code, Rider, etc.)

### Getting Started

```bash
# Clone your fork
git clone https://github.com/your-username/WhatsApp.Client.git
cd WhatsApp.Client

# Add upstream remote
git remote add upstream https://github.com/original-owner/WhatsApp.Client.git

# Install dependencies
dotnet restore

# Build the project
dotnet build

# Run tests (when available)
dotnet test
```

## Coding Standards

### C# Style Guidelines

- Follow [Microsoft's C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/inside-a-program/coding-conventions)
- Use **PascalCase** for public members
- Use **camelCase** for private members and parameters
- Use **meaningful names** for variables and methods
- **Document public APIs** with XML comments

### Code Organization

- Keep classes **focused and cohesive**
- Use **async/await** for asynchronous operations
- Handle exceptions **appropriately** with logging
- Follow **SOLID principles** where applicable

### Example

```csharp
/// <summary>
/// Sends a message to the specified chat
/// </summary>
/// <param name="chatId">The target chat identifier</param>
/// <param name="message">The message content to send</param>
/// <param name="cancellationToken">Cancellation token</param>
/// <returns>The sent message or null if failed</returns>
public async Task<Message?> SendMessageAsync(
    string chatId, 
    string message, 
    CancellationToken cancellationToken = default)
{
    if (string.IsNullOrWhiteSpace(chatId))
        throw new ArgumentException("Chat ID cannot be null or empty", nameof(chatId));
    
    try
    {
        _logger.LogDebug("Sending message to chat {ChatId}", chatId);
        // Implementation here
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to send message to chat {ChatId}", chatId);
        return null;
    }
}
```

## 🧪 Testing Guidelines

### Test Structure

- Use **descriptive test names** that explain the scenario
- Follow **AAA pattern** (Arrange, Act, Assert)
- Use **test fixtures** for setup and cleanup
- Mock external dependencies

### Example Test

```csharp
[Fact]
public async Task SendMessageAsync_WithValidChatId_ShouldReturnMessage()
{
    // Arrange
    var client = new WhatsAppClient(options, logger);
    var chatId = "1234567890@c.us";
    var messageText = "Hello World";
    
    // Act
    var result = await client.SendMessageAsync(chatId, messageText);
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal(messageText, result.Body);
}
```

## Documentation

### XML Comments

All public APIs must have XML documentation:

```csharp
/// <summary>
/// Brief description of what the method does
/// </summary>
/// <param name="paramName">Description of the parameter</param>
/// <returns>Description of what is returned</returns>
/// <exception cref="ExceptionType">When this exception is thrown</exception>
```

### README Updates

- Update examples if you change public APIs
- Add new features to the feature list
- Update installation instructions if needed

## 🔄 Pull Request Process

### Before Submitting

1. **Sync with upstream**: `git pull upstream main`
2. **Run tests**: Ensure all tests pass
3. **Build successfully**: No compilation errors or warnings
4. **Update documentation**: Include relevant updates

### PR Description Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
- [ ] Added new tests
- [ ] All existing tests pass
- [ ] Manual testing performed

## Checklist
- [ ] Code follows style guidelines
- [ ] Self-review completed
- [ ] Documentation updated
- [ ] No breaking changes (or marked as such)
```

### Review Process

1. **Automated checks** must pass (build, tests, linting)
2. **Code review** by maintainers
3. **Address feedback** and make requested changes
4. **Final approval** and merge by maintainers

## Issue Labels

We use labels to categorize issues:

- **bug**: Something isn't working
- **enhancement**: New feature or improvement
- **documentation**: Documentation needs
- **good first issue**: Good for newcomers
- **help wanted**: Community help needed
- **question**: Further information requested

## Tips for Contributors

### Getting Started

- Look for **"good first issue"** labels
- Start with **documentation improvements**
- **Ask questions** if something is unclear
- **Be patient** with the review process

### Best Practices

- **Keep PRs focused** and small when possible
- **Write clear commit messages**
- **Reference issues** in commits (`fixes #123`)
- **Be responsive** to review feedback

### Communication

- **Be respectful** and constructive
- **Use clear, professional language**
- **Provide context** for your changes
- **Ask for help** when needed

## 📞 Getting Help

### Where to Ask

- **GitHub Issues**: For bugs and feature requests
- **GitHub Discussions**: For questions and general discussion
- **Code Comments**: For specific implementation questions

### Response Times

- We aim to respond to issues within **48 hours**
- Complex issues may take longer to resolve
- PRs are typically reviewed within **one week**

## 🎉 Recognition

Contributors will be:

- **Listed in the repository** contributors section
- **Mentioned in release notes** for significant contributions
- **Invited to join** the maintainers team for ongoing contributors

## 📄 Code of Conduct

This project adheres to a code of conduct. By participating, you are expected to uphold this code. Please report unacceptable behavior to the maintainers.

### Our Pledge

- **Be welcoming** to newcomers
- **Be respectful** of differing viewpoints
- **Accept constructive criticism** gracefully
- **Focus on community benefit**

---

Thank you for contributing to WhatsApp.Client! Your efforts help make this library better for everyone.
