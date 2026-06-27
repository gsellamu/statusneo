// ============================================================================
// Modutecture Twin — Neo4j Graph Model
// ----------------------------------------------------------------------------
// THREE graphs, one store. The agent pipeline READS layer 2 (grounding) and
// WRITES layer 1 (truth) ONLY through the deterministic gate. AI never owns truth.
//
//   Layer 1  DOMAIN / TWIN ........ the source of record (tree + DAG + earned edges)
//   Layer 2  KNOWLEDGE / GROUNDING . codes, rules, ports (GraphRAG retrieval target)
//   (Layer 3 LangGraph pipeline is control-flow, not data — see twin-ai-pipeline.md)
// ============================================================================

// ---- constraints (idempotent) ---------------------------------------------
CREATE CONSTRAINT building_id   IF NOT EXISTS FOR (b:Building)     REQUIRE b.id IS UNIQUE;
CREATE CONSTRAINT floor_id      IF NOT EXISTS FOR (f:Floor)        REQUIRE f.id IS UNIQUE;
CREATE CONSTRAINT room_id       IF NOT EXISTS FOR (r:Room)         REQUIRE r.id IS UNIQUE;
CREATE CONSTRAINT instance_id   IF NOT EXISTS FOR (i:Instance)     REQUIRE i.id IS UNIQUE;
CREATE CONSTRAINT mtype_id      IF NOT EXISTS FOR (t:ModuculeType) REQUIRE t.id IS UNIQUE;
CREATE CONSTRAINT template_id   IF NOT EXISTS FOR (x:RoomTemplate) REQUIRE x.id IS UNIQUE;
CREATE CONSTRAINT rule_id       IF NOT EXISTS FOR (u:Rule)         REQUIRE u.id IS UNIQUE;

// ============================================================================
// LAYER 1 — DOMAIN / TWIN  (the truth)
// ============================================================================

// --- containment: an N-ARY TREE (strict, exclusive ownership) ---------------
//     Building -[:HAS_FLOOR]-> Floor -[:HAS_ROOM]-> Room -[:CONTAINS]-> Instance
MERGE (b:Building {id:'stmary', name:"St. Mary's Medical Center"})
MERGE (f1:Floor {id:'floor-1', name:'Floor 1 - ICU'})
MERGE (f2:Floor {id:'floor-2', name:'Floor 2 - Exam'})
MERGE (b)-[:HAS_FLOOR]->(f1)
MERGE (b)-[:HAS_FLOOR]->(f2)
WITH f1, f2
UNWIND [['icu-101','ICU Room 101'],['icu-102','ICU Room 102'],['icu-103','ICU Room 103']] AS r
  MERGE (room:Room {id:r[0]}) SET room.name=r[1], room.program='observation room', room.status='EMPTY'
  MERGE (f1)-[:HAS_ROOM]->(room);
MATCH (f2:Floor {id:'floor-2'})
UNWIND [['exam-201','Exam Room 201'],['exam-202','Exam Room 202']] AS r
  MERGE (room:Room {id:r[0]}) SET room.name=r[1], room.program='exam room', room.status='EMPTY'
  MERGE (f2)-[:HAS_ROOM]->(room);

// --- type catalogue + Room Moducules: a COMPOSITION DAG (shared = many parents)
//     A ModuculeType reused by many RoomTemplates is a SHARED node -> the blast radius.
MERGE (hw:ModuculeType   {id:'headwall-hw204'}) SET hw.name='Headwall HW-204', hw.version='2.3.0'
MERGE (bed:ModuculeType  {id:'bed-icu'})        SET bed.name='ICU Bed',       bed.version='1.4.0'
MERGE (sink:ModuculeType {id:'sink-clinical'})  SET sink.name='Clinical Sink', sink.version='1.0.1';
// Room Moducules (a room is itself a Moducule) composed of content types
MERGE (icuTpl:RoomTemplate  {id:'std-icu-room'})  SET icuTpl.name='Standard ICU Room'
MERGE (examTpl:RoomTemplate {id:'std-exam-room'}) SET examTpl.name='Standard Exam Room'
WITH icuTpl, examTpl
MATCH (hw:ModuculeType {id:'headwall-hw204'}), (bed:ModuculeType {id:'bed-icu'}), (sink:ModuculeType {id:'sink-clinical'})
// headwall is COMPOSED_OF by BOTH templates -> a shared node with many parents == DAG, not tree
MERGE (icuTpl)-[:COMPOSED_OF]->(hw)
MERGE (icuTpl)-[:COMPOSED_OF]->(bed)
MERGE (icuTpl)-[:COMPOSED_OF]->(sink)
MERGE (examTpl)-[:COMPOSED_OF]->(hw)
MERGE (examTpl)-[:COMPOSED_OF]->(bed)
MERGE (examTpl)-[:COMPOSED_OF]->(sink);

