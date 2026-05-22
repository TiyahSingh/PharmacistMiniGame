using System;
using System.Collections.Generic;
using PharmacySim.Data;
using PharmacySim.Progression;
using UnityEngine;

namespace PharmacySim.Prescription
{
    /// <summary>
    /// Generates prescription orders scaled to progression level.
    /// </summary>
    public class OrderGenerator : MonoBehaviour
    {
        [SerializeField] private MedicationDatabase database;
        [SerializeField] private PrescriptionManager prescriptionManager;

        private ProgressionManager _progression;
        private readonly System.Random _rng = new();

        private static readonly string[] SamplePatients =
        {
            "Elena Martinez", "James Chen", "Priya Nair", "Robert Okonkwo", "Sofia Lindstrom"
        };

        private static readonly string[][] SampleAllergies =
        {
            Array.Empty<string>(),
            new[] { "Penicillin" },
            new[] { "Sulfa" },
            new[] { "Aspirin" }
        };

        public void Initialize(ProgressionManager progression)
        {
            _progression = progression;
            database?.Initialize();
        }

        public void GenerateInitialQueue()
        {
            int count = Mathf.Clamp(1 + (_progression?.CurrentLevel ?? 0), 1, 4);
            for (int i = 0; i < count; i++)
                GenerateOrder();
        }

        public PrescriptionOrder GenerateOrder()
        {
            var meds = database?.All;
            if (meds == null || meds.Count == 0)
            {
                Debug.LogWarning("[OrderGenerator] No medications in database.");
                return null;
            }

            int level = _progression?.CurrentLevel ?? 0;
            var available = new List<PillData>();
            foreach (var m in meds)
            {
                if (m == null) continue;
                if (level < 2 && m.controlledStatus != ControlledStatus.NonControlled) continue;
                available.Add(m);
            }

            if (available.Count == 0) available.AddRange(meds);

            var med = available[_rng.Next(available.Count)];
            var order = new PrescriptionOrder
            {
                prescriptionId = $"RX-{DateTime.UtcNow:yyyyMMdd}-{_rng.Next(1000, 9999)}",
                patientName = SamplePatients[_rng.Next(SamplePatients.Length)],
                patientAge = _rng.Next(18, 85),
                allergies = SampleAllergies[_rng.Next(SampleAllergies.Length)],
                medicationName = med.medicationName,
                dosage = med.dosage,
                requiredShape = med.shape,
                requiredImprint = med.imprintCode,
                quantity = Mathf.Clamp(10 + level * 5, 10, 60),
                instructions = med.instructions,
                warnings = med.warnings,
                urgency = level > 3 ? (PrescriptionUrgency)_rng.Next(0, 3) : PrescriptionUrgency.Routine,
                isControlled = med.controlledStatus != ControlledStatus.NonControlled,
                requiresDoubleVerification = med.requiresDoubleVerification || med.controlledStatus != ControlledStatus.NonControlled,
                expiryCheckDate = DateTime.Today.AddMonths(_rng.Next(3, 24))
            };

            if (level >= 4 && _rng.NextDouble() < 0.3)
            {
                order.interactionWarnings = new[]
                {
                    "Potential interaction: verify concurrent medications in patient profile."
                };
            }

            prescriptionManager?.EnqueueOrder(order);
            return order;
        }
    }
}
