using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SequenceCubes.Rules;
using MindArcade.Core;
using MindArcade.Core.UI;
using MindArcade.Core.Llm;

namespace SequenceCubes
{
    /// <summary>
    /// Orchestrator: Oyun yaşam döngüsünü yönetir ve alt sistemleri koordine eder.
    /// Artık 350 satır değil, sadece ~150 satır.
    /// 
    /// Sorumluluk dağılımı:
    /// - SequenceGenerator     → Dizi üretimi ve kural seçimi
    /// - SequencePlayer        → Animasyon oynatma sıralaması
    /// - SequenceInputHandler  → Mouse/Touch input
    /// - LivesManager          → Can sistemi
    /// - SequenceUIController  → UI feedback
    /// - ISequenceRule (4 impl) → Kural stratejileri
    /// </summary>
    public class SequenceGameManager : MonoBehaviour, IGameManager
    {
        [Header("Ekranda Bulunan Küpler")]
        [SerializeField] private SequenceCube[] availableCubes;

        [Header("Zorluk / Oyun Ayarları")]
        [SerializeField] private int sequenceLength = 4;
        [Range(0f, 1f)]
        [SerializeField] private float decoyChance = 0.35f;

        [Header("Alt Sistemler (Inspector'dan Ata)")]
        [SerializeField] private SequencePlayer sequencePlayer;
        [SerializeField] private SequenceInputHandler inputHandler;
        [SerializeField] private LivesManager livesManager;
        [SerializeField] private SequenceUIController uiController;

        [Header("Core UI Panelleri (Inspector'dan Ata)")]
        [SerializeField] private GameOverPanel gameOverPanel;

        [Header("Olaylar (Eventler)")]
        public UnityEvent<int> OnLivesChanged;
        public UnityEvent OnGameWin;
        public UnityEvent OnGameOver;

        // --- IGameManager ---
        public bool IsGameRunning { get; private set; }

        private SequenceGenerator _sequenceGenerator;
        private List<SequenceAction> _generatedActions;
        private List<SequenceCube> _expectedClickSequence;
        private ISequenceRule _currentRule;
        private int _currentInputIndex;

        private int _score;
        private int _roundsCompleted;
        private int _currentLevel = 1;

        private void Start()
        {
            if (availableCubes == null || availableCubes.Length == 0) return;

            // Küpleri initialize et
            for (int i = 0; i < availableCubes.Length; i++)
            {
                availableCubes[i].Initialize(i, OnCubeClicked);
            }

            _sequenceGenerator = new SequenceGenerator(availableCubes);

            // Event bağlantıları
            livesManager.OnLivesChanged += (lives) => OnLivesChanged?.Invoke(lives);
            livesManager.OnAllLivesLost += HandleGameOver;

            // GameOverPanel tekrar oyna butonu
            if (gameOverPanel != null)
            {
                gameOverPanel.OnPlayAgainClicked += RestartGame;
                gameOverPanel.OnMainMenuClicked += GoToMainMenu;
            }

            StartGame();
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
            if (livesManager != null)
            {
                livesManager.OnAllLivesLost -= HandleGameOver;
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.OnPlayAgainClicked -= RestartGame;
                gameOverPanel.OnMainMenuClicked -= GoToMainMenu;
            }
        }

        // ─── IGameManager Implementation ────────────────────────

        public void StartGame()
        {
            IsGameRunning = true;
            _score = 0;
            _roundsCompleted = 0;
            _currentLevel = 1;
            livesManager.ResetLives();

            // İlk zorluğu varsayılan parametrelerle yerel olarak ata
            ApplyDifficultySettings(1, 1.0f, 0.2f);

            // GameOver panelini kapat
            if (gameOverPanel != null)
                gameOverPanel.HideGameOver();

            // LLM'den başlangıç zorluğu talep et ve oyunu başlat
            StartCoroutine(FetchDifficultyAndStartRound());
            GameEvents.RaiseGameStarted();
        }

        private void StartNewRound()
        {
            // Yeni dizi üret
            _sequenceGenerator.Generate(
                sequenceLength,
                decoyChance,
                out _generatedActions,
                out _expectedClickSequence,
                out _currentRule
            );

            // Sekansı oynat
            StartCoroutine(ShowSequenceThenAcceptInput());
        }

