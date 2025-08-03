using System.Text.RegularExpressions;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using SmartDocumentReview.Models;

namespace SmartDocumentReview.Services
{
    public class PdfKeywordTagger
    {
        public List<TagMatch> ProcessPdf(Stream pdfStream, List<string> keywords, string createdBy)
        {
            var matches = new List<TagMatch>();

            using var reader = new PdfReader(pdfStream);
            reader.SetCloseStream(false);
            using var pdf = new PdfDocument(reader);

            for (int i = 1; i <= pdf.GetNumberOfPages(); i++)
            {
                var page = pdf.GetPage(i);
                var text = PdfTextExtractor.GetTextFromPage(page);
                var sectionTitle = $"Page {i}";

                foreach (var keyword in keywords)
                {
                    var pattern = Regex.Escape(keyword);
                    var regex = new Regex($@"(.{{0,140}}{pattern}.{{0,140}})", RegexOptions.IgnoreCase);
                    foreach (Match match in regex.Matches(text))
                    {
                        matches.Add(new TagMatch
                        {
                            Keyword = keyword,
                            SectionTitle = sectionTitle,
                            MatchedText = match.Value,
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
