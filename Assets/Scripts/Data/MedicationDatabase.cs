using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PharmacySim.Data
{
    /// <summary>
    /// Central registry of all medications available in the simulation.
    /// </summary>
    [CreateAssetMenu(fileName = "MedicationDatabase", menuName = "Pharmacy Sim/Medication Database")]
    public class MedicationDatabase : ScriptableObject
    {
        [SerializeField] private List<PillData> medications = new();

        private Dictionary<string, PillData> _byImprint;

        public IReadOnlyList<PillData> All => medications;

        public void Initialize()
        {
            _byImprint = medications
                .Where(m => m != null && !string.IsNullOrEmpty(m.imprintCode))
                .GroupBy(m => m.imprintCode.ToUpperInvariant())
                .ToDictionary(g => g.Key, g => g.First());
        }

        public PillData GetByImprint(string imprint)
        {
            if (_byImprint == null) Initialize();
            if (string.IsNullOrWhiteSpace(imprint)) return null;
            _byImprint.TryGetValue(imprint.Trim().ToUpperInvariant(), out var data);
            return data;
        }

        public PillData GetByNameAndDosage(string name, string dosage) =>
            medications.FirstOrDefault(m =>
                m != null &&
                m.medicationName.Equals(name, System.StringComparison.OrdinalIgnoreCase) &&
                m.dosage.Equals(dosage, System.StringComparison.OrdinalIgnoreCase));

        public IEnumerable<PillData> GetSimilarMedications(PillData reference)
        {
            if (reference == null) yield break;
            foreach (var med in medications)
            {
                if (med == null || med == reference) continue;
                if (med.shape == reference.shape || med.sizeMillimeters == reference.sizeMillimeters)
                    yield return med;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            var dupes = medications
                .Where(m => m != null)
                .GroupBy(m => m.imprintCode.ToUpperInvariant())
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (dupes.Count > 0)
                Debug.LogWarning($"[MedicationDatabase] Duplicate imprint codes: {string.Join(", ", dupes)}");
        }
#endif
    }
}
