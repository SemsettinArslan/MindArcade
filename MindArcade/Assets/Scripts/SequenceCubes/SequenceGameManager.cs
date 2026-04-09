using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SequenceCubes.Rules;
using MindArcade.Core;

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

            StartGame();
        }

        private void OnDestroy()
        {
            if (livesManager != null)
            {
                livesManager.OnAllLivesLost -= HandleGameOver;
            }
        }

        // ─── IGameManager Implementation ────────────────────────

        public void StartGame()
        {
            IsGameRunning = true;
            livesManager.ResetLives();

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

            GameEvents.RaiseGameStarted();
        }

        public void EndGame()
        {
            IsGameRunning = false;
            inputHandler.IsAcceptingInput = false;
            SetCubesInteractable(false);
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

            if (livesManager.IsAlive)
            {
                uiController.ShowFeedback($"HATA!\nKalan Hak: {livesManager.CurrentLives}\n(Tekrar İzle)", 2f);
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
            OnGameOver?.Invoke();
            StartCoroutine(RestartAfterDelay(2f));
        }

        private void HandleSuccess()
        {
            inputHandler.IsAcceptingInput = false;
            SetCubesInteractable(false);

            OnGameWin?.Invoke();
            uiController.ShowFeedback("<color=green>BRAVO!\nDizi Çözüldü.</color>", 1.5f);

            StartCoroutine(NextRoundAfterDelay(2f));
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

        private IEnumerator RestartAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            StartGame();
        }

        private IEnumerator NextRoundAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            StartGame();
        }
    }
}
