using System;
using System.Threading.Tasks;
using UnityEngine;

namespace MindArcade.Core.Llm
{
    /// <summary>
    /// LLM Tabanlı Zorluk Sistemi Orkestratörü.
    /// persistent singleton olarak çalışır.
    /// API anahtarını yerel bir dosyadan (Resources/gemini_key) veya Inspector'dan alabilir.
    /// </summary>
    public class LlmDifficultyManager : MonoBehaviour
    {
        private static LlmDifficultyManager _instance;
        public static LlmDifficultyManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<LlmDifficultyManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("LlmDifficultyManager");
                        _instance = go.AddComponent<LlmDifficultyManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [Header("API Settings")]
        [Tooltip("Gemini API Anahtarı. Resources/gemini_key.txt dosyası varsa oradan otomatik yüklenir.")]
        [SerializeField] private string apiKey;

        [Header("Cooldown Settings")]
        [Tooltip("API istekleri arasındaki minimum süre (Saniye cinsinden - Ücretsiz tier limit koruması)")]
        [SerializeField] private float requestCooldown = 10f;

        private float _lastRequestTime = -999f;
        private bool _isRequestInProgress;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                LoadApiKeyFromResources();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Resources altında gemini_key.txt dosyası varsa API anahtarını oradan okur.
        /// Bu yöntem, anahtarın Git/GitHub depolarına sızmasını önlemek için önerilir.
        /// </summary>
        private void LoadApiKeyFromResources()
        {
            if (!string.IsNullOrEmpty(apiKey)) return;

            TextAsset keyAsset = Resources.Load<TextAsset>("gemini_key");
            if (keyAsset != null)
            {
                apiKey = keyAsset.text.Trim();
                Debug.Log("[LlmDifficultyManager] API anahtarı 'Resources/gemini_key.txt' dosyasından yüklendi.");
            }
            else
            {
                Debug.LogWarning("[LlmDifficultyManager] API anahtarı bulunamadı! Lütfen Inspector'dan atayın veya 'Assets/Resources/gemini_key.txt' dosyası oluşturun.");
            }
        }

        /// <summary>
        /// Gemini API'ye istek atarak yeni zorluk parametrelerini alır.
        /// Cooldown veya herhangi bir hata durumunda fallback mekanizmasını tetikler.
        /// </summary>
        public async void RequestDifficultyConfig(
            string gameName,
            int currentSessionLevel,
            float currentSessionAccuracy,
            int currentSessionScore,
            float currentSessionTime,
            Action<LlmDifficultyConfig> onSuccess,
            Action onFailure)
        {
            // Cooldown veya aktif istek kontrolü
            if (_isRequestInProgress || Time.time - _lastRequestTime < requestCooldown)
            {
                Debug.LogWarning("[LlmDifficultyManager] İstek reddedildi: Cooldown aktif veya başka bir istek devam ediyor. Fallback çalıştırılıyor.");
                onFailure?.Invoke();
                return;
            }

            if (string.IsNullOrEmpty(apiKey))
            {
                LoadApiKeyFromResources();
                if (string.IsNullOrEmpty(apiKey))
                {
                    Debug.LogError("[LlmDifficultyManager] API anahtarı atanmamış! Fallback çalıştırılıyor.");
                    onFailure?.Invoke();
                    return;
                }
            }

            _isRequestInProgress = true;
            _lastRequestTime = Time.time;

            // Oyuncu profili geçmişini diskten oku
            PlayerProfile profile = PlayerProfileManager.LoadProfile();

            // Prompt oluştur
            string prompt = BuildPrompt(gameName, currentSessionLevel, currentSessionAccuracy, currentSessionScore, currentSessionTime, profile);

            Debug.Log($"[LlmDifficultyManager] LLM'e istek gönderiliyor ({gameName})...");

            try
            {
                LlmDifficultyConfig config = await GeminiClient.RequestDifficultyConfig(apiKey, prompt);
                _isRequestInProgress = false;

                if (config != null)
                {
                    Debug.Log($"[LlmDifficultyManager] LLM Başarı. Öneri Seviye: {config.suggestedLevel} | Hız Çarpanı: {config.speedMultiplier} | AI Mesajı: {config.aiComment}");
                    
                    // LLM'in sunduğu bilişsel özeti oyuncu profiline kaydet
                    if (!string.IsNullOrEmpty(config.aiComment))
                    {
                        PlayerProfileManager.UpdateCognitiveSummary(config.aiComment);
                    }

                    onSuccess?.Invoke(config);
                }
                else
                {
                    Debug.LogError("[LlmDifficultyManager] LLM yanıtı boş döndü. Fallback tetikleniyor.");
                    onFailure?.Invoke();
                }
            }
            catch (Exception ex)
            {
                _isRequestInProgress = false;
                Debug.LogError($"[LlmDifficultyManager] LLM isteğinde hata oluştu: {ex.Message}. Fallback tetikleniyor.");
                onFailure?.Invoke();
            }
        }

