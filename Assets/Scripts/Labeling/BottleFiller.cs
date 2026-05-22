using PharmacySim.Data;
using PharmacySim.Events;
using PharmacySim.Prescription;
using System.Collections.Generic;
using UnityEngine;

namespace PharmacySim.Labeling
{
    /// <summary>
    /// Pour dispensed pills into prescription bottle with physics.
    /// </summary>
    public class BottleFiller : MonoBehaviour
    {
        [SerializeField] private Transform bottleOpening;
        [SerializeField] private float pourForce = 2f;
        [SerializeField] private AudioSource pourAudio;

        private readonly List<PillInstance> _bottleContents = new();

        public int Count => _bottleContents.Count;

        public void PourPill(PillInstance pill)
        {
            if (pill == null || bottleOpening == null) return;

            _bottleContents.Add(pill);
            pill.transform.position = bottleOpening.position + Random.insideUnitSphere * 0.02f;
            var rb = pill.Rigidbody;
            if (rb != null)
            {
                rb.AddForce(Vector3.down * pourForce, ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * 0.5f, ForceMode.Impulse);
            }

            if (pourAudio != null && !pourAudio.isPlaying)
                pourAudio.Play();
        }

        public bool TrySealForOrder(PrescriptionOrder order)
        {
            if (order == null) return false;
            if (_bottleContents.Count != order.quantity)
            {
                PharmacyEvents.RaiseSafetyWarning($"Bottle contains {_bottleContents.Count} units; expected {order.quantity}.");
                return false;
            }

            foreach (var pill in _bottleContents)
            {
                if (!pill.Data.MatchesImprint(order.requiredImprint))
                {
                    PharmacyEvents.RaiseSafetyWarning("Foreign tablet detected in bottle.");
                    return false;
                }
            }

            order.currentStep = WorkflowStep.PrintLabel;
            return true;
        }

        public void ClearBottle()
        {
            _bottleContents.Clear();
        }
    }
}
