using UnityEngine;
using MindArcade.Core;

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

        private int _correctMatches;
        private int _totalAttempts;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            StartGame();
        }

        public void StartGame()
        {
            Score = 0;
            Accuracy = 1.0f;
            _correctMatches = 0;
            _totalAttempts = 0;

            IsGameRunning = true;

            if (RuleManager.Instance != null)
                RuleManager.Instance.SetRandomRule();

            GameEvents.RaiseGameStarted();
        }

        public void EndGame()
        {
            IsGameRunning = false;
            GameEvents.RaiseGameEnded();
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