        private LlmDifficultyConfig _preFetchedConfig;
        private string _preFetchedGameName;

        /// <summary>
        /// Sahne geçişi (kararma) esnasında Gemini API'den başlangıç zorluğunu önceden talep eder.
        /// </summary>
        public async System.Threading.Tasks.Task PreFetchDifficultyConfig(string gameName)
        {
            _preFetchedGameName = gameName;
            _preFetchedConfig = null;

            // API anahtarı kontrolü
            if (string.IsNullOrEmpty(apiKey))
            {
                LoadApiKeyFromResources();
                if (string.IsNullOrEmpty(apiKey))
                {
                    Debug.LogError("[LlmDifficultyManager] PreFetch iptal: API anahtarı atanmamış!");
                    return;
                }
            }

            // Oyuncu profili geçmişini diskten oku
            PlayerProfile profile = PlayerProfileManager.LoadProfile();

            // İlk açılışta seans bilgileri varsayılan (Seviye 1, Ortalama Doğruluk 1.0, Skor 0, Süre 0)
            string prompt = BuildPrompt(gameName, 1, 1.0f, 0, 0f, profile);

            Debug.Log($"[LlmDifficultyManager] Sahne geçişinde zorluk önceden alınıyor (Pre-fetch: {gameName})...");

            try
            {
                LlmDifficultyConfig config = await GeminiClient.RequestDifficultyConfig(apiKey, prompt);
                if (config != null)
                {
                    _preFetchedConfig = config;
                    Debug.Log($"[LlmDifficultyManager] Pre-fetch Başarı! Öneri Seviye: {config.suggestedLevel} | Hız: {config.speedMultiplier}");
                    
                    if (!string.IsNullOrEmpty(config.aiComment))
                    {
                        PlayerProfileManager.UpdateCognitiveSummary(config.aiComment);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LlmDifficultyManager] Pre-fetch esnasında hata oluştu: {ex.Message}");
                _preFetchedConfig = null;
            }
        }

        /// <summary>
        /// Önceden alınmış zorluk ayarını döndürür ve belleği temizler.
        /// </summary>
        public LlmDifficultyConfig GetAndClearPreFetchedConfig(string gameName)
        {
            if (_preFetchedGameName == gameName)
            {
                LlmDifficultyConfig config = _preFetchedConfig;
                _preFetchedConfig = null;
                _preFetchedGameName = null;
                return config;
            }
            return null;
        }

        private string BuildPrompt(string gameName, int currentSessionLevel, float currentSessionAccuracy, int currentSessionScore, float currentSessionTime, PlayerProfile profile)
        {
            GameStats stats = PlayerProfileManager.GetStatsByGameName(profile, gameName);

            // Oyun türüne göre yenilgi sebebini özelleştirerek LLM'in halüsinasyon görmesini (olmayan süre sınırından bahsetmesini) önlüyoruz.
            string failureReason = "EVET";
            if (stats.lastGameFailed)
            {
                if (gameName == "SequenceCubes")
                    failureReason = "HAYIR (Tüm canlar/haklar tükendi)";
                else if (gameName == "FocusRunner")
                    failureReason = "HAYIR (Engellere çok çarpıldı veya canlar tükendi)";
                else // MemoryMatch
                    failureReason = "HAYIR (Toplam oyun süresi bitti)";
            }

            return 
                $"Oyun Adı: {gameName}\n" +
                $"-- OYUNCUNUN GEÇMİŞ BİLGİLERİ (Cihaz Profili) --\n" +
                $"Toplam Oynadığı Oyun Sayısı: {stats.totalGamesPlayed}\n" +
                $"Geçmiş Ortalama Doğruluk (Accuracy): {stats.averageAccuracy:P2}\n" +
                $"Geçmiş En Yüksek Skor: {stats.highestScore}\n" +
                $"Geçmiş En Yüksek Seviye: {stats.highestLevel}\n" +
                $"Geçmiş Ortalama Oyunu Tamamlama Süresi: {stats.averageCompletionTime:F1} saniye\n" +
                $"Geçmiş Ortalama Hata (Yanlış) Sayısı: {stats.averageMistakes:F1}\n" +
                $"-- SON OYNANAN OYUN BİLGİLERİ --\n" +
                $"Son Oyun Başarılı mı: {failureReason}\n" +
                $"Son Oyun Doğruluğu: {stats.lastGameAccuracy:P2}\n" +
                $"Son Oyun Skoru: {stats.lastGameScore}\n" +
                $"Son Oyun Süresi: {stats.lastGameTime:F1} saniye\n" +
                $"Son Oyun Hata (Yanlış) Sayısı: {stats.lastGameMistakes}\n" +
                $"-- AKTİF SEANS BİLGİLERİ --\n" +
                $"Şu Anki Seviye: {currentSessionLevel}\n" +
                $"Şu Anki Tur Doğruluğu: {currentSessionAccuracy:P2}\n" +
                $"Şu Anki Skor: {currentSessionScore}\n" +
                $"Şu Anki Oyun/Tur Bitirme Süresi: {currentSessionTime:F1} saniye\n\n" +
                "Lütfen bu oyuncunun performansını değerlendirerek yeni zorluk parametrelerini ayarla. " +
                "1. EĞER OYUNCU SON OYUNU KAYBETTİYSE (Son Oyun Başarılı mı: HAYIR) zorluğu azalt (suggestedLevel ve speedMultiplier değerlerini düşür). Ancak dikkat: SequenceCubes ve FocusRunner oyunları sonsuz (survival) yapıda olduğundan oyuncunun eninde sonunda kaybetmesi normaldir. Bu yüzden bu iki oyunda sadece 'HAYIR' olmasına bakarak seviyeyi direkt 1'e düşürme; ulaştığı skora, can durumuna ve doğruluk oranına göre makul bir seviye belirle. " +
                "2. Eğer oyuncu çok yüksek doğrulukla, çok az hatayla ve hızlıca (geçmiş ortalamasından daha kısa sürede) bitiriyorsa hızı ve zorluğu artır (suggestedLevel'ı ve speedMultiplier'ı yükselt). " +
                "3. aiComment alanında oyuncuya Türkçe olarak doğrudan hitap eden, neden zorlaştırdığını/kolaylaştırdığını açıklayan kısa ve motive edici bir cümle yaz. ÖNEMLİ: Font uyumsuzluğu nedeniyle, yazacağın Türkçe cümlede kesinlikle Türkçe özel karakterler (ş, Ş, ı, İ, ğ, Ğ, ç, Ç, ö, Ö, ü, Ü) kullanma! Bunların yerine İngilizce alternatiflerini (s, S, i, I, g, G, c, C, o, O, u, U) kullan (Örn: 'Harika bir hiz! Son oyunu ortalamandan cok daha kisa surede tamamladin. Simdi seviye 2'de yeteneklerini goster!').";
        }
    }
}
