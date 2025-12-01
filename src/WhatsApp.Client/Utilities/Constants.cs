namespace WhatsAppDotnet.Utilities;

/// <summary>
/// Constants used throughout the WhatsApp client
/// </summary>
public static class Constants
{
    /// <summary>
    /// WhatsApp Web URL
    /// </summary>
    public const string WhatsWebUrl = "https://web.whatsapp.com/";

    /// <summary>
    /// Default user agent string
    /// </summary>
    public const string DefaultUserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14_0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/101.0.4951.67 Safari/537.36";

    /// <summary>
    /// Default device name
    /// </summary>
    public const string DefaultDeviceName = "WhatsAppDotnet";

    /// <summary>
    /// Default FFmpeg path
    /// </summary>
    public const string DefaultFfmpegPath = "ffmpeg";
}

/// <summary>
/// Client status enumeration
/// </summary>
public enum ClientStatus
{
    Initializing = 0,
    Authenticating = 1,
    Ready = 3
}

/// <summary>
/// Message acknowledgment status
/// </summary>
public enum MessageAck
{
    None = 0,
    Sent = 1,
    Delivered = 2,
    Read = 3,
    Played = 4
}

/// <summary>
/// Message types
/// </summary>
public enum MessageTypes
{
    Text,
    Audio,
    Voice,
    Image,
    Video,
    Document,
    Sticker,
    Location,
    VCard,
    MultiVCard,
    RevokeMember,
    Order,
    Product,
    Payment,
    Unknown
}

/// <summary>
/// Events that can be emitted by the client
/// </summary>
public static class Events
{
    public const string Authenticated = "authenticated";
    public const string AuthenticationFailure = "auth_failure";
    public const string Ready = "ready";
    public const string QrReceived = "qr";
    public const string ChatRemoved = "chat_removed";
    public const string ChatArchived = "chat_archived";
    public const string MessageReceived = "message";
    public const string MessageCiphertext = "message_ciphertext";
    public const string MessageCreate = "message_create";
    public const string MessageRevokedEveryone = "message_revoke_everyone";
    public const string MessageRevokedMe = "message_revoke_me";
    public const string MessageAck = "message_ack";
    public const string UnreadCount = "unread_count";
    public const string MessageReaction = "message_reaction";
    public const string MediaUploaded = "media_uploaded";
    public const string ContactChanged = "contact_changed";
    public const string GroupJoin = "group_join";
    public const string GroupLeave = "group_leave";
    public const string GroupAdminChanged = "group_admin_changed";
    public const string GroupMembershipRequest = "group_membership_request";
    public const string GroupUpdate = "group_update";
    public const string Disconnected = "disconnected";
    public const string StateChanged = "change_state";
    public const string Call = "call";
}
