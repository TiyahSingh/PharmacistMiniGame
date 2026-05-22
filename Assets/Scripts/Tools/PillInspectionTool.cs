using PharmacySim.Data;
using PharmacySim.Events;
using UnityEngine;

namespace PharmacySim.Tools
{
    /// <summary>
    /// Magnifier and scan light for imprint verification (not color).
    /// </summary>
    public class PillInspectionTool : MonoBehaviour
    {
        [SerializeField] private Camera inspectionCamera;
        [SerializeField] private float zoomFOV = 15f;
        [SerializeField] private float normalFOV = 45f;
        [SerializeField] private Light scanLight;
        [SerializeField] private LayerMask pillLayer;
        [SerializeField] private float inspectRange = 2f;

        private PillInstance _focusedPill;
        private bool _active;

        private void Update()
        {
            if (!_active) return;

            var ray = inspectionCamera != null
                ? inspectionCamera.ScreenPointToRay(Input.mousePosition)
                : Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit, inspectRange, pillLayer))
            {
                _focusedPill = hit.collider.GetComponent<PillInstance>();
                if (_focusedPill != null)
                    _focusedPill.MarkIdentified();
            }
        }

        public void ActivateMagnifier(bool on)
        {
            _active = on;
            if (inspectionCamera != null)
                inspectionCamera.fieldOfView = on ? zoomFOV : normalFOV;
        }

        public void ToggleScanLight(bool on)
        {
            if (scanLight != null)
                scanLight.enabled = on;
        }

        public string GetFocusedImprint() => _focusedPill?.Data?.imprintCode ?? string.Empty;

        public void VerifyFocusedAgainst(string requiredImprint)
        {
            if (_focusedPill == null)
            {
                PharmacyEvents.RaiseSafetyWarning("No pill in focus. Position magnifier over tablet.");
                return;
            }

            bool match = _focusedPill.VerifyAgainstImprint(requiredImprint);
            if (match)
                _focusedPill.MarkVerifiedForDispense();
            else
                PharmacyEvents.RaiseSafetyWarning(
                    $"Imprint '{_focusedPill.Data?.imprintCode}' does not match required '{requiredImprint}'.");
        }
    }
}