        public void EndGame()
        {
            IsGameRunning = false;
            inputHandler.IsAcceptingInput = false;
            SetCubesInteractable(false);

            // Profil verilerini kaydet
            float finalAccuracy = CalculateAccuracy();
            bool failed = livesManager.CurrentLives <= 0;
            int mistakes = 3 - livesManager.CurrentLives; // Varsayılan 3 can üzerinden kayıpları hesapla
            PlayerProfileManager.UpdateProfile("SequenceCubes", finalAccuracy, _score, _currentLevel, Time.timeSinceLevelLoad, mistakes, failed: failed);

            GameEvents.RaiseGameEnded();
        }

        // ─── Sequence Flow ──────────────────────────────────────

        private IEnumerator ShowSequenceThenAcceptInput()
        {
            inputHandler.IsAcceptingInput = false;
            SetCubesInteractable(false);

            // Kural açıklamasını göster
            uiController.ShowRuleDescription(_currentRule.GetDescription(), 3f);
            yield return new WaitForSeconds(3.0f);
            uiController.HideInstruction();

            // Sekansı oynat
            bool playbackDone = false;
            sequencePlayer.PlaySequence(_generatedActions, () => playbackDone = true);
            while (!playbackDone) yield return null;

            // Input aç
            _currentInputIndex = 0;

            if (_expectedClickSequence.Count == 0)
            {
                HandleSuccess();
                yield break;
            }

            inputHandler.IsAcceptingInput = true;
            SetCubesInteractable(true);
            uiController.ShowFeedback("<color=green>SIRA SENDE!</color>", 1f);
        }

        // ─── Cube Click Handler ─────────────────────────────────

        private void OnCubeClicked(SequenceCube clickedCube)
        {
            if (!inputHandler.IsAcceptingInput) return;

            SequenceCube expectedCube = _expectedClickSequence[_currentInputIndex];

            if (clickedCube.Id != expectedCube.Id)
            {
                HandleMistake();
                return;
            }

            _currentInputIndex++;

            if (_currentInputIndex >= _expectedClickSequence.Count)
            {
                HandleSuccess();
            }
        }

        // ─── Result Handlers ────────────────────────────────────

        private void HandleMistake()
        {
            inputHandler.IsAcceptingInput = false;
            SetCubesInteractable(false);

            livesManager.LoseLife();
            
            GameEvents.RaiseSFXRequested("wrong");

            if (livesManager.IsAlive)
            {
                uiController.ShowFeedback($"HATA!\nKalan Hak: {livesManager.CurrentLives}\n(Tekrar Izle)", 2f);
                // Aynı diziyi tekrar göster
                StartCoroutine(RetryAfterDelay(2f));
            }
            else
            {
                uiController.ShowFeedback("Hak Bitti!\nSistem Yenileniyor...", 2f);
            }
        }

        private void HandleGameOver()
        {
            EndGame();
            OnGameOver?.Invoke();

            // Core GameOverPanel göster
            if (gameOverPanel != null)
            {
                gameOverPanel.ShowGameOver(
                    title: "Oyun Bitti!",
                    score: _score,
                    subtitle: $"{_roundsCompleted} tur tamamlandi",
                    isVictory: false
                );
            }

            GameEvents.RaiseSFXRequested("defeat");
        }

        private void HandleSuccess()
        {
            inputHandler.IsAcceptingInput = false;
            SetCubesInteractable(false);

            _score += 100;
            _roundsCompleted++;
            OnGameWin?.Invoke();

            uiController.ShowFeedback("<color=green>BRAVO!\nDizi Cozuldu.</color>", 1.5f);
            GameEvents.RaiseSFXRequested("correct");
            GameEvents.RaiseScoreChanged(_score);

            // Sonraki raund öncesi LLM'den parametre güncellemesi iste
            StartCoroutine(NextRoundAfterLlmUpdate(2f));
        }

        private IEnumerator NextRoundAfterLlmUpdate(float delay)
        {
            yield return new WaitForSeconds(delay);
            yield return StartCoroutine(FetchDifficultyAndStartRound());
        }

        private IEnumerator FetchDifficultyAndStartRound()
        {
            if (uiController != null)
            {
                uiController.ShowRuleDescription("Yapay Zeka yeni soru hazirliyor...", 0f);
            }

            bool isRequestDone = false;
            LlmDifficultyConfig receivedConfig = null;

            RequestLlmDifficulty((config) =>
            {
                receivedConfig = config;
                isRequestDone = true;
            });

            while (!isRequestDone)
            {
                yield return null;
            }

            if (receivedConfig != null && !string.IsNullOrEmpty(receivedConfig.aiComment))
            {
                yield return new WaitForSeconds(2.5f);
            }

            StartNewRound();
        }

