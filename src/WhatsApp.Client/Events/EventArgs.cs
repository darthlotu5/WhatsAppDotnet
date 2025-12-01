using WhatsAppDotnet.Structures;
using WhatsAppDotnet.Utilities;

namespace WhatsAppDotnet.Events;

/// <summary>
/// Base event arguments for WhatsApp events
/// </summary>
public abstract class WhatsAppEventArgs : EventArgs
{
}

/// <summary>
/// Event arguments for QR code events
/// </summary>
public class QrEventArgs : WhatsAppEventArgs
{
    /// <summary>
    /// The QR code data
    /// </summary>
    public string QrCode { get; }

    /// <summary>
    /// Initializes a new instance of the QrEventArgs class
    /// </summary>
    /// <param name="qrCode">The QR code data</param>
    public QrEventArgs(string qrCode)
    {
        QrCode = qrCode;
    }
}

/// <summary>
/// Event arguments for message events
/// </summary>
public class MessageEventArgs : WhatsAppEventArgs
{
    /// <summary>
    /// The message that triggered the event
    /// </summary>
    public Message Message { get; }

    /// <summary>
    /// Initializes a new instance of the MessageEventArgs class
    /// </summary>
    /// <param name="message">The message that triggered the event</param>
    public MessageEventArgs(Message message)
    {
        Message = message;
    }
}

/// <summary>
/// Event arguments for authentication events
/// </summary>
public class AuthenticationEventArgs : WhatsAppEventArgs
{
    /// <summary>
    /// Indicates if authentication was successful
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Error message if authentication failed
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Initializes a new instance of the AuthenticationEventArgs class
    /// </summary>
    /// <param name="isSuccess">Indicates if authentication was successful</param>
    /// <param name="errorMessage">Error message if authentication failed</param>
    public AuthenticationEventArgs(bool isSuccess, string? errorMessage = null)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }
}

/// <summary>
/// Event arguments for state change events
/// </summary>
public class StateChangeEventArgs : WhatsAppEventArgs
{
    /// <summary>
    /// The previous state
    /// </summary>
    public ClientStatus PreviousState { get; }

    /// <summary>
    /// The new state
    /// </summary>
    public ClientStatus NewState { get; }

    /// <summary>
    /// Initializes a new instance of the StateChangeEventArgs class
    /// </summary>
    /// <param name="previousState">The previous state</param>
    /// <param name="newState">The new state</param>
    public StateChangeEventArgs(ClientStatus previousState, ClientStatus newState)
    {
        PreviousState = previousState;
        NewState = newState;
    }
}

/// <summary>
/// Event arguments for disconnection events
/// </summary>
public class DisconnectionEventArgs : WhatsAppEventArgs
{
    /// <summary>
    /// The reason for disconnection
    /// </summary>
    public string Reason { get; }

    /// <summary>
    /// Initializes a new instance of the DisconnectionEventArgs class
    /// </summary>
    /// <param name="reason">The reason for disconnection</param>
    public DisconnectionEventArgs(string reason)
    {
        Reason = reason;
    }
}
