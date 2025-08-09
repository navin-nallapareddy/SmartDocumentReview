// File: Pages/PDFResult.razor.cs
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using SmartDocumentReview.Shared;  // KeywordRegex
using SmartDocumentReview.Models;  // Keyword

namespace SmartDocumentReview.Pages
{
    public partial class PDFResult
    {
        private sealed record Hit(int Start, int End, Keyword Keyword)
        {
            public int Length => End - Start;
        }

        /// <summary>
        /// Convenience wrapper if your .razor passes a color dictionary.
        /// </summary>
        protected MarkupString HighlightKeywords(string text, IEnumerable<Keyword> keywords, IDictionary<Keyword, string> colorMap)
            => BuildHighlights(text, keywords, k => colorMap[k]);

        /// <summary>
        /// Build safe, accurate multi-keyword highlights for the given text.
        /// Call from your .razor like:
        /// @BuildHighlights(snippetText, CurrentKeywords, k => ColorMap[k])
        /// </summary>
        protected MarkupString BuildHighlights(string text, IEnumerable<Keyword> keywords, Func<Keyword, string> colorForKeyword)
        {
            if (string.IsNullOrEmpty(text) || keywords is null)
                return new MarkupString(HtmlEncoder.Default.Encode(text ?? string.Empty));

            var kwList = keywords.ToList();
            var wholeKw = kwList.Where(k => !k.AllowPartial).ToList();
            var partKw  = kwList.Where(k =>  k.AllowPartial).ToList();

            var matches = new List<Hit>(32);

            if (wholeKw.Count > 0)
            {
                var rxWhole = KeywordRegex.BuildWholeWordRegex(wholeKw.Select(k => k.Text));
                Collect(rxWhole, text, wholeKw, matches);
            }
            if (partKw.Count > 0)
            {
                var rxPart = KeywordRegex.BuildPartialRegex(partKw.Select(k => k.Text));
                Collect(rxPart, text, partKw, matches);
            }

            // Sort deterministic: start asc, length desc
            var ordered = matches
                .OrderBy(m => m.Start)
                .ThenByDescending(m => m.Length)
                .ToList();

            // Dedupe identical spans; skip true overlaps (keep earlier/longer)
            var canonical = new List<Hit>(ordered.Count);
            var seen = new HashSet<(int s, int e)>();
            int lastEnd = -1;

            foreach (var m in ordered)
            {
                if (!seen.Add((m.Start, m.End))) continue;
                if (m.Start < lastEnd) continue;
                canonical.Add(m);
                lastEnd = m.End;
            }

            // Build HTML with safe encoding (only <mark> wrapper is raw)
            var sb = new StringBuilder(text.Length + canonical.Count * 40);
            int cursor = 0;

            foreach (var m in canonical)
            {
                if (cursor < m.Start)
                    sb.Append(HtmlEncoder.Default.Encode(text.AsSpan(cursor, m.Start - cursor)));

                var encoded = HtmlEncoder.Default.Encode(text.AsSpan(m.Start, m.Length));
                sb.Append($"<mark style=\"background-color:{colorForKeyword(m.Keyword)}\">{encoded}</mark>");
                cursor = m.End;
            }

            if (cursor < text.Length)
                sb.Append(HtmlEncoder.Default.Encode(text.AsSpan(cursor)));

            return new MarkupString(sb.ToString());
        }

        /// <summary>
        /// Map named-group matches back to their Keyword instances.
        /// </summary>
        private static void Collect(Regex rx, string text, List<Keyword> sourceKeywords, List<Hit> sink)
        {
            foreach (Match m in rx.Matches(text))
            {
                for (int i = 0; i < sourceKeywords.Count; i++)
                {
                    var g = m.Groups[$"k{i}"];
                    if (g.Success)
                    {
                        sink.Add(new Hit(g.Index, g.Index + g.Length, sourceKeywords[i]));
                        break; // exactly one will be set
                    }
                }
            }
        }
    }
}
