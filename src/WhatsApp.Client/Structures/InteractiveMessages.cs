namespace WhatsAppDotnet.Structures;

/// <summary>
/// A single button attached to a text or media message, sent via WA-JS's
/// "interactive message" native-flow buttons. Exactly one of
/// <see cref="Id"/>, <see cref="Url"/>, <see cref="PhoneNumber"/>, or
/// <see cref="Code"/> should be set — this determines the button's
/// behavior (quick-reply / open-link / call / copy-code respectively).
///
/// Mirrors WPPConnect wa-js's MessageButtonsTypes union
/// (src/chat/functions/prepareMessageButtons.ts).
/// </summary>
public class ButtonOption
{
    /// <summary>Button label shown to the recipient. Required.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Set to make this a "quick reply" button: tapping it sends this id
    /// back as the reply payload. Mutually exclusive with Url/PhoneNumber/Code.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>Set to make this an "open URL" button.</summary>
    public string? Url { get; set; }

    /// <summary>Set to make this a "call this number" button.</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>Set to make this a "copy code" button.</summary>
    public string? Code { get; set; }

    /// <summary>Quick-reply button. Tapping it replies with <paramref name="id"/>.</summary>
    public static ButtonOption QuickReply(string id, string text) => new() { Id = id, Text = text };

    /// <summary>Button that opens <paramref name="url"/> when tapped.</summary>
    public static ButtonOption Link(string url, string text) => new() { Url = url, Text = text };

    /// <summary>Button that calls <paramref name="phoneNumber"/> when tapped.</summary>
    public static ButtonOption Call(string phoneNumber, string text) => new() { PhoneNumber = phoneNumber, Text = text };

    /// <summary>Button that copies <paramref name="code"/> to the clipboard when tapped.</summary>
    public static ButtonOption CopyCode(string code, string text) => new() { Code = code, Text = text };
}

/// <summary>
/// A single selectable row within a <see cref="ListSection"/>.
/// Mirrors wa-js's ListMessageOptions.sections[].rows[].
/// </summary>
public class ListMessageRow
{
    /// <summary>Row title, shown in bold. Required.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Row subtitle/description shown under the title.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Opaque id returned to you (typically via the button-reply webhook
    /// event) when the recipient selects this row. Must be unique across
    /// all sections in the same list message.
    /// </summary>
    public string RowId { get; set; } = string.Empty;
}

/// <summary>
/// A named group of rows within a list message.
/// Mirrors wa-js's ListMessageOptions.sections[].
/// </summary>
public class ListSection
{
    /// <summary>Section heading shown above its rows.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Rows in this section. At least one required.</summary>
    public List<ListMessageRow> Rows { get; set; } = new();
}
