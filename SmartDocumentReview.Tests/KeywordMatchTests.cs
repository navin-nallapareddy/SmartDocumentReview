using System.Collections.Generic;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using SmartDocumentReview.Models;
using SmartDocumentReview.Services;
using Xunit;

namespace SmartDocumentReview.Tests
{
    public class KeywordMatchTests
    {
        private MemoryStream CreatePdf(string text)
        {
            var ms = new MemoryStream();
            using var writer = new PdfWriter(ms);
            using var pdf = new PdfDocument(writer);
            using var doc = new Document(pdf);
            doc.Add(new Paragraph(text));
            doc.Close();
            ms.Position = 0;
            return ms;
        }

        [Fact]
        public void DoesNotMatchInsideWordWhenWholeWord()
        {
            using var pdf = CreatePdf("bankruptcy");
            var tagger = new PdfKeywordTagger();
            var keywords = new List<Keyword> { new Keyword("bank", false) };
            var matches = tagger.ProcessPdf(pdf, keywords, "tester");
            Assert.Empty(matches);
        }

        [Fact]
        public void MatchesInsideWordWhenPartialAllowed()
        {
            using var pdf = CreatePdf("bankruptcy");
            var tagger = new PdfKeywordTagger();
            var keywords = new List<Keyword> { new Keyword("bank", true) };
            var matches = tagger.ProcessPdf(pdf, keywords, "tester");
            Assert.Single(matches);
            Assert.Equal("bank", matches[0].Keyword);
        }
    }
}
