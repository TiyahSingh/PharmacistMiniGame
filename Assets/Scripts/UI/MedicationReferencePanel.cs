using PharmacySim.Data;
using System.Text;
using TMPro;
using UnityEngine;

namespace PharmacySim.UI
{
    /// <summary>
    /// In-game pill reference database — imprint-first identification aid.
    /// </summary>
    public class MedicationReferencePanel : MonoBehaviour
    {
        [SerializeField] private MedicationDatabase database;
        [SerializeField] private TMP_InputField searchField;
        [SerializeField] private TextMeshProUGUI resultsText;

        private void Start() => database?.Initialize();

        public void SearchByImprint()
        {
            if (resultsText == null) return;
            var imprint = searchField != null ? searchField.text : string.Empty;
            var med = database?.GetByImprint(imprint);

            if (med == null)
            {
                resultsText.text = "No medication found for that imprint code.";
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"<b>{med.medicationName}</b> {med.dosage}");
            sb.AppendLine($"Imprint: {med.imprintCode}");
            sb.AppendLine($"Shape: {med.shape}");
            sb.AppendLine($"Size: {med.sizeMillimeters:F1} mm | Weight: {med.weightGrams:F2} g");
            sb.AppendLine($"Controlled: {med.controlledStatus}");
            sb.AppendLine();
            sb.AppendLine("<i>Color is not a reliable identifier. Always verify imprint and dosage.</i>");
            sb.AppendLine();
            sb.AppendLine(med.warnings);
            resultsText.text = sb.ToString();
        }
    }
}