        private void RestartGame()
        {
            if (SceneNavigator.Instance != null)
            {
                SceneNavigator.Instance.LoadScene(SceneType.SequenceCubes);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }
        }

        // ─── LLM & Difficulty Helpers ───────────────────────────

        private void RequestLlmDifficulty(Action<LlmDifficultyConfig> onComplete)
        {
            if (LlmDifficultyManager.Instance != null)
            {
                // Önce pre-fetch edilmiş zorluğu kontrol et
                LlmDifficultyConfig preFetched = LlmDifficultyManager.Instance.GetAndClearPreFetchedConfig("SequenceCubes");
                if (preFetched != null)
                {
                    Debug.Log("[SequenceGameManager] Pre-fetched LLM zorlugu uygulaniyor.");
                    ApplyLlmConfig(preFetched);
                    onComplete?.Invoke(preFetched);
                    return;
                }

                float accuracy = CalculateAccuracy();
                LlmDifficultyManager.Instance.RequestDifficultyConfig(
                    gameName: "SequenceCubes",
                    currentSessionLevel: _currentLevel,
                    currentSessionAccuracy: accuracy,
                    currentSessionScore: _score,
                    currentSessionTime: Time.timeSinceLevelLoad,
                    onSuccess: (config) =>
                    {
                        ApplyLlmConfig(config);
                        onComplete?.Invoke(config);
                    },
                    onFailure: () =>
                    {
                        Debug.Log("[SequenceGameManager] LLM baglantisi kurulamadi veya Cooldown aktif, yerel zorluk devrede.");
                        ApplyLocalFallbackDifficulty();
                        onComplete?.Invoke(null);
                    }
                );
            }
            else
            {
                ApplyLocalFallbackDifficulty();
                onComplete?.Invoke(null);
            }
        }

        private float CalculateAccuracy()
        {
            int mistakes = 3 - livesManager.CurrentLives;
            if (_roundsCompleted + mistakes == 0) return 1.0f;
            return (float)_roundsCompleted / (_roundsCompleted + mistakes);
        }

        private void ApplyDifficultySettings(int level, float speedMultiplier, float decoyChanceVal)
        {
            _currentLevel = level;
            
            // 1. Aktif Küp Sayısı
            // Seviye 1: 3 küp, Seviye 2: 4 küp, Seviye 3: 5 küp, Seviye 4: 6 küp, Seviye 5+: Hepsi
            int cubesToActivate = Mathf.Clamp(3 + (level - 1), 3, availableCubes.Length);
            for (int i = 0; i < availableCubes.Length; i++)
            {
                if (availableCubes[i] != null)
                {
                    availableCubes[i].gameObject.SetActive(i < cubesToActivate);
                }
            }

            // 2. Rastgele Dizi Uzunluğu Belirleme (Min - Max range)
            int minLen = Mathf.Clamp(level + 2, 3, 7);
            int maxLen = Mathf.Clamp(level + 4, 4, 9);
            sequenceLength = UnityEngine.Random.Range(minLen, maxLen + 1);

            // 3. Küp Animasyon ve Oynatma Hızlarını Güncelleme
            foreach (var cube in availableCubes)
            {
                if (cube != null)
                {
                    cube.SetSpeedMultiplier(speedMultiplier);
                }
            }
            if (sequencePlayer != null)
            {
                sequencePlayer.SetSpeedMultiplier(speedMultiplier);
            }

            // 4. Decoy Şansı
            decoyChance = Mathf.Clamp(decoyChanceVal, 0.1f, 0.6f);
        }

        private void ApplyLocalFallbackDifficulty()
        {
            int calculatedLevel = 1 + (_roundsCompleted / 2);
            float speedMultiplier = 1.0f + (calculatedLevel - 1) * 0.1f;
            float localDecoyChance = 0.2f + (calculatedLevel - 1) * 0.05f;

            ApplyDifficultySettings(calculatedLevel, speedMultiplier, localDecoyChance);
        }

        private void ApplyLlmConfig(LlmDifficultyConfig config)
        {
            ApplyDifficultySettings(config.suggestedLevel, config.speedMultiplier, config.decoyChance);

            if (uiController != null && !string.IsNullOrEmpty(config.aiComment))
            {
                uiController.ShowFeedback(config.aiComment, 2.5f);
            }
        }

        // ─── Helpers ────────────────────────────────────────────

        private void SetCubesInteractable(bool state)
        {
            foreach (var cube in availableCubes)
                cube.SetInteractable(state);
        }

        private IEnumerator RetryAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            StartCoroutine(ShowSequenceThenAcceptInput());
        }
    }
}
