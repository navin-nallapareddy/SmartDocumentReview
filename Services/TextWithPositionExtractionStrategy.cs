using System.Text;
using System.Collections.Generic;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using SmartDocumentReview.Models;

namespace SmartDocumentReview.Services
{
    internal class TextCharWithPosition
    {
        public string Text { get; set; } = string.Empty;
        public PdfTextPosition Position { get; set; } = new PdfTextPosition();
    }

    internal class TextWithPositionExtractionStrategy : IEventListener
    {
        private readonly StringBuilder _builder = new StringBuilder();
        public List<TextCharWithPosition> Characters { get; } = new List<TextCharWithPosition>();

        public void EventOccurred(IEventData data, EventType type)
        {
            if (type != EventType.RENDER_TEXT) return;
            var renderInfo = (TextRenderInfo)data;
            foreach (var charInfo in renderInfo.GetCharacterRenderInfos())
            {
                var glyph = charInfo.GetText();
                _builder.Append(glyph);
                var descent = charInfo.GetDescentLine().GetStartPoint();
                var ascent = charInfo.GetAscentLine().GetEndPoint();
                Characters.Add(new TextCharWithPosition
                {
                    Text = glyph,
                    Position = new PdfTextPosition
                    {
                        X = descent.Get(0),
                        Y = descent.Get(1),
                        Width = ascent.Get(0) - descent.Get(0),
                        Height = ascent.Get(1) - descent.Get(1)
                    }
                });
            }
        }

        public ICollection<EventType> GetSupportedEvents()
            => new HashSet<EventType> { EventType.RENDER_TEXT };

        public string GetResultantText() => _builder.ToString();
    }
}

