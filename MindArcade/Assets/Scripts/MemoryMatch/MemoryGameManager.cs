using UnityEngine;
using MindArcade.Core;
using MindArcade.Core.Llm;

namespace MemoryMatch
{
    /// <summary>
    /// Orchestrator: Oyun yaşam döngüsünü yönetir ve alt sistemleri koordine eder.
    /// </summary>
    public class MemoryGameManager : MonoBehaviour, IGameManager
    {
        [Header("Oyun Modu")]
        public StroopMatchMode currentMatchMode = StroopMatchMode.TextVsColor;

        [Header("Test / Debug Ayarları")]
        [Tooltip("Eğer işaretlenirse oyun 'desiredMatchMode' ile başlar. İşaretlenmezse rastgele bir mod seçilir.")]
        [SerializeField] private bool forceDesiredMode = false;
        [SerializeField] private StroopMatchMode desiredMatchMode = StroopMatchMode.TextVsText;

        [Header("Bağımlılıklar (Inspector'dan Ata)")]
        [SerializeField] private MemoryColorPalette colorPalette;
        [SerializeField] private BoardManager boardManager;
        [SerializeField] private TurnTimer turnTimer;
        [SerializeField] private MemoryUIController uiController;

        // --- IGameManager ---
        public bool IsGameRunning { get; private set; }

        private DeckGenerator _deckGenerator;
        private MatchValidator _matchValidator;

        private int _totalMatches = 8;
        private int _currentMatches;
        private float _totalGameTime;
        private int _currentLevel = 1;
        private int _totalAttempts = 0;

        // Genel Oyun Zamanlayıcısı Değişkenleri
        private float _sessionTimeRemaining;
        private float _sessionTimeLimit = 60f;

        // Durum Takip Değişkenleri (Bilişsel Analiz Tanıtımı & Kart Animasyon Senkronizasyonu)
        private bool _entranceAnimFinished;
        private bool _introFinished;

        private void Start()
        {
            _deckGenerator = new DeckGenerator(colorPalette);
            _matchValidator = new MatchValidator();

            // Event bağlantıları
            _matchValidator.OnMatchEvaluated += HandleMatchResult;
            turnTimer.OnTimeOut += HandleTimeOut;

            uiController.Initialize(() => 
            {
                if (SceneNavigator.Instance != null)
                {
                    SceneNavigator.Instance.LoadScene(SceneType.MemoryMatch);
                }
                else
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
                }
            });

            StartGame();
        }

        private void OnDestroy()
        {
            if (_matchValidator != null)
                _matchValidator.OnMatchEvaluated -= HandleMatchResult;

            if (turnTimer != null)
                turnTimer.OnTimeOut -= HandleTimeOut;
        }

        private void Update()
        {
            if (IsGameRunning && !uiController.IsGameOverActive)
            {
                _totalGameTime += Time.deltaTime;
                _sessionTimeRemaining -= Time.deltaTime;

                // UI zamanlayıcılarını güncelle
                uiController.UpdateTimersDisplay(turnTimer.IsRunning ? turnTimer.RemainingTime : 0f, _sessionTimeRemaining);

                if (_sessionTimeRemaining <= 0)
                {
                    HandleGameTimeOut();
                }
            }
        }

        // ─── IGameManager Implementation ────────────────────────

        public void StartGame()
        {
            IsGameRunning = false; // Animasyon bitene kadar oyun başlamıyor
            _totalGameTime = 0f;
            _currentMatches = 0;
            _totalAttempts = 0;
            _entranceAnimFinished = false;
            _introFinished = false;

            uiController.HideGameOver();
            uiController.ShowReady();

            if (uiController != null)
            {
                uiController.HideAllGameplayUI();
            }

            boardManager.ClearBoard();
            _matchValidator.ResetTurn();

            // LLM'den zorluk iste
            if (LlmDifficultyManager.Instance != null)
            {
                // Önce pre-fetch edilmiş zorluğu kontrol et
                LlmDifficultyConfig preFetched = LlmDifficultyManager.Instance.GetAndClearPreFetchedConfig("MemoryMatch");
                if (preFetched != null)
                {
                    Debug.Log("[MemoryGameManager] Pre-fetched LLM zorlugu uygulaniyor.");
                    ApplyLlmConfigAndSetup(preFetched);
                }
                else
                {
                    // Geçmiş profil doğruluğuna göre başlangıç zorluğu al
                    float currentAccuracy = _totalAttempts > 0 ? (float)_currentMatches / _totalAttempts : 1.0f;
                    int currentScore = (int)(currentAccuracy * 1000f);

                    LlmDifficultyManager.Instance.RequestDifficultyConfig(
                        gameName: "MemoryMatch",
                        currentSessionLevel: _currentLevel,
                        currentSessionAccuracy: currentAccuracy,
                        currentSessionScore: currentScore,
                        currentSessionTime: _totalGameTime,
                        onSuccess: ApplyLlmConfigAndSetup,
                        onFailure: SetupBoardLocal
                    );
                }
            }
            else
            {
                SetupBoardLocal();
            }
        }

