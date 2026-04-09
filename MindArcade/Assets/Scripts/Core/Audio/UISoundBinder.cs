using UnityEngine;
using UnityEngine.UI;

namespace MindArcade.Core.Audio
{
    /// <summary>
    /// Sahnedeki tüm butonlara otomatik click sesi ekler.
    /// Sahne başladığında tüm butonları bulur ve onClick'lerine ses bağlar.
    /// 
    /// Kullanım: Sahneye boş bir GameObject koyup bu component'i ekle.
    /// </summary>
    public class UISoundBinder : MonoBehaviour
    {
        [Header("Ses Anahtarları")]
        [SerializeField] private string clickSoundKey = "button_click";

        [Header("Ayarlar")]
        [Tooltip("true ise Start'ta sahnedeki tüm butonları otomatik bulur")]
        [SerializeField] private bool autoBindOnStart = true;

        private void Start()
        {
            if (autoBindOnStart)
                BindAllButtons();
        }

        /// <summary>
        /// Sahnedeki tüm butonlara click sesi bağlar.
        /// </summary>
        public void BindAllButtons()
        {
            Button[] allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);

            foreach (Button btn in allButtons)
            {
                // Aynı butona birden fazla bağlanmayı engelle
                string key = clickSoundKey;
                btn.onClick.AddListener(() =>
                {
                    if (AudioManager.Instance != null)
                        AudioManager.Instance.PlaySFX(key);
                });
            }
        }

        /// <summary>
        /// Belirli bir butona ses bağlar.
        /// </summary>
        public void BindButton(Button button, string soundKey = null)
        {
            string key = soundKey ?? clickSoundKey;
            button.onClick.AddListener(() =>
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX(key);
            });
        }
    }
}
