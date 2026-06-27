using System;

namespace MindArcade.Core.Llm
{
    /// <summary>
    /// Oyuncunun yerel olarak kaydedilen geçmiş performans verileri.
    /// </summary>
    /// <summary>
    /// Her bir oyun için ayrı ayrı tutulan performans istatistikleri.
    /// </summary>
    [Serializable]
    public class GameStats
    {
        public int totalGamesPlayed;
        public float averageAccuracy;
        public int highestScore;
        public int highestLevel;
        public float averageCompletionTime; // Ortalama bitirme süresi
        public float averageMistakes;       // Ortalama hata sayısı
        
        // Son Oyun Performans İstatistikleri
        public float lastGameAccuracy;
        public int lastGameScore;
        public float lastGameTime;
        public bool lastGameFailed;
        public int lastGameMistakes;

        public GameStats()
        {
            totalGamesPlayed = 0;
            averageAccuracy = 1.0f;
            highestScore = 0;
            highestLevel = 1;
            averageCompletionTime = 0f;
            averageMistakes = 0f;
            lastGameAccuracy = 1.0f;
            lastGameScore = 0;
            lastGameTime = 0f;
            lastGameFailed = false;
            lastGameMistakes = 0;
        }
    }

    [Serializable]
    public class PlayerProfile
    {
        public GameStats focusRunnerStats = new GameStats();
        public GameStats memoryMatchStats = new GameStats();
        public GameStats sequenceCubesStats = new GameStats();

        public string cognitiveSummary; // LLM'in genel bilişsel analiz özeti
    }

    /// <summary>
    /// LLM'den dönecek yapılandırılmış JSON zorluk parametreleri.
    /// </summary>
    [Serializable]
    public class LlmDifficultyConfig
    {
        public int suggestedLevel;
        public float speedMultiplier;
        public float decoyChance;
        public float timeLimit;
        public int maxMistakesAllowed = 5; // Oyuncunun seviyede yapabileceği maksimum yanlış sayısı
        public string aiComment; // Oyuncuya gösterilecek motive edici / zorlayıcı LLM mesajı
    }
}
