"""
store.py — the spine's long-term memory: an append-only event journal.

The journal is the single source of truth on disk. Current state is never
stored directly; it is *derived* by replaying the journal through domain.fold.
Stop the process, restart it, and the twin reconstitutes itself from history.

Architectural mapping:
  - events table  -> action-potential log (immutable, ordered, replayable)
  - append()      -> commit (the only write)
  - read_model()  -> CQRS read side (a fold over the log)
"""
from __future__ import annotations
import json, sqlite3, time
from domain import fold

ROOM = (0.0, 0.0, 4000.0, 3000.0)  # demo room AABB, mm

class EventStore:
    def __init__(self, path: str = "spine.db"):
        self.db = sqlite3.connect(path, check_same_thread=False)
        self.db.execute("""
            CREATE TABLE IF NOT EXISTS events (
              seq        INTEGER PRIMARY KEY AUTOINCREMENT,
              room_id    TEXT NOT NULL,
              type       TEXT NOT NULL,
              payload    TEXT NOT NULL,
              actor      TEXT NOT NULL,
              command_id TEXT,
              ts         REAL NOT NULL
            )""")
        self.db.commit()

    def append(self, room_id: str, etype: str, payload: dict, actor: str,
               command_id: str | None = None) -> dict:
        ts = time.time()
        cur = self.db.execute(
            "INSERT INTO events(room_id,type,payload,actor,command_id,ts) VALUES(?,?,?,?,?,?)",
            (room_id, etype, json.dumps(payload), actor, command_id, ts))
        self.db.commit()
        return {"seq": cur.lastrowid, "room_id": room_id, "type": etype,
                "payload": payload, "actor": actor, "command_id": command_id, "ts": ts}

    def version(self, room_id: str) -> int:
        # the room's optimistic-concurrency token = its latest event seq (0 if empty)
        row = self.db.execute(
            "SELECT MAX(seq) FROM events WHERE room_id=?", (room_id,)).fetchone()
        return row[0] or 0

    def by_command_id(self, room_id: str, command_id: str) -> dict | None:
        row = self.db.execute(
            "SELECT seq,type,payload,actor,command_id,ts FROM events "
            "WHERE room_id=? AND command_id=? ORDER BY seq LIMIT 1",
            (room_id, command_id)).fetchone()
        if not row:
            return None
        s, t, p, a, cid, ts = row
        return {"seq": s, "type": t, "payload": json.loads(p), "actor": a,
                "command_id": cid, "ts": ts}

    def events(self, room_id: str) -> list[dict]:
        rows = self.db.execute(
            "SELECT seq,type,payload,actor,ts FROM events WHERE room_id=? ORDER BY seq",
            (room_id,)).fetchall()
        return [{"seq": s, "type": t, "payload": json.loads(p), "actor": a, "ts": ts}
                for (s, t, p, a, ts) in rows]

    def read_model(self, room_id: str) -> dict:
        rm = fold(self.events(room_id), ROOM)
        rm["version"] = self.version(room_id)        # optimistic-concurrency token
        return rm