        private void ApplyLlmConfigAndSetup(LlmDifficultyConfig config)
        {
            _currentLevel = config.suggestedLevel;
            
            // Zorluğa göre toplam eşleşme sayısını (kart sayısını) belirle
            // Seviye 1 -> 4 eşleşme (8 kart)
            // Seviye 2 -> 6 eşleşme (12 kart)
            // Seviye 3 -> 8 eşleşme (16 kart)
            // Seviye 4 -> 10 eşleşme (20 kart)
            // Seviye 5+ -> 12 eşleşme (24 kart)
            _totalMatches = Mathf.Clamp(config.suggestedLevel * 2 + 2, 4, 12);

            // Toplam oyun süresi limitini eşleşme sayısına ve LLM hız çarpanına göre belirle
            _sessionTimeLimit = (20f + _totalMatches * 8f) * (2.0f - config.speedMultiplier);
            _sessionTimeLimit = Mathf.Clamp(_sessionTimeLimit, 15f, 120f);
            _sessionTimeRemaining = _sessionTimeLimit;

            // Kart açma süre sınırını (turn limit) LLM hız çarpanına göre belirle
            float customTimeLimit = Mathf.Clamp(5.0f * (2.0f - config.speedMultiplier), 3.0f, 15.0f);
            turnTimer.SetTimeLimit(customTimeLimit);

            // Eşleştirme modunu belirle (Test/Debug öncelikli)
            if (forceDesiredMode)
            {
                currentMatchMode = desiredMatchMode;
            }
            else
            {
                // Rastgele bir mod seç (0 veya 1)
                currentMatchMode = (StroopMatchMode)UnityEngine.Random.Range(0, 2);
            }

            // Arayüz giriş akışını (Bilişsel Analiz ve Oyun Talimatı) başlat
            _introFinished = false;
            if (uiController != null)
            {
                uiController.StartGameplayIntro(config.aiComment, currentMatchMode, OnIntroFinished);
            }
            else
            {
                _introFinished = true;
            }

            SetupBoardWithMode(currentMatchMode, _totalMatches);
        }

        private void SetupBoardLocal()
        {
            _totalMatches = 8;
            _sessionTimeLimit = 60f;
            _sessionTimeRemaining = _sessionTimeLimit;
            turnTimer.SetTimeLimit(5.0f);

            // Eşleştirme modunu belirle (Test/Debug öncelikli)
            if (forceDesiredMode)
            {
                currentMatchMode = desiredMatchMode;
            }
            else
            {
                // Rastgele bir mod seç (0 veya 1)
                currentMatchMode = (StroopMatchMode)UnityEngine.Random.Range(0, 2);
            }

            _introFinished = false;
            if (uiController != null)
            {
                uiController.StartGameplayIntro(null, currentMatchMode, OnIntroFinished);
            }
            else
            {
                _introFinished = true;
            }

            SetupBoardWithMode(currentMatchMode, _totalMatches);
        }

        private void SetupBoardWithMode(StroopMatchMode mode, int matches)
        {
            var deck = _deckGenerator.GenerateDeck(mode, matches);
            _deckGenerator.ShuffleDeck(deck);

            boardManager.SetupBoard(deck, OnCardClicked, () =>
            {
                _entranceAnimFinished = true;
                CheckGameStartCondition();
            });
        }

        private void OnIntroFinished()
        {
            _introFinished = true;
            CheckGameStartCondition();
        }

        private void CheckGameStartCondition()
        {
            if (_entranceAnimFinished && _introFinished)
            {
                IsGameRunning = true;
                if (uiController != null)
                {
                    uiController.ShowGameplayUI();
                }
                GameEvents.RaiseGameStarted();
            }
        }

        public void EndGame()
        {
            IsGameRunning = false;
            turnTimer.StopTimer();
            uiController.ShowGameOver(_totalGameTime, isVictory: true);

            // Profil verilerini güncelle
            float finalAccuracy = _totalAttempts > 0 ? (float)_currentMatches / _totalAttempts : 1.0f;
            int finalScore = (int)(finalAccuracy * 1000f - _totalGameTime * 2f);
            if (finalScore < 0) finalScore = 0;

            int mistakes = _totalAttempts - _currentMatches;
            PlayerProfileManager.UpdateProfile("MemoryMatch", finalAccuracy, finalScore, _currentLevel, _totalGameTime, mistakes, failed: false);

            GameEvents.RaiseGameEnded();
        }

        private void HandleGameTimeOut()
        {
            IsGameRunning = false;
            turnTimer.StopTimer();
            uiController.ShowGameOver(_totalGameTime, isVictory: false);

            // Başarısızlığı profile kaydet (skor 0)
            float finalAccuracy = _totalAttempts > 0 ? (float)_currentMatches / _totalAttempts : 0f;
            int mistakes = _totalAttempts - _currentMatches;
            PlayerProfileManager.UpdateProfile("MemoryMatch", finalAccuracy, 0, _currentLevel, _totalGameTime, mistakes, failed: true);

            GameEvents.RaiseGameEnded();
        }

        // ─── Card Click Handler ─────────────────────────────────

        private void OnCardClicked(Card clickedCard)
        {
            if (!IsGameRunning) return;

            bool evaluationDone = _matchValidator.ProcessCardSelection(clickedCard);

            if (!evaluationDone)
            {
                // İlk kart seçildi, timer başlasın
                turnTimer.StartTimer();
            }
            else
            {
                // İkinci kart seçildi, timer dursun
                turnTimer.StopTimer();
            }
        }

        // ─── Event Handlers ─────────────────────────────────────

        private void HandleMatchResult(Card card1, Card card2, bool isMatch)
        {
            uiController.ShowMatchResult(isMatch);
            _matchValidator.ResetTurn();

            _totalAttempts++;

            if (isMatch)
            {
                _currentMatches++;
                GameEvents.RaiseMatchResult(true);
                GameEvents.RaiseSFXRequested("correct");

                if (_currentMatches >= _totalMatches)
                {
                    EndGame();
                }
            }
            else
            {
                GameEvents.RaiseMatchResult(false);
                GameEvents.RaiseSFXRequested("wrong");
            }
        }

        private void HandleTimeOut()
        {
            if (_matchValidator.FirstCard != null)
            {
                _matchValidator.FirstCard.PlayErrorEffect();
            }

            _matchValidator.ResetTurn();
            boardManager.ShuffleBoardVisuals();
        }
    }
}
