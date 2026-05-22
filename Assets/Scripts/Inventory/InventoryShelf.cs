using System.Collections.Generic;
using PharmacySim.Data;
using UnityEngine;

namespace PharmacySim.Inventory
{
    /// <summary>
    /// Shelf stock tracking with expiry dates for verification gameplay.
    /// </summary>
    [System.Serializable]
    public class StockEntry
    {
        public PillData medication;
        public int quantityOnHand;
        public string expiryIso = "";

        public System.DateTime ExpiryDate =>
            System.DateTime.TryParse(expiryIso, out var d) ? d : System.DateTime.MaxValue;
    }

    public class InventoryShelf : MonoBehaviour
    {
        [SerializeField] private List<StockEntry> stock = new();
        [SerializeField] private Transform[] shelfSlots;

        public IReadOnlyList<StockEntry> Stock => stock;

        public StockEntry FindStock(PillData med)
        {
            return stock.Find(s => s.medication == med);
        }

        public bool TryReserve(PillData med, int amount, out StockEntry entry)
        {
            entry = FindStock(med);
            if (entry == null || entry.quantityOnHand < amount)
                return false;
            entry.quantityOnHand -= amount;
            return true;
        }

        public bool CheckExpiry(PillData med)
        {
            var entry = FindStock(med);
            return entry != null && entry.ExpiryDate >= System.DateTime.Today;
        }
    }
}
