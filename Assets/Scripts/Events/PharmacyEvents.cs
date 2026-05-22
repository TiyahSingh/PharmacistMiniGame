using PharmacySim.Data;
using PharmacySim.Prescription;
using System;
using UnityEngine;

namespace PharmacySim.Events
{
    /// <summary>
    /// Event-driven communication between pharmacy subsystems.
    /// </summary>
    public static class PharmacyEvents
    {
        public static event Action<PrescriptionOrder> OnPrescriptionReceived;
        public static event Action<PrescriptionOrder> OnPrescriptionSelected;
        public static event Action<PrescriptionOrder> OnPrescriptionCompleted;
        public static event Action<PrescriptionOrder, string> OnPrescriptionFailed;

        public static event Action<PillInstance> OnPillSpawned;
        public static event Action<PillInstance> OnPillIdentified;
        public static event Action<PillInstance> OnPillDispensed;

        public static event Action<int, int> OnDispenseCountChanged;
        public static event Action OnTrayShaken;
        public static event Action OnLabelApplied;
        public static event Action OnBarcodeScanned;

        public static event Action<string> OnSafetyWarning;
        public static event Action<string> OnDrugInteractionAlert;
        public static event Action<string> OnAllergyAlert;
        public static event Action OnDoubleVerificationRequired;
        public static event Action OnDoubleVerificationCompleted;

        public static event Action<int> OnProgressionLevelChanged;

        public static void RaisePrescriptionReceived(PrescriptionOrder order) =>
            OnPrescriptionReceived?.Invoke(order);

        public static void RaisePrescriptionSelected(PrescriptionOrder order) =>
            OnPrescriptionSelected?.Invoke(order);

        public static void RaisePrescriptionCompleted(PrescriptionOrder order) =>
            OnPrescriptionCompleted?.Invoke(order);

        public static void RaisePrescriptionFailed(PrescriptionOrder order, string reason) =>
            OnPrescriptionFailed?.Invoke(order, reason);

        public static void RaisePillSpawned(PillInstance pill) => OnPillSpawned?.Invoke(pill);
        public static void RaisePillIdentified(PillInstance pill) => OnPillIdentified?.Invoke(pill);
        public static void RaisePillDispensed(PillInstance pill) => OnPillDispensed?.Invoke(pill);

        public static void RaiseDispenseCountChanged(int current, int required) =>
            OnDispenseCountChanged?.Invoke(current, required);

        public static void RaiseTrayShaken() => OnTrayShaken?.Invoke();
        public static void RaiseLabelApplied() => OnLabelApplied?.Invoke();
        public static void RaiseBarcodeScanned() => OnBarcodeScanned?.Invoke();

        public static void RaiseSafetyWarning(string message) => OnSafetyWarning?.Invoke(message);
        public static void RaiseDrugInteractionAlert(string message) => OnDrugInteractionAlert?.Invoke(message);
        public static void RaiseAllergyAlert(string message) => OnAllergyAlert?.Invoke(message);
        public static void RaiseDoubleVerificationRequired() => OnDoubleVerificationRequired?.Invoke();
        public static void RaiseDoubleVerificationCompleted() => OnDoubleVerificationCompleted?.Invoke();
        public static void RaiseProgressionLevelChanged(int level) => OnProgressionLevelChanged?.Invoke(level);

        public static void ClearAll()
        {
            OnPrescriptionReceived = null;
            OnPrescriptionSelected = null;
            OnPrescriptionCompleted = null;
            OnPrescriptionFailed = null;
            OnPillSpawned = null;
            OnPillIdentified = null;
            OnPillDispensed = null;
            OnDispenseCountChanged = null;
            OnTrayShaken = null;
            OnLabelApplied = null;
            OnBarcodeScanned = null;
            OnSafetyWarning = null;
            OnDrugInteractionAlert = null;
            OnAllergyAlert = null;
            OnDoubleVerificationRequired = null;
            OnDoubleVerificationCompleted = null;
            OnProgressionLevelChanged = null;
        }
    }
}
