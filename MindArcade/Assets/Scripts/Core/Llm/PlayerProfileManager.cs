using System;
using System.IO;
using UnityEngine;

namespace MindArcade.Core.Llm
{
    /// <summary>
    /// Oyuncunun gelişim profili verilerini yerel diskte (Application.persistentDataPath)
    /// player_profile.json olarak saklar, yükler ve günceller.
    /// </summary>
    public static class PlayerProfileManager
    {
        private static readonly string ProfilePath = Path.Combine(Application.persistentDataPath, "player_profile.json");
        private static PlayerProfile _currentProfile;

        // Aktif Hasta Bilgileri (Firebase Entegrasyonu İçin)
        public static string ActivePatientId { get; set; }
        public static string ActivePatientName { get; set; }

        /// <summary>
        /// Aktif oyuncu profilini yükler veya yeni bir profil oluşturur.
        /// </summary>
        public static PlayerProfile LoadProfile()
        {
            if (_currentProfile != null)
                return _currentProfile;

            if (File.Exists(ProfilePath))
            {
                try
                {
                    string json = File.ReadAllText(ProfilePath);
                    _currentProfile = JsonUtility.FromJson<PlayerProfile>(json);
                    Debug.Log($"[ProfileManager] Oyuncu profili yüklendi: {ProfilePath}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ProfileManager] Profil yüklenirken hata oluştu, yeni profil oluşturuluyor: {e.Message}");
                    _currentProfile = CreateDefaultProfile();
                }
            }
            else
            {
                _currentProfile = CreateDefaultProfile();
            }

            return _currentProfile;
        }

        /// <summary>
        /// Oyuncu profilini diske kaydeder.
        /// </summary>
        public static void SaveProfile(PlayerProfile profile)
        {
            _currentProfile = profile;
            try
            {
                string json = JsonUtility.ToJson(profile, true);
                File.WriteAllText(ProfilePath, json);
                Debug.Log($"[ProfileManager] Profil diske kaydedildi.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ProfileManager] Profil kaydedilemedi: {e.Message}");
            }
        }

        /// <summary>
        /// Tamamlanan oyun sonrasında oyun bazlı istatistikleri günceller ve kaydeder.
        /// </summary>
        public static void UpdateProfile(string gameName, float sessionAccuracy, int sessionScore, int sessionLevel, float sessionTime, int sessionMistakes, bool failed = false)
        {
            var profile = LoadProfile();
            var stats = GetStatsByGameName(profile, gameName);
 
            // Kümülatif doğruluk, bitirme süresi ve hatalar hesaplama
            float totalAccuracySum = stats.averageAccuracy * stats.totalGamesPlayed;
            float totalTimeSum = stats.averageCompletionTime * stats.totalGamesPlayed;
            float totalMistakesSum = stats.averageMistakes * stats.totalGamesPlayed;
 
            stats.totalGamesPlayed++;
            stats.averageAccuracy = (totalAccuracySum + sessionAccuracy) / stats.totalGamesPlayed;
            stats.averageCompletionTime = (totalTimeSum + sessionTime) / stats.totalGamesPlayed;
            stats.averageMistakes = (totalMistakesSum + sessionMistakes) / stats.totalGamesPlayed;
 
            // Son oyun verilerini güncelle
            stats.lastGameAccuracy = sessionAccuracy;
            stats.lastGameScore = sessionScore;
            stats.lastGameTime = sessionTime;
            stats.lastGameFailed = failed;
            stats.lastGameMistakes = sessionMistakes;
 
            // En yüksek skor ve seviye kontrolü
            if (sessionScore > stats.highestScore)
                stats.highestScore = sessionScore;
            
            // highestLevel değerini seans seviyesi ile karşılaştırıp güncelle
            if (sessionLevel > stats.highestLevel)
                stats.highestLevel = sessionLevel;
 
            SaveProfile(profile);

            // Firebase'e Gönder
            if (!string.IsNullOrEmpty(ActivePatientId) && MindArcade.Core.Firebase.FirebaseClient.Instance != null)
            {
                MindArcade.Core.Firebase.FirebaseClient.Instance.SavePatientProfile(ActivePatientId, profile);
                MindArcade.Core.Firebase.FirebaseClient.Instance.SaveSessionData(
                    ActivePatientId,
                    gameName,
                    sessionScore,
                    sessionAccuracy,
                    sessionLevel,
                    sessionTime,
                    sessionMistakes,
                    profile.cognitiveSummary
                );
            }
        }

        public static GameStats GetStatsByGameName(PlayerProfile profile, string gameName)
        {
            if (gameName == "FocusRunner")
                return profile.focusRunnerStats;
            if (gameName == "MemoryMatch")
                return profile.memoryMatchStats;
            if (gameName == "SequenceCubes")
                return profile.sequenceCubesStats;

            // Güvenlik için varsayılan bir tane döner
            return profile.focusRunnerStats;
        }

        /// <summary>
        /// LLM tarafından yazılan genel bilişsel analizi günceller.
        /// </summary>
        public static void UpdateCognitiveSummary(string summary)
        {
            var profile = LoadProfile();
            profile.cognitiveSummary = summary;
            SaveProfile(profile);

            // Firebase'e Güncel Profili Gönder
            if (!string.IsNullOrEmpty(ActivePatientId) && MindArcade.Core.Firebase.FirebaseClient.Instance != null)
            {
                MindArcade.Core.Firebase.FirebaseClient.Instance.SavePatientProfile(ActivePatientId, profile);
            }
        }

        private static PlayerProfile CreateDefaultProfile()
        {
            Debug.Log("[ProfileManager] Yeni varsayılan oyuncu profili oluşturuldu.");
            return new PlayerProfile
            {
                focusRunnerStats = new GameStats(),
                memoryMatchStats = new GameStats(),
                sequenceCubesStats = new GameStats(),
                cognitiveSummary = "Yeni Oyuncu - Henüz genel bilişsel analiz yapılmadı."
            };
        }
    }
}
