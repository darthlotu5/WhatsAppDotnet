using System.Text.Json.Serialization;

namespace WhatsAppDotnet.Structures;

/// <summary>
/// Represents a chat on WhatsApp
/// </summary>
public class Chat : Base
{
    private dynamic? _data;

    /// <summary>
    /// Initializes a new instance of the Chat class
    /// </summary>
    /// <param name="client">The client that instantiated this chat</param>
    /// <param name="data">The chat data</param>
    public Chat(WhatsAppClient client, dynamic? data = null) : base(client)
    {
        if (data != null)
        {
            Patch(data);
        }
    }

    /// <summary>
    /// Chat ID (WhatsApp ID)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the chat
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether the chat is a group
    /// </summary>
    public bool IsGroup { get; set; }

    /// <summary>
    /// Whether the chat is read-only
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// Number of unread messages
    /// </summary>
    public int UnreadCount { get; set; }

    /// <summary>
    /// Unix timestamp of the last message
    /// </summary>
    public long Timestamp { get; set; }

    /// <summary>
    /// Whether the chat is archived
    /// </summary>
    public bool Archived { get; set; }

    /// <summary>
    /// Whether the chat is pinned
    /// </summary>
    public bool Pinned { get; set; }

    /// <summary>
    /// Whether the chat is muted
    /// </summary>
    public bool IsMuted { get; set; }

    /// <summary>
    /// Mute expiration timestamp
    /// </summary>
    public long MuteExpiration { get; set; }

    /// <summary>
    /// Last message in the chat
    /// </summary>
    public Message? LastMessage { get; set; }

    /// <summary>
    /// Group metadata (if this is a group chat)
    /// </summary>
    public GroupMetadata? GroupMetadata { get; set; }

    /// <summary>
    /// Sends a message to this chat
    /// </summary>
    /// <param name="content">The message content</param>
    /// <param name="options">Optional message options</param>
    /// <returns>The sent message</returns>
    public virtual async Task<Message?> SendMessageAsync(string content, MessageOptions? options = null)
    {
        // Implementation would send message through client
        return null;
    }

    /// <summary>
    /// Sends media to this chat
    /// </summary>
    /// <param name="media">The media to send</param>
    /// <param name="options">Optional message options</param>
    /// <returns>The sent message</returns>
    public virtual async Task<Message?> SendMediaAsync(MessageMedia media, MessageOptions? options = null)
    {
        // Implementation would send media through client
        return null;
    }

    /// <summary>
    /// Gets messages from this chat
    /// </summary>
    /// <param name="searchOptions">Search options</param>
    /// <returns>List of messages</returns>
    public virtual async Task<List<Message>> GetMessagesAsync(MessageSearchOptions? searchOptions = null)
    {
        // Implementation would fetch messages through client
        return new List<Message>();
    }

    /// <summary>
    /// Marks this chat as read
    /// </summary>
    /// <returns>True if successful</returns>
    public virtual async Task<bool> MarkAsReadAsync()
    {
        // Implementation would mark chat as read through client
        return false;
    }

    /// <summary>
    /// Archives this chat
    /// </summary>
    /// <returns>True if successful</returns>
    public virtual async Task<bool> ArchiveAsync()
    {
        // Implementation would archive chat through client
        return false;
    }

    /// <summary>
    /// Unarchives this chat
    /// </summary>
    /// <returns>True if successful</returns>
    public virtual async Task<bool> UnarchiveAsync()
    {
        // Implementation would unarchive chat through client
        return false;
    }

    /// <summary>
    /// Pins this chat
    /// </summary>
    /// <returns>True if successful</returns>
    public virtual async Task<bool> PinAsync()
    {
        // Implementation would pin chat through client
        return false;
    }

    /// <summary>
    /// Unpins this chat
    /// </summary>
    /// <returns>True if successful</returns>
    public virtual async Task<bool> UnpinAsync()
    {
        // Implementation would unpin chat through client
        return false;
    }

    /// <summary>
    /// Mutes this chat
    /// </summary>
    /// <param name="duration">Mute duration in seconds</param>
    /// <returns>True if successful</returns>
    public virtual async Task<bool> MuteAsync(long? duration = null)
    {
        // Implementation would mute chat through client
        return false;
    }

    /// <summary>
    /// Unmutes this chat
    /// </summary>
    /// <returns>True if successful</returns>
    public virtual async Task<bool> UnmuteAsync()
    {
        // Implementation would unmute chat through client
        return false;
    }

    /// <summary>
    /// Clears messages from this chat
    /// </summary>
    /// <returns>True if successful</returns>
    public virtual async Task<bool> ClearMessagesAsync()
    {
        // Implementation would clear messages through client
        return false;
    }

    /// <summary>
    /// Deletes this chat
    /// </summary>
    /// <returns>True if successful</returns>
    public virtual async Task<bool> DeleteAsync()
    {
        // Implementation would delete chat through client
        return false;
    }

    /// <summary>
    /// Patches this chat with data from the provided object
    /// </summary>
    /// <param name="data">The data to patch with</param>
    public override void Patch(object data)
    {
        _data = data;
        // Implementation would extract properties from data object
        base.Patch(data);
    }
}

/// <summary>
/// Group metadata for group chats
/// </summary>
public class GroupMetadata
{
    /// <summary>
    /// Group ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Group subject (name)
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Group description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Group creation timestamp
    /// </summary>
    public long CreationTime { get; set; }

    /// <summary>
    /// Group owner
    /// </summary>
    public string Owner { get; set; } = string.Empty;

    /// <summary>
    /// Group participants
    /// </summary>
    public List<GroupParticipant> Participants { get; set; } = new();

    /// <summary>
    /// Whether the group is restricted (only admins can send messages)
    /// </summary>
    public bool Restrict { get; set; }

    /// <summary>
    /// Whether the group is announcement-only
    /// </summary>
    public bool Announce { get; set; }

    /// <summary>
    /// Group invite code
    /// </summary>
    public string? InviteCode { get; set; }
}

/// <summary>
/// Represents a group participant
/// </summary>
public class GroupParticipant
{
    /// <summary>
    /// Participant ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Whether the participant is an admin
    /// </summary>
    public bool IsAdmin { get; set; }

    /// <summary>
    /// Whether the participant is a super admin
    /// </summary>
    public bool IsSuperAdmin { get; set; }
}

/// <summary>
/// Options for searching messages
/// </summary>
public class MessageSearchOptions
{
    /// <summary>
    /// Maximum number of messages to return
    /// </summary>
    public int Limit { get; set; } = 50;

    /// <summary>
    /// Message ID to start searching from
    /// </summary>
    public string? FromMessageId { get; set; }

    /// <summary>
    /// Whether to include media messages
    /// </summary>
    public bool IncludeMedia { get; set; } = true;

    /// <summary>
    /// Start date for search
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date for search
    /// </summary>
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// Represents message media
/// </summary>
public class MessageMedia
{
    /// <summary>
    /// Media type
    /// </summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// Media data
    /// </summary>
    public byte[] Data { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Media filename
    /// </summary>
    public string? Filename { get; set; }

    /// <summary>
    /// Media caption
    /// </summary>
    public string? Caption { get; set; }
}
