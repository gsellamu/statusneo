/* protect.js — confidential deterrents + watermark + footnote for the portal.
 * Loaded on every page via nav.js.
 *
 * IMPORTANT: these are DETERRENTS, not security. Client-side code can deter
 * casual copy / right-click / print, and the watermark ensures any screenshot
 * carries the confidential notice. It CANNOT stop OS-level screenshots (PrtScn,
 * Snipping Tool, phone camera), and a determined user can bypass everything via
 * dev-tools, "view source", or by disabling JavaScript. True protection needs
 * server-side authentication. This raises the bar and marks provenance; nothing
 * more. To disable: remove the protect.js loader line at the top of nav.js. */
(function () {
  "use strict";

  var NOTICE_HTML =
    "<b>CONFIDENTIAL</b> &mdash; Copyright of <b>Jithendran Sellamuthu</b>. " +
    "Created for <b>StatusNeo</b> as part of the hiring process. " +
    "Not for distribution, reproduction, or printing.";
  var WM = "CONFIDENTIAL \u00b7 \u00a9 Jithendran Sellamuthu \u00b7 created for StatusNeo hiring process";

  /* ---------- styles ---------- */
  var st = document.createElement("style");
  st.textContent =
    "html,body{-webkit-user-select:none;-moz-user-select:none;-ms-user-select:none;user-select:none;-webkit-touch-callout:none;}" +
    "input,textarea,select,[contenteditable=true]{-webkit-user-select:text;-moz-user-select:text;user-select:text;}" +
    "img,a{-webkit-user-drag:none;user-drag:none;}" +
    "#__wm{position:fixed;inset:0;z-index:2147483000;pointer-events:none;background-repeat:repeat;}" +
    "#__ft{position:fixed;left:0;right:0;bottom:0;z-index:2147482500;padding:9px 16px;" +
      "border-top:2px solid #c9952f;background:#0e1b2e;color:#c9d6e8;" +
      "font:12px/1.4 Calibri,Segoe UI,system-ui,sans-serif;text-align:center;}" +
    "#__ft b{color:#e8a816;}" +
    "body{padding-bottom:46px;}" +
    "@media print{html,body{background:#fff !important;}" +
      "body *{visibility:hidden !important;}" +
      "#__wm,#__ft{display:none !important;}" +
      "body::before{visibility:visible !important;position:fixed;top:25mm;left:0;right:0;" +
      "padding:0 20mm;white-space:normal;text-align:center;font:bold 14pt/1.6 Arial,sans-serif;color:#000;" +
      "content:'CONFIDENTIAL \\2014 Copyright of Jithendran Sellamuthu. Created for StatusNeo as part of the hiring process. Printing and reproduction are prohibited.';}}";
  (document.head || document.documentElement).appendChild(st);

  /* ---------- watermark: tiled diagonal SVG covering the viewport ---------- */
  /* fixed => it always covers whatever is on screen, so every screenshot carries it */
  function wmURI() {
    var svg =
      "<svg xmlns='http://www.w3.org/2000/svg' width='300' height='150'>" +
      "<text x='6' y='120' transform='rotate(-32 6 120)' fill='rgba(90,107,126,0.13)' " +
      "font-family='Arial,sans-serif' font-size='13' font-weight='bold'>" + WM + "</text></svg>";
    return "url(\"data:image/svg+xml;utf8," + encodeURIComponent(svg) + "\")";
  }
  function buildWM() {
    var wm = document.getElementById("__wm");
    if (!wm) { wm = document.createElement("div"); wm.id = "__wm"; document.body.appendChild(wm); }
    wm.style.backgroundImage = wmURI();
  }

  /* ---------- confidential footnote ---------- */
  function buildFoot() {
    if (document.getElementById("__ft")) return;
    var ft = document.createElement("div");
    ft.id = "__ft";
    ft.innerHTML = NOTICE_HTML;
    document.body.appendChild(ft);
  }

  /* ---------- deterrents ---------- */
  function stop(e) { try { e.preventDefault(); e.stopPropagation(); } catch (_) {} return false; }
  ["contextmenu", "copy", "cut", "dragstart"].forEach(function (ev) {
    document.addEventListener(ev, stop, true);
  });
  document.addEventListener("selectstart", function (e) {
    var t = e.target, tag = t && t.tagName;
    if (tag === "INPUT" || tag === "TEXTAREA" || (t && t.isContentEditable)) return;
    return stop(e);
  }, true);
  document.addEventListener("keydown", function (e) {
    var k = (e.key || "").toLowerCase();
    // Ctrl/Cmd + P(print) S(save) C(copy) A(select-all) U(view-source)
    if ((e.ctrlKey || e.metaKey) && ["p", "s", "c", "a", "u"].indexOf(k) !== -1) return stop(e);
    // PrintScreen — cannot truly block the OS, best-effort clipboard clear + swallow
    if (k === "printscreen" || e.keyCode === 44) {
      try { navigator.clipboard && navigator.clipboard.writeText && navigator.clipboard.writeText(""); } catch (_) {}
      return stop(e);
    }
    // dev-tools shortcuts (deterrent only)
    if (k === "f12") return stop(e);
    if ((e.ctrlKey || e.metaKey) && e.shiftKey && ["i", "j", "c"].indexOf(k) !== -1) return stop(e);
  }, true);

  /* ---------- init ---------- */
  function init() { buildWM(); buildFoot(); }
  if (document.body) init(); else document.addEventListener("DOMContentLoaded", init);
  var rt;
  window.addEventListener("resize", function () { clearTimeout(rt); rt = setTimeout(buildWM, 150); });
})();
