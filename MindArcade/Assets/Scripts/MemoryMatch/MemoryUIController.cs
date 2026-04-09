using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MemoryMatch
{
    /// <summary>
    /// SRP: Sadece Memory Match oyununun UI güncellemelerinden sorumludur.
    /// Event'leri dinleyerek UI'ı günceller, oyun mantığına dokunmaz.
    /// </summary>
    public class MemoryUIController : MonoBehaviour
    {
        [Header("UI Referansları")]
        [SerializeField] private TMP_Text instructionText;
        [SerializeField] private TMP_Text turnTimerText;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TMP_Text gameOverTimeText;
        [SerializeField] private Button playAgainButton;

        [Header("Bağımlılıklar")]
        [SerializeField] private TurnTimer turnTimer;

        private System.Action _onPlayAgainClicked;

        private void OnEnable()
        {
            if (turnTimer != null)
            {
                turnTimer.OnTimerUpdated += UpdateTimerDisplay;
                turnTimer.OnTimeOut += ShowTimeOutMessage;
            }
        }

        private void OnDisable()
        {
            if (turnTimer != null)
            {
                turnTimer.OnTimerUpdated -= UpdateTimerDisplay;
                turnTimer.OnTimeOut -= ShowTimeOutMessage;
            }
        }

        public void Initialize(System.Action onPlayAgain)
        {
            _onPlayAgainClicked = onPlayAgain;
            if (playAgainButton != null)
                playAgainButton.onClick.AddListener(() => _onPlayAgainClicked?.Invoke());
        }

        public void ShowInstruction(StroopMatchMode mode)
        {
            if (instructionText != null)
            {
                instructionText.text = mode == StroopMatchMode.TextVsText
                    ? "Aynı 'KELİME' Yazan Kartları Eşleştir!"
                    : "Renk İsmi İle 'DÜZ RENK' Kartını Eşleştir!";
            }
        }

        public void ShowMatchResult(bool isMatch)
        {
            if (turnTimerText != null)
            {
                turnTimerText.text = isMatch
                    ? "<color=green>Bildiniz!</color>"
                    : "<color=red>Yanlış Eşleşme!</color>";
            }
        }

        public void ShowReady()
        {
            if (turnTimerText != null)
                turnTimerText.text = "Hazır!";
        }

        public void ShowGameOver(float totalTime)
        {
            if (turnTimerText != null)
                turnTimerText.text = "OYUN BİTTİ!";

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                if (gameOverTimeText != null)
                    gameOverTimeText.text = "Toplam Tamamlama Süresi:\n" + totalTime.ToString("F1") + "s";
            }
        }

        public void HideGameOver()
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
        }

        public bool IsGameOverActive =>
            gameOverPanel != null && gameOverPanel.activeSelf;

        // ─── Timer Event Handlers ─────────────────────────────

        private void UpdateTimerDisplay(float remainingTime)
        {
            if (turnTimerText != null)
                turnTimerText.text = "Kalan: " + remainingTime.ToString("F1") + "s";
        }

        private void ShowTimeOutMessage()
        {
            if (turnTimerText != null)
                turnTimerText.text = "<color=red>SÜRE DOLDU! KARTLAR KARIŞTI...</color>";
        }
    }
}
