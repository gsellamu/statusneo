/* nav.js — shared top navigation injected into every Spine page.
 * One script, linked from each wwwroot page. A single "Menu" button opens a
 * click-dropdown listing every surface + artifact, grouped. Brand + health
 * pip stay visible. Highlights the current page. Closes on outside-click/Esc. */
(function () {
  // grouped link model: [href, label, optional download flag]
  var GROUPS = [
    ["Live surfaces", [
      ["/showcase.html", "★ Showcase"],
      ["/interview.html", "⚡ AI Interview"],
      ["/index.html", "Planner"],
      ["/studio.html", "Lens Studio"],
      ["/hierarchy.html", "Hierarchy"],
      ["/telemetry.html", "Operational"],
      ["/architecture.html", "Architecture"],
      ["/ai-engine.html", "AI Engine"],
      ["/ecosystem.html", "Ecosystem"],
      ["/vision.html", "Vision"],
      ["/two-track-org.html", "Two-Track Org"],
      ["/statusneo.html", "★ StatusNeo FDE"],
    ]],
    ["StatusNeo · Agentic FDE suite", [
      ["/statusneo.html", "FDE Practice — overview"],
      ["/artifacts/statusneo/index.html", "Artifact index & file map"],
      ["/artifacts/statusneo/reader.html?doc=00-MASTER-AGENTIC-FDE-REFERENCE", "Master Reference"],
      ["/artifacts/statusneo/reader.html?doc=01-FDE-CHARTER-PROPOSAL", "FDE Charter Proposal"],
      ["/artifacts/statusneo/reader.html?doc=06-FDE-80-20-ROLES-AND-LIFECYCLE", "80:20 Roles & Lifecycle"],
      ["/artifacts/statusneo/reader.html?doc=05-AGENTIC-FDE-LIFECYCLE", "Agentic FDE Lifecycle"],
      ["/artifacts/statusneo/reader.html?doc=07-MULTI-CLOUD-AGENTIC-FDE-TOOLING-DIRECTORY", "Multi-Cloud Tooling Directory"],
      ["/artifacts/statusneo/reader.html?doc=08-AEC-MODUTECTURE-FDE-USECASE", "AEC / Modutecture Use Case"],
      ["/artifacts/statusneo/reader.html?doc=02-FDE-LOOP-COOKBOOK", "FDE Loop Cookbook"],
      ["/artifacts/statusneo/reader.html?doc=03-LANDING-RUNBOOK", "Landing Runbook"],
      ["/artifacts/statusneo/reader.html?doc=09-FDE-30-60-90-OKR-STAR", "30/60/90 OKR & STAR"],
      ["/artifacts/statusneo/reader.html?doc=10-FDE-OKR-SCORECARD", "OKR Scorecard"],
      ["/artifacts/statusneo/StatusNeo_FDE_Charter.pptx", "Board Deck (.pptx)", true],
      ["/artifacts/statusneo/StatusNeo_FDE_PR-FAQ_Playbook.pdf", "PR-FAQ Playbook (.pdf)", true],
    ]],
    ["Architecture artifacts", [
      ["/artifacts/ADR-009-edge-feed-layer.html", "ADR-009 · Edge-Feed (page)"],
      ["/artifacts/ADR-009-edge-feed-layer.pptx", "ADR-009 · Deck (.pptx)", true],
      ["/artifacts/ADR-009-edge-feed-layer.docx", "ADR-009 · Doc (.docx)", true],
      ["/artifacts/ADR-009-edge-feed-layer.md", "ADR-009 · Markdown (.md)", true],
    ]],
  ];

  var here = location.pathname.replace(/\/$/, "/index.html") || "/index.html";
  if (here === "/") here = "/index.html";

  // ---- bar ----
  var bar = document.createElement("nav");
  bar.setAttribute("data-spine-nav", "1");
  bar.style.cssText =
    "display:flex;gap:10px;align-items:center;background:#0e1b2e;" +
    "padding:6px 14px;font-family:Calibri,Segoe UI,system-ui,sans-serif;" +
    "border-bottom:1px solid #22344f;position:sticky;top:0;z-index:1000;";

  var brand = document.createElement("span");
  brand.textContent = "Modutecture Spine";
  brand.style.cssText =
    "color:#e8a816;font-weight:bold;font-size:12.5px;letter-spacing:.5px;";
  bar.appendChild(brand);

  // ---- menu button + dropdown wrapper (position:relative anchor) ----
  var wrap = document.createElement("div");
  wrap.style.cssText = "position:relative;";

  var btn = document.createElement("button");
  btn.type = "button";
  btn.setAttribute("aria-haspopup", "true");
  btn.setAttribute("aria-expanded", "false");
  // label shows current page so the bar still tells you where you are
  var hereLabel = "Menu";
  GROUPS.forEach(function (g) { g[1].forEach(function (p) { if (p[0] === here) hereLabel = p[1]; }); });
  btn.innerHTML = "&#9776;&nbsp; " + hereLabel + " &#9662;";
  btn.style.cssText =
    "font-family:inherit;font-size:12px;cursor:pointer;color:#16263f;background:#e8a816;" +
    "border:0;border-radius:8px;padding:5px 12px;font-weight:bold;";
  btn.onmouseover = function () { btn.style.background = "#f0b020"; };
  btn.onmouseout = function () { btn.style.background = "#e8a816"; };
  wrap.appendChild(btn);

  var menu = document.createElement("div");
  menu.setAttribute("role", "menu");
  menu.style.cssText =
    "display:none;position:absolute;left:0;top:calc(100% + 6px);min-width:268px;" +
    "background:#14233c;border:1px solid #2c456c;border-radius:12px;" +
    "box-shadow:0 12px 34px rgba(0,0,0,.42);padding:8px;z-index:1001;";

  GROUPS.forEach(function (group, gi) {
    var hd = document.createElement("div");
    hd.textContent = group[0];
    hd.style.cssText =
      "color:#7f97b8;font-size:9.5px;font-weight:bold;letter-spacing:1.4px;" +
      "text-transform:uppercase;padding:" + (gi ? "10px 10px 4px" : "4px 10px 4px") + ";";
    menu.appendChild(hd);

    group[1].forEach(function (p) {
      var a = document.createElement("a");
      a.href = p[0];
      if (p[2]) a.setAttribute("download", "");
      var active = here === p[0];
      a.textContent = p[1];
      a.setAttribute("role", "menuitem");
      a.style.cssText =
        "display:block;text-decoration:none;font-size:12.5px;padding:7px 10px;border-radius:8px;" +
        "color:" + (active ? "#16263f" : "#d6e2f2") + ";" +
        "background:" + (active ? "#e8a816" : "transparent") + ";" +
        "font-weight:" + (active ? "bold" : "normal") + ";";
      a.onmouseover = function () { if (!active) a.style.background = "#1f3559"; };
      a.onmouseout = function () { if (!active) a.style.background = "transparent"; };
      menu.appendChild(a);
    });
  });
  wrap.appendChild(menu);
  bar.appendChild(wrap);

  // ---- contextual StatusNeo link: each portal page -> its most relevant FDE doc ----
  var RELATED = {
    "/index.html": ["/statusneo.html", "StatusNeo FDE suite"],
    "/studio.html": ["/statusneo.html", "StatusNeo FDE suite"],
    "/interview.html": ["/statusneo.html", "StatusNeo FDE suite"],
    "/hierarchy.html": ["/artifacts/statusneo/reader.html?doc=08-AEC-MODUTECTURE-FDE-USECASE", "AEC Use Case"],
    "/telemetry.html": ["/artifacts/statusneo/reader.html?doc=06-FDE-80-20-ROLES-AND-LIFECYCLE", "80:20 Roles & Lifecycle"],
    "/architecture.html": ["/artifacts/statusneo/reader.html?doc=08-AEC-MODUTECTURE-FDE-USECASE", "AEC Architecture & Use Case"],
    "/ai-engine.html": ["/artifacts/statusneo/reader.html?doc=05-AGENTIC-FDE-LIFECYCLE", "Agentic FDE Lifecycle"],
    "/ecosystem.html": ["/artifacts/statusneo/reader.html?doc=00-MASTER-AGENTIC-FDE-REFERENCE", "FDE Master Reference"],
    "/vision.html": ["/artifacts/statusneo/reader.html?doc=09-FDE-30-60-90-OKR-STAR", "FDE 30/60/90 — OKR & STAR"],
    "/two-track-org.html": ["/statusneo.html", "StatusNeo FDE suite"]
  };
  var rel = RELATED[here];
  if (rel) {
    var relA = document.createElement("a");
    relA.href = rel[0];
    relA.innerHTML = "\u2605 " + rel[1] + " \u2192";
    relA.title = "Related StatusNeo FDE document";
    relA.style.cssText =
      "text-decoration:none;font-size:11.5px;font-weight:bold;color:#16263f;background:#e8a816;" +
      "border-radius:8px;padding:5px 11px;white-space:nowrap;";
    relA.onmouseover = function () { relA.style.background = "#f0b020"; };
    relA.onmouseout = function () { relA.style.background = "#e8a816"; };
    bar.appendChild(relA);
  }

  // ---- open/close logic ----
  var open = false;
  function setOpen(v) {
    open = v;
    menu.style.display = v ? "block" : "none";
    btn.setAttribute("aria-expanded", v ? "true" : "false");
  }
  btn.onclick = function (e) { e.stopPropagation(); setOpen(!open); };
  document.addEventListener("click", function (e) {
    if (open && !wrap.contains(e.target)) setOpen(false);
  });
  document.addEventListener("keydown", function (e) {
    if (e.key === "Escape" && open) setOpen(false);
  });

  // ---- health pip (right) ----
  var pip = document.createElement("span");
  pip.style.cssText = "margin-left:auto;font-size:11px;color:#9fc3e8;";
  pip.textContent = "● …";
  bar.appendChild(pip);
  fetch("/health").then(function (r) { return r.json(); }).then(function (h) {
    var ok = h.status === "healthy";
    pip.textContent = (ok ? "● healthy" : "● degraded");
    pip.style.color = ok ? "#7ee0a8" : "#e8c97e";
  }).catch(function () { pip.textContent = "● offline"; pip.style.color = "#e08a7e"; });

  function inject() {
    if (document.querySelector("nav[data-spine-nav]")) return;
    document.body.insertBefore(bar, document.body.firstChild);
  }
  if (document.body) inject();
  else document.addEventListener("DOMContentLoaded", inject);
})();
