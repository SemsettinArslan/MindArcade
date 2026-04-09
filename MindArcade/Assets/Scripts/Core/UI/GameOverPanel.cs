using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace MindArcade.Core.UI
{
    /// <summary>
    /// Ortak Game Over paneli.
    /// UIPanel'den türer → otomatik açılma/kapanma animasyonları.
    /// Her oyun bu component'i kullanabilir.
    /// </summary>
    public class GameOverPanel : UIPanel
    {
        [Header("Game Over — UI Elemanları")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text subtitleText;
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Başarı Efekti")]
        [SerializeField] private ParticleSystem confettiEffect;

        // ─── Events ─────────────────────────────────────────────
        public event Action OnPlayAgainClicked;
        public event Action OnMainMenuClicked;

        protected override void Awake()
        {
            base.Awake();

            if (playAgainButton != null)
                playAgainButton.onClick.AddListener(() => OnPlayAgainClicked?.Invoke());

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(() => OnMainMenuClicked?.Invoke());
        }

        // ─── Public API ─────────────────────────────────────────

        /// <summary>
        /// Game Over panelini verilen bilgilerle gösterir.
        /// </summary>
        /// <param name="title">Başlık ("Tebrikler!", "Oyun Bitti!" vb.)</param>
        /// <param name="score">Skor (-1 ise gizlenir)</param>
        /// <param name="time">Süre (-1 ise gizlenir)</param>
        /// <param name="subtitle">Alt yazı (opsiyonel)</param>
        /// <param name="isVictory">true ise konfeti efekti</param>
        public void ShowGameOver(string title, int score = -1, float time = -1f, string subtitle = "", bool isVictory = true)
        {
            // Başlık
            if (titleText != null)
                titleText.text = title;

            // Skor
            if (scoreText != null)
            {
                scoreText.gameObject.SetActive(score >= 0);
                if (score >= 0)
                    scoreText.text = $"Puan: {score}";
            }

            // Süre
            if (timeText != null)
            {
                timeText.gameObject.SetActive(time >= 0);
                if (time >= 0)
                    timeText.text = $"Süre: {time:F1}s";
            }

            // Alt Yazı
            if (subtitleText != null)
            {
                subtitleText.gameObject.SetActive(!string.IsNullOrEmpty(subtitle));
                subtitleText.text = subtitle;
            }

            // Panel aç (UIPanel'in animasyonu)
            Show();

            // Konfeti
            if (isVictory && confettiEffect != null)
                confettiEffect.Play();

            // Ses
            GameEvents.RaiseSFXRequested(isVictory ? "victory" : "defeat");
        }

        /// <summary>
        /// Paneli kapatır.
        /// </summary>
        public void HideGameOver()
        {
            if (confettiEffect != null)
                confettiEffect.Stop();

            Hide();
        }

        protected override void OnShowComplete()
        {
            // Başlık bounce efekti
            if (titleText != null)
            {
                titleText.transform.localScale = Vector3.one;
                titleText.transform.DOPunchScale(Vector3.one * 0.15f, 0.4f, 8, 0.5f);
            }
        }
    }
}
