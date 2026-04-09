using UnityEngine;
using MindArcade.Core;
using MindArcade.Core.UI;

namespace FocusRunner
{
    /// <summary>
    /// Oyun yaşam döngüsü ve skor yönetimi.
    /// IGameManager interface'i implement eder.
    /// </summary>
    public class FocusRunnerManager : MonoBehaviour, IGameManager
    {
        public static FocusRunnerManager Instance { get; private set; }

        public bool IsGameRunning { get; private set; }

        [Header("Stats")]
        public int Score;
        public float Accuracy;

        [Header("Core UI (Inspector'dan Ata)")]
        [SerializeField] private GameOverPanel gameOverPanel;
        [SerializeField] private FocusRunnerUI focusRunnerUI;

        private int _correctMatches;
        private int _totalAttempts;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            // GameOverPanel tekrar oyna butonu
            if (gameOverPanel != null)
                gameOverPanel.OnPlayAgainClicked += StartGame;

            StartGame();
        }

        public void EndGame()
        {
            IsGameRunning = false;
            GameEvents.RaiseGameEnded();

            // GameOverPanel göster
            if (gameOverPanel != null)
            {
                gameOverPanel.ShowGameOver(
                    title: "Oyun Bitti!",
                    score: Score,
                    subtitle: $"Doğruluk: {Accuracy:P0} | Lvl: {DifficultyManager.Instance.currentLevel}",
                    isVictory: false
                );
            }
        }

        public void StartGame()
        {
            Score = 0;
            Accuracy = 1.0f;
            _correctMatches = 0;
            _totalAttempts = 0;

            IsGameRunning = true;

            // GameOver panelini kapat
            if (gameOverPanel != null)
                gameOverPanel.HideGameOver();

            if (RuleManager.Instance != null)
                RuleManager.Instance.SetRandomRule();

            GameEvents.RaiseGameStarted();
        }

        public void ReportMatchResult(bool isCorrect)
        {
            _totalAttempts++;
            if (isCorrect)
            {
                _correctMatches++;
                Score += 10;
            }
            else
            {
                Score -= 5;
            }

            Accuracy = (float)_correctMatches / _totalAttempts;
            DifficultyManager.Instance.ReportPlayerAction(isCorrect);

            GameEvents.RaiseScoreChanged(Score);
            GameEvents.RaiseMatchResult(isCorrect);
        }
    }
}
