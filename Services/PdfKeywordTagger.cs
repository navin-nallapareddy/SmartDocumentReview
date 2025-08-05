using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using SmartDocumentReview.Models;

namespace SmartDocumentReview.Services
{
    public class PdfKeywordTagger
    {
        public List<TagMatch> ProcessPdf(Stream pdfStream, List<Keyword> keywords, string createdBy)
        {
            var matches = new List<TagMatch>();

            using var pdf = PdfDocument.Open(pdfStream);

            for (int i = 1; i <= pdf.NumberOfPages; i++)
            {
                var page = pdf.GetPage(i);
                var letters = page.Letters;

                var pageTextBuilder = new StringBuilder();
                var spans = new List<(int start, int end, Letter letter)>();
                int offset = 0;
                foreach (var letter in letters)
                {
                    var text = letter.Value.ToString();
                    pageTextBuilder.Append(text);
                    spans.Add((offset, offset + text.Length, letter));
                    offset += text.Length;
                }

                var pageText = pageTextBuilder.ToString();
                var sectionTitle = $"Page {i}";

                foreach (var keyword in keywords)
                {
                    var escaped = Regex.Escape(keyword.Text);
                    // Use custom boundaries when partial matches aren't allowed to avoid partial word highlights.
                    // Treat common word punctuation like apostrophes, hyphens and underscores as part of the word
                    // to prevent matches such as "bank" in "bank's" when whole-word matching is requested.
                    var pattern = keyword.AllowPartial
                        ? escaped
                        : $@"(?<![\p{{L}}\p{{N}}_\u2019'-]){escaped}(?![\p{{L}}\p{{N}}_\u2019'-])";

                    var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

                    foreach (Match match in regex.Matches(pageText))
                    {
                        double x1 = 0, y1 = 0, x2 = 0, y2 = 0;
                        var relevant = spans
                            .Where(s => s.start < match.Index + match.Length && s.end > match.Index)
                            .Select(s => s.letter.GlyphRectangle)
                            .ToList();

                        if (relevant.Count > 0)
                        {
                            x1 = relevant.Min(r => r.Left);
                            y1 = relevant.Min(r => r.Bottom);
                            x2 = relevant.Max(r => r.Right);
                            y2 = relevant.Max(r => r.Top);
                        }

                        // Extract matched context (±60 characters around the match)
                        var start = Math.Max(0, match.Index - 60);
                        var end = Math.Min(pageText.Length, match.Index + match.Length + 60);
                        var context = pageText.Substring(start, end - start);

                        matches.Add(new TagMatch
                        {
                            Keyword = keyword.Text,
                            SectionTitle = sectionTitle,
                            MatchedText = context,
                            CreatedBy = createdBy,
                            CreatedAt = DateTime.UtcNow,
                            PageNumber = i,
                            PageX = (float)x1,
                            PageY = (float)y1,
                            Width = (float)(x2 - x1)
                        });
                    }
                }
            }

            return matches;
        }
    }
}

