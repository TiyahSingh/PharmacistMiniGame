using PharmacySim.Data;
using PharmacySim.Events;
using PharmacySim.Prescription;
using System;
using UnityEngine;

namespace PharmacySim.Verification
{
    /// <summary>
    /// Multi-step verification: imprint, quantity, controlled substance, expiry, label, barcode.
    /// </summary>
    public class VerificationSystem : MonoBehaviour
    {
        [SerializeField] private MedicationDatabase database;

        public bool VerifyImprint(PrescriptionOrder order, string enteredImprint)
        {
            if (order == null) return false;

            var med = database?.GetByImprint(enteredImprint);
            bool match = med != null &&
                         med.MatchesPrescription(order.medicationName, order.dosage, order.requiredShape);

            order.ImprintVerified = match;
            if (!match)
            {
                PharmacyEvents.RaiseSafetyWarning(
                    "Imprint verification failed. Do not rely on color — confirm imprint code.");
                return false;
            }

            order.currentStep = WorkflowStep.DispenseQuantity;
            return true;
        }

        public bool VerifyQuantity(PrescriptionOrder order, int count)
        {
            if (order == null) return false;
            if (count != order.quantity)
            {
                PharmacyEvents.RaiseSafetyWarning($"Quantity mismatch: expected {order.quantity}, found {count}.");
                return false;
            }
            return true;
        }

        public bool VerifyExpiry(PrescriptionOrder order, DateTime stockExpiry)
        {
            if (stockExpiry < DateTime.Today)
            {
                PharmacyEvents.RaiseSafetyWarning("Expired stock detected. Do not dispense.");
                return false;
            }
            return true;
        }

        public bool CompleteDoubleVerification(PrescriptionOrder order, string pharmacistId)
        {
            if (order == null || !order.requiresDoubleVerification) return true;
            if (string.IsNullOrWhiteSpace(pharmacistId))
            {
                PharmacyEvents.RaiseSafetyWarning("Controlled substance requires second pharmacist verification.");
                return false;
            }

            order.DoubleVerificationComplete = true;
            PharmacyEvents.RaiseDoubleVerificationCompleted();
            return true;
        }

        public bool ValidateFinalSubmission(PrescriptionOrder order)
        {
            if (order == null) return false;

            if (!order.ImprintVerified)
            {
                PharmacyEvents.RaiseSafetyWarning("Final check: imprint not verified.");
                return false;
            }

            if (order.DispensedCount != order.quantity)
            {
                PharmacyEvents.RaiseSafetyWarning("Final check: incorrect dispensed quantity.");
                return false;
            }

            if (!order.LabelApplied)
            {
                PharmacyEvents.RaiseSafetyWarning("Final check: label not applied.");
                return false;
            }

            if (!order.BarcodeVerified)
            {
                PharmacyEvents.RaiseSafetyWarning("Final check: barcode scan required.");
                return false;
            }

            if (order.requiresDoubleVerification && !order.DoubleVerificationComplete)
            {
                PharmacyEvents.RaiseSafetyWarning("Final check: double verification incomplete.");
                return false;
            }

            order.currentStep = WorkflowStep.SubmitOrder;
            return true;
        }
    }
}
