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
                var text = PdfTextExtractor.GetTextFromPage(page);
                var sectionTitle = $"Page {i}";

                foreach (var keyword in keywords)
                {
                    var escaped = Regex.Escape(keyword.Text);
                    var pattern = (keyword.AllowPartial || Regex.IsMatch(keyword.Text, @"\W"))
                        ? escaped
                        : $@"\b{escaped}\b";
                    var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                    foreach (Match match in regex.Matches(text))
                    {
                        var start = Math.Max(0, match.Index - 60);
                        var end = Math.Min(text.Length, match.Index + match.Length + 60);
                        var context = text.Substring(start, end - start);

                        matches.Add(new TagMatch
                        {
                            Keyword = keyword.Text,
                            SectionTitle = sectionTitle,
                            MatchedText = context,
                            CreatedBy = createdBy,
                            CreatedAt = DateTime.UtcNow,
                            PageNumber = i
                        });
                    }
                }
            }

            return matches;
        }
    }
}
