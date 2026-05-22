using PharmacySim.Events;
using PharmacySim.Prescription;
using UnityEngine;

namespace PharmacySim.Tools
{
    public class BarcodeScanner : MonoBehaviour
    {
        [SerializeField] private AudioSource scanBeep;
        [SerializeField] private float scanCooldown = 0.5f;

        private float _lastScanTime;

        public bool ScanPrescriptionBarcode(PrescriptionOrder order, string scannedCode)
        {
            if (order == null || Time.time - _lastScanTime < scanCooldown) return false;
            _lastScanTime = Time.time;

            bool match = scannedCode != null &&
                         scannedCode.Trim().Equals(order.prescriptionId, System.StringComparison.OrdinalIgnoreCase);

            if (match)
            {
                order.BarcodeVerified = true;
                PharmacyEvents.RaiseBarcodeScanned();
                if (scanBeep != null) scanBeep.Play();
            }
            else
            {
                PharmacyEvents.RaiseSafetyWarning("Barcode mismatch. Re-scan label.");
            }

            return match;
        }

        public void ScanFromUI(string code)
        {
            var manager = FindObjectOfType<PrescriptionManager>();
            ScanPrescriptionBarcode(manager?.ActiveOrder, code);
        }
    }
}
