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
        [SerializeField] private TMP_Text cognitiveText;

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

            if (_introCoroutine != null)
            {
                StopCoroutine(_introCoroutine);
                _introCoroutine = null;
            }

            // Alpha değerini temizle
            if (instructionText != null)
            {
                instructionText.alpha = 1f;
            }
            if (cognitiveText != null)
            {
                cognitiveText.alpha = 1f;
            }
        }

        public void Initialize(System.Action onPlayAgain)
        {
            _onPlayAgainClicked = onPlayAgain;

            if (gameOverPanel != null)
            {
                gameOverPanel.OnPlayAgainClicked += () => _onPlayAgainClicked?.Invoke();
                gameOverPanel.OnMainMenuClicked += GoToMainMenu;
            }
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

        public void HideAllGameplayUI()
        {
            if (instructionText != null) instructionText.alpha = 0f;
            if (cognitiveText != null) cognitiveText.alpha = 0f;
            if (turnTimerText != null) turnTimerText.alpha = 0f;
        }

        public void ShowGameplayUI()
        {
            if (turnTimerText != null)
            {
                turnTimerText.transform.DOKill();
                turnTimerText.alpha = 0f;
                DOTween.To(() => turnTimerText.alpha, x => turnTimerText.alpha = x, 1f, 0.35f).SetUpdate(true);
            }
        }

        public void ShowInstruction(StroopMatchMode mode)
        {
            if (instructionText != null)
            {
                instructionText.text = mode == StroopMatchMode.TextVsText
                    ? "Ayni 'KELIME' Yazan Kartlari Eslestir!"
                    : "Renk Ismi Ile 'DUZ RENK' Kartini Eslestir!";
                
                instructionText.transform.DOKill();
                DOTween.To(() => instructionText.alpha, x => instructionText.alpha = x, 1f, 0.35f).SetUpdate(true);
            }
        }

        private Coroutine _introCoroutine;

        public void StartGameplayIntro(string comment, StroopMatchMode mode, System.Action onComplete)
        {
            if (_introCoroutine != null)
                StopCoroutine(_introCoroutine);

            _introCoroutine = StartCoroutine(GameplayIntroRoutine(comment, mode, onComplete));
        }

        private System.Collections.IEnumerator GameplayIntroRoutine(string comment, StroopMatchMode mode, System.Action onComplete)
        {
            // 1. AŞAMA: BİLİŞSEL ANALİZ YAZISI (Eğer varsa)
            if (!string.IsNullOrEmpty(comment))
            {
                TMP_Text activeText = cognitiveText != null ? cognitiveText : instructionText;

                if (activeText != null)
                {
                    // Yazıları sıfırla
                    if (cognitiveText != null && instructionText != null)
                    {
                        instructionText.alpha = 0f;
                        cognitiveText.alpha = 0f;
                    }
                    else
                    {
                        activeText.alpha = 0f;
                    }

                    // Bilişsel analiz yazısını ata ve Fade-In yap
                    activeText.text = $"<color=#FFA500>Bilissel Analiz: {comment}</color>";
                    yield return DOTween.To(() => activeText.alpha, x => activeText.alpha = x, 1f, 0.25f)
                        .SetUpdate(true)
                        .WaitForCompletion();

                    // 5 saniye okuma süresi
                    yield return new WaitForSeconds(5.0f);

                    // Fade-Out yap ve metni temizle
                    yield return DOTween.To(() => activeText.alpha, x => activeText.alpha = x, 0f, 0.25f)
                        .SetUpdate(true)
                        .WaitForCompletion();
                    activeText.text = "";
                }
            }

            // 2. AŞAMA: OYUN MODU TALİMATI (Her zaman gösterilir)
            if (instructionText != null)
            {
                // Yazıları sıfırla
                instructionText.alpha = 0f;
                instructionText.text = mode == StroopMatchMode.TextVsText
                    ? "Ayni 'KELIME' Yazan Kartlari Eslestir!"
                    : "Renk Ismi Ile 'DUZ RENK' Kartini Eslestir!";

                // Talimatı Fade-In yap
                yield return DOTween.To(() => instructionText.alpha, x => instructionText.alpha = x, 1f, 0.25f)
                    .SetUpdate(true)
                    .WaitForCompletion();

                // 5 saniye talimat gösterme süresi
                yield return new WaitForSeconds(5.0f);

                // Talimatı Fade-Out yap (Süre dolunca ekrandan kaybolur)
                yield return DOTween.To(() => instructionText.alpha, x => instructionText.alpha = x, 0f, 0.25f)
                    .SetUpdate(true)
                    .WaitForCompletion();
            }

            // Arayüz giriş akışı bitti, oyunu ve süreyi başlat
            onComplete?.Invoke();
        }

        // ─── Match Sonuçları ────────────────────────────────────

        public void ShowMatchResult(bool isMatch)
        {
            if (turnTimerText != null)
            {
                turnTimerText.text = isMatch
                    ? "<color=green>Bildiniz!</color>"
                    : "<color=red>Yanlis Eslesme!</color>";
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
                turnTimerText.text = "Hazir!";
        }

        // ─── Game Over (Core Panel) ─────────────────────────────

        public void ShowGameOver(float totalTime, bool isVictory = true)
        {
            if (turnTimerText != null)
                turnTimerText.text = isVictory ? "TEBRIKLER!" : "SURE BITTI!";

            if (gameOverPanel != null)
            {
                gameOverPanel.ShowGameOver(
                    title: isVictory ? "Tebrikler!" : "Sure Bitti!",
                    score: -1,
                    time: totalTime,
                    subtitle: isVictory ? "Tum kartlari eslestirdin!" : "Zamaninda yetisemedin.",
                    isVictory: isVictory
                );
            }
        }

        public void UpdateTimersDisplay(float turnTimeLeft, float sessionTimeLeft)
        {
            if (turnTimerText != null)
            {
                string turnStr = turnTimeLeft > 0f ? $"{turnTimeLeft:F1}s" : "-";
                string sessionStr = sessionTimeLeft > 0f ? $"{sessionTimeLeft:F1}s" : "0.0s";
                turnTimerText.text = $"Tur: {turnStr} | Kalan: {sessionStr}";

                // Kalan süre azaldıkça kırmızı yap
                if (sessionTimeLeft <= 10f)
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
                turnTimerText.text = "<color=red>SURE DOLDU! KARTLAR KARISTI...</color>";
                turnTimerText.color = Color.white;

                // Sarsılma efekti
                turnTimerText.transform.DOKill();
                turnTimerText.transform.DOShakePosition(0.5f, 10f, 20, 90f, false, true, ShakeRandomnessMode.Harmonic);
            }

            GameEvents.RaiseSFXRequested("wrong");
        }

        private void OnDestroy()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.OnMainMenuClicked -= GoToMainMenu;
            }
        }
    }
}
