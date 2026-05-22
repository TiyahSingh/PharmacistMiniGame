using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace PharmacySim.Tray
{
    /// <summary>
    /// Loads PharmacyControls input asset and feeds TrayShaker.
    /// </summary>
    public class TrayInputBridge : MonoBehaviour
    {
        [SerializeField] private TrayShaker trayShaker;
#if ENABLE_INPUT_SYSTEM
        [SerializeField] private InputActionAsset inputAsset;
        private InputAction _shakeAction;
#endif

        private void Awake()
        {
#if ENABLE_INPUT_SYSTEM
            if (inputAsset == null)
                inputAsset = Resources.Load<InputActionAsset>("PharmacyControls");

            if (inputAsset != null)
            {
                _shakeAction = inputAsset.FindActionMap("Workstation")?.FindAction("TrayShake");
                _shakeAction?.Enable();
            }
#endif
        }

        private void Update()
        {
#if ENABLE_INPUT_SYSTEM
            if (_shakeAction == null || trayShaker == null) return;
            var v = _shakeAction.ReadValue<Vector2>();
            trayShaker.ApplyExternalShake(v);
#endif
        }

        private void OnDisable()
        {
#if ENABLE_INPUT_SYSTEM
            _shakeAction?.Disable();
#endif
        }
    }
}
