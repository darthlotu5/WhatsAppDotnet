using System.Text.Json.Serialization;
using WhatsAppDotnet.Utilities;

namespace WhatsAppDotnet.Structures;

/// <summary>
/// Represents a message identifier on WhatsApp
/// </summary>
public class MessageId
{
    /// <summary>
    /// The message ID
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The sender of the message
    /// </summary>
    [JsonPropertyName("fromMe")]
    public bool FromMe { get; set; }

    /// <summary>
    /// The remote JID (WhatsApp ID)
    /// </summary>
    [JsonPropertyName("remote")]
    public string Remote { get; set; } = string.Empty;

    /// <summary>
    /// Participant for group messages
    /// </summary>
    [JsonPropertyName("participant")]
    public string? Participant { get; set; }

    /// <summary>
    /// Serialization number
    /// </summary>
    [JsonPropertyName("serialized")]
    public string Serialized { get; set; } = string.Empty;
}

/// <summary>
/// Represents a message on WhatsApp
/// </summary>
public class Message : Base
{
    private dynamic? _data;

    /// <summary>
    /// Initializes a new instance of the Message class
    /// </summary>
    /// <param name="client">The client that instantiated this message</param>
    /// <param name="data">The message data</param>
    public Message(WhatsAppClient client, dynamic? data = null) : base(client)
    {
        if (data != null)
        {
            Patch(data);
        }
    }

    /// <summary>
    /// MediaKey that represents the sticker 'ID'
    /// </summary>
    public string? MediaKey { get; set; }

    /// <summary>
    /// ID that represents the message
    /// </summary>
    public MessageId? Id { get; set; }

    /// <summary>
    /// ACK status for the message
    /// </summary>
    public MessageAck Ack { get; set; }

    /// <summary>
    /// Indicates if the message has media available for download
    /// </summary>
    public bool HasMedia { get; set; }

    /// <summary>
    /// Message content
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Message type
    /// </summary>
    public MessageTypes Type { get; set; } = MessageTypes.Text;

    /// <summary>
    /// Unix timestamp for when the message was created
    /// </summary>
    public long Timestamp { get; set; }

    /// <summary>
    /// Indicates if the message was sent by the current user
    /// </summary>
    public bool FromMe { get; set; }

    /// <summary>
    /// Author of the message (for group chats)
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// Device type that sent the message
    /// </summary>
    public string? DeviceType { get; set; }

    /// <summary>
    /// Indicates if the message is forwarded
    /// </summary>
    public bool IsForwarded { get; set; }

    /// <summary>
    /// Forward score (number of times forwarded)
    /// </summary>
    public int ForwardingScore { get; set; }

    /// <summary>
    /// Indicates if the message is a status message
    /// </summary>
    public bool IsStatus { get; set; }

    /// <summary>
    /// Indicates if the message is starred
    /// </summary>
    public bool IsStarred { get; set; }

    /// <summary>
    /// Broadcast information if the message is from a broadcast
    /// </summary>
    public bool Broadcast { get; set; }

    /// <summary>
    /// Chat ID where this message was sent
    /// </summary>
    public string? From { get; set; }

    /// <summary>
    /// Recipient of the message
    /// </summary>
    public string? To { get; set; }

    /// <summary>
    /// Gets the chat this message belongs to
    /// </summary>
    /// <returns>The chat object</returns>
    public virtual async Task<Chat?> GetChatAsync()
    {
        // Implementation would fetch chat from client
        // For now, return null as placeholder
        return null;
    }

    /// <summary>
    /// Gets the contact that sent this message
    /// </summary>
    /// <returns>The contact object</returns>
    public virtual async Task<Contact?> GetContactAsync()
    {
        // Implementation would fetch contact from client
        // For now, return null as placeholder
        return null;
    }

    /// <summary>
    /// Replies to this message
    /// </summary>
    /// <param name="content">The reply content</param>
    /// <param name="options">Optional reply options</param>
    /// <returns>The sent message</returns>
    public virtual async Task<Message?> ReplyAsync(string content, MessageOptions? options = null)
    {
        // Implementation would send reply through client
        // For now, return null as placeholder
        return null;
    }

    /// <summary>
    /// Forwards this message to another chat
    /// </summary>
    /// <param name="chatId">The chat ID to forward to</param>
    /// <returns>True if successful</returns>
    public virtual async Task<bool> ForwardAsync(string chatId)
    {
        // Implementation would forward message through client
        return false;
    }

    /// <summary>
    /// Deletes this message
    /// </summary>
    /// <param name="everyone">Whether to delete for everyone</param>
    /// <returns>True if successful</returns>
    public virtual async Task<bool> DeleteAsync(bool everyone = false)
    {
        // Implementation would delete message through client
        return false;
    }

    /// <summary>
    /// Downloads media from this message
    /// </summary>
    /// <returns>The media data as byte array</returns>
    public virtual async Task<byte[]?> DownloadMediaAsync()
    {
        if (!HasMedia)
            return null;

        // Implementation would download media through client
        return null;
    }

    /// <summary>
    /// Stars this message
    /// </summary>
    /// <returns>True if successful</returns>
    public virtual async Task<bool> StarAsync()
    {
        // Implementation would star message through client
        return false;
    }

    /// <summary>
    /// Unstars this message
    /// </summary>
    /// <returns>True if successful</returns>
    public virtual async Task<bool> UnstarAsync()
    {
        // Implementation would unstar message through client
        return false;
    }

    /// <summary>
    /// Patches this message with data from the provided object
    /// </summary>
    /// <param name="data">The data to patch with</param>
    public override void Patch(object data)
    {
        _data = data;
        // Implementation would extract properties from data object
        // This is a simplified version
        base.Patch(data);
    }
}

/// <summary>
/// Options for sending messages
/// </summary>
public class MessageOptions
{
    /// <summary>
    /// Message to quote/reply to
    /// </summary>
    public Message? QuotedMessage { get; set; }

    /// <summary>
    /// Whether to send the message as a quote
    /// </summary>
    public bool SendAsQuote { get; set; } = true;

    /// <summary>
    /// Mentions in the message
    /// </summary>
    public List<string>? Mentions { get; set; }

    /// <summary>
    /// Parse mode for the message
    /// </summary>
    public string? ParseMode { get; set; }

    /// <summary>
    /// Link preview options
    /// </summary>
    public bool LinkPreview { get; set; } = true;

    /// <summary>
    /// Send as ephemeral message
    /// </summary>
    public bool Ephemeral { get; set; } = false;
}
