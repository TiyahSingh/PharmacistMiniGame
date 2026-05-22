using System.Collections.Generic;
using PharmacySim.Data;
using PharmacySim.Events;
using PharmacySim.Tray;
using PharmacySim.Verification;
using UnityEngine;

namespace PharmacySim.Prescription
{
    public class PrescriptionManager : MonoBehaviour
    {
        [SerializeField] private MedicationDatabase database;
        [SerializeField] private SortingTrayController sortingTray;
        [SerializeField] private VerificationSystem verificationSystem;

        private readonly List<PrescriptionOrder> _queue = new();
        private PrescriptionOrder _active;

        public PrescriptionOrder ActiveOrder => _active;
        public IReadOnlyList<PrescriptionOrder> Queue => _queue;

        public void Initialize()
        {
            database?.Initialize();
            PharmacyEvents.OnPrescriptionSelected += SelectOrder;
        }

        private void OnDestroy()
        {
            PharmacyEvents.OnPrescriptionSelected -= SelectOrder;
        }

        public void EnqueueOrder(PrescriptionOrder order)
        {
            order.ResolvedMedication = database?.GetByNameAndDosage(order.medicationName, order.dosage);
            if (order.ResolvedMedication != null)
                order.requiredImprint = order.ResolvedMedication.imprintCode;

            _queue.Add(order);
            PharmacyEvents.RaisePrescriptionReceived(order);
        }

        public void SelectOrder(PrescriptionOrder order)
        {
            _active = order;
            order.currentStep = WorkflowStep.ReadPrescription;

            if (order.HasAllergyRisk())
                PharmacyEvents.RaiseAllergyAlert($"Allergy alert: review {order.patientName} profile before dispensing.");

            if (order.interactionWarnings != null)
            {
                foreach (var w in order.interactionWarnings)
                    PharmacyEvents.RaiseDrugInteractionAlert(w);
            }

            if (order.isControlled || order.requiresDoubleVerification)
                PharmacyEvents.RaiseDoubleVerificationRequired();

            PrepareSortingTray(order);
        }

        private void PrepareSortingTray(PrescriptionOrder order)
        {
            if (sortingTray == null || order.ResolvedMedication == null) return;

            var decoys = new List<PillData>();
            foreach (var similar in database.GetSimilarMedications(order.ResolvedMedication))
                decoys.Add(similar);

            sortingTray.PopulateTray(order.ResolvedMedication, decoys, order.quantity + 3);
            order.currentStep = WorkflowStep.SortPills;
        }

        public void AdvanceStep(WorkflowStep step) => _active.currentStep = step;

        public void RegisterDispense(PillInstance pill)
        {
            if (_active == null || pill?.Data == null) return;

            if (!pill.Data.MatchesImprint(_active.requiredImprint))
            {
                PharmacyEvents.RaisePrescriptionFailed(_active, "Imprint mismatch — wrong medication selected.");
                PharmacyEvents.RaiseSafetyWarning("Dispensing blocked: imprint does not match prescription.");
                return;
            }

            if (_active.ImprintVerified && pill.Data.MatchesImprint(_active.requiredImprint))
                pill.MarkVerifiedForDispense();

            if (!pill.IsVerifiedForDispense)
            {
                PharmacyEvents.RaiseSafetyWarning("Verify imprint and dosage before dispensing.");
                return;
            }

            _active.DispensedCount++;
            PharmacyEvents.RaisePillDispensed(pill);
            PharmacyEvents.RaiseDispenseCountChanged(_active.DispensedCount, _active.quantity);

            if (_active.DispensedCount >= _active.quantity)
                _active.currentStep = WorkflowStep.FillBottle;
        }

        public bool TryCompleteOrder()
        {
            if (_active == null) return false;

            var result = verificationSystem?.ValidateFinalSubmission(_active) ?? false;
            if (!result) return false;

            var completed = _active;
            PharmacyEvents.RaisePrescriptionCompleted(completed);
            _queue.Remove(completed);
            _active = null;
            return true;
        }
    }
}
