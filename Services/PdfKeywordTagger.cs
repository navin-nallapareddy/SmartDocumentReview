using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
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
                var strategy = new TextWithPositionExtractionStrategy();
                var processor = new PdfCanvasProcessor(strategy);
                processor.ProcessPageContent(page);
                var text = strategy.GetResultantText();
                var characters = strategy.Characters;
                var sectionTitle = $"Page {i}";

                foreach (var keyword in keywords)
                {
                    var escaped = Regex.Escape(keyword.Text);
                    var corePattern = (keyword.AllowPartial || Regex.IsMatch(keyword.Text, @"\W"))
                        ? escaped
                        : $"\\b{escaped}\\b";
                    var regex = new Regex(corePattern, RegexOptions.IgnoreCase);

                    foreach (Match match in regex.Matches(text))
                    {
                        var start = Math.Max(0, match.Index - 60);
                        var length = Math.Min(text.Length - start, match.Length + 120);
                        var snippet = text.Substring(start, length);
                        var positions = new List<PdfTextPosition>();
                        for (int idx = match.Index; idx < match.Index + match.Length && idx < characters.Count; idx++)
                        {
                            positions.Add(characters[idx].Position);
                        }

                        matches.Add(new TagMatch
                        {
                            Keyword = keyword.Text,
                            SectionTitle = sectionTitle,
                            MatchedText = snippet,
                            CreatedBy = createdBy,
                            CreatedAt = DateTime.UtcNow,
                            PageNumber = i,
                            Positions = positions
                        });
                    }
                }
            }

            return matches;
        }
    }
}
