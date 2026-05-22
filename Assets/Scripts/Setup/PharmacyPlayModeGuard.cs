using UnityEngine;
using UnityEngine.EventSystems;

namespace PharmacySim.Setup
{
    /// <summary>
    /// Ensures EventSystem exists when entering Play Mode.
    /// </summary>
    public class PharmacyPlayModeGuard : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
    }
}
