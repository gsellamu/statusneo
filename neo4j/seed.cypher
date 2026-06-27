// seed.cypher — load a real space-planning knowledge graph for the demo.
// Run:  cat neo4j/seed.cypher | docker exec -i jeethhypno-neo4j cypher-shell -u neo4j -p jeeth2025
//
// This is the grounding the agent retrieves from: Moducule types + typed ports, rules
// with real citations, and room programs that reference them. The twin (Postgres) holds
// live instances; this graph holds the design knowledge that grounds the LLM.

// clean slate (demo scope only)
MATCH (n) WHERE n:ModuculeType OR n:Port OR n:Rule OR n:RoomProgram DETACH DELETE n;

// --- Moducule types + ports ---
CREATE (hw:ModuculeType {id:'headwall-hw204', name:'Headwall HW-204', version:'2.3.0',
        role:'med_gas_source', footprintW:1800, footprintD:300})
CREATE (bed:ModuculeType {id:'bed-icu', name:'ICU Bed', version:'1.4.0',
        role:'patient_bed', footprintW:1000, footprintD:2200})
CREATE (sink:ModuculeType {id:'sink-clinical', name:'Clinical Sink', version:'1.0.1',
        role:'hygiene', footprintW:600, footprintD:600})
CREATE (mgOut:Port {name:'mg-out', kind:'med_gas', role:'provides'})
CREATE (mgIn:Port  {name:'mg-in',  kind:'med_gas', role:'requires'})
CREATE (hw)-[:HAS_PORT]->(mgOut)
CREATE (bed)-[:HAS_PORT]->(mgIn)
CREATE (mgIn)-[:SATISFIED_BY]->(mgOut);

// --- rules with real-style citations (what the agent cites) ---
CREATE (r1:Rule {id:'R1-boundary',  text:'All equipment footprints must lie within the room boundary.', ref:'internal'})
CREATE (r2:Rule {id:'R2-clearance', text:'Maintain 600mm egress clearance on both long sides of a patient bed.', ref:'FGI 2.1-3.3'})
CREATE (r3:Rule {id:'R3-medgas',    text:'Each patient bed must be within 2500mm of a med-gas source.', ref:'FGI 2.1-8.4 / NFPA 99'})
CREATE (r4:Rule {id:'R4-orientation', text:'Beds should face the entry wall where practical (advisory).', ref:'design-guidance'});

// --- room programs (the grounding entry point) ---
MATCH (hw:ModuculeType {id:'headwall-hw204'})
MATCH (r1:Rule {id:'R1-boundary'}),(r2:Rule {id:'R2-clearance'}),(r3:Rule {id:'R3-medgas'}),(r4:Rule {id:'R4-orientation'})
CREATE (p1:RoomProgram {id:'observation room', beds:4, medGasReachMm:2500})
CREATE (p2:RoomProgram {id:'exam room', beds:1, medGasReachMm:2500})
CREATE (p1)-[:USES]->(hw)
CREATE (p2)-[:USES]->(hw)
FOREACH (r IN [r1,r2,r3,r4] | CREATE (p1)-[:GOVERNED_BY]->(r))
FOREACH (r IN [r1,r2,r3]    | CREATE (p2)-[:GOVERNED_BY]->(r));

// verify
MATCH (p:RoomProgram)-[:GOVERNED_BY]->(r:Rule)
RETURN p.id AS program, p.medGasReachMm AS reach, collect(r.id) AS rules;
