using System.Collections.Generic;
using UnityEngine;

namespace MindArcade.Core.Audio
{
    /// <summary>
    /// Merkezi ses yönetimi (Singleton, DontDestroyOnLoad).
    /// SFX ve Müzik çalma, ses seviyesi ayarı.
    /// AudioSource object pooling ile GC baskısı yok.
    /// 
    /// GameEvents.OnSFXRequested event'ini otomatik dinler.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Ses Kütüphanesi")]
        [SerializeField] private SoundLibrary soundLibrary;

        [Header("Müzik")]
        [SerializeField] private AudioSource musicSource;
        [Tooltip("Bütün sahnelerde çalacak varsayılan arka plan müziği.")]
        [SerializeField] private AudioClip defaultBackgroundMusic;

        [Header("Ayarlar")]
        [Range(0f, 1f)]
        [SerializeField] private float masterVolume = 1f;
        [Range(0f, 1f)]
        [SerializeField] private float sfxVolume = 1f;
        [Range(0f, 1f)]
        [SerializeField] private float musicVolume = 0.5f;

        [Header("Pool Ayarları")]
        [SerializeField] private int initialPoolSize = 5;

        // ─── Pool ───────────────────────────────────────────────
        private readonly Queue<AudioSource> _sfxPool = new Queue<AudioSource>();
        private readonly List<AudioSource> _activeSources = new List<AudioSource>();

