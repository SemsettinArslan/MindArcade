using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using MindArcade.Core;
using MindArcade.Core.UI;

namespace MemoryMatch
{
    /// <summary>
    /// SRP: Sadece Memory Match oyununun UI güncellemelerinden sorumludur.
    /// Core UI Framework'ü kullanır: GameOverPanel, animasyonlar.
    /// Event'leri dinleyerek UI'ı günceller, oyun mantığına dokunmaz.
    /// </summary>
    public class MemoryUIController : MonoBehaviour
    {
        [Header("Oyuna Özel UI")]
        [SerializeField] private TMP_Text instructionText;
        [SerializeField] private TMP_Text turnTimerText;

        [Header("Core UI Panelleri (Inspector'dan Ata)")]
        [SerializeField] private GameOverPanel gameOverPanel;

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

            if (gameOverPanel != null)
            {
                gameOverPanel.OnPlayAgainClicked += () => _onPlayAgainClicked?.Invoke();
            }
        }

        // ─── Instruction / Mod Gösterimi ────────────────────────

        public void ShowInstruction(StroopMatchMode mode)
        {
            if (instructionText != null)
            {
                instructionText.text = mode == StroopMatchMode.TextVsText
                    ? "Aynı 'KELİME' Yazan Kartları Eşleştir!"
                    : "Renk İsmi İle 'DÜZ RENK' Kartını Eşleştir!";
            }
        }

        // ─── Match Sonuçları ────────────────────────────────────

        public void ShowMatchResult(bool isMatch)
        {
            if (turnTimerText != null)
            {
                turnTimerText.text = isMatch
                    ? "<color=green>Bildiniz!</color>"
                    : "<color=red>Yanlış Eşleşme!</color>";
            }

            // DOTween ile text punch efekti
            if (turnTimerText != null)
            {
                turnTimerText.transform.DOKill();
                turnTimerText.transform.localScale = Vector3.one;
                turnTimerText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 6, 0.5f);
            }
        }

        public void ShowReady()
        {
            if (turnTimerText != null)
                turnTimerText.text = "Hazır!";
        }

        // ─── Game Over (Core Panel) ─────────────────────────────

        public void ShowGameOver(float totalTime)
        {
            if (turnTimerText != null)
                turnTimerText.text = "OYUN BİTTİ!";

            if (gameOverPanel != null)
            {
                gameOverPanel.ShowGameOver(
                    title: "Tebrikler!",
                    score: -1,
                    time: totalTime,
                    subtitle: "Tüm kartları eşleştirdin!",
                    isVictory: true
                );
            }
        }

        public void HideGameOver()
        {
            if (gameOverPanel != null)
                gameOverPanel.HideGameOver();
        }

        public bool IsGameOverActive =>
            gameOverPanel != null && gameOverPanel.IsVisible;

        // ─── Timer Event Handlers ───────────────────────────────

        private void UpdateTimerDisplay(float remainingTime)
        {
            if (turnTimerText != null)
            {
                turnTimerText.text = "Kalan: " + remainingTime.ToString("F1") + "s";

                // Son 2 saniyede kırmızı yanıp sönme
                if (remainingTime <= 2f)
                {
                    float pulse = Mathf.PingPong(Time.time * 4f, 1f);
                    turnTimerText.color = Color.Lerp(Color.white, Color.red, pulse);
                }
                else
                {
                    turnTimerText.color = Color.white;
                }
            }
        }

        private void ShowTimeOutMessage()
        {
            if (turnTimerText != null)
            {
                turnTimerText.text = "<color=red>SÜRE DOLDU! KARTLAR KARIŞTI...</color>";
                turnTimerText.color = Color.white;

                // Sarsılma efekti
                turnTimerText.transform.DOKill();
                turnTimerText.transform.DOShakePosition(0.5f, 10f, 20, 90f, false, true, ShakeRandomnessMode.Harmonic);
            }

            GameEvents.RaiseSFXRequested("wrong");
        }
    }
}
