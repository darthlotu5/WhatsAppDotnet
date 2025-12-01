using WhatsAppDotnet.Structures;

namespace WhatsAppDotnet.Factories;

/// <summary>
/// Factory for creating Contact instances
/// </summary>
public class ContactFactory
{
    private readonly WhatsAppClient _client;

    /// <summary>
    /// Initializes a new instance of the ContactFactory class
    /// </summary>
    /// <param name="client">The WhatsApp client</param>
    public ContactFactory(WhatsAppClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Creates a Contact instance from raw data
    /// </summary>
    /// <param name="data">The raw contact data</param>
    /// <returns>A Contact instance</returns>
    public Contact Create(dynamic data)
    {
        if (data?.isBusiness == true)
        {
            return new BusinessContact(_client, data);
        }
        else
        {
            return new PrivateContact(_client, data);
        }
    }

    /// <summary>
    /// Creates a list of Contact instances from raw data array
    /// </summary>
    /// <param name="dataArray">Array of raw contact data</param>
    /// <returns>List of Contact instances</returns>
    public List<Contact> CreateList(dynamic[] dataArray)
    {
        return dataArray.Select(Create).ToList();
    }
}

/// <summary>
/// Represents a private contact
/// </summary>
public class PrivateContact : Contact
{
    /// <summary>
    /// Initializes a new instance of the PrivateContact class
    /// </summary>
    /// <param name="client">The client that instantiated this contact</param>
    /// <param name="data">The contact data</param>
    public PrivateContact(WhatsAppClient client, dynamic? data = null) : base(client, (object?)data)
    {
        IsBusiness = false;
    }
}

/// <summary>
/// Represents a business contact
/// </summary>
public class BusinessContact : Contact
{
    /// <summary>
    /// Initializes a new instance of the BusinessContact class
    /// </summary>
    /// <param name="client">The client that instantiated this contact</param>
    /// <param name="data">The contact data</param>
    public BusinessContact(WhatsAppClient client, dynamic? data = null) : base(client, (object?)data)
    {
        IsBusiness = true;
    }

    /// <summary>
    /// Business profile information
    /// </summary>
    public BusinessProfile? BusinessProfile { get; set; }

    /// <summary>
    /// Gets the business profile
    /// </summary>
    /// <returns>The business profile</returns>
    public async Task<BusinessProfile?> GetBusinessProfileAsync()
    {
        // Implementation would fetch business profile through client
        return null;
    }
}

/// <summary>
/// Represents a business profile
/// </summary>
public class BusinessProfile
{
    /// <summary>
    /// Business ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Business name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Business category
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Business description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Business website
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// Business email
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Business address
    /// </summary>
    public BusinessAddress? Address { get; set; }

    /// <summary>
    /// Business hours
    /// </summary>
    public List<BusinessHour> BusinessHours { get; set; } = new();
}

/// <summary>
/// Represents a business address
/// </summary>
public class BusinessAddress
{
    /// <summary>
    /// Street address
    /// </summary>
    public string Street { get; set; } = string.Empty;

    /// <summary>
    /// City
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// State/Province
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Postal/ZIP code
    /// </summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// Country
    /// </summary>
    public string Country { get; set; } = string.Empty;
}

/// <summary>
/// Represents business hours
/// </summary>
public class BusinessHour
{
    /// <summary>
    /// Day of the week (0 = Sunday, 6 = Saturday)
    /// </summary>
    public int DayOfWeek { get; set; }

    /// <summary>
    /// Opening time (in minutes from midnight)
    /// </summary>
    public int OpenTime { get; set; }

    /// <summary>
    /// Closing time (in minutes from midnight)
    /// </summary>
    public int CloseTime { get; set; }

    /// <summary>
    /// Whether the business is open on this day
    /// </summary>
    public bool IsOpen { get; set; }
}
