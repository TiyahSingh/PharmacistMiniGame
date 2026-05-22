using PharmacySim.Events;
using PharmacySim.Save;
using UnityEngine;

namespace PharmacySim.Progression
{
    public class ProgressionManager : MonoBehaviour
    {
        [SerializeField] private int ordersPerLevel = 3;
        [SerializeField] private int maxLevel = 8;

        private SaveData _save;
        private int _ordersThisLevel;

        public int CurrentLevel => _save?.progressionLevel ?? 0;
        public int SuccessfulOrders => _save?.successfulOrders ?? 0;
        public int SafetyIncidents => _save?.safetyIncidents ?? 0;

        public void Initialize(SaveData save)
        {
            _save = save ?? new SaveData();
            _ordersThisLevel = 0;
        }

        public void RegisterSuccessfulOrder()
        {
            _save.successfulOrders++;
            _ordersThisLevel++;

            if (_ordersThisLevel >= ordersPerLevel && _save.progressionLevel < maxLevel)
            {
                _save.progressionLevel++;
                _ordersThisLevel = 0;
                PharmacyEvents.RaiseProgressionLevelChanged(_save.progressionLevel);
            }
        }

        public void RegisterSafetyIncident()
        {
            _save.safetyIncidents++;
        }

        public float GetAccuracyScore()
        {
            int total = _save.successfulOrders + _save.safetyIncidents;
            if (total == 0) return 1f;
            return (float)_save.successfulOrders / total;
        }
    }
}
