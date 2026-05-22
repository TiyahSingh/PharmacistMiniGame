using PharmacySim.Events;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace PharmacySim.Tray
{
    /// <summary>
    /// Physical tray shaking via slider, mouse drag, or controller analog input.
    /// </summary>
    public class TrayShaker : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform trayTransform;
        [SerializeField] private Rigidbody trayRigidbody;

        [Header("Shake Settings")]
        [SerializeField] private float shakeSensitivity = 1.2f;
        [SerializeField] private float maxShakeForce = 8f;
        [SerializeField] private float damping = 4f;
        [SerializeField] private Vector2 shakeLimits = new(-0.08f, 0.08f);

        [Header("Input")]
        [SerializeField] private bool useVerticalSlider = true;
        [SerializeField] private bool useMouseDrag = true;
        [SerializeField] private float sliderValue;

        private Vector3 _restLocalPosition;
        private Vector3 _shakeVelocity;
        private bool _dragging;
        private Vector3 _lastMouseWorld;

#if ENABLE_INPUT_SYSTEM
        [SerializeField] private InputActionReference shakeAction;
        [SerializeField] private InputActionAsset pharmacyInputAsset;
        private InputAction _shakeActionDirect;
#endif

        private void Awake()
        {
            if (trayTransform == null)
                trayTransform = transform;
            _restLocalPosition = trayTransform.localPosition;
        }

        private void OnEnable()
        {
#if ENABLE_INPUT_SYSTEM
            shakeAction?.action?.Enable();
#endif
        }

        private void OnDisable()
        {
#if ENABLE_INPUT_SYSTEM
            shakeAction?.action?.Disable();
#endif
        }

        private void Update()
        {
            float inputY = 0f;
            float inputX = 0f;

            if (useVerticalSlider)
            {
                inputY = (sliderValue - 0.5f) * 2f;
            }

#if ENABLE_INPUT_SYSTEM
            if (shakeAction?.action != null)
            {
                var v = shakeAction.action.ReadValue<Vector2>();
                inputX = v.x;
                inputY += v.y;
            }
#endif

            if (useMouseDrag && Input.GetMouseButton(0))
            {
                var ray = Camera.main != null
                    ? Camera.main.ScreenPointToRay(Input.mousePosition)
                    : default;

                if (Physics.Raycast(ray, out var hit, 50f))
                {
                    if (hit.collider != null && hit.collider.transform.IsChildOf(trayTransform))
                    {
                        if (!_dragging)
                        {
                            _dragging = true;
                            _lastMouseWorld = hit.point;
                        }
                        else
                        {
                            var delta = hit.point - _lastMouseWorld;
                            inputX += delta.x * shakeSensitivity * 10f;
                            inputY += delta.z * shakeSensitivity * 10f;
                            _lastMouseWorld = hit.point;
                        }
                    }
                }
            }
            else
            {
                _dragging = false;
            }

            if (Mathf.Abs(inputX) > 0.01f || Mathf.Abs(inputY) > 0.01f)
            {
                _shakeVelocity += new Vector3(inputX, 0f, inputY) * shakeSensitivity * Time.deltaTime;
                PharmacyEvents.RaiseTrayShaken();
            }

            _shakeVelocity = Vector3.Lerp(_shakeVelocity, Vector3.zero, damping * Time.deltaTime);

            var offset = new Vector3(
                Mathf.Clamp(_shakeVelocity.x, shakeLimits.x, shakeLimits.y),
                0f,
                Mathf.Clamp(_shakeVelocity.z, shakeLimits.x, shakeLimits.y));

            trayTransform.localPosition = _restLocalPosition + offset;

            if (trayRigidbody != null && _shakeVelocity.sqrMagnitude > 0.001f)
            {
                var force = new Vector3(_shakeVelocity.x, 0f, _shakeVelocity.z) * maxShakeForce;
                trayRigidbody.AddForce(force, ForceMode.Force);
            }
        }

        public void SetSliderValue(float normalized)
        {
            sliderValue = Mathf.Clamp01(normalized);
        }

        public void ApplyExternalShake(Vector2 input)
        {
            if (input.sqrMagnitude < 0.0001f) return;
            _shakeVelocity += new Vector3(input.x, 0f, input.y) * shakeSensitivity * Time.deltaTime * 2f;
            PharmacyEvents.RaiseTrayShaken();
        }

        public void ResetTray()
        {
            _shakeVelocity = Vector3.zero;
            trayTransform.localPosition = _restLocalPosition;
        }
    }
}
