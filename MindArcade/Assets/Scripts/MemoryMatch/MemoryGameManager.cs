using UnityEngine;
using MindArcade.Core;

namespace MemoryMatch
{
    /// <summary>
    /// Orchestrator: Oyun yaşam döngüsünü yönetir ve alt sistemleri koordine eder.
    /// Artık 344 satır değil, sadece ~90 satır.
    /// 
    /// Sorumluluk dağılımı:
    /// - DeckGenerator  → Deste üretimi ve karıştırma
    /// - BoardManager   → Kart instantiate, board temizleme, görsel karıştırma
    /// - MatchValidator → İki kartın eşleşme kontrolü
    /// - TurnTimer      → Tur zamanlayıcısı
    /// - MemoryUIController → Tüm UI güncellemeleri
    /// - MemoryColorPalette → Renk verileri (ScriptableObject)
    /// </summary>
    public class MemoryGameManager : MonoBehaviour, IGameManager
    {
        [Header("Oyun Modu")]
        public StroopMatchMode currentMatchMode = StroopMatchMode.TextVsColor;

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

        private void Start()
        {
            _deckGenerator = new DeckGenerator(colorPalette);
            _matchValidator = new MatchValidator();

            // Event bağlantıları
            _matchValidator.OnMatchEvaluated += HandleMatchResult;
            turnTimer.OnTimeOut += HandleTimeOut;

            uiController.Initialize(StartGame);

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
            }
        }

        // ─── IGameManager Implementation ────────────────────────

        public void StartGame()
        {
            IsGameRunning = false; // Animasyon bitene kadar oyun başlamıyor
            _totalGameTime = 0f;
            _currentMatches = 0;

            uiController.HideGameOver();
            uiController.ShowInstruction(currentMatchMode);
            uiController.ShowReady();

            boardManager.ClearBoard();
            _matchValidator.ResetTurn();

            var deck = _deckGenerator.GenerateDeck(currentMatchMode);
            _deckGenerator.ShuffleDeck(deck);

            // Kartlar giriş animasyonu bitince oyun başlar
            boardManager.SetupBoard(deck, OnCardClicked, () =>
            {
                IsGameRunning = true;
                GameEvents.RaiseGameStarted();
            });
        }

        public void EndGame()
        {
            IsGameRunning = false;
            turnTimer.StopTimer();
            uiController.ShowGameOver(_totalGameTime);
            GameEvents.RaiseGameEnded();
        }

        // ─── Card Click Handler ─────────────────────────────────

        private void OnCardClicked(Card clickedCard)
        {
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

            if (isMatch)
            {
                _currentMatches++;
                GameEvents.RaiseMatchResult(true);

                if (_currentMatches >= _totalMatches)
                {
                    EndGame();
                }
            }
            else
            {
                GameEvents.RaiseMatchResult(false);
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
