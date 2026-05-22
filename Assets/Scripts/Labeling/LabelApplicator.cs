using PharmacySim.Events;
using PharmacySim.Prescription;
using UnityEngine;

namespace PharmacySim.Labeling
{
    /// <summary>
    /// Alignment minigame: rotate bottle, place label, quality affects wrinkle.
    /// </summary>
    public class LabelApplicator : MonoBehaviour
    {
        [SerializeField] private Transform bottleTransform;
        [SerializeField] private Transform labelAnchor;
        [SerializeField] private float alignmentThreshold = 8f;
        [SerializeField] private float maxWrinkleAngle = 25f;

        private GameObject _appliedLabel;
        private float _placementQuality = 1f;

        public void ApplyLabel(GameObject labelObject, PrescriptionOrder order)
        {
            if (labelObject == null || bottleTransform == null) return;

            float angleError = Mathf.Abs(Mathf.DeltaAngle(bottleTransform.eulerAngles.y, labelAnchor.eulerAngles.y));
            _placementQuality = Mathf.Clamp01(1f - angleError / maxWrinkleAngle);

            _appliedLabel = labelObject;
            labelObject.transform.SetParent(labelAnchor, false);
            labelObject.transform.localPosition = Vector3.zero;

            if (_placementQuality < 0.6f)
                SimulateWrinkle(labelObject);

            order.LabelApplied = true;
            order.currentStep = WorkflowStep.FinalVerification;
            PharmacyEvents.RaiseLabelApplied();

            if (_placementQuality < 0.65f)
                PharmacyEvents.RaiseSafetyWarning("Label applied with poor alignment — wrinkling detected. Reposition when possible.");
        }

        private void SimulateWrinkle(GameObject label)
        {
            var rend = label.GetComponent<Renderer>();
            if (rend != null && rend.material != null && rend.material.HasProperty("_Wrinkle"))
                rend.material.SetFloat("_Wrinkle", 1f - _placementQuality);
        }

        public void RotateBottle(float degrees)
        {
            if (bottleTransform != null)
                bottleTransform.Rotate(Vector3.up, degrees, Space.World);
        }
    }
}
