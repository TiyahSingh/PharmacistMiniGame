using PharmacySim.Data;
using UnityEngine;

namespace PharmacySim.Pills
{
    /// <summary>
    /// Tunes rigidbody behavior for realistic pill movement and weight.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PillPhysicsController : MonoBehaviour
    {
        [SerializeField] private PillInstance pillInstance;
        [SerializeField] private float drag = 0.15f;
        [SerializeField] private float angularDrag = 0.2f;
        [SerializeField] private float bounciness = 0.25f;
        [SerializeField] private float friction = 0.4f;
        [SerializeField] private bool allowNaturalRotation = true;

        private Rigidbody _rb;
        private PhysicMaterial _physicMaterial;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            if (pillInstance == null)
                pillInstance = GetComponent<PillInstance>();

            _rb.drag = drag;
            _rb.angularDrag = angularDrag;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            if (!allowNaturalRotation)
                _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            EnsurePhysicMaterial();
        }

        private void EnsurePhysicMaterial()
        {
            var col = GetComponent<Collider>();
            if (col == null) return;

            _physicMaterial = new PhysicMaterial("PillMaterial")
            {
                bounciness = bounciness,
                dynamicFriction = friction,
                staticFriction = friction * 1.2f,
                frictionCombine = PhysicMaterialCombine.Average,
                bounceCombine = PhysicMaterialCombine.Minimum
            };
            col.material = _physicMaterial;
        }

        public void ApplyShapeTuning(PillShape shape)
        {
            switch (shape)
            {
                case PillShape.SmallBead:
                    _rb.mass = 0.05f;
                    bounciness = 0.35f;
                    break;
                case PillShape.LargeCoatedTablet:
                    _rb.mass = 0.9f;
                    bounciness = 0.15f;
                    friction = 0.55f;
                    break;
                case PillShape.Capsule:
                    friction = 0.5f;
                    break;
                default:
                    _rb.mass = pillInstance?.Data?.weightGrams ?? 0.3f;
                    break;
            }

            EnsurePhysicMaterial();
        }

        private void OnEnable()
        {
            if (pillInstance?.Data != null)
                ApplyShapeTuning(pillInstance.Data.shape);
        }
    }
}
