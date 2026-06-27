using UnityEngine;
using MindArcade.Core;
using MindArcade.Core.UI;
using MindArcade.Core.Llm;

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
        public int Score; // Seviye içi puan
        public int TotalScore; // Toplam biriken puan
        public float Accuracy;
        
        [Header("Level & Timer Progression")]
        public int TargetScore { get; private set; } = 100;
        public float LevelTimeLimit { get; private set; } = 60f;
        public float LevelTimeRemaining { get; private set; }
        public int Mistakes { get; private set; }
        public int MaxMistakesAllowed { get; private set; } = 5;

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
            if (focusRunnerUI == null)
                focusRunnerUI = FindObjectOfType<FocusRunnerUI>();

            if (focusRunnerUI != null && gameOverPanel == null)
                gameOverPanel = focusRunnerUI.GameOverPanel;

            // GameOverPanel tekrar oyna butonu
            if (gameOverPanel != null)
            {
                gameOverPanel.OnPlayAgainClicked += OnPlayAgain;
                gameOverPanel.OnMainMenuClicked += GoToMainMenu;
            }

            StartGame();
        }

        private void Update()
        {
            if (IsGameRunning)
            {
                LevelTimeRemaining -= Time.deltaTime;
                if (LevelTimeRemaining <= 0)
                {
                    LevelTimeRemaining = 0;
                    EndGame(); // Zaman bitti, oyun sonlanır!
                }
            }
        }

        public void EndGame()
        {
            IsGameRunning = false;
            GameEvents.RaiseGameEnded();

            // Profil verilerini kaydet
            int totalMistakes = _totalAttempts - _correctMatches;
            PlayerProfileManager.UpdateProfile("FocusRunner", Accuracy, TotalScore, DifficultyManager.Instance.currentLevel, Time.timeSinceLevelLoad, totalMistakes, failed: true);

            // GameOverPanel göster
            if (gameOverPanel != null)
            {
                gameOverPanel.ShowGameOver(
                    title: "Oyun Bitti!",
                    score: TotalScore,
                    subtitle: $"Ulaşılan Seviye: {DifficultyManager.Instance.currentLevel} | Doğruluk: {Accuracy:P0}",
                    isVictory: false
                );
            }
        }

        public void StartGame()
        {
            Score = 0;
            TotalScore = 0;
            Accuracy = 1.0f;
            _correctMatches = 0;
            _totalAttempts = 0;
            TargetScore = 100;
            LevelTimeLimit = 60f;
            LevelTimeRemaining = 60f;
            Mistakes = 0;
            MaxMistakesAllowed = 5;

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
                TotalScore += 10;
            }
            else
            {
                Score -= 5;
                if (Score < 0) Score = 0;
                TotalScore -= 5;
                if (TotalScore < 0) TotalScore = 0;

                Mistakes++;
            }

            Accuracy = (float)_correctMatches / _totalAttempts;

            // Hata sayısı limiti aşılırsa direkt oyunu bitir (Game Over)
            if (Mistakes >= MaxMistakesAllowed)
            {
                EndGame();
                return;
            }

            // Aksi halde zorluğu bilgilendir
            DifficultyManager.Instance.ReportPlayerAction(isCorrect);

            // Skor hedefe ulaştıysa seviye atla
            if (Score >= TargetScore)
            {
                DifficultyManager.Instance.AdvanceLevel();
            }

            GameEvents.RaiseScoreChanged(Score);
            GameEvents.RaiseMatchResult(isCorrect);
        }

        public void OnLevelAdvanced(int newLevel, int maxMistakes)
        {
            Score = 0;
            TargetScore = newLevel * 100;
            Mistakes = 0;
            MaxMistakesAllowed = maxMistakes;

            // Yeni süre sınırını hesapla
            float speedMult = DifficultyManager.Instance != null ? DifficultyManager.Instance.currentSpeedMultiplier : 1.0f;
            LevelTimeLimit = Mathf.Clamp(60f / speedMult, 30f, 90f);
            LevelTimeRemaining = LevelTimeLimit;
        }

        public void OnLevelDown(int newLevel, int maxMistakes)
        {
            Score = 0;
            TargetScore = newLevel * 100;
            Mistakes = 0;
            MaxMistakesAllowed = maxMistakes;

            // Süreyi yenile
            float speedMult = DifficultyManager.Instance != null ? DifficultyManager.Instance.currentSpeedMultiplier : 1.0f;
            LevelTimeLimit = Mathf.Clamp(60f / speedMult, 30f, 90f);
            LevelTimeRemaining = LevelTimeLimit;
        }

        private void OnPlayAgain()
        {
            if (SceneNavigator.Instance != null)
            {
                SceneNavigator.Instance.LoadScene(SceneType.FocusRunner);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }
        }

        private void GoToMainMenu()
        {
            if (SceneNavigator.Instance != null)
            {
                SceneNavigator.Instance.LoadScene(SceneType.MainMenu);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
        }

        private void OnDestroy()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.OnPlayAgainClicked -= OnPlayAgain;
                gameOverPanel.OnMainMenuClicked -= GoToMainMenu;
            }
        }
    }
}
