using UnityEngine;

namespace PharmacySim.Data
{
    /// <summary>
    /// ScriptableObject defining a medication pill archetype.
    /// Identification must use imprint, shape, and dosage — never color alone.
    /// </summary>
    [CreateAssetMenu(fileName = "PillData", menuName = "Pharmacy Sim/Medication Pill Data")]
    public class PillData : ScriptableObject
    {
        [Header("Identification (Primary)")]
        [Tooltip("FDA-style imprint code. Primary identification field.")]
        public string imprintCode = "AMOX500";

        [Tooltip("Generic or brand medication name.")]
        public string medicationName = "Amoxicillin";

        [Tooltip("Strength label, e.g. 500mg.")]
        public string dosage = "500mg";

        [Header("Physical Properties")]
        public PillShape shape = PillShape.Capsule;
        public float sizeMillimeters = 12f;
        public float weightGrams = 0.45f;

        [Header("Visual (Secondary — not for identification)")]
        [Tooltip("Color is supplementary only. Gameplay verifies imprint.")]
        public Color primaryColor = Color.White;
        public Color secondaryColor = Color.gray;

        [Header("Regulatory")]
        public ControlledStatus controlledStatus = ControlledStatus.NonControlled;
        public bool requiresDoubleVerification;

        [Header("Safety")]
        [TextArea(2, 4)]
        public string warnings = "Take with water. Complete full course.";
        [TextArea(2, 4)]
        public string instructions = "Take one capsule every 8 hours with food.";

        [Header("Prefab")]
        public GameObject pillPrefab;

        public string DisplayName => $"{medicationName} {dosage}";

        public bool MatchesImprint(string imprint) =>
            !string.IsNullOrWhiteSpace(imprint) &&
            string.Equals(imprintCode.Trim(), imprint.Trim(), System.StringComparison.OrdinalIgnoreCase);

        public bool MatchesPrescription(string medName, string dose, PillShape requiredShape)
        {
            return medicationName.Equals(medName, System.StringComparison.OrdinalIgnoreCase) &&
                   dosage.Equals(dose, System.StringComparison.OrdinalIgnoreCase) &&
                   shape == requiredShape;
        }
    }
}
