/* SmartDocumentReview/wwwroot/pdf-viewer/init.js
 *
 * Use this only if you load pdf.js yourself (not the stock /pdfjs/web/viewer.html).
 * It makes sure the Web Worker loads, otherwise the canvas stays gray.
 *
 * Make sure pdfjsLib is already loaded on the page before this runs.
 * For example, include:
 *   <script src="/pdfjs/build/pdf.js"></script>
 *   <script src="/pdf-viewer/init.js"></script>
 */

/* global pdfjsLib */
(async function () {
  if (!window.pdfjsLib) {
    console.warn("pdfjsLib not found. Did you include /pdfjs/build/pdf.js ?");
    return;
  }
  // Try known worker locations (local first, then CDN)
  const workerCandidates = [
    "/pdfjs/build/pdf.worker.min.js",
    "/pdfjs/lib/pdf.worker.min.js",
    "https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.9.179/pdf.worker.min.js",
  ];
  let workerSrc = null;
  for (const src of workerCandidates) {
    try {
      const res = await fetch(src, { method: "HEAD", cache: "no-store" });
      if (res.ok) {
        workerSrc = src;
        if (src !== workerCandidates[0]) {
          console.warn("Falling back to PDF.js worker at", src);
        }
        break;
      }
    } catch (e) {
      // ignore and try next
    }
  }
  if (!workerSrc) {
    console.error("Could not locate a pdf.worker.min.js file. PDF rendering will fail.");
    return;
  }
  pdfjsLib.GlobalWorkerOptions.workerSrc = workerSrc;

  // Optional: small helper to render into a <canvas id="pdfCanvas">
  window.renderPdfToCanvas = async function (url, canvasId) {
    const canvas = document.getElementById(canvasId || "pdfCanvas");
    if (!canvas) {
      console.error("Canvas element not found:", canvasId);
      return;
    }
    const loadingTask = pdfjsLib.getDocument(url);
    const pdf = await loadingTask.promise;
    const page = await pdf.getPage(1);
    const viewport = page.getViewport({ scale: 1.5 });
    const ctx = canvas.getContext("2d");
    canvas.width = viewport.width;
    canvas.height = viewport.height;
    await page.render({ canvasContext: ctx, viewport }).promise;
  };
})();
