const pptxgen = require("pptxgenjs");
const p = new pptxgen();
p.layout = "LAYOUT_16x9";              // 10 x 5.625 — match the target deck
p.author = "Jeeth";

const NAVY="16263F", INK="1F2A38", GOLDK="C9952F", GOLD="E8A816",
      CARD="F4F6FA", BODY="5B6B7E", LINE="DCE3EC", WHITE="FFFFFF",
      RED="B23A3A", GREEN="1E7A4C", AMBER="C77D2A";
const SERIF="Cambria", SANS="Calibri";
const sh=()=>({type:"outer",color:"000000",blur:6,offset:2,angle:90,opacity:0.10});

function head(s,kick,title){
  s.background={color:WHITE};
  s.addText(kick.toUpperCase(),{x:0.55,y:0.34,w:9,h:0.28,fontFace:SANS,fontSize:10.5,color:GOLDK,bold:true,charSpacing:3,margin:0});
  s.addText(title,{x:0.55,y:0.6,w:9,h:0.6,fontFace:SERIF,fontSize:28,color:NAVY,bold:true,margin:0});
}
function band(s,txt){
  s.addShape(p.shapes.ROUNDED_RECTANGLE,{x:0.4,y:4.84,w:9.2,h:0.66,fill:{color:NAVY},rectRadius:0.07});
  s.addShape(p.shapes.OVAL,{x:0.62,y:5.0,w:0.34,h:0.34,fill:{color:GOLD},line:{color:GOLD}});
  s.addText(txt,{x:1.12,y:4.84,w:8.34,h:0.66,valign:"middle",fontFace:SANS,fontSize:10,color:"E7EEF7",margin:0});
}
function numCircle(s,x,y,n,color){
  s.addShape(p.shapes.OVAL,{x,y,w:0.42,h:0.42,fill:{color},line:{color}});
  s.addText(String(n),{x,y,w:0.42,h:0.42,align:"center",valign:"middle",fontFace:SERIF,fontSize:16,color:WHITE,bold:true,margin:0});
}

/* ================= SLIDE 1 — THE BLEEDING ================= */
let s1=p.addSlide();
head(s1,"The stakes — why an SDE, not just a process","What's actually bleeding");
const blockers=[
  ["1","No paying customers","The product hasn't reached a revenue-able, shippable state."],
  ["2","E2E flow is broken","Features don't complete a full, testable end-to-end path."],
  ["3","Doesn't scale","The architecture won't carry production load or growth."],
  ["4","No Path-to-Green","Engineering and business are blocked, with no workable way out."],
];
blockers.forEach((b,i)=>{
  const x=0.4+i*2.32;
  s1.addShape(p.shapes.ROUNDED_RECTANGLE,{x,y:1.45,w:2.18,h:2.95,fill:{color:CARD},line:{color:LINE},rectRadius:0.08,shadow:sh()});
  numCircle(s1,x+0.22,y_(1.68),b[0],RED);
  s1.addText(b[1],{x:x+0.2,y:2.28,w:1.82,h:0.66,fontFace:SANS,fontSize:13,color:NAVY,bold:true,margin:0,valign:"top"});
  s1.addText(b[2],{x:x+0.2,y:2.96,w:1.82,h:1.3,fontFace:SANS,fontSize:10,color:BODY,margin:0,valign:"top"});
});
function y_(v){return v;} // (kept for clarity)
band(s1,"StatusNeo's transformation fixes HOW we work \u2014 ownership, SDLC, velocity. These four bleed because of WHAT we've built. They're architecture problems \u2014 that's the rescue an SDE is for.");

/* ================= SLIDE 2 — ROOT CAUSE ================= */
let s2=p.addSlide();
head(s2,"Root-cause diagnosis","Why it's bleeding: there is no spine");
const causes=[
  ["E2E broken","Domain truth lives in the Unity client; no deterministic contract between layers \u2014 so flows are non-deterministic and can't be tested headlessly.",RED],
  ["Doesn't scale","Stateful fat client, synchronous coupling, no CQRS or event streaming, no edge strategy for geometry.",RED],
  ["No customers","Not production-grade; rules enforced in batch (ASP/SPA), not inline; no BIM interoperability buyers expect.",AMBER],
  ["No Path-to-Green","No single source of truth and no seams \u2014 every change risks everything; nothing can be isolated, tested, or shipped.",AMBER],
];
causes.forEach((c,i)=>{
  const y=1.5+i*0.8;
  s2.addShape(p.shapes.ROUNDED_RECTANGLE,{x:0.4,y,w:2.15,h:0.66,fill:{color:c[2]},line:{color:c[2]},rectRadius:0.06});
  s2.addText(c[0],{x:0.4,y,w:2.15,h:0.66,align:"center",valign:"middle",fontFace:SANS,fontSize:12.5,color:WHITE,bold:true,margin:0});
  s2.addShape(p.shapes.LINE,{x:2.62,y:y+0.33,w:0.2,h:0,line:{color:BODY,width:1.5,endArrowType:"triangle"}});
  s2.addShape(p.shapes.ROUNDED_RECTANGLE,{x:2.92,y,w:6.68,h:0.66,fill:{color:CARD},line:{color:LINE},rectRadius:0.06});
  s2.addText(c[1],{x:3.08,y,w:6.4,h:0.66,valign:"middle",fontFace:SANS,fontSize:10,color:INK,margin:0});
});
band(s2,"The product sells \"the single source of truth.\" Internally, that truth is scattered across Unity, GraphQL, SQL Server, and Lakebase. Build the spine and the bleeding stops \u2014 I confirm specifics Day 1; the pattern holds.");

