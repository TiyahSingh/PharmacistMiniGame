using System.Collections.Generic;
using PharmacySim.Data;
using PharmacySim.Pills;
using UnityEngine;

namespace PharmacySim.Tray
{
    /// <summary>
    /// Manages mixed pill population on the sorting tray for an active order.
    /// </summary>
    public class SortingTrayController : MonoBehaviour
    {
        [SerializeField] private PillPool pillPool;
        [SerializeField] private TrayShaker trayShaker;
        [SerializeField] private Transform spawnArea;
        [SerializeField] private Vector3 spawnExtents = new(0.12f, 0.02f, 0.12f);
        [SerializeField] private int decoyPillRatio = 2;

        private readonly List<PillInstance> _trayPills = new();

        public void PopulateTray(PillData target, IEnumerable<PillData> decoys, int targetCount)
        {
            ClearTray();

            for (int i = 0; i < targetCount; i++)
                SpawnPill(target);

            if (decoys != null)
            {
                foreach (var decoy in decoys)
                {
                    for (int i = 0; i < decoyPillRatio; i++)
                        SpawnPill(decoy);
                }
            }

            trayShaker?.ResetTray();
        }

        private void SpawnPill(PillData data)
        {
            var pos = spawnArea.position + new Vector3(
                Random.Range(-spawnExtents.x, spawnExtents.x),
                Random.Range(0f, spawnExtents.y),
                Random.Range(-spawnExtents.z, spawnExtents.z));

            var pill = pillPool.Spawn(data, pos, Random.rotation);
            if (pill != null)
                _trayPills.Add(pill);
        }

        public void ClearTray()
        {
            foreach (var pill in _trayPills)
                pillPool.Despawn(pill);
            _trayPills.Clear();
        }

        public int CountMatchingOnTray(string imprint) =>
            _trayPills.FindAll(p => p.Data != null && p.Data.MatchesImprint(imprint)).Count;
    }
}
