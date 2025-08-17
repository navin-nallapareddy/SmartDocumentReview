SmartDocumentReview / wwwroot / pdfjs
=====================================

Goal
----
Host the stock pdf.js viewer at:
  /pdfjs/web/viewer.html
with its worker at:
  /pdfjs/build/pdf.worker.min.js

Folder Layout (must match exactly)
----------------------------------
wwwroot/
  pdfjs/
    web/
      viewer.html
      viewer.css
      viewer.js
      locale/                 (directory)
      images/                 (directory)
      cmaps/                  (optional, for CJK/vertical fonts)
    build/
      pdf.js                  (or pdf.mjs if using modules)
      pdf.min.js              (optional)
      pdf.worker.min.js
      pdf.worker.js           (optional)
      pdf.sandbox.js          (optional)
    LICENSE
    README.md                 (optional upstream readme)

Where to get these files
------------------------
Option A: Official prebuilt bundle (recommended)
  1) Download the "pdfjs-dist" package (NPM or GitHub release).
     - NPM:   npm i pdfjs-dist
     - GitHub: https://github.com/mozilla/pdf.js/releases
  2) Copy the contents:
     node_modules/pdfjs-dist/web/*           -> wwwroot/pdfjs/web/
     node_modules/pdfjs-dist/build/*         -> wwwroot/pdfjs/build/
     node_modules/pdfjs-dist/cmaps/*         -> wwwroot/pdfjs/web/cmaps/   (optional)

Option B: Use a release zip from GitHub
  - Extract the archive and copy the same folders into wwwroot/pdfjs/

Do NOT rename "web" or "build" — the viewer expects these paths.

App Integration
---------------
- If you use the stock viewer (recommended), navigate to:
    /pdfjs/web/viewer.html?file=/uploads/<your-file.pdf>
  (Your app builds this URL; see Pages/PDFResult.razor.)

- If you embed the library directly, set the worker path:
    pdfjsLib.GlobalWorkerOptions.workerSrc = '/pdfjs/build/pdf.worker.min.js';
  (We include a helper in /wwwroot/pdf-viewer/init.js.)

Verification Checklist
----------------------
1) Browse to /pdfjs/web/viewer.html  → Viewer UI loads without errors.
2) Open DevTools → Network:
   - "pdf.worker.min.js" should be 200 OK (not 404/blocked).
   - "viewer.css", "viewer.js" load with 200 OK.
3) Open a file URL:
   /pdfjs/web/viewer.html?file=/uploads/<an-existing.pdf>
   - The canvas should render page 1 (no endless gray area).
4) Console should have **no** CSP or CORS errors.

Common Pitfalls
---------------
- 404 for pdf.worker.min.js  → Paths are wrong or files weren’t published.
- CSP blocks worker          → Ensure Program.cs sets worker-src 'self' blob:
- App under sub-path (/app)  → Update <base href="/app/"> in Pages/_Host.cshtml.
- Wrong MIME for PDFs        → Program.cs adds mapping to "application/pdf".
- Mixed content (http vs https) → Serve PDFs on the same origin/protocol.

Publishing Note
---------------
Ensure your .csproj copies all static files:
  <ItemGroup>
    <Content Include="wwwroot\**\*.*" CopyToOutputDirectory="PreserveNewest" />
  </ItemGroup>

That’s it—once these assets are in place, the viewer won’t sit “greyed out.”
