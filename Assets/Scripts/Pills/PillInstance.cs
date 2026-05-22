using PharmacySim.Data;
using PharmacySim.Events;
using UnityEngine;

namespace PharmacySim.Data
{
    /// <summary>
    /// Runtime pill entity with physics and identification state.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PillInstance : MonoBehaviour
    {
        [SerializeField] private PillData data;
        [SerializeField] private Renderer meshRenderer;

        private Rigidbody _rb;
        private bool _identified;
        private bool _verifiedForDispense;

        public PillData Data => data;
        public bool IsIdentified => _identified;
        public bool IsVerifiedForDispense => _verifiedForDispense;
        public Rigidbody Rigidbody => _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        public void Initialize(PillData pillData)
        {
            data = pillData;
            _identified = false;
            _verifiedForDispense = false;

            if (data != null)
            {
                _rb.mass = Mathf.Max(0.01f, data.weightGrams);
                ApplyVisuals();
            }

            gameObject.name = data != null ? $"Pill_{data.imprintCode}" : "Pill";
        }

        private void ApplyVisuals()
        {
            if (meshRenderer == null) return;
            var mat = meshRenderer.material;
            if (mat != null)
            {
                mat.color = data.primaryColor;
                if (mat.HasProperty("_Color2"))
                    mat.SetColor("_Color2", data.secondaryColor);
            }
        }

        public void MarkIdentified()
        {
            if (_identified) return;
            _identified = true;
            PharmacyEvents.RaisePillIdentified(this);
        }

        public void MarkVerifiedForDispense()
        {
            _verifiedForDispense = true;
        }

        public bool VerifyAgainstImprint(string imprint)
        {
            if (data == null) return false;
            bool match = data.MatchesImprint(imprint);
            if (match) MarkIdentified();
            return match;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.relativeVelocity.magnitude > 0.5f && data?.shape == PillShape.Capsule)
            {
                // Capsules can exhibit slight static cling after contact
                if (Random.value < 0.08f)
                    _rb.AddForce(-collision.contacts[0].normal * 0.02f, ForceMode.Impulse);
            }
        }
    }
}
