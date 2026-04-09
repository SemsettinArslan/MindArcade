using UnityEngine;
using System.Collections.Generic;

namespace MindArcade.Core.Audio
{
    /// <summary>
    /// Ses kliplerini tutan ScriptableObject.
    /// Inspector'dan kolayca yönetilebilir.
    /// Unity'de Assets > Create > MindArcade > Sound Library ile oluşturulur.
    /// </summary>
    [CreateAssetMenu(fileName = "SoundLibrary", menuName = "MindArcade/Sound Library")]
    public class SoundLibrary : ScriptableObject
    {
        [System.Serializable]
        public struct SoundEntry
        {
            [Tooltip("Ses klibinin benzersiz anahtarı (örn: 'button_click', 'victory', 'panel_open')")]
            public string key;

            [Tooltip("Çalınacak ses klibi")]
            public AudioClip clip;

            [Tooltip("Ses seviyesi (0-1)")]
            [Range(0f, 1f)]
            public float volume;

            [Tooltip("Pitch rastgeleliği (doğal hissettirmek için)")]
            [Range(0f, 0.3f)]
            public float pitchVariation;
        }

        [Header("UI Sesleri")]
        [SerializeField] private SoundEntry[] uiSounds = new SoundEntry[]
        {
            new SoundEntry { key = "button_click", volume = 0.7f, pitchVariation = 0.05f },
            new SoundEntry { key = "button_hover", volume = 0.3f, pitchVariation = 0.02f },
            new SoundEntry { key = "panel_open", volume = 0.5f, pitchVariation = 0f },
            new SoundEntry { key = "panel_close", volume = 0.4f, pitchVariation = 0f },
        };

        [Header("Oyun Sesleri")]
        [SerializeField] private SoundEntry[] gameSounds = new SoundEntry[]
        {
            new SoundEntry { key = "correct", volume = 0.8f, pitchVariation = 0.1f },
            new SoundEntry { key = "wrong", volume = 0.6f, pitchVariation = 0.05f },
            new SoundEntry { key = "victory", volume = 1f, pitchVariation = 0f },
            new SoundEntry { key = "defeat", volume = 0.7f, pitchVariation = 0f },
            new SoundEntry { key = "level_up", volume = 0.8f, pitchVariation = 0f },
            new SoundEntry { key = "countdown_tick", volume = 0.5f, pitchVariation = 0.1f },
        };

        // Hızlı erişim için Dictionary cache
        private Dictionary<string, SoundEntry> _cache;

        /// <summary>
        /// Key ile SoundEntry döndürür. Bulamazsa null AudioClip döner.
        /// </summary>
        public SoundEntry? GetSound(string key)
        {
            BuildCacheIfNeeded();

            if (_cache.TryGetValue(key, out SoundEntry entry))
                return entry;

            return null;
        }

        private void BuildCacheIfNeeded()
        {
            if (_cache != null) return;

            _cache = new Dictionary<string, SoundEntry>();

            if (uiSounds != null)
            {
                foreach (var s in uiSounds)
                {
                    if (!string.IsNullOrEmpty(s.key) && !_cache.ContainsKey(s.key))
                        _cache[s.key] = s;
                }
            }

            if (gameSounds != null)
            {
                foreach (var s in gameSounds)
                {
                    if (!string.IsNullOrEmpty(s.key) && !_cache.ContainsKey(s.key))
                        _cache[s.key] = s;
                }
            }
        }

        /// <summary>
        /// Cache'i temizler (Inspector'dan değişiklik yapıldığında).
        /// </summary>
        private void OnValidate()
        {
            _cache = null;
        }
    }
}
