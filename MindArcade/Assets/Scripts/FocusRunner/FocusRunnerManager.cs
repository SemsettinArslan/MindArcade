using UnityEngine;
using System.Collections.Generic;

namespace FocusRunner
{
    public class FocusRunnerManager : MonoBehaviour
    {
        public static FocusRunnerManager Instance { get; private set; }

        public bool IsGameRunning { get; private set; }
        
        [Header("Stats")]
        public int Score;
        public float Accuracy;
        public float AverageReactionTime;

        private int _correctMatches;
        private int _totalAttempts;
        private float _gameStartTime;

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
            _gameStartTime = Time.time;
            
            IsGameRunning = true;

            if (RuleManager.Instance != null)
                RuleManager.Instance.SetRandomRule();
        }

        public void EndGame()
        {
            IsGameRunning = false;
            // UI trigger for Game Over screen
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
        }
    }
}
