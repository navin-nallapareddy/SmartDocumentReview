using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using SmartDocumentReview.Models;

namespace SmartDocumentReview.Services
{
    public class PdfKeywordTagger
    {
        public List<TagMatch> ProcessPdf(Stream pdfStream, List<Keyword> keywords, string createdBy)
        {
            var matches = new List<TagMatch>();

            using var reader = new PdfReader(pdfStream);
            using var pdf = new PdfDocument(reader);

            for (int i = 1; i <= pdf.GetNumberOfPages(); i++)
            {
                var page = pdf.GetPage(i);
                var strategy = new LocationTextExtractionStrategy();
                var pageText = PdfTextExtractor.GetTextFromPage(page, strategy);

                var method = typeof(LocationTextExtractionStrategy).GetMethod("GetResultantLocations", BindingFlags.Instance | BindingFlags.NonPublic);
                var locations = method?.Invoke(strategy, null) as IEnumerable<IPdfTextLocation>;
                var locationList = locations?.ToList() ?? new List<IPdfTextLocation>();
                var spans = new List<(int start, int end, IPdfTextLocation loc)>();
                int offset = 0;
                foreach (var loc in locationList)
                {
                    var textLoc = loc.GetText();
                    spans.Add((offset, offset + textLoc.Length, loc));
                    offset += textLoc.Length;
                }

                var sectionTitle = $"Page {i}";

                foreach (var keyword in keywords)
                {
                    var escaped = Regex.Escape(keyword.Text);
                    var pattern = (keyword.AllowPartial || Regex.IsMatch(keyword.Text, @"\w"))
                        ? escaped
                        : $@"\b{escaped}\b";

                    var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

                    foreach (Match match in regex.Matches(pageText))
                    {
                        // Get the bounding box location from spans
                        IPdfTextLocation? loc = null;
                        if (spans.Count > 0)
                        {
                            var span = spans.FirstOrDefault(s => match.Index >= s.start && match.Index < s.end);
                            loc = span.loc;
                        }
                        Rectangle rect = loc?.GetRectangle() ?? new Rectangle(0, 0, 0, 0);

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
                            PageX = rect.GetX(),
                            PageY = rect.GetY(),
                            Width = rect.GetWidth(),
                            Height = rect.GetHeight()
                        });
                    }
                }
            }

            return matches;
        }
    }
}

