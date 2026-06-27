// ModuBridge.jslib — WebGL → host-page bridge for the Unity Moducule lens.
// Place in Assets/Plugins/ of the Unity project. Unity compiles this into the
// WebGL build; UnityBridge.cs calls ModuNotifyIntent via [DllImport("__Internal")].
//
// It posts a structured message to the parent window (the studio page). The host
// listens for { source:"modutecture-unity" } and refreshes its other lenses.
mergeInto(LibraryManager.library, {
  ModuNotifyIntent: function (jsonPtr) {
    try {
      var json = UTF8ToString(jsonPtr);
      var payload = {};
      try { payload = JSON.parse(json); } catch (e) { payload = { raw: json }; }
      if (typeof window !== "undefined" && window.parent) {
        window.parent.postMessage({ source: "modutecture-unity", type: "intent", payload: payload }, "*");
      }
    } catch (e) {
      console.error("[ModuBridge] notify failed", e);
    }
  }
});
