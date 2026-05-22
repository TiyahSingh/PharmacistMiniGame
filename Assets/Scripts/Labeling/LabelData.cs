using System;

namespace PharmacySim.Labeling
{
    [Serializable]
    public class LabelData
    {
        public string prescriptionId;
        public string patientName;
        public string drugName;
        public string dosage;
        public string instructions;
        public string warnings;
        public string barcode;
        public string ndcFormat;

        public static LabelData FromPrescription(Prescription.PrescriptionOrder order)
        {
            return new LabelData
            {
                prescriptionId = order.prescriptionId,
                patientName = order.patientName,
                drugName = order.medicationName,
                dosage = order.dosage,
                instructions = order.instructions,
                warnings = order.warnings,
                barcode = order.prescriptionId,
                ndcFormat = $"NDC {order.medicationName.ToUpperInvariant().Substring(0, Math.Min(3, order.medicationName.Length))}-0000-01"
            };
        }
    }
}
