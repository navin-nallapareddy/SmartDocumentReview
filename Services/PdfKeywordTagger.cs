// File: Services/PdfKeywordTagger.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using SmartDocumentReview.Models;
using SmartDocumentReview.Shared; // unified regex helpers

namespace SmartDocumentReview.Services
{
    public class PdfKeywordTagger
    {
        public List<TagMatch> ProcessPdf(Stream pdfStream, List<Keyword> keywords, string createdBy)
        {
            var matches = new List<TagMatch>();

            using var memory = new MemoryStream();
            pdfStream.CopyTo(memory);
            var bytes = memory.ToArray();
            var tempPdf = Path.GetTempFileName();
            File.WriteAllBytes(tempPdf, bytes);
            memory.Position = 0;

            using var pdf = PdfDocument.Open(memory);

            var wholeKw = keywords.Where(k => !k.AllowPartial).ToList();
            var partKw  = keywords.Where(k =>  k.AllowPartial).ToList();

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

                if (string.IsNullOrWhiteSpace(pageText))
                {
                    pageText = RunOcr(tempPdf, i);
                    spans.Clear();
                }

                var sectionTitle = $"Page {i}";
                var rawHits = new List<(int start, int end, Keyword kw)>(64);

                static void Collect(Regex rx, string text, List<Keyword> srcKw, List<(int start, int end, Keyword kw)> sink)
                {
                    foreach (Match m in rx.Matches(text))
                    {
                        for (int gi = 0; gi < srcKw.Count; gi++)
                        {
                            var g = m.Groups[$"k{gi}"];
                            if (g.Success)
                            {
                                sink.Add((g.Index, g.Index + g.Length, srcKw[gi]));
                                break;
                            }
                        }
                    }
                }

                if (wholeKw.Count > 0)
                {
                    var rxWhole = KeywordRegex.BuildWholeWordRegex(wholeKw.Select(k => k.Text));
                    Collect(rxWhole, pageText, wholeKw, rawHits);
                }
                if (partKw.Count > 0)
                {
                    var rxPart = KeywordRegex.BuildPartialRegex(partKw.Select(k => k.Text));
                    Collect(rxPart, pageText, partKw, rawHits);
                }

                if (rawHits.Count == 0) continue;

                var ordered = rawHits
                    .OrderBy(h => h.start)
                    .ThenByDescending(h => h.end - h.start)
                    .ToList();

                var canonical = new List<(int start, int end, Keyword kw)>(ordered.Count);
                var seen = new HashSet<(int s, int e)>();
                int lastEnd = -1;

                foreach (var h in ordered)
                {
                    if (!seen.Add((h.start, h.end))) continue;
                    if (h.start < lastEnd) continue;
                    canonical.Add(h);
                    lastEnd = h.end;
                }

                foreach (var h in canonical)
                {
                    double x1 = 0, y1 = 0, x2 = 0, y2 = 0;

                    if (spans.Count > 0)
                    {
                        var relevantRects = spans
                            .Where(s => s.start < h.end && s.end > h.start)
                            .Select(s => s.letter.GlyphRectangle)
                            .ToList();

                        if (relevantRects.Count > 0)
                        {
                            x1 = relevantRects.Min(r => r.Left);
                            y1 = relevantRects.Min(r => r.Bottom);
                            x2 = relevantRects.Max(r => r.Right);
                            y2 = relevantRects.Max(r => r.Top);
                        }
                    }

                    var ctxStart = Math.Max(0, h.start - 60);
                    var ctxEnd   = Math.Min(pageText.Length, h.end + 60);
                    var context  = pageText.Substring(ctxStart, ctxEnd - ctxStart);

                    matches.Add(new TagMatch
                    {
                        Keyword      = h.kw.Text,
                        SectionTitle = sectionTitle,
                        MatchedText  = context,
                        CreatedBy    = createdBy,
                        CreatedAt    = DateTime.UtcNow,
                        PageNumber   = i,
                        PageX        = (float)x1,
                        PageY        = (float)y1,
                        Width        = (float)(x2 - x1)
                        // If you add Height: Height = (float)(y2 - y1)
                    });
                }
            }

            File.Delete(tempPdf);
            return matches;
        }

        private string RunOcr(string pdfPath, int pageNumber)
        {
            try
            {
                var tempDir = Path.GetTempPath();
                var baseName = Path.Combine(tempDir, $"ocr_page_{Guid.NewGuid()}");

                var pdftoppm = new ProcessStartInfo
                {
                    FileName = "pdftoppm",
                    Arguments = $"-f {pageNumber} -l {pageNumber} -png -singlefile \"{pdfPath}\" \"{baseName}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var proc = Process.Start(pdftoppm))
                {
                    proc?.WaitForExit();
                }

                var imagePath = baseName + ".png";
                if (!File.Exists(imagePath))
                {
                    return string.Empty;
                }

                string text = string.Empty;
                var tesseract = new ProcessStartInfo
                {
                    FileName = "tesseract",
                    Arguments = $"\"{imagePath}\" stdout",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var proc = Process.Start(tesseract))
                {
                    if (proc != null)
                    {
                        text = proc.StandardOutput.ReadToEnd();
                        proc.WaitForExit();
                    }
                }

                File.Delete(imagePath);
                return text;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
