using PharmacySim.Core;
using PharmacySim.Prescription;
using UnityEngine;

namespace PharmacySim.Pills
{
    /// <summary>
    /// Click-to-inspect and dispense pills on the sorting tray.
    /// </summary>
    public class PillClickInteractor : MonoBehaviour
    {
        [SerializeField] private PharmacyWorkflowController workflow;
        [SerializeField] private KeyCode dispenseKey = KeyCode.E;

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0)) return;

            var ray = Camera.main != null
                ? Camera.main.ScreenPointToRay(Input.mousePosition)
                : default;

            if (!Physics.Raycast(ray, out var hit, 5f)) return;

            var pill = hit.collider.GetComponent<PillInstance>();
            if (pill == null) return;

            if (Input.GetKey(dispenseKey))
                workflow?.DispenseFocusedPill();
            else
            {
                var pm = FindObjectOfType<PrescriptionManager>();
                var order = pm?.ActiveOrder;
                if (order != null)
                    pill.VerifyAgainstImprint(order.requiredImprint);
            }
        }
    }
}
