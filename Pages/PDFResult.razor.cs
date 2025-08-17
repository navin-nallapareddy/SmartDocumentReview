/* SmartDocumentReview/Pages/PDFResult.razor.css
   Keep styles simple so nothing overlays or dims the iframe.
*/

:root {
  /* you can tweak these if desired */
  --pdf-frame-height: 80vh;
}

/* Ensure the iframe fills the width and isn't visually “greyed out” by styles */
iframe[title="PDF Viewer"] {
  display: block;
  width: 100%;
  height: var(--pdf-frame-height);
  border: none;
  opacity: 1;            /* guard against accidental opacity */
  pointer-events: auto;  /* allow interactions inside the viewer */
  z-index: 1;            /* keep it above page background, below any modal */
}

/* Defensive: if any parent sets a dimming effect, neutralize it here */
.pdf-container,
.page-content {
  opacity: 1 !important;
  filter: none !important;
}

/* If you wrap the iframe with a container, ensure it doesn't collapse */
.pdf-wrapper {
  position: relative;
  min-height: var(--pdf-frame-height);
}
