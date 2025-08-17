// SmartDocumentReview/Pages/PDFResult.razor.cs
using System;
using Microsoft.AspNetCore.Components;

namespace SmartDocumentReview.Pages
{
    public partial class PDFResult : ComponentBase
    {
        [Parameter] public string? FileName { get; set; }

        [Inject] public NavigationManager Nav { get; set; } = default!;

        public string ViewerUrl { get; private set; } = string.Empty;

        protected override void OnParametersSet()
        {
            if (string.IsNullOrWhiteSpace(FileName))
            {
                ViewerUrl = string.Empty;
                return;
            }

            // Respect path base (e.g., ASPNETCORE_PATHBASE=/app)
            var basePath = new Uri(Nav.BaseUri).AbsolutePath.TrimEnd('/'); // "" or "/app"
            string PathBaseAware(string relative) =>
                string.IsNullOrEmpty(basePath) || basePath == "/"
                    ? relative
                    : $"{basePath}{relative}";

            // Build a same-origin, URL-encoded file path for the viewer
            var encodedFileName = Uri.EscapeDataString(FileName);
            var pdfUrl = PathBaseAware($"/uploads/{encodedFileName}");
            var encodedPdfUrl = Uri.EscapeDataString(pdfUrl);

            ViewerUrl = PathBaseAware($"/pdfjs/web/viewer.html?file={encodedPdfUrl}");
        }
    }
}
