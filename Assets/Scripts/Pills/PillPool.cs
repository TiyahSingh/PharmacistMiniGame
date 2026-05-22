using System.Collections.Generic;
using PharmacySim.Data;
using PharmacySim.Events;
using UnityEngine;

namespace PharmacySim.Pills
{
    /// <summary>
    /// Object pool for pill rigidbodies to reduce allocation during sorting.
    /// </summary>
    public class PillPool : MonoBehaviour
    {
        [SerializeField] private Transform poolRoot;
        [SerializeField] private int defaultCapacity = 64;

        private readonly Dictionary<PillData, Queue<PillInstance>> _pools = new();
        private readonly List<PillInstance> _active = new();

        public IReadOnlyList<PillInstance> ActivePills => _active;

        public PillInstance Spawn(PillData data, Vector3 position, Quaternion rotation)
        {
            if (data == null || data.pillPrefab == null)
            {
                Debug.LogError("[PillPool] Missing pill prefab on PillData.");
                return null;
            }

            PillInstance pill;
            if (!_pools.TryGetValue(data, out var queue))
            {
                queue = new Queue<PillInstance>();
                _pools[data] = queue;
            }

            if (queue.Count > 0)
            {
                pill = queue.Dequeue();
                pill.gameObject.SetActive(true);
            }
            else
            {
                var go = Instantiate(data.pillPrefab, poolRoot);
                pill = go.GetComponent<PillInstance>();
                if (pill == null)
                    pill = go.AddComponent<PillInstance>();
            }

            pill.transform.SetPositionAndRotation(position, rotation);
            pill.Initialize(data);

            var rb = pill.Rigidbody;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;

            _active.Add(pill);
            PharmacyEvents.RaisePillSpawned(pill);
            return pill;
        }

        public void Despawn(PillInstance pill)
        {
            if (pill == null) return;
            _active.Remove(pill);
            pill.gameObject.SetActive(false);

            if (pill.Data != null)
            {
                if (!_pools.ContainsKey(pill.Data))
                    _pools[pill.Data] = new Queue<PillInstance>();
                _pools[pill.Data].Enqueue(pill);
            }
            else
            {
                Destroy(pill.gameObject);
            }
        }

        public void DespawnAll()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
                Despawn(_active[i]);
        }

        public void Prewarm(PillData data, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var pill = Spawn(data, Vector3.one * 1000f, Quaternion.identity);
                Despawn(pill);
            }
        }
    }
}
