using PharmacySim.Events;
using UnityEngine;

namespace PharmacySim.Audio
{
    public class PharmacyAudioManager : MonoBehaviour
    {
        [Header("Clips")]
        [SerializeField] private AudioClip pillRattle;
        [SerializeField] private AudioClip trayShake;
        [SerializeField] private AudioClip bottlePour;
        [SerializeField] private AudioClip printer;
        [SerializeField] private AudioClip scannerBeep;
        [SerializeField] private AudioClip ambientPharmacy;

        [Header("Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource ambientSource;

        [SerializeField] private float rattleCooldown = 0.15f;
        private float _lastRattle;

        private void OnEnable()
        {
            PharmacyEvents.OnTrayShaken += OnTrayShaken;
            PharmacyEvents.OnLabelApplied += OnLabelApplied;
            PharmacyEvents.OnBarcodeScanned += OnBarcodeScanned;
        }

        private void OnDisable()
        {
            PharmacyEvents.OnTrayShaken -= OnTrayShaken;
            PharmacyEvents.OnLabelApplied -= OnLabelApplied;
            PharmacyEvents.OnBarcodeScanned -= OnBarcodeScanned;
        }

        public void PlayAmbient()
        {
            if (ambientSource == null || ambientPharmacy == null) return;
            ambientSource.clip = ambientPharmacy;
            ambientSource.loop = true;
            ambientSource.Play();
        }

        private void OnTrayShaken()
        {
            PlayOneShot(trayShake, 0.6f);
            if (Time.time - _lastRattle > rattleCooldown)
            {
                PlayOneShot(pillRattle, 0.4f);
                _lastRattle = Time.time;
            }
        }

        private void OnLabelApplied() => PlayOneShot(printer, 0.5f);
        private void OnBarcodeScanned() => PlayOneShot(scannerBeep, 1f);

        public void PlayPour() => PlayOneShot(bottlePour, 0.7f);
        public void PlayPrinter() => PlayOneShot(printer, 0.8f);

        private void PlayOneShot(AudioClip clip, float volume)
        {
            if (clip == null || sfxSource == null) return;
            sfxSource.PlayOneShot(clip, volume);
        }
    }
}
