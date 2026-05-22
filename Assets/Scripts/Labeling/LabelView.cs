using TMPro;
using UnityEngine;

namespace PharmacySim.Labeling
{
    public class LabelView : MonoBehaviour
    {
        [SerializeField] private TextMeshPro patientNameText;
        [SerializeField] private TextMeshPro drugText;
        [SerializeField] private TextMeshPro dosageText;
        [SerializeField] private TextMeshPro instructionsText;
        [SerializeField] private TextMeshPro warningsText;
        [SerializeField] private TextMeshPro rxIdText;

        public void ConfigureRuntime(TextMeshPro patient, TextMeshPro drug, TextMeshPro dose,
            TextMeshPro instruct, TextMeshPro warn, TextMeshPro rx)
        {
            patientNameText = patient;
            drugText = drug;
            dosageText = dose;
            instructionsText = instruct;
            warningsText = warn;
            rxIdText = rx;
        }

        public void Bind(LabelData data)
        {
            if (data == null) return;
            if (patientNameText) patientNameText.text = data.patientName;
            if (drugText) drugText.text = data.drugName;
            if (dosageText) dosageText.text = data.dosage;
            if (instructionsText) instructionsText.text = data.instructions;
            if (warningsText) warningsText.text = data.warnings;
            if (rxIdText) rxIdText.text = $"Rx: {data.prescriptionId}";
        }
    }
}
