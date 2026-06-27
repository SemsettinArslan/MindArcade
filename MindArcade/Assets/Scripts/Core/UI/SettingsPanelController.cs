using UnityEngine;
using UnityEngine.UI;
using MindArcade.Core.Audio;

namespace MindArcade.Core.UI
{
    /// <summary>
    /// Ayarlar paneli arayüz denetleyicisi.
    /// Ses ayarlarını Slider'lar aracılığıyla AudioManager ile senkronize eder.
    /// Kaydırıcılar hareket ettirildiğinde gerçek zamanlı geri bildirim (ses testi) sunar.
    /// </summary>
    public class SettingsPanelController : MonoBehaviour
    {
        [Header("Ses Kaydırıcıları (Sliders)")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;

        [Header("Premium Geri Bildirim Ayarları")]
        [Tooltip("SFX barı kaydırılırken belirli aralıklarla ses testi çalınsın mı?")]
        [SerializeField] private bool playSFXTestOnDrag = true;
        [SerializeField] private float sfxTestCooldown = 0.15f;

        private float _lastSfxTestTime;
        private bool _isInitialized;

        private void OnEnable()
        {
            InitializeSliders();
        }

        private void Start()
        {
            InitializeSliders();
        }

        /// <summary>
        /// Kaydırıcıları AudioManager'daki güncel ses seviyelerine göre senkronize eder.
        /// </summary>
        private void InitializeSliders()
        {
            if (_isInitialized) return;
            if (AudioManager.Instance == null) return;

            // Slider'ların varsayılan listener'larını geçici olarak kaldır (sonsuz döngüyü önlemek için)
            RemoveListeners();

            // AudioManager'dan güncel değerleri çek (0 - 1 arası)
            if (masterVolumeSlider != null)
                masterVolumeSlider.value = AudioManager.Instance.GetMasterVolume();

            if (sfxVolumeSlider != null)
                sfxVolumeSlider.value = AudioManager.Instance.GetSFXVolume();

            if (musicVolumeSlider != null)
                musicVolumeSlider.value = AudioManager.Instance.GetMusicVolume();

            // Dinleyicileri tekrar ekle
            AddListeners();

            _isInitialized = true;
        }

        private void AddListeners()
        {
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);

            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        private void RemoveListeners()
        {
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);

            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);

            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        }

        // ─── Slider Event Handlers ──────────────────────────────

        private void OnMasterVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMasterVolume(value);
            }
        }

        private void OnSFXVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSFXVolume(value);

                // Premium Detay: Kullanıcı sesi ayarlarken gerçek zamanlı olarak şiddetini duyması için kısa bir klik sesi çalarız
                if (playSFXTestOnDrag && Time.unscaledTime - _lastSfxTestTime > sfxTestCooldown)
                {
                    _lastSfxTestTime = Time.unscaledTime;
                    // "button_hover" veya hafif bir "button_click" çalabiliriz
                    AudioManager.Instance.PlaySFX("button_hover");
                }
            }
        }

        private void OnMusicVolumeChanged(float value)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMusicVolume(value);
            }
        }

        private void OnDisable()
        {
            RemoveListeners();
            _isInitialized = false;
        }
    }
}