/* ================= SLIDE 3 — PATH TO GREEN ================= */
let s3=p.addSlide();
head(s3,"The rescue","Path-to-Green: six steps, twelve weeks");
const steps=[
  ["0","Wk 1\u20132","Establish the spine","Extract truth from Unity into an event-sourced twin. Unity becomes the GPU.","foundation",NAVY],
  ["1","Wk 2\u20134","Make E2E real","Inline deterministic gate + versioned contracts \u2192 deterministic, headlessly testable flows.","clears E2E",NAVY],
  ["2","Wk 4\u20138","Make it scale","CQRS + event streaming + content-addressed edge geometry.","clears scale",NAVY],
  ["3","Wk 6\u201310","Make it intelligent","Agentic AI \u2014 LangGraph / MCP / GraphRAG, gate-governed.","the moat",NAVY],
  ["4","Wk 8\u201312","Federate BIM","IFC / BCF / Autodesk APS interoperability.","buyer fit",NAVY],
  ["5","Wk 10\u201312","Prove it","One room type, end-to-end, production-grade vertical slice.","GREEN",GREEN],
];
steps.forEach((st,i)=>{
  const col=i%3, row=Math.floor(i/3);
  const x=0.4+col*3.08, y=1.5+row*1.55;
  s3.addShape(p.shapes.ROUNDED_RECTANGLE,{x,y,w:2.92,h:1.42,fill:{color:CARD},line:{color:st[5]===GREEN?GREEN:LINE},rectRadius:0.07,shadow:sh()});
  numCircle(s3,x+0.16,y+0.16,st[0],st[5]);
  s3.addText(st[1],{x:x+0.66,y:y+0.17,w:1.0,h:0.3,fontFace:SANS,fontSize:9.5,color:GOLDK,bold:true,margin:0,valign:"middle"});
  s3.addText(st[2],{x:x+1.72,y:y+0.17,w:1.12,h:0.3,align:"right",fontFace:SANS,fontSize:9,color:st[5]===GREEN?GREEN:BODY,bold:true,italic:true,margin:0,valign:"middle"});
  s3.addText(st[3],{x:x+0.16,y:y+0.6,w:2.6,h:0.62,fontFace:SANS,fontSize:9.5,color:INK,bold:true,margin:0,valign:"top"});
  // "clears X" tag bottom
  s3.addText([{text:"\u2192 "+st[4],options:{color:st[5]===GREEN?GREEN:GOLDK,bold:true}}],{x:x+0.16,y:y+1.12,w:2.6,h:0.24,fontFace:SANS,fontSize:8.5,margin:0,valign:"middle"});
});
band(s3,"Each step clears a named blocker. Green = a production-grade vertical slice, shippable to customer #1. This is a sequenced rescue on clean seams \u2014 not a rewrite.");

/* ================= SLIDE 4 — GREEN TARGET ================= */
let s4=p.addSlide();
head(s4,"The target state","Green: production-grade, scalable, sellable");
const green=[
  ["Single source of truth","a testable, observable, scalable spine"],
  ["Correct-by-construction","E2E flows that work, headlessly tested"],
  ["Horizontally scalable","CQRS, event-streaming, edge, multi-region"],
  ["Intelligent & governed","agentic AI, gated \u2014 the moat Autodesk lacks"],
  ["Interoperable","BIM-federated \u2014 credible to AEC buyers"],
  ["Shippable","production-grade slice \u2192 first paying customers"],
];
green.forEach((g,i)=>{
  const col=i%2, row=Math.floor(i/2);
  const x=0.4+col*4.62, y=1.5+row*0.66;
  s4.addShape(p.shapes.OVAL,{x,y:y+0.07,w:0.26,h:0.26,fill:{color:GREEN},line:{color:GREEN}});
  s4.addText("\u2713",{x,y:y+0.07,w:0.26,h:0.26,align:"center",valign:"middle",fontFace:SANS,fontSize:12,color:WHITE,bold:true,margin:0});
  s4.addText([{text:g[0]+"  ",options:{bold:true,color:NAVY,fontSize:12}},
              {text:"\u2014 "+g[1],options:{color:BODY,fontSize:10.5}}],
    {x:x+0.36,y,w:4.2,h:0.5,valign:"middle",fontFace:SANS,margin:0});
});
band(s4,"StatusNeo's transformation + this architecture rescue = the complete picture. Process fixes HOW we work; architecture fixes WHAT we ship. One without the other never reaches green.");

p.writeFile({fileName:"/tmp/rescue.pptx"}).then(f=>console.log("WROTE",f));
