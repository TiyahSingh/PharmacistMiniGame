using System;

namespace PharmacySim.Save
{
    [Serializable]
    public class SaveData
    {
        public int progressionLevel;
        public int successfulOrders;
        public int safetyIncidents;
        public string lastShiftDate;
    }
}