        // ─── PlayerPrefs Anahtarları ────────────────────────────
        private const string PREF_MASTER = "MindArcade_MasterVolume";
        private const string PREF_SFX = "MindArcade_SFXVolume";
        private const string PREF_MUSIC = "MindArcade_MusicVolume";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializePool();
                LoadVolumeSettings();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (defaultBackgroundMusic != null)
            {
                PlayMusic(defaultBackgroundMusic);
            }
        }

        private void OnEnable()
        {
            GameEvents.OnSFXRequested += PlaySFX;
        }

        private void OnDisable()
        {
            GameEvents.OnSFXRequested -= PlaySFX;
        }

        private void Update()
        {
            // Biten AudioSource'ları pool'a geri ver
            for (int i = _activeSources.Count - 1; i >= 0; i--)
            {
                if (!_activeSources[i].isPlaying)
                {
                    ReturnToPool(_activeSources[i]);
                    _activeSources.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Key ile SFX çalar.
        /// SoundLibrary'den klibi bulur. Klip bulunamazsa prosedürel ses üretir.
        /// </summary>
        public void PlaySFX(string key)
        {
            // 1. SoundLibrary'den bul
            if (soundLibrary != null)
            {
                var soundEntry = soundLibrary.GetSound(key);
                if (soundEntry != null && soundEntry.Value.clip != null)
                {
                    var entry = soundEntry.Value;
                    AudioSource source = GetFromPool();

                    source.clip = entry.clip;
                    source.volume = entry.volume * sfxVolume * masterVolume;
                    source.pitch = 1f + Random.Range(-entry.pitchVariation, entry.pitchVariation);
                    source.loop = false;
                    source.Play();

                    _activeSources.Add(source);
                    return;
                }
            }

            // 2. Fallback: Prosedürel ses üret (SoundLibrary'de klip yoksa)
            PlayProceduralSFX(key);
        }

        /// <summary>
        /// Ses dosyası olmadan basit prosedürel sesler üretir.
        /// SoundLibrary'ye klip eklenince bunlar devre dışı kalır.
        /// </summary>
        private void PlayProceduralSFX(string key)
        {
            float frequency;
            float duration;
            float volume = 0.3f * sfxVolume * masterVolume;

            switch (key)
            {
                case "button_click":
                    frequency = 800f; duration = 0.05f; volume *= 0.5f;
                    break;
                case "button_hover":
                    frequency = 600f; duration = 0.03f; volume *= 0.2f;
                    break;
                case "correct":
                    frequency = 880f; duration = 0.15f;
                    break;
                case "wrong":
                    frequency = 220f; duration = 0.2f;
                    break;
                case "panel_open":
                    frequency = 500f; duration = 0.08f; volume *= 0.4f;
                    break;
                case "panel_close":
                    frequency = 400f; duration = 0.06f; volume *= 0.3f;
                    break;
                case "victory":
                    frequency = 1200f; duration = 0.3f;
                    break;
                case "defeat":
                    frequency = 180f; duration = 0.4f;
                    break;
                case "level_up":
                    frequency = 1000f; duration = 0.12f;
                    break;
                default:
                    frequency = 440f; duration = 0.08f; volume *= 0.3f;
                    break;
            }

            if (volume < 0.01f) return;

            AudioClip clip = CreateToneClip(frequency, duration);
            AudioSource source = GetFromPool();
            source.clip = clip;
            source.volume = volume;
            source.pitch = 1f + Random.Range(-0.05f, 0.05f);
            source.loop = false;
            source.Play();
            _activeSources.Add(source);
        }

        /// <summary>
        /// Belirtilen frekans ve sürede sinüs dalga AudioClip oluşturur.
        /// </summary>
        private AudioClip CreateToneClip(float frequency, float duration)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - (t / duration); // Fade out
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope;
            }

            AudioClip clip = AudioClip.Create("ProceduralSFX", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// Doğrudan AudioClip ile SFX çalar.
        /// </summary>
        public void PlaySFX(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;

            AudioSource source = GetFromPool();
            source.clip = clip;
            source.volume = volume * sfxVolume * masterVolume;
            source.pitch = 1f;
            source.loop = false;
            source.Play();

            _activeSources.Add(source);
        }

        // ─── Public API — Müzik ─────────────────────────────────

        /// <summary>
        /// Arka plan müziği çalar (loop).
        /// </summary>
        public void PlayMusic(AudioClip clip)
        {
            if (musicSource == null || clip == null) return;

            musicSource.clip = clip;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.loop = true;
            musicSource.Play();
        }

        /// <summary>
        /// Arka plan müziğini durdurur.
        /// </summary>
        public void StopMusic()
        {
            if (musicSource != null)
                musicSource.Stop();
        }

        /// <summary>
        /// Arka plan müziğini duraklatır.
        /// </summary>
        public void PauseMusic()
        {
            if (musicSource != null)
                musicSource.Pause();
        }

        /// <summary>
        /// Arka plan müziğine devam eder.
        /// </summary>
        public void ResumeMusic()
        {
            if (musicSource != null)
                musicSource.UnPause();
        }

        // ─── Ses Seviyesi ───────────────────────────────────────

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateMusicVolume();
            SaveVolumeSettings();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            SaveVolumeSettings();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            UpdateMusicVolume();
            SaveVolumeSettings();
        }

        public float GetMasterVolume() => masterVolume;
        public float GetSFXVolume() => sfxVolume;
        public float GetMusicVolume() => musicVolume;

        // ─── Pool Management ────────────────────────────────────

        private void InitializePool()
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                AudioSource source = CreatePooledSource();
                _sfxPool.Enqueue(source);
            }
        }

        private AudioSource GetFromPool()
        {
            if (_sfxPool.Count > 0)
            {
                AudioSource source = _sfxPool.Dequeue();
                source.gameObject.SetActive(true);
                return source;
            }

            // Pool boşsa yeni oluştur
            return CreatePooledSource();
        }

        private void ReturnToPool(AudioSource source)
        {
            source.Stop();
            source.clip = null;
            source.gameObject.SetActive(false);
            _sfxPool.Enqueue(source);
        }

        private AudioSource CreatePooledSource()
        {
            GameObject obj = new GameObject("PooledSFX");
            obj.transform.SetParent(transform);
            AudioSource source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            obj.SetActive(false);
            return source;
        }

        // ─── Kayıt / Yükleme ───────────────────────────────────

        private void SaveVolumeSettings()
        {
            PlayerPrefs.SetFloat(PREF_MASTER, masterVolume);
            PlayerPrefs.SetFloat(PREF_SFX, sfxVolume);
            PlayerPrefs.SetFloat(PREF_MUSIC, musicVolume);
            PlayerPrefs.Save();
        }

        private void LoadVolumeSettings()
        {
            masterVolume = PlayerPrefs.GetFloat(PREF_MASTER, 1f);
            sfxVolume = PlayerPrefs.GetFloat(PREF_SFX, 1f);
            musicVolume = PlayerPrefs.GetFloat(PREF_MUSIC, 0.5f);
            UpdateMusicVolume();
        }

        private void UpdateMusicVolume()
        {
            if (musicSource != null)
                musicSource.volume = musicVolume * masterVolume;
        }
    }
}
