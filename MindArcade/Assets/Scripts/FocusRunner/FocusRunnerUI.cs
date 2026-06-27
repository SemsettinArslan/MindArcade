using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using MindArcade.Core;
using MindArcade.Core.UI;

namespace FocusRunner
{
    /// <summary>
    /// SRP: Sadece FocusRunner oyununun UI güncellemelerinden sorumludur.
    /// Core UI Framework kullanır: GameOverPanel, AnimatedButton.
    /// GameEvents üzerinden event-driven çalışır.
    /// </summary>
    public class FocusRunnerUI : MonoBehaviour
    {
        [Header("Texts")]
        [SerializeField] private TMP_Text _ruleText;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _statsText; // Eski tek satır statlar için fallback
        [SerializeField] private TMP_Text _livesText; // Ayrı can göstergesi
        [SerializeField] private TMP_Text _timeText;  // Ayrı süre göstergesi
        [SerializeField] private TMP_Text _accuracyText; // Ayrı doğruluk göstergesi
        [SerializeField] private TMP_Text _levelNotificationText;

        [Header("Feedback Effects")]
        [SerializeField] private Image _flashOverlay;
        [SerializeField] private Color _correctFlashColor = new Color(0, 1, 0, 0.3f);
        [SerializeField] private Color _wrongFlashColor = new Color(1, 0, 0, 0.3f);

        [Header("Core UI Panelleri")]
        [SerializeField] private GameOverPanel _gameOverPanel;

        public GameOverPanel GameOverPanel => _gameOverPanel;

        private Tween _flashTween;

        private void OnEnable()
        {
            GameEvents.OnFlashFeedback += TriggerFlash;
            GameEvents.OnLevelChanged += OnLevelChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnFlashFeedback -= TriggerFlash;
            GameEvents.OnLevelChanged -= OnLevelChanged;
        }

        private void Start()
        {
            if (RuleManager.Instance != null)
                RuleManager.Instance.OnRuleChanged += UpdateRuleUI;

            if (_levelNotificationText != null)
                _levelNotificationText.gameObject.SetActive(false);

            if (_flashOverlay != null)
                _flashOverlay.color = Color.clear;
        }

        private void OnDestroy()
        {
            if (RuleManager.Instance != null)
                RuleManager.Instance.OnRuleChanged -= UpdateRuleUI;

            _flashTween?.Kill();
        }

        private void Update()
        {
            if (FocusRunnerManager.Instance == null) return;

            if (_scoreText != null)
                _scoreText.text = $"Puan: {FocusRunnerManager.Instance.Score} / {FocusRunnerManager.Instance.TargetScore}";

            int remainingMistakes = FocusRunnerManager.Instance.MaxMistakesAllowed - FocusRunnerManager.Instance.Mistakes;
            if (remainingMistakes < 0) remainingMistakes = 0;

            // Eğer ayrı text alanları atanmışsa onları güncelle
            if (_livesText != null || _timeText != null || _accuracyText != null)
            {
                if (_livesText != null)
                    _livesText.text = $"Can: {remainingMistakes} / {FocusRunnerManager.Instance.MaxMistakesAllowed}";

                if (_timeText != null)
                    _timeText.text = $"Sure: {FocusRunnerManager.Instance.LevelTimeRemaining:F1}s";

                if (_accuracyText != null)
                    _accuracyText.text = $"Dogruluk: {FocusRunnerManager.Instance.Accuracy:P0}";
            }
            // Aksi takdirde eski tek satır fallback alanını güncelle (Level bilgisini yazmadan)
            else if (_statsText != null)
            {
                _statsText.text = $"Hak: {remainingMistakes}/{FocusRunnerManager.Instance.MaxMistakesAllowed} | Sure: {FocusRunnerManager.Instance.LevelTimeRemaining:F1}s | Dogruluk: {FocusRunnerManager.Instance.Accuracy:P0}";
            }
        }

        // ─── Event Handlers ─────────────────────────────────────

        private void TriggerFlash(bool isCorrect)
        {
            if (_flashOverlay == null) return;

            // DOTween ile flash (Coroutine yerine)
            _flashTween?.Kill();
            Color targetColor = isCorrect ? _correctFlashColor : _wrongFlashColor;
            _flashOverlay.color = targetColor;

            _flashTween = _flashOverlay
                .DOFade(0f, 0.25f)
                .SetEase(Ease.OutQuad);

            // Skor text'i punch efekti
            if (_scoreText != null)
            {
                _scoreText.transform.DOKill();
                _scoreText.transform.localScale = Vector3.one;
                _scoreText.transform.DOPunchScale(
                    isCorrect ? Vector3.one * 0.15f : Vector3.one * -0.1f,
                    0.3f, 6, 0.5f
                );
            }

            GameEvents.RaiseSFXRequested(isCorrect ? "correct" : "wrong");
        }

        private void OnLevelChanged(int newLevel)
        {
            ShowLevelUpNotification($"SEVIYE: {newLevel}");
            GameEvents.RaiseSFXRequested("level_up");
        }

        // ─── UI Methods ─────────────────────────────────────────

        private void UpdateRuleUI(GameRule rule, string description)
        {
            if (_ruleText != null)
            {
                _ruleText.text = description;

                // Kural değişim animasyonu
                _ruleText.transform.DOKill();
                _ruleText.transform.localScale = Vector3.one * 1.3f;
                _ruleText.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
            }

            ShowLevelUpNotification("HEDEF DEGISTI!");
        }

        public void ShowLevelUpNotification(string text)
        {
            if (_levelNotificationText == null) return;

            _levelNotificationText.text = text;
            _levelNotificationText.gameObject.SetActive(true);

            // DOTween ile göster/gizle animasyonu
            _levelNotificationText.transform.DOKill();
            _levelNotificationText.transform.localScale = Vector3.zero;

            Sequence seq = DOTween.Sequence();
            seq.Append(_levelNotificationText.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack));
            seq.AppendInterval(1.2f);
            seq.Append(_levelNotificationText.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack));
            seq.OnComplete(() =>
            {
                if (_levelNotificationText != null)
                    _levelNotificationText.gameObject.SetActive(false);
            });
        }

        public void ShowGameOver(int score)
        {
            if (_gameOverPanel != null)
            {
                _gameOverPanel.ShowGameOver(
                    title: "Oyun Bitti!",
                    score: score,
                    subtitle: $"Dogruluk: {FocusRunnerManager.Instance.Accuracy:P0}"
                );
            }
        }
    }
}
