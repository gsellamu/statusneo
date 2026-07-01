/* tour.js — guided voice tour across all pages, using the browser's built-in Web Speech API.
   Reads every content block in order, scrolls + highlights it, speaks it, and (in whole-site mode)
   auto-advances to the next page. No network needed. Read-only deterrents (docpage.js) still apply. */
(function () {
  "use strict";

  var ORDER  = ["index", "suite", "deck", "charter", "prfaq", "okr-star"];
  var LABELS = { index:"Home", suite:"The Suite", deck:"Board Deck", charter:"FDE Charter", prfaq:"PR-FAQ", "okr-star":"OKR & STAR" };
  var RESUME_KEY = "voxTourResume", VOICE_KEY = "voxTourVoice", HIDE_KEY = "voxTourCollapsed", SPEED_KEY = "voxTourRate";
  var synth = window.speechSynthesis || null;

  var st = { steps:[], idx:0, playing:false, paused:false, muted:false, voice:null, rate:1, site:false, timer:null };
  var voices = [];
  var bar, ctrlRow, statusEl, sel, speedSel, startBtn, silentBtn, pauseBtn, stopBtn, pill;
  var hlEl = null;

  function baseName(){ var s=(location.pathname.split("/").pop()||"").toLowerCase().replace(/\.html$/,""); return s||"index"; }
  function curIdx(){ var i=ORDER.indexOf(baseName()); return i<0?0:i; }
  function nextHref(){ var i=curIdx(); return (i+1<ORDER.length)?ORDER[i+1]+".html":null; }
  function escHtml(s){ return String(s).replace(/&/g,"&amp;").replace(/</g,"&lt;").replace(/>/g,"&gt;"); }
  function escAttr(s){ return String(s).replace(/&/g,"&amp;").replace(/"/g,"&quot;"); }

  // ---------- control bar ----------
  function buildBar(){
    bar = document.createElement("div");
    bar.className = "voxbar vox-skip";
    bar.innerHTML =
      '<div class="voxbar-main">'
      +   '<div class="voxbar-lead"><span class="voxbar-title">Guided voice tour</span> &mdash; sit back; it narrates and walks the whole site.</div>'
      +   '<div class="voxbar-actions">'
      +     '<button type="button" class="vox-btn vox-primary" data-vox="start">&#9654; Start voice tour</button>'
      +     '<button type="button" class="vox-btn vox-secondary" data-vox="silent">Tour without voice</button>'
      +     '<label class="vox-voice"><span>voice</span><select class="vox-select" aria-label="Voice"></select></label>'
      +     '<label class="vox-voice"><span>speed</span><select class="vox-select vox-speed" aria-label="Speech speed"><option value="0.75">0.75&times;</option><option value="0.9">0.9&times;</option><option value="1" selected>1&times;</option><option value="1.15">1.15&times;</option><option value="1.25">1.25&times;</option><option value="1.5">1.5&times;</option></select></label>'
      +   '</div>'
      +   '<button type="button" class="vox-collapse" data-vox="collapse" title="Hide" aria-label="Hide">&times;</button>'
      + '</div>'
      + '<div class="voxbar-tip">Tip: voice uses your browser&#39;s built-in speech &mdash; no internet needed. Make sure your speakers are on. Starting the tour begins from Home and walks every page.</div>'
      + '<div class="voxbar-controls" hidden>'
      +   '<button type="button" class="vox-btn vox-secondary" data-vox="pause">&#10073;&#10073; Pause</button>'
      +   '<button type="button" class="vox-btn vox-secondary" data-vox="stop">&#9632; Stop tour</button>'
      +   '<span class="vox-status"></span>'
      + '</div>'
      + '<div class="voxpill" data-vox="expand" hidden>&#128266; Voice tour</div>';

    var nav = document.querySelector("nav.nav");
    if (nav && nav.parentNode) nav.insertAdjacentElement("afterend", bar);
    else document.body.insertBefore(bar, document.body.firstChild);

    sel = bar.querySelector(".vox-select");
    speedSel = bar.querySelector(".vox-speed");
    ctrlRow = bar.querySelector(".voxbar-controls");
    statusEl = bar.querySelector(".vox-status");
    startBtn = bar.querySelector('[data-vox="start"]');
    silentBtn = bar.querySelector('[data-vox="silent"]');
    pauseBtn = bar.querySelector('[data-vox="pause"]');
    stopBtn = bar.querySelector('[data-vox="stop"]');
    pill = bar.querySelector(".voxpill");

    bar.addEventListener("click", function (e) {
      var b = e.target.closest && e.target.closest("[data-vox]"); if (!b) return;
      var a = b.getAttribute("data-vox");
      if (a==="start") startTour(false, false);
      else if (a==="silent") startTour(true, false);
      else if (a==="pause") togglePause();
      else if (a==="stop") stopTour();
      else if (a==="collapse") setCollapsed(true);
      else if (a==="expand") setCollapsed(false);
    });
    sel.addEventListener("change", function () {
      try { localStorage.setItem(VOICE_KEY, sel.value); } catch (e) {}
      st.voice = voiceByName(sel.value);
    });
    loadRate();
    speedSel.addEventListener("change", function () {
      st.rate = parseFloat(speedSel.value) || 1;
      try { localStorage.setItem(SPEED_KEY, speedSel.value); } catch (e) {}
    });

    if (!synth) { startBtn.disabled = true; startBtn.title = "No speech support in this browser; use Tour without voice."; sel.disabled = true; }
    var collapsed = false; try { collapsed = localStorage.getItem(HIDE_KEY) === "1"; } catch (e) {}
    if (collapsed) setCollapsed(true, true);
  }

  function setCollapsed(on, silentInit){
    var main = bar.querySelector(".voxbar-main"), tip = bar.querySelector(".voxbar-tip");
    if (on){ bar.classList.add("collapsed"); main.hidden=true; tip.hidden=true; pill.hidden=false; }
    else { bar.classList.remove("collapsed"); main.hidden=false; tip.hidden=false; pill.hidden=true; }
    if (!silentInit){ try { localStorage.setItem(HIDE_KEY, on?"1":"0"); } catch (e) {} }
  }

  // ---------- voices ----------
  function loadVoices(){
    if (!synth) return;
    voices = synth.getVoices() || [];
    if (!voices.length) return;
    var en = voices.filter(function(v){ return /^en/i.test(v.lang); });
    var rest = voices.filter(function(v){ return !/^en/i.test(v.lang); });
    var ordered = en.concat(rest);
    sel.innerHTML = ordered.map(function(v){ return '<option value="'+escAttr(v.name)+'">'+escHtml(v.name+" — "+v.lang)+'</option>'; }).join("");
    var saved=null; try { saved=localStorage.getItem(VOICE_KEY); } catch(e) {}
    var pick = (saved && voiceByName(saved)) ? saved
             : (pref(ordered,"David") || pref(ordered,"Google US English") || pref(ordered,"Samantha") || pref(ordered,"en-US") || (ordered[0]&&ordered[0].name));
    if (pick){ sel.value=pick; st.voice=voiceByName(pick); }
  }
  function pref(list, needle){ needle=needle.toLowerCase(); for (var i=0;i<list.length;i++){ if ((list[i].name+" "+list[i].lang).toLowerCase().indexOf(needle)>=0) return list[i].name; } return null; }
  function voiceByName(n){ for (var i=0;i<voices.length;i++){ if (voices[i].name===n) return voices[i]; } return null; }
  function loadRate(){ var r=null; try { r=localStorage.getItem(SPEED_KEY); } catch(e){} if (r && speedSel){ speedSel.value=r; } st.rate=parseFloat(speedSel&&speedSel.value)||1; }

  // ---------- collect readable content in document order ----------
  var BLOCK = {H1:1,H2:1,H3:1,H4:1,H5:1,H6:1,P:1,LI:1,BLOCKQUOTE:1,TR:1};
  var SKIP  = {NAV:1,FOOTER:1,SCRIPT:1,STYLE:1,SVG:1,NOSCRIPT:1,OPTION:1,SELECT:1,BUTTON:1,PRE:1};
  var BLOCKSEL = "div,section,header,footer,main,article,aside,nav,h1,h2,h3,h4,h5,h6,p,ul,ol,li,table,thead,tbody,tfoot,tr,td,th,blockquote,figure,pre,form";
  function hasBlockChild(el){ return !!el.querySelector(BLOCKSEL); }
  function clean(s){ return (s||"").replace(/\s+/g," ").trim(); }
  function tiny(t){ return t.length<=1 || /^[\s\d.,;:!?/&·—–()"'’-]{1,3}$/.test(t); }

  function collectSteps(){
    var steps=[];
    (function walk(node){
      for (var c=node.firstElementChild;c;c=c.nextElementSibling){
        var tag=c.tagName;
        if (SKIP[tag]) continue;
        if (c.classList && (c.classList.contains("voxbar")||c.classList.contains("vox-skip")||c.classList.contains("wm"))) continue;
        if (c.hidden || c.getAttribute("aria-hidden")==="true") continue;
        if (BLOCK[tag]){ var t=clean(c.innerText||c.textContent); if (t && !tiny(t)) steps.push({el:c,text:t}); }
        else if (!hasBlockChild(c)){ if (tag!=="A"){ var t2=clean(c.innerText||c.textContent); if (t2 && !tiny(t2)) steps.push({el:c,text:t2}); } }
        else walk(c);
      }
    })(document.body);
    return steps;
  }

  // ---------- flow ----------
  function startTour(muted, fromResume){
    if (!synth && !muted) muted = true;
    // whole-site tour always begins at Home
    if (!fromResume && baseName()!=="index"){
      try { sessionStorage.setItem(RESUME_KEY, JSON.stringify({ muted:!!muted, voice:(sel&&sel.value)||null })); } catch (e) {}
      location.href = "index.html"; return;
    }
    stopSpeaking();
    st.steps = collectSteps();
    st.idx = 0; st.playing = true; st.paused = false; st.muted = !!muted; st.site = true;
    if (sel && sel.value) st.voice = voiceByName(sel.value);
    startBtn.disabled = true; silentBtn.disabled = true;
    ctrlRow.hidden = false; if (bar.classList.contains("collapsed")) setCollapsed(false);
    updatePauseLabel();
    playStep();
  }

  function playStep(){
    if (!st.playing || st.paused) return;
    var step = st.steps[st.idx];
    setStatus();
    if (!step){ endPage(); return; }
    highlight(step.el);
    try { step.el.scrollIntoView({ behavior:"smooth", block:"center" }); } catch (e) { try { step.el.scrollIntoView(); } catch (e2) {} }

    if (st.muted || !synth){
      st.timer = setTimeout(advance, (Math.min(9000, Math.max(1500, step.text.length*42)))/(st.rate||1));
      return;
    }
    var chunks = step.text.match(/[^.!?]+[.!?]*/g) || [step.text];
    var ci = 0;
    (function say(){
      if (!st.playing || st.paused) return;
      if (ci >= chunks.length){ advance(); return; }
      var piece = chunks[ci].trim(); ci++;
      if (!piece){ say(); return; }
      var u = new SpeechSynthesisUtterance(piece);
      if (st.voice) u.voice = st.voice;
      u.rate = st.rate || 1; u.pitch = 1.0; u.volume = 1.0;
      var done = false;
      var wd = setTimeout(function tick(){ if (done) return; if (st.paused){ wd = setTimeout(tick, 500); return; } done = true; say(); }, Math.max(5000, piece.length*150/(st.rate||1)));
      u.onend = function(){ if (done) return; done = true; clearTimeout(wd); say(); };
      u.onerror = function(){ if (done) return; done = true; clearTimeout(wd); say(); };
      try { synth.speak(u); } catch (e) { done = true; clearTimeout(wd); say(); }
    })();
  }
  function advance(){ if (!st.playing) return; st.idx++; if (st.idx >= st.steps.length) endPage(); else playStep(); }

  function endPage(){
    clearHighlight();
    if (st.site){
      var nh = nextHref();
      if (nh){
        try { sessionStorage.setItem(RESUME_KEY, JSON.stringify({ muted:st.muted, voice:(sel&&sel.value)||null })); } catch (e) {}
        location.href = nh; return;
      }
    }
    finishTour();
  }
  function finishTour(){
    st.playing=false; st.paused=false; stopSpeaking(); clearHighlight();
    if (st.timer){ clearTimeout(st.timer); st.timer=null; }
    try { sessionStorage.removeItem(RESUME_KEY); } catch (e) {}
    startBtn.disabled=false; silentBtn.disabled=false; ctrlRow.hidden=true;
    setStatus("Tour complete.");
    try { window.scrollTo({ top:0, behavior:"smooth" }); } catch (e) {}
  }
  function stopTour(){
    st.playing=false; st.paused=false; st.site=false; stopSpeaking(); clearHighlight();
    if (st.timer){ clearTimeout(st.timer); st.timer=null; }
    try { sessionStorage.removeItem(RESUME_KEY); } catch (e) {}
    startBtn.disabled=false; silentBtn.disabled=false; ctrlRow.hidden=true; setStatus("");
  }
  function togglePause(){
    if (!st.playing) return;
    if (st.paused){
      st.paused=false; updatePauseLabel(); setStatus();
      if (!st.muted && synth){ try { synth.resume(); } catch (e) {} }
      if (st.muted) playStep();
      else playStep();
    } else {
      st.paused=true;
      if (!st.muted && synth){ try { synth.pause(); } catch (e) {} }
      if (st.timer){ clearTimeout(st.timer); st.timer=null; }
      updatePauseLabel(); setStatus("Paused.");
    }
  }
  function updatePauseLabel(){ if (pauseBtn) pauseBtn.innerHTML = st.paused ? "&#9654; Resume" : "&#10073;&#10073; Pause"; }
  function stopSpeaking(){ try { if (synth) synth.cancel(); } catch (e) {} }

  function highlight(el){ clearHighlight(); hlEl=el; if (el && el.classList) el.classList.add("vox-hl"); }
  function clearHighlight(){ if (hlEl && hlEl.classList) hlEl.classList.remove("vox-hl"); hlEl=null; }

  function setStatus(msg){
    if (!statusEl) return;
    if (msg){ statusEl.textContent = msg; return; }
    var lbl = LABELS[baseName()] || baseName();
    var n = Math.min(st.idx+1, st.steps.length);
    statusEl.textContent = st.playing ? ("Touring — " + lbl + " · " + n + "/" + st.steps.length) : "";
  }

  // ---------- init ----------
  function init(){
    buildBar();
    if (synth){ loadVoices(); try { synth.onvoiceschanged = loadVoices; } catch (e) {} }

    var res=null; try { res=sessionStorage.getItem(RESUME_KEY); } catch (e) {}
    if (res){
      try { sessionStorage.removeItem(RESUME_KEY); } catch (e) {}   // consume once; endPage re-sets before each hop
      var cfg={}; try { cfg=JSON.parse(res)||{}; } catch (e) {}
      var wantVoice = !cfg.muted && synth, tries=0;
      (function go(){
        if (wantVoice && !(voices && voices.length) && tries<25){ tries++; setTimeout(go, 120); return; }
        if (cfg.voice && sel){ sel.value=cfg.voice; st.voice=voiceByName(cfg.voice); }
        startTour(!!cfg.muted, true);
      })();
    }
    window.addEventListener("pagehide", stopSpeaking);
    window.addEventListener("beforeunload", stopSpeaking);
  }

  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", init);
  else init();
})();
