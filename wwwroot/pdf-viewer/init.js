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
(function () {
  if (!window.pdfjsLib) {
    console.warn("pdfjsLib not found. Did you include /pdfjs/build/pdf.js ?");
    return;
  }
  // Point the worker to your local pdf.worker build
  pdfjsLib.GlobalWorkerOptions.workerSrc = "/pdfjs/build/pdf.worker.min.js";

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
