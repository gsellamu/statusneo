// TwinModels.cs — the ONLY contract this client knows: the GraphQL twin payload.
// Thin-Unity thesis in types: these mirror the spine's TwinDto exactly
// (version / instances / bindings). The client holds no domain truth — every
// payload is a fresh projection of server state. "Unity becomes the GPU."

using System;
using System.Collections.Generic;

namespace Modutecture.Lens
{
    [Serializable] public class Instance
    {
        public string instanceId;
        public string typeId;
        public float x;
        public float y;
        public int rotation;
    }

    [Serializable] public class Binding
    {
        public string kind;
        public string from;
        public string to;
    }

    [Serializable] public class Twin
    {
        public int version;
        public List<Instance> instances = new();
        public List<Binding> bindings = new();
    }

    [Serializable] public class Violation
    {
        public string rule;
        public string severity;
        public string message;
    }

    [Serializable] public class EventRef { public int seq; }

    [Serializable] public class CommandResult
    {
        public string status;            // ACCEPTED | REJECTED
        public int version;
        public List<Violation> violations = new();
        public EventRef @event;
    }

    // --- GraphQL envelope wrappers for JsonUtility ---
    [Serializable] public class TwinResponse { public TwinData data; }
    [Serializable] public class TwinData { public Twin twin; }

    [Serializable] public class PlaceResponse { public PlaceData data; }
    [Serializable] public class PlaceData { public CommandResult placeModucule; }

    // graphql-transport-ws "next" frame: { type, id, payload:{ data:{ onTwinChanged } } }
    [Serializable] public class WsNextPayload { public WsTwinChanged data; }
    [Serializable] public class WsTwinChanged { public Twin onTwinChanged; }
}
