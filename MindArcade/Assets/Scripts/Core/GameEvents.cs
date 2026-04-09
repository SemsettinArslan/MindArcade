using System;

namespace MindArcade.Core
{
    /// <summary>
    /// Merkezi event bus. Singleton yerine static event'ler üzerinden
    /// gevşek bağlı (loosely coupled) iletişim sağlar.
    /// Modüller arası bağımlılığı ortadan kaldırır.
    /// </summary>
    public static class GameEvents
    {
        // --- Oyun Yaşam Döngüsü ---
        public static event Action OnGameStarted;
        public static event Action OnGameEnded;

        // --- Skor ---
        public static event Action<int> OnScoreChanged;
        public static event Action<bool> OnMatchResult; // true = doğru, false = yanlış

        // --- Zorluk ---
        public static event Action<int> OnLevelChanged;
        public static event Action<float> OnDifficultyParametersChanged;

        // --- UI Feedback ---
        public static event Action<string> OnFeedbackMessage;
        public static event Action<bool> OnFlashFeedback; // true = yeşil, false = kırmızı

        // Publish Helpers
        public static void RaiseGameStarted() => OnGameStarted?.Invoke();
        public static void RaiseGameEnded() => OnGameEnded?.Invoke();
        public static void RaiseScoreChanged(int newScore) => OnScoreChanged?.Invoke(newScore);
        public static void RaiseMatchResult(bool isCorrect) => OnMatchResult?.Invoke(isCorrect);
        public static void RaiseLevelChanged(int newLevel) => OnLevelChanged?.Invoke(newLevel);
        public static void RaiseDifficultyChanged(float value) => OnDifficultyParametersChanged?.Invoke(value);
        public static void RaiseFeedbackMessage(string msg) => OnFeedbackMessage?.Invoke(msg);
        public static void RaiseFlashFeedback(bool isCorrect) => OnFlashFeedback?.Invoke(isCorrect);

        /// <summary>
        /// Sahne geçişlerinde event leak olmaması için tüm dinleyicileri temizler.
        /// </summary>
        public static void ClearAll()
        {
            OnGameStarted = null;
            OnGameEnded = null;
            OnScoreChanged = null;
            OnMatchResult = null;
            OnLevelChanged = null;
            OnDifficultyParametersChanged = null;
            OnFeedbackMessage = null;
            OnFlashFeedback = null;
        }
    }
}
