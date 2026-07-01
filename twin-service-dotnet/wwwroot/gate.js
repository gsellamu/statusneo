/* gate.js - client-side "page access key" gate for the portal.
 * Loaded on every page via nav.js, before content is usable.
 *
 * IMPORTANT: this is a SOFT GATE / deterrent, NOT real security. The key check
 * runs in the browser, so anyone using dev-tools, "view source", or direct
 * requests to the served files (e.g. the raw .md sources) can bypass it. Real
 * access control must be enforced server-side by the .NET app (a login/cookie
 * check before wwwroot is served). This stops casual access and signals that the
 * material is confidential and access-controlled - nothing more.
 *
 * To change the key: edit KEY below.
 * To disable the gate: remove the "/gate.js" entry from the loader at the top of nav.js.
 */
(function () {
  "use strict";
  var FLAG = "__portal_access_granted";

  // The access key, reconstructed from char codes so it is not a plaintext
  // string in the source (obfuscation only - trivially reversible, not secure).
  // Current key: f f e e d d *
  var KEY = String.fromCharCode(102, 102, 101, 101, 100, 100, 42);

  // Already unlocked this browser session? then do nothing.
  try { if (sessionStorage.getItem(FLAG) === "yes") return; } catch (_) {}

  function build() {
    if (document.getElementById("__gate")) return;

    var g = document.createElement("div");
    g.id = "__gate";
    g.setAttribute("style",
      "position:fixed;inset:0;z-index:2147483600;background:linear-gradient(150deg,#16263f,#23375a);" +
      "display:flex;align-items:center;justify-content:center;padding:24px;" +
      "font-family:Calibri,Segoe UI,system-ui,sans-serif;");

    g.innerHTML =
      "<div style='max-width:442px;width:100%;background:#fff;border-radius:16px;padding:30px 30px 24px;" +
        "box-shadow:0 20px 60px rgba(0,0,0,.42);text-align:center;'>" +
        "<div style='font-size:30px;line-height:1;margin-bottom:8px;'>&#128274;</div>" +
        "<div style='font:bold 11px/1 Arial,sans-serif;letter-spacing:2.5px;color:#c9952f;text-transform:uppercase;'>Confidential &middot; Access Controlled</div>" +
        "<h1 style='font-family:Cambria,Georgia,serif;color:#16263f;font-size:23px;margin:9px 0 6px;'>Page Access Key Required</h1>" +
        "<p style='color:#5b6b7e;font-size:13.5px;margin:0 0 18px;line-height:1.5;'>This material is confidential and access-controlled. Enter your access key to continue.</p>" +
        "<input id='__gatekey' type='password' placeholder='Access key' autocomplete='off' spellcheck='false' " +
          "style='width:100%;padding:11px 13px;font-size:15px;border:1.5px solid #dfe6ee;border-radius:9px;" +
          "outline:none;text-align:center;letter-spacing:3px;box-sizing:border-box;'/>" +
        "<div id='__gateerr' style='color:#b3402f;font-size:12.5px;min-height:18px;margin:8px 0 2px;'></div>" +
        "<button id='__gatebtn' style='width:100%;padding:11px;font-size:14px;font-weight:bold;color:#16263f;" +
          "background:#e8a816;border:none;border-radius:9px;cursor:pointer;letter-spacing:.5px;'>Unlock</button>" +
        "<div style='margin-top:20px;padding-top:16px;border-top:1px solid #eef1f5;'>" +
          "<div style='font:bold 10.5px/1 Arial,sans-serif;letter-spacing:1.5px;color:#8090a4;text-transform:uppercase;margin-bottom:8px;'>Need an access key?</div>" +
          "<div style='color:#1f2a38;font-size:14px;font-weight:bold;'>Jeeth Sellamuthu</div>" +
          "<div style='margin-top:5px;font-size:13px;'>" +
            "<a href='mailto:gsellamu@gmail.com' style='color:#2e5a88;text-decoration:none;font-weight:bold;'>gsellamu@gmail.com</a>" +
            "<span style='color:#c3ccd8;'> &nbsp;&middot;&nbsp; </span>" +
            "<a href='tel:+18582260619' style='color:#2e5a88;text-decoration:none;font-weight:bold;'>+1 858 226 0619</a>" +
          "</div>" +
        "</div>" +
        "<div style='margin-top:16px;font-size:10.5px;color:#8090a4;line-height:1.5;'>" +
          "&copy; Jithendran Sellamuthu &middot; created for StatusNeo as part of the hiring process.</div>" +
      "</div>";

    (document.body || document.documentElement).appendChild(g);
    document.documentElement.style.overflow = "hidden";

    var input = document.getElementById("__gatekey");
    var btn = document.getElementById("__gatebtn");
    var err = document.getElementById("__gateerr");
    try { input.focus(); } catch (_) {}

    function submit() {
      if (input.value === KEY) {
        try { sessionStorage.setItem(FLAG, "yes"); } catch (_) {}
        if (g.parentNode) g.parentNode.removeChild(g);
        document.documentElement.style.overflow = "";
      } else {
        err.textContent = "Incorrect access key. Try again, or contact Jeeth for access.";
        input.value = "";
        try { input.focus(); } catch (_) {}
      }
    }
    btn.addEventListener("click", submit);
    input.addEventListener("keydown", function (e) {
      if ((e.key || "") === "Enter" || e.keyCode === 13) { e.preventDefault(); e.stopPropagation(); submit(); }
    });
  }

  if (document.body) build();
  else document.addEventListener("DOMContentLoaded", build);
})();
