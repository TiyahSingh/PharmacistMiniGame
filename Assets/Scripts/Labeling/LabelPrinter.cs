using PharmacySim.Prescription;
using UnityEngine;

namespace PharmacySim.Labeling
{
    /// <summary>
    /// Simulates label printing from prescription data.
    /// </summary>
    public class LabelPrinter : MonoBehaviour
    {
        [SerializeField] private Transform labelSpawnPoint;
        [SerializeField] private GameObject labelPrefab;
        [SerializeField] private float printDelay = 1.2f;

        private LabelData _lastPrinted;
        private GameObject _printedLabel;
        private bool _printing;

        public LabelData LastPrinted => _lastPrinted;
        public GameObject PrintedLabelObject => _printedLabel;

        public void PrintLabel(PrescriptionOrder order)
        {
            if (_printing || order == null) return;
            _printing = true;
            _lastPrinted = LabelData.FromPrescription(order);
            Invoke(nameof(SpawnLabel), printDelay);
        }

        private void SpawnLabel()
        {
            _printing = false;
            if (_printedLabel != null)
                Destroy(_printedLabel);

            if (labelPrefab != null && labelSpawnPoint != null)
            {
                _printedLabel = Instantiate(labelPrefab, labelSpawnPoint.position, labelSpawnPoint.rotation);
                var view = _printedLabel.GetComponent<LabelView>();
                view?.Bind(_lastPrinted);
            }
        }

        public void CancelPrint()
        {
            CancelInvoke(nameof(SpawnLabel));
            _printing = false;
        }
    }
}
