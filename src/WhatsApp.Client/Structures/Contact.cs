using System.Text.Json.Serialization;

namespace WhatsAppDotnet.Structures;

/// <summary>
/// Represents a contact on WhatsApp
/// </summary>
public class Contact : Base
{
    private dynamic? _data;

    /// <summary>
    /// Initializes a new instance of the Contact class
    /// </summary>
    /// <param name="client">The client that instantiated this contact</param>
    /// <param name="data">The contact data</param>
    public Contact(WhatsAppClient client, dynamic? data = null) : base(client)
    {
        if (data != null)
        {
            Patch(data);
        }
    }

    /// <summary>
    /// Contact ID (WhatsApp ID)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Contact number
    /// </summary>
    public string Number { get; set; } = string.Empty;

    /// <summary>
    /// Whether the number is a business account
    /// </summary>
    public bool IsBusiness { get; set; }

    /// <summary>
    /// Whether the number is an enterprise account  
    /// </summary>
    public bool IsEnterprise { get; set; }

    /// <summary>
    /// Labels applied to the contact
    /// </summary>
    public List<string> Labels { get; set; } = new();

    /// <summary>
    /// Display name of the contact
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Push name (name set by the contact)
    /// </summary>
    public string PushName { get; set; } = string.Empty;

    /// <summary>
    /// Short name of the contact
    /// </summary>
    public string ShortName { get; set; } = string.Empty;

    /// <summary>
    /// Whether the contact is a group
    /// </summary>
    public bool IsGroup { get; set; }

    /// <summary>
    /// Whether the contact is the current user
    /// </summary>
    public bool IsMe { get; set; }

    /// <summary>
    /// Whether the contact is a user (has WhatsApp)
    /// </summary>
    public bool IsUser { get; set; }

    /// <summary>
    /// Whether the contact is in the address book
    /// </summary>
    public bool IsWAContact { get; set; }

    /// <summary>
    /// Profile picture URL
    /// </summary>
    public string? ProfilePicUrl { get; set; }

    /// <summary>
    /// Gets the profile picture of the contact
    /// </summary>
    /// <returns>Profile picture as byte array</returns>
    public virtual async Task<byte[]?> GetProfilePictureAsync()
    {
        // Implementation would fetch profile picture through client
        return null;
    }

    /// <summary>
    /// Gets the chat with this contact
    /// </summary>
    /// <returns>The chat object</returns>
    public virtual async Task<Chat?> GetChatAsync()
    {
        // Implementation would fetch chat through client
        return null;
    }

    /// <summary>
    /// Blocks this contact
    /// </summary>
    /// <returns>True if successful</returns>
    public virtual async Task<bool> BlockAsync()
    {
        // Implementation would block contact through client
        return false;
    }

    /// <summary>
    /// Unblocks this contact
    /// </summary>
    /// <returns>True if successful</returns>
    public virtual async Task<bool> UnblockAsync()
    {
        // Implementation would unblock contact through client
        return false;
    }

    /// <summary>
    /// Gets the "about" information of the contact
    /// </summary>
    /// <returns>About information</returns>
    public virtual async Task<string?> GetAboutAsync()
    {
        // Implementation would fetch about info through client
        return null;
    }

    /// <summary>
    /// Patches this contact with data from the provided object
    /// </summary>
    /// <param name="data">The data to patch with</param>
    public override void Patch(object data)
    {
        _data = data;
        // Implementation would extract properties from data object
        base.Patch(data);
    }
}
