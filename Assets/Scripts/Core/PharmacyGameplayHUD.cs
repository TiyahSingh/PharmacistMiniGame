using PharmacySim.Tools;
using PharmacySim.UI;
using UnityEngine;

namespace PharmacySim.Core
{
    /// <summary>
    /// Public button handlers for UI — wired by scene setup or UI builder.
    /// </summary>
    public class PharmacyGameplayHUD : MonoBehaviour
    {
        [SerializeField] private PharmacyWorkflowController workflow;
        [SerializeField] private PharmacyUIManager ui;
        [SerializeField] private PillInspectionTool inspectionTool;
        [SerializeField] private bool magnifierActive;

        public void OnConfirmImprint() => ui?.OnConfirmImprint();
        public void OnSubmitOrder() => ui?.OnSubmitOrder();
        public void OnSealBottle() => workflow?.SealBottle();
        public void OnApplyLabel() => workflow?.ApplyPrintedLabel();
        public void OnScanBarcode() => workflow?.ScanLabelBarcode();
        public void OnDispensePill() => workflow?.DispenseFocusedPill();

        public void OnToggleMagnifier()
        {
            magnifierActive = !magnifierActive;
            inspectionTool?.ActivateMagnifier(magnifierActive);
            inspectionTool?.ToggleScanLight(magnifierActive);
        }

        public void OnDoubleVerify()
        {
            var verification = FindObjectOfType<Verification.VerificationSystem>();
            var order = FindObjectOfType<Prescription.PrescriptionManager>()?.ActiveOrder;
            if (order != null)
                verification?.CompleteDoubleVerification(order, "PHARM-VERIFY-02");
        }
    }
}
