using UnityEngine;
using System;

namespace FocusRunner
{
    public class DifficultyManager : MonoBehaviour
    {
        public static DifficultyManager Instance { get; private set; }

        [Header("Global Settings")]
        public int currentLevel = 1;

        [Header("Dynamic Parameters")]
        public float currentSpeed = 10f;
        public float spawnInterval = 2.0f;
        public float ruleChangeInterval = 10f;
        public float ObstacleFrequency = 0.3f;

        private int _consecutiveCorrect = 0;
        private int _consecutiveWrong = 0;

        private float _ruleTimer;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Update()
        {
            if (!FocusRunnerManager.Instance.IsGameRunning) return;

            _ruleTimer += Time.deltaTime;
            if (_ruleTimer >= ruleChangeInterval)
            {
                RuleManager.Instance.SetRandomRule();
                _ruleTimer = 0;
            }
        }

        public void ReportPlayerAction(bool isCorrect)
        {
            if (isCorrect)
            {
                _consecutiveCorrect++;
                _consecutiveWrong = 0;
                
                if (_consecutiveCorrect >= 5)
                {
                    LevelUp();
                }
            }
            else
            {
                _consecutiveWrong++;
                _consecutiveCorrect = 0;
                
                if (_consecutiveWrong >= 2)
                {
                    LevelDown();
                }
            }
        }

        private void LevelUp()
        {
            currentLevel++;
            _consecutiveCorrect = 0;
            
            // Adjust parameters
            currentSpeed = Mathf.Clamp(currentSpeed + 1f, 5f, 25f);
            spawnInterval = Mathf.Clamp(spawnInterval - 0.1f, 0.5f, 3f);
            ObstacleFrequency = Mathf.Clamp(ObstacleFrequency + 0.05f, 0.1f, 0.6f);
            ruleChangeInterval = Mathf.Clamp(ruleChangeInterval - 0.5f, 5f, 15f);

            ApplyToSystems();
        }

        private void LevelDown()
        {
            if (currentLevel > 1) currentLevel--;
            _consecutiveWrong = 0;

            // Adjust parameters
            currentSpeed = Mathf.Clamp(currentSpeed - 1.5f, 5f, 25f);
            spawnInterval = Mathf.Clamp(spawnInterval + 0.2f, 0.5f, 3f);
            ObstacleFrequency = Mathf.Clamp(ObstacleFrequency - 0.1f, 0.1f, 0.6f);

            ApplyToSystems();
        }

        private void ApplyToSystems()
        {
            LevelGenerator gen = FindObjectOfType<LevelGenerator>();
            if (gen != null)
            {
                gen.SetSpeed(currentSpeed);
                gen.SetSpawnInterval(spawnInterval);
            }
        }
    }
}
