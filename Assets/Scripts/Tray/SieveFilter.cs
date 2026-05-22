using PharmacySim.Data;
using PharmacySim.Pills;
using UnityEngine;

namespace PharmacySim.Tray
{
    /// <summary>
    /// Tray sieve that allows only matching pill shapes/sizes to pass through.
    /// </summary>
    public class SieveFilter : MonoBehaviour
    {
        [SerializeField] private SieveFilterType filterType;
        [SerializeField] private float maxDiameterMm = 8f;
        [SerializeField] private float minDiameterMm = 4f;
        [SerializeField] private PillShape[] allowedShapes;
        [SerializeField] private Transform catchTray;
        [SerializeField] private LayerMask pillLayer;

        private void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & pillLayer) == 0) return;

            var pill = other.GetComponent<PillInstance>();
            if (pill?.Data == null) return;

            if (PassesFilter(pill.Data))
            {
                if (catchTray != null)
                    pill.transform.SetParent(catchTray, true);
            }
            else
            {
                // Block — nudge pill back
                var rb = pill.Rigidbody;
                if (rb != null)
                    rb.AddForce(Vector3.up * 0.5f + transform.forward * -0.3f, ForceMode.Impulse);
            }
        }

        public bool PassesFilter(PillData data)
        {
            if (data == null) return false;

            if (allowedShapes != null && allowedShapes.Length > 0)
            {
                bool shapeOk = false;
                foreach (var s in allowedShapes)
                {
                    if (s == data.shape) { shapeOk = true; break; }
                }
                if (!shapeOk) return false;
            }

            return filterType switch
            {
                SieveFilterType.CircularSmall => data.sizeMillimeters <= maxDiameterMm,
                SieveFilterType.CircularMedium => data.sizeMillimeters > minDiameterMm && data.sizeMillimeters <= maxDiameterMm,
                SieveFilterType.CircularLarge => data.sizeMillimeters > maxDiameterMm * 0.8f,
                SieveFilterType.CapsuleSlot => data.shape == PillShape.Capsule,
                SieveFilterType.HexagonalSlot => data.shape == PillShape.HexagonalTablet,
                SieveFilterType.GridFine => data.shape == PillShape.SmallBead,
                SieveFilterType.GridCoarse => data.shape != PillShape.SmallBead,
                _ => true
            };
        }
    }
}
