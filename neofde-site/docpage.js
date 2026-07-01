/* docpage.js — renders embedded markdown (#src -> #doc) and applies read-only deterrents.
   Shared by charter/prfaq/okr-star (full render) and deck (deterrents only). */
(function () {
  // ---------- light normalization for public consistency (cosmetic only) ----------
  function normalize(t) {
    return t
      .replace(/AuthenticAI™/g, "Authentic AI™")
      .replace(/\bdefense\b/g, "airlines");
  }

  // ---------- minimal offline markdown -> HTML ----------
  function esc(s){return s.replace(/&/g,"&amp;").replace(/</g,"&lt;").replace(/>/g,"&gt;");}
  function inline(s){
    var parts=s.split(/(`[^`]+`)/);
    return parts.map(function(p){
      if(/^`[^`]+`$/.test(p)) return "<code>"+esc(p.slice(1,-1))+"</code>";
      p=esc(p);
      p=p.replace(/\*\*([^*]+)\*\*/g,"<strong>$1</strong>");
      p=p.replace(/(^|[^*])\*([^*]+)\*/g,"$1<em>$2</em>");
      return p;
    }).join("");
  }
  function mdToHtml(md){
    var lines=md.replace(/\r\n/g,"\n").split("\n");
    var out=[], i=0, para=[];
    function flush(){ if(para.length){ out.push("<p>"+inline(para.join(" "))+"</p>"); para.length=0; } }
    while(i<lines.length){
      var ln=lines[i];
      if(/^```/.test(ln)){ flush(); var code=[]; i++; while(i<lines.length && !/^```/.test(lines[i])){ code.push(lines[i]); i++; } i++; out.push("<pre><code>"+esc(code.join("\n"))+"</code></pre>"); continue; }
      if(/^\s*\|.*\|\s*$/.test(ln) && i+1<lines.length && /^\s*\|[\s:|-]+\|\s*$/.test(lines[i+1])){
        flush();
        var head=ln.trim().replace(/^\||\|$/g,"").split("|").map(function(c){return c.trim();});
        i+=2; var rows=[];
        while(i<lines.length && /^\s*\|.*\|\s*$/.test(lines[i])){ rows.push(lines[i].trim().replace(/^\||\|$/g,"").split("|").map(function(c){return c.trim();})); i++; }
        var t="<table><thead><tr>"+head.map(function(h){return "<th>"+inline(h)+"</th>";}).join("")+"</tr></thead><tbody>";
        t+=rows.map(function(r){return "<tr>"+r.map(function(c){return "<td>"+inline(c)+"</td>";}).join("")+"</tr>";}).join("");
        out.push(t+"</tbody></table>"); continue;
      }
      var h=ln.match(/^(#{1,4})\s+(.*)$/);
      if(h){ flush(); var lvl=h[1].length; out.push("<h"+lvl+">"+inline(h[2])+"</h"+lvl+">"); i++; continue; }
      if(/^\s*---+\s*$/.test(ln)){ flush(); out.push("<hr>"); i++; continue; }
      if(/^\s*>\s?/.test(ln)){ flush(); var bq=[]; while(i<lines.length && /^\s*>\s?/.test(lines[i])){ bq.push(lines[i].replace(/^\s*>\s?/,"")); i++; } out.push("<blockquote>"+inline(bq.join(" "))+"</blockquote>"); continue; }
      if(/^\s*[-*]\s+/.test(ln)){ flush(); var ul=[]; while(i<lines.length && /^\s*[-*]\s+/.test(lines[i])){ ul.push("<li>"+inline(lines[i].replace(/^\s*[-*]\s+/,""))+"</li>"); i++; } out.push("<ul>"+ul.join("")+"</ul>"); continue; }
      if(/^\s*\d+\.\s+/.test(ln)){ flush(); var ol=[]; while(i<lines.length && /^\s*\d+\.\s+/.test(lines[i])){ ol.push("<li>"+inline(lines[i].replace(/^\s*\d+\.\s+/,""))+"</li>"); i++; } out.push("<ol>"+ol.join("")+"</ol>"); continue; }
      if(/^\s*$/.test(ln)){ flush(); i++; continue; }
      para.push(ln.trim()); i++;
    }
    flush();
    return out.join("\n");
  }

  // ---------- render, if this page carries embedded markdown ----------
  var src=document.getElementById("src");
  var target=document.getElementById("doc");
  if(src && target){
    try{ target.innerHTML=mdToHtml(normalize(src.textContent)); }
    catch(e){ target.innerHTML='<p class="loading">Content could not be rendered.</p>'; }
  }

  // ---------- read-only deterrents (soft; discourage casual copy/save/print) ----------
  ["contextmenu","copy","cut","dragstart"].forEach(function(ev){
    document.addEventListener(ev,function(e){ e.preventDefault(); },{capture:true});
  });
  document.addEventListener("keydown",function(e){
    var k=(e.key||"").toLowerCase();
    if((e.ctrlKey||e.metaKey) && ["c","s","p","u"].indexOf(k)!==-1){ e.preventDefault(); }
    if(k==="f12"){ e.preventDefault(); }
    if((e.ctrlKey||e.metaKey) && e.shiftKey && ["i","j","c"].indexOf(k)!==-1){ e.preventDefault(); }
  },{capture:true});
})();
