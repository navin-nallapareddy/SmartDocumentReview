using System;

namespace SmartDocumentReview.Models
{
    /// <summary>
    /// Represents a keyword entered by the user.
    /// When <see cref="AllowPartial"/> is true, the keyword may match inside other words.
    /// </summary>
    public record Keyword(string Text, bool AllowPartial);
}
