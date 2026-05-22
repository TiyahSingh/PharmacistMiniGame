using PharmacySim.Core;
using PharmacySim.Events;
using PharmacySim.Prescription;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PharmacySim.UI
{
    /// <summary>
    /// Professional pharmacy software-style UI: queue, database, checklist, alerts.
    /// </summary>
    public class PharmacyUIManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject prescriptionQueuePanel;
        [SerializeField] private GameObject identificationPanel;
        [SerializeField] private GameObject verificationChecklistPanel;

        [Header("Queue")]
        [SerializeField] private Transform queueListRoot;
        [SerializeField] private GameObject queueEntryPrefab;

        [Header("Prescription Detail")]
        [SerializeField] private TextMeshProUGUI patientNameText;
        [SerializeField] private TextMeshProUGUI medicationText;
        [SerializeField] private TextMeshProUGUI imprintRequiredText;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private TextMeshProUGUI instructionsText;
        [SerializeField] private TextMeshProUGUI urgencyText;

        [Header("Identification")]
        [SerializeField] private TMP_InputField imprintInput;
        [SerializeField] private TextMeshProUGUI identificationResultText;
        [SerializeField] private Image shapeHintImage;

        [Header("Checklist")]
        [SerializeField] private Toggle imprintVerifiedToggle;
        [SerializeField] private Toggle quantityVerifiedToggle;
        [SerializeField] private Toggle labelAppliedToggle;
        [SerializeField] private Toggle barcodeVerifiedToggle;
        [SerializeField] private Toggle doubleVerificationToggle;

        [Header("Alerts")]
        [SerializeField] private TextMeshProUGUI alertBannerText;
        [SerializeField] private Color warningColor = new(0.9f, 0.55f, 0.1f);
        [SerializeField] private Color errorColor = new(0.85f, 0.2f, 0.2f);

        [Header("Tray")]
        [SerializeField] private Slider trayShakeSlider;

        private PharmacyGameManager _game;
        private readonly List<GameObject> _queueEntries = new();

        public void Initialize(PharmacyGameManager game)
        {
            _game = game;

            PharmacyEvents.OnPrescriptionReceived += RefreshQueue;
            PharmacyEvents.OnPrescriptionSelected += ShowPrescriptionDetail;
            PharmacyEvents.OnSafetyWarning += ShowWarning;
            PharmacyEvents.OnDrugInteractionAlert += ShowWarning;
            PharmacyEvents.OnAllergyAlert += ShowError;
            PharmacyEvents.OnDispenseCountChanged += UpdateDispenseCount;
            PharmacyEvents.OnDoubleVerificationRequired += ShowDoubleVerificationPrompt;

            if (trayShakeSlider != null)
            {
                trayShakeSlider.onValueChanged.AddListener(v =>
                {
                    var shaker = FindObjectOfType<Tray.TrayShaker>();
                    shaker?.SetSliderValue(v);
                });
            }
        }

        private void OnDestroy()
        {
            PharmacyEvents.OnPrescriptionReceived -= RefreshQueue;
            PharmacyEvents.OnPrescriptionSelected -= ShowPrescriptionDetail;
            PharmacyEvents.OnSafetyWarning -= ShowWarning;
            PharmacyEvents.OnDrugInteractionAlert -= ShowWarning;
            PharmacyEvents.OnAllergyAlert -= ShowError;
            PharmacyEvents.OnDispenseCountChanged -= UpdateDispenseCount;
            PharmacyEvents.OnDoubleVerificationRequired -= ShowDoubleVerificationPrompt;
        }

        public void ShowPrescriptionQueue() => prescriptionQueuePanel?.SetActive(true);

        public void ShowSafetyWarning(string message) => ShowWarning(message);

        private void RefreshQueue(PrescriptionOrder order) => RebuildQueue();

        private void RebuildQueue()
        {
            foreach (var e in _queueEntries)
                Destroy(e);
            _queueEntries.Clear();

            if (_game?.Prescriptions?.Queue == null || queueEntryPrefab == null) return;

            foreach (var order in _game.Prescriptions.Queue)
            {
                var entry = Instantiate(queueEntryPrefab, queueListRoot);
                _queueEntries.Add(entry);
                var label = entry.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                    label.text = $"{order.urgency} | {order.DisplayLine}";
                var btn = entry.GetComponent<Button>();
                if (btn != null)
                {
                    var captured = order;
                    btn.onClick.AddListener(() => PharmacyEvents.RaisePrescriptionSelected(captured));
                }
            }
        }

        private void ShowPrescriptionDetail(PrescriptionOrder order)
        {
            if (order == null) return;
            if (patientNameText) patientNameText.text = order.patientName;
            if (medicationText) medicationText.text = $"{order.medicationName} {order.dosage}";
            if (imprintRequiredText) imprintRequiredText.text = $"Required imprint: {order.requiredImprint}";
            if (imprintInput != null) imprintInput.text = string.Empty;
            if (quantityText) quantityText.text = $"Quantity: {order.quantity}";
            if (instructionsText) instructionsText.text = order.instructions;
            if (urgencyText) urgencyText.text = order.urgency.ToString().ToUpperInvariant();

            identificationPanel?.SetActive(true);
            verificationChecklistPanel?.SetActive(true);
            UpdateChecklist(order);
        }

        private void UpdateChecklist(PrescriptionOrder order)
        {
            if (imprintVerifiedToggle) imprintVerifiedToggle.isOn = order.ImprintVerified;
            if (quantityVerifiedToggle) quantityVerifiedToggle.isOn = order.DispensedCount >= order.quantity;
            if (labelAppliedToggle) labelAppliedToggle.isOn = order.LabelApplied;
            if (barcodeVerifiedToggle) barcodeVerifiedToggle.isOn = order.BarcodeVerified;
            if (doubleVerificationToggle) doubleVerificationToggle.isOn = order.DoubleVerificationComplete;
        }

        private void UpdateDispenseCount(int current, int required)
        {
            if (quantityText && _game?.Prescriptions?.ActiveOrder != null)
                quantityText.text = $"Quantity: {current} / {required}";
            UpdateChecklist(_game.Prescriptions.ActiveOrder);
        }

        public void OnConfirmImprint()
        {
            var order = _game?.Prescriptions?.ActiveOrder;
            if (order == null) return;

            var imprint = imprintInput != null ? imprintInput.text : string.Empty;
            var verification = FindObjectOfType<Verification.VerificationSystem>();
            bool ok = verification != null && verification.VerifyImprint(order, imprint);

            if (identificationResultText != null)
            {
                identificationResultText.text = ok
                    ? "Imprint verified. Proceed to dispense."
                    : "No match. Use reference database — do not identify by color alone.";
                identificationResultText.color = ok ? Color.green : errorColor;
            }

            UpdateChecklist(order);
        }

        public void OnSubmitOrder()
        {
            if (_game?.Prescriptions?.TryCompleteOrder() == true)
            {
                ShowWarning("Order verified and submitted.");
                _game.CompleteOrderSuccessfully();
                _game.Orders?.GenerateOrder();
                RebuildQueue();
                if (_game.Prescriptions.Queue.Count > 0)
                    PharmacyEvents.RaisePrescriptionSelected(_game.Prescriptions.Queue[0]);
            }
        }

        private void ShowWarning(string msg)
        {
            if (alertBannerText == null) return;
            alertBannerText.text = msg;
            alertBannerText.color = warningColor;
        }

        private void ShowError(string msg)
        {
            if (alertBannerText == null) return;
            alertBannerText.text = msg;
            alertBannerText.color = errorColor;
        }

        private void ShowDoubleVerificationPrompt()
        {
            ShowWarning("Controlled substance: second pharmacist verification required.");
        }
    }
}