// --- a stamped, COMPLIANT room (truth) with EARNED edges --------------------
//     Instances pin the exact validated type version. The MED_GAS edge exists
//     ONLY because the gate's reach check passed -> "every edge is earned".
MATCH (r:Room {id:'icu-101'}), (hw:ModuculeType {id:'headwall-hw204'}), (bed:ModuculeType {id:'bed-icu'})
MERGE (ihw:Instance {id:'i-hw-101'})  SET ihw.x=2000, ihw.y=250,  ihw.rotation=0
MERGE (ibed:Instance {id:'i-bed-101'}) SET ibed.x=2000, ibed.y=1550, ibed.rotation=0
MERGE (r)-[:CONTAINS]->(ihw)
MERGE (r)-[:CONTAINS]->(ibed)
MERGE (ihw)-[:OF_TYPE  {pinnedVersion:'2.3.0'}]->(hw)
MERGE (ibed)-[:OF_TYPE {pinnedVersion:'1.4.0'}]->(bed)
MERGE (ibed)-[:MED_GAS {validatedAt: datetime(), reachMm: 1300}]->(ihw)   // earned, directed
SET r.status='COMPLIANT';

// --- room adjacency (a relationship edge, not containment) ------------------
MATCH (a:Room {id:'icu-101'}), (c:Room {id:'icu-102'}) MERGE (a)-[:ADJACENT_TO]->(c);

// ============================================================================
// LAYER 2 — KNOWLEDGE / GROUNDING  (GraphRAG retrieval target for the agent)
// ============================================================================
MATCH (hw:ModuculeType {id:'headwall-hw204'}), (bed:ModuculeType {id:'bed-icu'})
MERGE (pMed:Port {kind:'MedGas'})
MERGE (hw)-[:PROVIDES]->(pMed)        // headwall supplies med-gas
MERGE (bed)-[:REQUIRES]->(pMed);      // bed needs med-gas
MERGE (r3:Rule {id:'R3-medgas'})
  SET r3.cite='FGI 2.1-8.4 / NFPA 99', r3.reachMm=2500,
      r3.text='A patient bed must be within 2500mm of a med-gas source.'
MERGE (r1:Rule {id:'R1-boundary'}) SET r1.cite='internal', r1.text='Footprint must lie within the room boundary.'
MERGE (r2:Rule {id:'R2-collision'}) SET r2.cite='internal', r2.text='No physical footprint overlap.'
WITH r3
MATCH (prog:Room {program:'observation room'}) WITH r3, collect(DISTINCT prog.program) AS _
MERGE (program:RoomProgram {id:'observation room'})
MERGE (program)-[:GOVERNED_BY]->(r3)
WITH r3
MATCH (pMed:Port {kind:'MedGas'}) MERGE (r3)-[:CONSTRAINS]->(pMed);

// ============================================================================
// QUERIES THE GRAPH MAKES NATIVE  (this is WHY a graph DB is the right tool)
// ============================================================================

// (A) BLAST RADIUS — revise a type, get the exact rooms to re-review. One hop.
//     This is the "celebrity write" as a traversal, not a table scan.
//   MATCH (t:ModuculeType {id:'headwall-hw204'})<-[:OF_TYPE]-(:Instance)<-[:CONTAINS]-(r:Room)
//   RETURN DISTINCT r.id, r.name;

// (B) GROUNDING for the agent — retrieve rules + ports for a room program (GraphRAG)
//   MATCH (p:RoomProgram {id:'observation room'})-[:GOVERNED_BY]->(rule:Rule)
//   OPTIONAL MATCH (rule)-[:CONSTRAINS]->(port:Port)
//   RETURN rule.id, rule.cite, rule.text, collect(port.kind) AS ports;

// (C) COMPLIANCE — a room's earned edges (does every bed have med-gas?)
//   MATCH (r:Room {id:'icu-101'})-[:CONTAINS]->(i:Instance)
//   OPTIONAL MATCH (i)-[mg:MED_GAS]->(:Instance)
//   RETURN r.status, count(DISTINCT i) AS instances, count(mg) AS earnedEdges;

// (D) REUSE — which content types does a Room Moducule compose, and where used?
//   MATCH (tpl:RoomTemplate {id:'std-icu-room'})-[:COMPOSED_OF]->(t:ModuculeType)
//   OPTIONAL MATCH (t)<-[:OF_TYPE]-(:Instance)<-[:CONTAINS]-(r:Room)
//   RETURN t.id, t.version, collect(DISTINCT r.id) AS deployedIn;
