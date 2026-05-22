using PharmacySim.Audio;
using PharmacySim.Prescription;
using PharmacySim.Progression;
using PharmacySim.Save;
using PharmacySim.UI;
using UnityEngine;

namespace PharmacySim.Core
{
    /// <summary>
    /// Central coordinator for pharmacy workflow state and scene services.
    /// </summary>
    public class PharmacyGameManager : MonoBehaviour
    {
        public static PharmacyGameManager Instance { get; private set; }

        [Header("Services")]
        [SerializeField] private PrescriptionManager prescriptionManager;
        [SerializeField] private OrderGenerator orderGenerator;
        [SerializeField] private ProgressionManager progressionManager;
        [SerializeField] private PharmacyUIManager uiManager;
        [SerializeField] private PharmacyAudioManager audioManager;
        [SerializeField] private SaveSystem saveSystem;

        [Header("Camera")]
        [SerializeField] private Camera workstationCamera;
        [SerializeField] private Vector3 isometricEuler = new(45f, 45f, 0f);

        public PrescriptionManager Prescriptions => prescriptionManager;
        public OrderGenerator Orders => orderGenerator;
        public ProgressionManager Progression => progressionManager;
        public PharmacyUIManager UI => uiManager;
        public PharmacyAudioManager Audio => audioManager;
        public SaveSystem Save => saveSystem;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (workstationCamera != null)
                workstationCamera.transform.rotation = Quaternion.Euler(isometricEuler);

            saveSystem?.Load();
            progressionManager?.Initialize(saveSystem?.Data);
            orderGenerator?.Initialize(progressionManager);
            prescriptionManager?.Initialize();
            uiManager?.Initialize(this);
        }

        private void OnApplicationQuit()
        {
            saveSystem?.Save();
        }

        public void BeginShift()
        {
            orderGenerator.GenerateInitialQueue();
            uiManager?.ShowPrescriptionQueue();
            audioManager?.PlayAmbient();

            if (prescriptionManager?.Queue != null && prescriptionManager.Queue.Count > 0)
                Events.PharmacyEvents.RaisePrescriptionSelected(prescriptionManager.Queue[0]);
        }

        public void CompleteOrderSuccessfully()
        {
            progressionManager?.RegisterSuccessfulOrder();
            saveSystem?.Save();
        }

        public void ReportSafetyIncident(string message)
        {
            uiManager?.ShowSafetyWarning(message);
            progressionManager?.RegisterSafetyIncident();
        }
    }
}
