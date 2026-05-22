using System;
using PharmacySim.Data;
using UnityEngine;

namespace PharmacySim.Prescription
{
    [Serializable]
    public class PrescriptionOrder
    {
        public string prescriptionId;
        public string patientName;
        public int patientAge;
        public string[] allergies;
        public string medicationName;
        public string dosage;
        public PillShape requiredShape;
        public string requiredImprint;
        public int quantity;
        public string instructions;
        public string warnings;
        public PrescriptionUrgency urgency;
        public bool isControlled;
        public bool requiresDoubleVerification;
        public DateTime expiryCheckDate;
        public string[] interactionWarnings;
        public WorkflowStep currentStep = WorkflowStep.ReceiveOrder;

        public PillData ResolvedMedication { get; set; }
        public int DispensedCount { get; set; }
        public bool LabelApplied { get; set; }
        public bool BarcodeVerified { get; set; }
        public bool ImprintVerified { get; set; }
        public bool DoubleVerificationComplete { get; set; }

        public string DisplayLine => $"{patientName} — {medicationName} {dosage} ({quantity})";

        /// <summary>
        /// Checks patient allergy list against prescribed medication (e.g. penicillin class).
        /// </summary>
        public bool HasAllergyRisk()
        {
            if (allergies == null || allergies.Length == 0) return false;
            foreach (var allergy in allergies)
            {
                if (string.IsNullOrWhiteSpace(allergy)) continue;
                if (medicationName.IndexOf(allergy, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                if (allergy.Equals("Penicillin", StringComparison.OrdinalIgnoreCase) &&
                    medicationName.IndexOf("cillin", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }
    }
}
