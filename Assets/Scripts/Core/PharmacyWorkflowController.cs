using PharmacySim.Core;
using PharmacySim.Data;
using PharmacySim.Labeling;
using PharmacySim.Prescription;
using PharmacySim.Tools;
using PharmacySim.Tray;
using UnityEngine;

namespace PharmacySim.Core
{
    /// <summary>
    /// Bridges player workstation interactions to the prescription workflow.
    /// </summary>
    public class PharmacyWorkflowController : MonoBehaviour
    {
        [SerializeField] private PharmacyGameManager gameManager;
        [SerializeField] private PrescriptionManager prescriptionManager;
        [SerializeField] private SortingTrayController sortingTray;
        [SerializeField] private PillInspectionTool inspectionTool;
        [SerializeField] private LabelPrinter labelPrinter;
        [SerializeField] private LabelApplicator labelApplicator;
        [SerializeField] private BottleFiller bottleFiller;
        [SerializeField] private BarcodeScanner barcodeScanner;

        private void Start()
        {
            gameManager?.BeginShift();
        }

        public void SelectPrescription(PrescriptionOrder order) =>
            PharmacySim.Events.PharmacyEvents.RaisePrescriptionSelected(order);

        public void VerifyActiveImprint()
        {
            var order = prescriptionManager?.ActiveOrder;
            if (order == null) return;
            inspectionTool?.VerifyFocusedAgainst(order.requiredImprint);
        }

        public void DispenseFocusedPill()
        {
            var order = prescriptionManager?.ActiveOrder;
            if (order == null) return;

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, 3f)) return;

            var pill = hit.collider.GetComponentInParent<PillInstance>();
            if (pill == null) return;

            inspectionTool?.VerifyFocusedAgainst(order.requiredImprint);
            pill.VerifyAgainstImprint(order.requiredImprint);
            prescriptionManager.RegisterDispense(pill);
            bottleFiller?.PourPill(pill);
        }

        public void SealBottle()
        {
            var order = prescriptionManager?.ActiveOrder;
            if (order == null) return;
            if (bottleFiller != null && bottleFiller.TrySealForOrder(order))
                labelPrinter?.PrintLabel(order);
        }

        public void ApplyPrintedLabel()
        {
            var order = prescriptionManager?.ActiveOrder;
            if (order == null || labelPrinter?.PrintedLabelObject == null) return;
            labelApplicator?.ApplyLabel(labelPrinter.PrintedLabelObject, order);
        }

        public void ScanLabelBarcode()
        {
            var order = prescriptionManager?.ActiveOrder;
            if (order == null) return;
            barcodeScanner?.ScanPrescriptionBarcode(order, order.prescriptionId);
        }

        public void SubmitCompletedOrder() => gameManager?.Prescriptions?.TryCompleteOrder();

        public void SetTrayShakeSlider(float v)
        {
            var shaker = FindObjectOfType<TrayShaker>();
            shaker?.SetSliderValue(v);
        }
    }
}
