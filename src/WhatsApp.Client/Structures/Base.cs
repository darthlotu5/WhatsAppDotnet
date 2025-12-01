namespace WhatsAppDotnet.Structures;

/// <summary>
/// Represents a base WhatsApp data structure
/// </summary>
public abstract class Base
{
    /// <summary>
    /// The client that instantiated this structure
    /// </summary>
    protected WhatsAppClient Client { get; }

    /// <summary>
    /// Initializes a new instance of the Base class
    /// </summary>
    /// <param name="client">The client that instantiated this structure</param>
    protected Base(WhatsAppClient client)
    {
        Client = client;
    }

    /// <summary>
    /// Creates a shallow clone of this instance
    /// </summary>
    /// <returns>A shallow clone of this instance</returns>
    public virtual Base Clone()
    {
        return (Base)MemberwiseClone();
    }

    /// <summary>
    /// Patches this instance with data from the provided object
    /// </summary>
    /// <param name="data">The data to patch with</param>
    public virtual void Patch(object data)
    {
        // Override in derived classes to implement data patching logic
    }
}
