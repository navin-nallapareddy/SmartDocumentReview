// File: Shared/KeywordRegex.cs
using System.Text.RegularExpressions;

namespace SmartDocumentReview.Shared
{
    /// <summary>
    /// Centralized helpers to build keyword regexes with a consistent notion of "word".
    /// </summary>
    public static class KeywordRegex
    {
        // Treat letters, numbers, underscore, hyphen, and the curly apostrophe as word characters.
        public const string WordCharClass = @"\p{L}\p{N}_\u2019'-";

        /// <summary>
        /// Builds a compiled regex that matches any of the keywords as whole words,
        /// using negative lookbehind/lookahead with the shared word-class.
        /// Each alternative is named (?&lt;k{i}&gt;...) so we can map match-&gt;keyword reliably.
        /// </summary>
        public static Regex BuildWholeWordRegex(IEnumerable<string> keywords, RegexOptions extra = RegexOptions.None)
        {
            var groups = keywords.Select((kw, i) => $"(?<k{i}>{Regex.Escape(kw)})");
            var pattern = $@"(?<![{WordCharClass}])(?:{string.Join("|", groups)})(?![{WordCharClass}])";
            return new Regex(pattern,
                RegexOptions.CultureInvariant |
                RegexOptions.IgnoreCase |
                RegexOptions.Compiled |
                extra);
        }

        /// <summary>
        /// Builds a compiled regex that matches any of the keywords as partial substrings.
        /// Also uses named groups (?&lt;k{i}&gt;...) for match-&gt;keyword mapping.
        /// </summary>
        public static Regex BuildPartialRegex(IEnumerable<string> keywords, RegexOptions extra = RegexOptions.None)
        {
            var groups = keywords.Select((kw, i) => $"(?<k{i}>{Regex.Escape(kw)})");
            var pattern = $"(?:{string.Join("|", groups)})";
            return new Regex(pattern,
                RegexOptions.CultureInvariant |
                RegexOptions.IgnoreCase |
                RegexOptions.Compiled |
                extra);
        }
    }
}
