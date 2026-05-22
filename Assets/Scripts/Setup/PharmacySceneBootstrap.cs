using PharmacySim.Core;
using PharmacySim.Data;
using PharmacySim.Pills;
using PharmacySim.Tray;
using UnityEngine;

namespace PharmacySim.Setup
{
    /// <summary>
    /// Runtime scene bootstrap when building the pharmacy lab from primitives.
    /// Attach to an empty GameObject in a new scene and assign MedicationDatabase.
    /// </summary>
    public class PharmacySceneBootstrap : MonoBehaviour
    {
        [SerializeField] private MedicationDatabase medicationDatabase;
        [SerializeField] private Material trayMaterial;
        [SerializeField] private Material pillMaterial;
        [SerializeField] private bool buildOnStart = true;
        [SerializeField] private Vector3 trayPosition = new(0f, 0.9f, 0f);

        private void Start()
        {
            if (buildOnStart)
                BuildWorkstation();
        }

        [ContextMenu("Build Workstation")]
        public void BuildWorkstation()
        {
            EnsureManager();
            BuildTray();
            BuildCamera();
            Debug.Log("[PharmacySceneBootstrap] Workstation primitives created. Assign UI and audio in inspector.");
        }

        private void EnsureManager()
        {
            if (FindObjectOfType<PharmacyGameManager>() != null) return;

            var root = new GameObject("PharmacySystems");
            root.AddComponent<PharmacyGameManager>();
            root.AddComponent<PharmacyWorkflowController>();
        }

        private void BuildTray()
        {
            var tray = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tray.name = "SortingTray";
            tray.transform.position = trayPosition;
            tray.transform.localScale = new Vector3(0.5f, 0.04f, 0.4f);

            if (trayMaterial != null)
                tray.GetComponent<Renderer>().material = trayMaterial;

            var rb = tray.AddComponent<Rigidbody>();
            rb.isKinematic = true;

            tray.AddComponent<TrayShaker>();

            var poolGo = new GameObject("PillPool");
            poolGo.transform.SetParent(tray.transform.parent);
            var pool = poolGo.AddComponent<PillPool>();

            var sorter = tray.AddComponent<SortingTrayController>();
            // Serialized refs wired via editor; bootstrap creates structure only
        }

        private void BuildCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("WorkstationCamera");
                cam = camGo.AddComponent<Camera>();
                camGo.tag = "MainCamera";
            }

            cam.transform.position = trayPosition + new Vector3(0.8f, 1.4f, 0.8f);
            cam.transform.rotation = Quaternion.Euler(45f, -45f, 0f);
            cam.orthographic = false;
            cam.fieldOfView = 40f;
        }
    }
}
